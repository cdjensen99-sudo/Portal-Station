# Rebuild checklist — Portal Station (clean start)

Use with STATION_BUILD_GUIDE.md and LESSONS_LEARNED.md.

---

## Phase 0 — Project setup

- [ ] New BepInEx mod project (net472, Valheim DLL refs)
- [ ] Copy `Station_Icon.png` to `art/` and embed or ship beside DLL
- [ ] `build.ps1` with deploy profile
- [ ] `ModConstants` with prefab name `runic_portal_station`
- [ ] Log line on load with build label
- [ ] Log `Harmony applied N patches` — verify zero errors

## Phase 1 — Visual prefab only (no custom networking patches)

- [ ] `NetworkPrefabHelper` with `forceDisableInit`
- [ ] `StationPrefabBuilder` through frame + board + signs
- [ ] Template **inactive** + DontDestroyOnLoad
- [ ] Register in ZNetScene (named prefab hash)
- [ ] Hammer menu entry (Misc) with icon + localization
- [ ] **Test:** Select in hammer → ghost appears, no crash, no log errors
- [ ] **Test:** Place → looks correct at 75% scale
- [ ] **Test:** Save/reload → no extra stations spawn at login

## Phase 2 — Placement lifecycle

- [ ] `SetActive(true)` before `Piece.SetCreator` for inactive prefab
- [ ] Confirm ZDO created with creator ID in save
- [ ] `StationInteractionColliders.Enable` after place
- [ ] **Do not** run `RunicStation.Start` logic on ghost (ghost layer check)
- [ ] **Test:** New place → inspect ZDO in devcommands / log

## Phase 3 — Config persistence

- [ ] `RunicStation` RPC register only when `nview.IsValid()`
- [ ] Config UI open (E on header)
- [ ] Commit writes ZDO fields without requiring nearby portal
- [ ] Re-open UI → fields persisted
- [ ] Block ZInput while UI open (use correct Harmony param names)
- [ ] **Test:** Type station name + slots, commit, reopen

## Phase 4 — Portal link & activation

- [ ] Find portal within 15 m on commit (optional link message)
- [ ] LMB on destination sign sets portal tag
- [ ] **Test:** Teleport using linked portal after activation

## Phase 5 — Destroy & refund

- [ ] Hammer remove near workbench
- [ ] Refund 58 wood + 19 coal (`ItemDrop.OnCreateNew`)
- [ ] **Test:** Place → destroy → correct drops

## Phase 6 — WearNTear (only if needed)

- [ ] Strip from template at build
- [ ] Add only on placed instance after valid ZDO
- [ ] Awake guard if `GetZDO() == null`
- [ ] Skip `UpdateWear` for station
- [ ] **Test:** No NRE spam after place; hammer remove still works

## Phase 7 — World load

- [ ] Custom `CreateObject` only if inactive prefab breaks save/load
- [ ] Bind `m_initZDO` before `SetActive`
- [ ] Orphan ZDO cleanup for old saves (creator == 0, no instance)

---

## Per-test hygiene

1. Fully restart Valheim after DLL change
2. Use **newly placed** station (old ones keep broken state)
3. Check `BepInEx\LogOutput.log` first
4. One change per build when debugging

---

## Success criteria (MVP)

- [ ] Select station in hammer — no crash
- [ ] Place station — correct visuals
- [ ] Login/logout — no duplicate spawns
- [ ] Configure + commit — saves names
- [ ] Hammer remove — destroys + full refund
- [ ] No repeating errors in log during 5 minutes of play

---

## If stuck

Stop adding patches. Return to Phase 1 visual-only baseline and bisect which phase introduces the regression.

Consider referencing:
- Jotunn PieceManager docs
- A minimal custom piece mod with inactive prefab pattern
- Valheim `ZNetView.Awake` decompile (ilspycmd on `assembly_valheim.dll`)
