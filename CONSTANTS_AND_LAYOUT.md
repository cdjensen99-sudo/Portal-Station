# Constants, layout, and tuned values

## Identity

| Constant | Value |
|----------|-------|
| Prefab name | `runic_portal_station` |
| Mod GUID (old) | `com.runicportals` |
| Piece localization name | `$piece_runic_station` |
| Hammer category | `Piece.PieceCategory.Misc` |

## Visual scale

| Constant | Value |
|----------|-------|
| `StationVisualScale` | **0.75** (applied to root after building at 1:1 coordinates) |

## Sign layout

| Constant | Value |
|----------|-------|
| Destination slots | **18** |
| Grid | **3 columns × 6 rows** |
| Sign height | **0.5 m** per row |
| Board bottom Y | **1.0** |
| Column X centers | **-1, 0, 1** |
| Header Y | **4.3** |
| `StationHeaderSignForwardOffset` | **0.1** |
| `StationDestinationSignForwardOffset` | **0.37** |
| `StationSignFaceEpsilon` | **0.02** |

## Craft cost (verified in-game)

Represents frame + board + 19 signs:

| Resource | Amount |
|----------|--------|
| Wood | **58** |
| Coal | **19** |

Destroy refund uses same amounts via `StationResourceRefund` (not vanilla material drops).

## WearNTear (when added at runtime)

| Field | Value |
|-------|-------|
| `m_health` | 500 |
| `m_materialType` | Wood |
| `m_supports` | false |
| `m_noRoofWear` | true |
| `m_noSupportWear` | true |

## Placement collider (root)

```
center: (0, 2.1, 0)
size:   (3.2, 4.2, 0.5)
```

## Portal / group radii

| Constant | Value |
|----------|-------|
| `PortalLinkRadius` | 15 m |
| `StationGroupRadius` | 10 m |

## Text limits

| Field | Max length |
|-------|------------|
| Portal name (pairing tag) | 10 |
| Display text | 50 |

## Frame coordinate reference (1:1 before scale)

See STATION_BUILD_GUIDE.md for full pole/beam/wall positions.

Summary bounding box (approx):
- Width: 3 m (x: -1.5 to +1.5)
- Height: 4 m (y: 0 to 4)
- Depth: vanilla wall thickness (~0.26 m)

## Vanilla prefab names used

| Role | Primary name | Fallbacks |
|------|--------------|-----------|
| Root clone | `woodwall` | `piece_wood_wall` |
| Legs | `wood_pole` | — |
| 2m side posts | `wood_pole2` | — |
| 2m beams | `wood_beam` | — |
| 1m beams | `wood_beam_1` | — |
| 2×2 wall | `woodwall` | `piece_wood_wall` |
| Half wall | `wood_wall_half` | — |
| Quarter wall | `wood_wall_quarter` | — |
| Signs | `sign` | `piece_sign` |
| Workbench ref | `piece_workbench` | — |
| Portal icon ref | `portal` | `portal_wood`, `piece_portal` |

## ZDO / RPC string constants

See `ModConstants.cs` in old repo — all prefixed `RunicPortals_` or `runic_portals.`.

## Build label convention (old project)

`ModConstants.BuildLabel = "v0.2.0-alphaNN"` — bump on every deploy; prefab cache clears when label changes.
