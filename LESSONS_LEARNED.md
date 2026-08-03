# Lessons learned — Portal Station (trial & error log)

Chronological themes from the RunicPortals build session. **Read this before writing new patches.**

---

## Golden rules for rebuild

1. **One lifecycle diagram first** — template vs ghost vs placed vs world-loaded. No patch until each phase is defined.
2. **Never run network/ZDO logic on the DDoL template or placement ghost** — only on real placed/world instances.
3. **Every `Instantiate` of a networked prefab during build** must use `ZNetView.m_forceDisableInit = true`.
4. **Prefab template stays inactive** in DontDestroyOnLoad.
5. **Test one axis at a time:** place → save/load → configure → destroy → re-login.
6. **Read `BepInEx\LogOutput.log` after every change** — look for `Harmony PatchAll failed` (means most patches silently skipped).

---

## Localization infinite loop (early)

**Symptom:** Game hung on mod load; log spammed "Loaded localization file".

**Cause:** `ModLocalization.Register()` called from both `Plugin.Awake` and `Localization.SetupLanguage` postfix; `AddWord` re-triggered setup.

**Fix:** One-time `registered` guard; register only from `FejdStartup.SetupGui` postfix.

---

## Harmony patch signature mismatches

**Symptom:** Mod "loaded" but features broken; log shows `Harmony PatchAll failed`.

**Examples:**
- `Player.PlacePiece` postfix with `bool __result` — method is `void`, not bool.
- `ZInput.GetKeyDown(KeyCode, bool)` — Harmony parameter must be named `logWarning`, not `gamepad`.

**Lesson:** When PatchAll fails once, **almost nothing is patched**. Always verify `Harmony applied N patch(es)` and zero errors.

---

## PieceTable null reference on world enter

**Symptom:** NRE in `PieceTable.UpdateAvailable` when equipping hammer.

**Cause:** `ZNetScene.Awake` runs again on world load; hammer `m_pieces` held destroyed/null prefab refs from menu load.

**Fix:**
- Static prefab cache (`BuiltPrefabs`) — build once, re-register on each scene wake.
- `SanitizeInvalidPieces` — remove null entries from `m_pieces`.

---

## Craft requirements resolved too early

**Symptom:** NRE in `UpdateKnownRecipesList` on item pickup.

**Cause:** `ApplyCraftCost` during prefab `Build()` at `ZNetScene.Awake` — `m_resItem` pointed at destroyed/null item prefabs.

**Fix:** Build with empty `m_resources`; apply costs only in `ApplyCraftingSetup` when `ObjectDB` is ready. `PieceRequirementHelper` null-guards missing items.

---

## Orphan ZDOs / stations spawning every login

**Symptom:** Multiple `runic_portal_station` appear on login; no creator; can't refund.

**Cause:** Instantiating Valheim prefabs **while active** during mod build creates real ZDOs without player creator.

**Fix:**
- `NetworkPrefabHelper.RunWithoutZdoCreation` around **all** build-time Instantiates.
- Inactive template in DontDestroyOnLoad.
- `StationOrphanZdoHelper` — purge ZDOs with station prefab hash, creator 0, no scene instance (world load + delayed cleanup).

**World note:** Bjornheim save accumulated ~8+ orphan station ZDOs from broken builds. Backup: `RunicPortals_backup_auto-20260626130731`.

---

## WearNTear / placement ghost saga

**Core conflict:** Valheim placement ghost uses `ZNetView.m_forceDisableInit = true` → ZNetView destroys itself on ghost. `WearNTear.Awake` expects `m_nview.GetZDO()` → NRE on hover.

### Attempts and outcomes

| Approach | Result |
|----------|--------|
| WearNTear on prefab, active template | Orphan ZDOs, ghost NREs |
| Strip WearNTear from prefab; AddComponent on place | Indestructible (Remove() no-op if RPCs not registered) |
| Keep WearNTear on prefab root (alpha42) | Endless `GetSupport()` NRE — broken instances in `WearNTearUpdater` |
| Remove WearNTear entirely (alpha43) | Indestructible again (hammer expects WNT or custom remove path) |
| Re-add WNT after ZDO + skip UpdateWear (alpha44) | Config "not ready" — ZNetView never valid on place |
| StationNetworkBinder without ghost guard (alpha45) | **Crash when selecting station in hammer menu** — binder ran on ghost `RunicStation.Start`, called `CreateNewZDO` |

### What actually works in theory (alpha46 direction)

- **No WearNTear on template** (sanitizer strips all at build).
- **Add WearNTear only after valid ZDO** on placed/world instance.
- **Skip `WearNTear.UpdateWear`** for station (decorative; no rain/support sim).
- **`WearNTear.Awake` prefix:** skip if `GetZDO() == null` (ghost/template).
- **`StationInstanceHelper.IsRuntimeInstance`:** false for `DontDestroyOnLoad` scene and `ghost` layer.
- **Never call `CreateNewZDO` except** from `SetCreator` postfix / world-load with `allowCreateZdo: true`.

**Never fully verified stable end-to-end.**

---

## Config UI "Station is not ready yet"

**Message from:** `RunicStation.TryApplyConfiguration` when `_nview == null || !_nview.IsValid()`.

**Cause:** ZDO never registered on placed piece, or RPCs never registered because `TryRegisterRpcs` ran before network ready.

**Partial fix attempted:** `StationNetworkBinder` — create/bind ZDO after place. Broke when same code ran on ghost.

---

## Config UI hotkey bleed

**Symptom:** M opens map while typing in config; movement keys move player.

**Cause:** Map uses `ZInput.GetButtonDown("Map")`, not blocked by `Player.TakeInput` patches alone.

**Fix:** `StationConfigZInputPatch` — prefix all `ZInput` get methods while `StationConfigUI.IsOpen`. Parameter name on `GetKeyDown(KeyCode, bool)` must be `logWarning`.

Also block `Minimap.Update`, `GameCamera` mouse capture while UI open.

---

## Config commit appeared to save nothing (alpha40 area)

**Cause:** `ServerApplyConfiguration` returned early without writing ZDO when no portal within 15 m; UI closed anyway.

**Fix:** Always write station name + slot keys to ZDO; portal link optional with user message.

---

## Destroy / refund

**Vanilla:** `WearNTear.Remove()` → RPC → `Destroy` → `Piece.DropResources()` (material-based, wrong for custom assembly).

**Needed:** Custom refund **58 wood + 19 coal** via `StationResourceRefund.DropCraftCost` + `ItemDrop.OnCreateNew`.

**Patches:** `WearNTear.Remove` and `WearNTear.Destroy` prefixes; `Player.RemovePiece` prefix for station (hammer path).

**Workbench:** Remove requires workbench in range if `m_craftingStation` set (copied from workbench piece).

---

## Alpha45 crash (select station in hammer menu)

**Cause:** `RunicStation.Start` → `StationNetworkBinder.EnsurePlacedStation` on **placement ghost** → `CreateNewZDO` on preview object.

**Fix (alpha46):** `StationInstanceHelper` — skip binder on ghost layer and DontDestroyOnLoad; `allowCreateZdo` only from placement postfix.

---

## Process failures (meta)

- Fixing one symptom without validating full lifecycle caused regressions every build (alpha35–alpha46).
- "Mod loaded" log line is **not** proof patches applied.
- Old placed stations retain broken component state — always test with **newly placed** station after prefab/DLL change.
- Patching faster than logging/debugging created a patch stack that is hard to reason about — **rebuild with minimal patches**.

---

## Recommended rebuild strategy

1. Visual prefab only first — no WearNTear, no custom networking patches. Confirm hammer ghost + placement visual.
2. Add inactive template + `forceDisableInit` build pipeline. Confirm zero orphan ZDOs after login.
3. Add placement activation (`SetActive` before `SetCreator`). Confirm ZDO + creator in save.
4. Add config UI + ZDO persistence. Test commit/reopen.
5. Add destroy + refund. Test hammer remove.
6. Add WearNTear **only if required** for vanilla tooltips/highlight — with ghost guards from day one.

Consider using **Jotunn PieceManager** or a known-good custom piece mod as reference instead of growing bespoke Harmony surface.
