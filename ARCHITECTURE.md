# Architecture — runtime behavior & patches

## Component diagram (intended)

```
runic_portal_station (root)
├── ZNetView          ← single network identity
├── Piece             ← hammer piece
├── RunicStation      ← config RPCs, sign refresh, portal link
├── WearNTear         ← added at runtime on place/load only (stripped from template)
├── BoxCollider       ← placement / hammer raycast (root)
├── [visual children] ← poles, beams, walls (no networking)
├── _station_header   ← StationHeaderSign + StationSignVisual
└── _station_slot_XX  ← StationDestinationSign × 18
```

---

## Lifecycle phases

| Phase | Object | Active? | ZNetView | WearNTear | Must NOT |
|-------|--------|---------|----------|-----------|----------|
| DDoL template | `runic_portal_station` in DontDestroyOnLoad | **No** | component present, `m_zdo = null` | stripped | Create ZDO, run Start/Awake logic |
| Placement ghost | clone while aiming | Yes, **ghost** layer | destroyed (`forceDisableInit`) | stripped / disabled | Create ZDO, register RPCs |
| Placed instance | after `PlacePiece` | Yes | valid ZDO + AddInstance | AddComponent after bind | — |
| World load | `ZNetScene.CreateObject` | activated on spawn | bind saved ZDO via `m_initZDO` | EnsureOnRoot | duplicate ZDO |

**Detection helper (alpha46):** `StationInstanceHelper.IsRuntimeInstance`
- Reject `scene.name == "DontDestroyOnLoad"`
- Reject `layer == ghost`

---

## ZDO custom fields

| Key | Purpose |
|-----|---------|
| `RunicPortals_StationName` | Display name on header |
| `RunicPortals_LinkedPortal` | `ZDOID` string of linked TeleportWorld |
| `RunicPortals_Slot{N}_Portal` | Portal tag name (max 10 chars) |
| `RunicPortals_Slot{N}_Display` | Optional display text (color codes) |

## RPCs (on station ZNetView)

| RPC | Direction | Purpose |
|-----|-----------|---------|
| `runic_portals.apply_station_config` | client → server | Save names/slots |
| `runic_portals.activate_destination` | client → server | Set linked portal tag |

---

## Config UI (`StationConfigUI`)

- IMGUI `OnGUI` modal on plugin GameObject
- Opened from `StationHeaderSign.Interact` (E key)
- While open:
  - `StationConfigInputPatch` — block Player/PlayerController input
  - `StationConfigZInputPatch` — block all `ZInput` (fixes map key while typing)
  - `Minimap.Update` / `GameCamera` patches — cursor unlock
- Commit → `RunicStation.TryApplyConfiguration` → server writes ZDO, refreshes signs

---

## Destination activation

`DestinationActivatePatch` on `Player.Update`:
- LMB (`ZInput.GetButtonDown("Attack")`) on `StationDestinationSign` collider
- Calls `RunicStation.ActivateDestination(slot)` → sets linked portal tag via `PortalTagHelper`

---

## Portal linking

- On config commit: `StationGroupHelper.FindNearestPortal` within `PortalLinkRadius` (15 m)
- Stores portal `ZDOID` on station ZDO
- Station groups: same station name within `StationGroupRadius` (10 m) share duplicate-name validation

---

## Harmony patches (alpha46 inventory)

### Critical (manual in Plugin.Awake)

| Patch | Target | Role |
|-------|--------|------|
| `ZNetSceneCreateObjectPatch` | `ZNetScene.CreateObject` | World-load instantiate + ZDO bind |

### Prefab / hammer

| Patch | Target | Role |
|-------|--------|------|
| `ZNetSceneAwakePatch` | `ZNetScene.Awake` | Build/cache/register prefab |
| `ZNetStartPrefabPatch` | `ZNet.Start` | Ensure prefab registered |
| `PieceTableUpdateAvailablePatch` | `PieceTable.UpdateAvailable` | Register + sanitize null pieces |
| `ObjectDbCopyOtherDbPatch` | `ObjectDB.CopyOtherDB` | Re-register after DB copy |
| `FejdStartupSetupGuiPatch` | `FejdStartup.SetupGui` | Localization |

### Placement

| Patch | Target | Role |
|-------|--------|------|
| `PlacedObjectActivatePatch` | `Piece.SetCreator` | Activate before creator; bind network + WNT after |

### WearNTear / destroy

| Patch | Target | Role |
|-------|--------|------|
| `StationWearNTearAwakePatch` | `WearNTear.Awake` | Skip on invalid ZDO |
| `StationWearNTearUpdatePatch` | `WearNTear.UpdateWear` | Skip for station |
| `StationWearNTearGetSupportPatch` | `WearNTear.GetSupport` | Guard broken WNT |
| `StationWearNTearRemovePatch` | `WearNTear.Remove` | Custom refund destroy |
| `StationWearNTearDestroyPatch` | `WearNTear.Destroy` | Custom refund destroy |
| `StationRemovePiecePatch` | `Player.RemovePiece` | Hammer remove for stations |

### Config input

| Patch | Target | Role |
|-------|--------|------|
| `StationConfigInputPatch` | Player, PlayerController, Minimap, GameCamera | Block gameplay while UI open |
| `StationConfigZInputPatch` | ZInput.* | Block hotkeys while UI open |

### Interaction

| Patch | Target | Role |
|-------|--------|------|
| `SignInteractBlockPatch` | `Sign.Interact` | Block vanilla sign edit on station signs |
| `DestinationActivatePatch` | `Player.Update` | LMB destination activate |
| `PortalPlacementPatch` | `WearNTear.OnPlaced` | Portal display sign (for TeleportWorld, not station) |

### Cleanup

| Patch | Target | Role |
|-------|--------|------|
| `StationOrphanCleanupOnLoadPatch` | world load | Delayed orphan ZDO purge |

---

## Network binder (`StationNetworkBinder`)

Called when placing (with `allowCreateZdo: true`):

1. Skip if not runtime instance
2. Ensure `ZNetView` exists
3. Create or repair ZDO
4. `ZNetScene.AddInstance` if missing
5. `Piece.SetCreator` if creator still 0

**Danger:** Calling with `allowCreateZdo: true` on ghost = crash / corrupt save.

---

## Dependencies

- **BepInEx 5.4.x**
- **HarmonyX** (0Harmony)
- **Jotunn** was in profile but station code is mostly manual Harmony
- Game DLLs: `assembly_valheim`, `assembly_utils` (for `ZInput`, `Utils`), `assembly_guiutils`
- Target: **.NET Framework 4.7.2**

---

## Log verification checklist

After each build:

```
[Info :Runic Portals] Harmony applied N patch(es).   ← no errors above this
[Info :Runic Portals] Runic Portals 0.2.0 (vX) loaded.
[Info :Runic Portals] ZNet.Start: registered 'runic_portal_station' ...
```

After place:

```
Placed runic_portal_station
```

Should **NOT** see:
- `Harmony PatchAll failed`
- Repeated `WearNTear.GetSupport` NRE
- `Removed orphan portal station ZDO` (except one-time cleanup of old save junk)
