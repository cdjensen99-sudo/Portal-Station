# How the Portal Station prefab is built

This documents the **working visual/layout recipe** from `StationPrefabBuilder` and related builders. This part was largely correct; most pain was in **runtime networking**, not geometry.

---

## 1. High-level build order (`StationPrefabBuilder.Build`)

Execute when `ZNetScene.instance` exists (hooked from `ZNetScene.Awake` / `ZNet.Start`).

```
1. Clone woodwall as inactive network root
2. Strip root PieceTable + ItemDrop (inherit from woodwall)
3. Hide root wall renderers
4. Destroy ALL colliders on root (re-add one placement BoxCollider later)
5. Configure Piece (name, category Misc, empty resources at build time)
6. Add BoxCollider for placement/hammer raycast
7. Configure ZNetView (persistent, not distant)
8. Add RunicStation component
9. Build frame (poles/beams) — visual children only
10. Build board (walls/quarters) — visual children only
11. Build header sign + 18 destination signs
12. Bind signs to RunicStation
13. Apply 0.75 scale to entire root
14. Sanitize children (strip child ZNetView, Piece, ItemDrop, WearNTear)
15. Set piece layer, disable child colliders on template
16. Finalize template: inactive + DontDestroyOnLoad
```

---

## 2. Root prefab clone (`NetworkPrefabHelper.CloneInactiveNetworkPrefab`)

**Critical rules:**

- Instantiate source with `ZNetView.m_forceDisableInit = true` so **no ZDO is created** during build.
- Immediately `SetActive(false)`.
- Rename to `runic_portal_station` (must match `ModConstants.PrefabStation`).
- Copy `ZNetView` settings from source (`m_persistent`, `m_type`, `m_distant`, `m_syncInitialScale`).
- `StripPrefabTemplateNetworkState` — clear any `m_zdo` on template.
- `DontDestroyOnLoad` — single cached template reused across `ZNetScene` wakes.

**Never** build the template while active without `forceDisableInit` — creates orphan ZDOs in the world save.

---

## 3. Frame geometry (`StationFrameBuilder`)

Origin: ground-level center of 3 m wide structure. **X** = left/right, **Y** = up, **Z** = forward (board face).

### Vertical levels

| Y | Element |
|---|---------|
| 0 | Ground leg poles |
| 1 | Bottom beams + start of side posts |
| 3 | 1 m pole extensions on sides |
| 4 | Top beams |

### X positions

- Left leg/beam end: **-1.5**
- Right leg/beam end: **+1.5**
- Right column center (quarter walls): **+1.0**

### Build sequence (matches manual in-game construction)

**Legs (#1, #4):** `wood_pole` at (-1.5, 0, 0) and (1.5, 0, 0)

**Bottom beams (#2, #3):**
- `wood_beam` (2 m) at (-0.5, 1, 0)
- `wood_beam_1` (1 m) at (1, 1, 0)

**Side posts (#5, #6):**
- `wood_pole2` at (-1.5, 1, 0) and (1.5, 1, 0)
- `wood_pole` stacked at (-1.5, 3, 0) and (1.5, 3, 0)

**Top beams (#7):**
- `wood_beam_1` at (-1, 4, 0)
- `wood_beam` at (0.5, 4, 0)

### Board (`BuildBoard`)

- **#8:** `woodwall` 2×2 at (-0.5, 1, 0) — left two-thirds
- **#9:** `wood_wall_half` at (-0.5, 3, 0) — upper left
- **#10:** three `wood_wall_quarter` in right column at x=1, y = 1, 2, 3

---

## 4. Visual child assembly (`StationVisualHelper`)

For each vanilla piece used as decoration:

1. `RunWithoutZdoCreation(() => Instantiate(prefab))`
2. `SetActive(false)`
3. `StripForVisualChild` — remove Sign, ZNetView, Piece, WearNTear, ItemDrop, colliders
4. `SetActive(true)`
5. `StationPiecePlacement.Place` with correct anchor (poles: BottomCenter, walls: BottomFrontCenter, beams: auto-rotated along X)

Beams: measure width at identity vs 90° Y rotation; pick orientation that spans X axis.

---

## 5. Sign grid (`StationSignGridBuilder`)

- **18 slots:** 3 columns × 6 rows
- Column X centers: **-1, 0, 1**
- Row height: **0.5 m** per sign (vanilla sign scale)
- Board bottom Y: **1**
- Header sign at **(0, 4.3, faceZ + 0.1)**

### Sign Z placement

Resolve wall front Z from renderer bounds per column/row:
- Columns 0–1, rows 0–3 → `woodwall` front
- Columns 0–1, rows 4–5 → `wood_wall_half` front
- Column 2 → `wood_wall_quarter` front

Destination signs: `wallFrontZ + 0.02 (epsilon) + 0.37 (forward offset)`

Each sign:
- Clone `sign` / `piece_sign`
- `PortalDisplaySignSpawner.StripSign`
- `StationSignOrientation` for board-facing rotation (180° on Y from mount)
- Add `StationSignVisual`, `StationDestinationSign` or `StationHeaderSign`
- BoxCollider on sign (disabled on template, enabled after place)

---

## 6. Root collider (placement only)

```csharp
BoxCollider on root:
  center = (0, 2.1, 0)
  size   = (3.2, 4.2, 0.5)
```

Child sign colliders: **disabled** on template, **enabled** on placed instance (`StationInteractionColliders`).

---

## 7. Piece / hammer registration

**Do not** set `piece.m_resources` with item refs during `ZNetScene.Awake` build — `ObjectDB` may not be ready; causes null refs in recipe UI.

Instead:
- Build with `piece.m_resources = empty`
- `ApplyCraftingSetup` from `ObjectDB` timing / `PieceTablePatch`:
  - Copy workbench `m_craftingStation` + `m_placeEffect`
  - Set resources: 58 Wood, 19 Coal
  - Apply hammer icon

Register in `_HammerPieceTable` **Misc** category via `piece.m_category = Misc`.

Insert after vanilla portal entry in piece list.

**Prefab cache:** one `DontDestroyOnLoad` instance per build label; re-register on each `ZNetScene.Awake` without rebuilding.

---

## 8. Hammer icon (`StationHammerIcon`)

- PNG embedded in DLL: `RunicPortals.Assets.Station_Icon.png`
- Fallback: `Station_Icon.png` next to DLL in plugin folder
- `Sprite.Create` with `pixelsPerUnit` matched to vanilla portal piece icon

---

## 9. Localization keys

Registered in `ModLocalization` on `FejdStartup.SetupGui` (with re-entry guard):

| Key | English |
|-----|---------|
| `$piece_runic_station` | Portal Station |
| `$piece_runic_station_desc` | (see LocalizationPatch.cs) |
| `$piece_runic_station_configure_title` | Portal Station Configuration |
| `$piece_runic_station_commit` | Commit |
| etc. | |

Use `$key` in `piece.m_name` / UI.

---

## 10. What happens when player places (intended flow)

```
Player.PlacePiece
  → Instantiate inactive prefab clone
  → Piece.SetCreator prefix: SetActive(true) if inactive
  → ZNetView.Awake: CreateNewZDO + AddInstance
  → Piece.SetCreator: write creator to ZDO
  → SetCreator postfix:
       StationNetworkBinder (allowCreateZdo: true)
       StationWearNTearHelper.EnsureOnRoot (AddComponent WearNTear)
       StationInteractionColliders.Enable
  → WearNTear.OnPlaced (if present)
```

**On world load:** custom `ZNetScene.CreateObject` prefix for `runic_portal_station` — instantiate inactive prefab with `m_initZDO`, bind ZDO, then same EnsureOnRoot path.

See ARCHITECTURE.md and LESSONS_LEARNED.md for why this flow was fragile.
