# Portal Station — Knowledge Transfer (from RunicPortals)

This folder preserves everything learned while building the **Portal Station** (`runic_portal_station`) in the abandoned `RunicPortals` project (`D:\ValheimProjects\RunicPortals`). Use it as the starting base when rebuilding in a clean repo.

**Source project state:** `v0.2.0-alpha46` (unstable — placement/network/destroy never fully settled).

## Documents in this folder

| File | Purpose |
|------|---------|
| [STATION_BUILD_GUIDE.md](STATION_BUILD_GUIDE.md) | Step-by-step how the station prefab is assembled from vanilla pieces |
| [LESSONS_LEARNED.md](LESSONS_LEARNED.md) | Trial-and-error log — what broke, why, and what to avoid |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Runtime lifecycle, Harmony patches, networking, UI |
| [CONSTANTS_AND_LAYOUT.md](CONSTANTS_AND_LAYOUT.md) | Numbers, coordinates, craft cost, ZDO keys |
| [FILE_MAP.md](FILE_MAP.md) | Map of source files in the old repo |
| [REBUILD_CHECKLIST.md](REBUILD_CHECKLIST.md) | Ordered checklist for a clean restart |

## Assets

- **`art/Station_Icon.png`** — Your hammer menu icon (already in this folder).
- **`assets/Station_Icon.png`** — Backup copy from the last deployed RunicPortals build.

In the old mod the icon was embedded as `RunicPortals.Assets.Station_Icon.png` and copied beside the DLL as `Station_Icon.png`. See `assets/README.md`.

## What the station is (design intent)

One craftable hammer piece in **Misc** category:

- Wooden frame + board built from vanilla pole/beam/wall prefabs (visual only)
- 1 header sign (E → config UI)
- 18 destination signs in a 3×6 grid (LMB activates)
- Links to nearest `TeleportWorld` portal within 15 m
- Craft cost: **58 Wood + 19 Coal** (frame + board + 19 signs)
- Workbench required to place/remove (copied from `piece_workbench`)
- Visual scale: **75%** of layout coordinates

## Old build & deploy (reference)

```powershell
# From RunicPortals root
powershell -File build.ps1
# Deployed to r2modman profile Portals by default
```

Valheim path in `Directory.Build.props`: `D:\SteamLibrary\steamapps\common\Valheim`

## Do not copy blindly

The old repo accumulated **34+ Harmony patches** to paper over networking/WearNTear/ghost issues. On rebuild, implement the **lifecycle rules** in LESSONS_LEARNED.md first, then add patches only where proven necessary.
