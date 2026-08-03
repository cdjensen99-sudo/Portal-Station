# File map — RunicPortals source (`src/RunicPortals/`)

Use this when copying concepts into a new `PortalStation` mod.

## Entry

| File | Role |
|------|------|
| `Plugin.cs` | BepInEx entry, Harmony PatchAll, manual CreateObject patch, StationConfigUI host |
| `ModConstants.cs` | Prefab name, scales, ZDO keys, RPC names, build label |
| `ModConfig.cs` | BepInEx config (portal link radius, logging) |

## Prefab construction

| File | Role |
|------|------|
| `Prefabs/StationPrefabBuilder.cs` | **Main build orchestrator** |
| `Prefabs/StationFrameBuilder.cs` | Pole/beam/wall layout |
| `Prefabs/StationSignGridBuilder.cs` | Header + 18 destination signs |
| `Prefabs/StationVisualHelper.cs` | Clone/strip/place visual children |
| `Prefabs/StationPiecePlacement.cs` | Anchor math for child pieces |
| `Prefabs/StationSignOrientation.cs` | Board-facing rotation for signs |
| `Prefabs/NetworkPrefabHelper.cs` | `forceDisableInit`, inactive clone, ZNetScene register |
| `Prefabs/StationPrefabSanitizer.cs` | Strip child networking from template |
| `Prefabs/StationWearNTearHelper.cs` | Runtime WNT add/strip |
| `Prefabs/StationInteractionColliders.cs` | Enable/disable child colliders |
| `Prefabs/StationHammerIcon.cs` | Custom PNG → Piece.m_icon |
| `Prefabs/StationResourceRefund.cs` | 58 wood / 19 coal drop on destroy |
| `Prefabs/PieceRequirementHelper.cs` | Safe craft requirement builder |
| `Prefabs/PrefabBuildHelper.cs` | Set piece layer |
| `Prefabs/PortalDisplaySignSpawner.cs` | Strip sign components; portal display sign helper |

## Components (MonoBehaviour)

| File | Role |
|------|------|
| `Components/RunicStation.cs` | Core logic: ZDO, RPCs, config, activation |
| `Components/StationHeaderSign.cs` | E → open config UI |
| `Components/StationDestinationSign.cs` | Per-slot sign + LMB activate |
| `Components/StationSignVisual.cs` | Text mesh display on signs |
| `Components/PortalNameSign.cs` | Portal-attached name sign (separate feature) |
| `Components/PortalSignInitRunner.cs` | Deferred portal sign init |

## UI

| File | Role |
|------|------|
| `UI/StationConfigUI.cs` | IMGUI config modal (station name + 18 slots) |

## Patches

| File | Role |
|------|------|
| `Patches/PrefabRegistrationPatch.cs` | ZNetScene prefab cache |
| `Patches/PieceTablePatch.cs` | Hammer menu registration |
| `Patches/PlacedObjectActivatePatch.cs` | SetActive + post-place setup |
| `Patches/ZNetSceneCreateObjectPatch.cs` | World load instantiate |
| `Patches/StationWearNTearPatch.cs` | Destroy + hammer remove |
| `Patches/StationWearNTearAwakePatch.cs` | WNT awake guard |
| `Patches/StationWearNTearHoverPatch.cs` | WNT update/highlight guards |
| `Patches/StationConfigInputPatch.cs` | Block player input in UI |
| `Patches/StationConfigZInputPatch.cs` | Block ZInput in UI |
| `Patches/PortalInteractionPatches.cs` | Sign block, destination LMB |
| `Patches/LocalizationPatch.cs` | English strings |
| `Patches/StationOrphanCleanupOnLoadPatch.cs` | Orphan ZDO cleanup |
| `Patches/KnownPieceUnlocker.cs` | (if used) piece unlock helper |

## Utilities

| File | Role |
|------|------|
| `Utilities/StationNetworkBinder.cs` | ZDO create/bind on place (**fragile**) |
| `Utilities/StationInstanceHelper.cs` | Ghost/template detection |
| `Utilities/StationOrphanZdoHelper.cs` | Orphan station ZDO detection/removal |
| `Utilities/StationGroupHelper.cs` | Linked station groups + portal find |
| `Utilities/PortalTagHelper.cs` | Read/write TeleportWorld tag |
| `Utilities/PortalTextHelper.cs` | Portal name clamp, display text |

## Project / build

| File | Role |
|------|------|
| `RunicPortals.csproj` | net472, Valheim refs, embedded Station_Icon.png |
| `Directory.Build.props` | ValheimPath, BepInExCore, ValheimManaged |
| `build.ps1` | dotnet build, deploy to r2modman profile, zip package |
| `art/Station_Icon.png` | Hammer icon source (embed + copy to plugin folder) |

## What to copy vs rewrite

**Copy concepts / port carefully:**
- `StationFrameBuilder` coordinates
- `StationSignGridBuilder` layout logic
- `NetworkPrefabHelper.RunWithoutZdoCreation`
- `StationPrefabBuilder` build order
- `StationHammerIcon` + PNG asset
- `ModLocalization` strings
- Craft cost numbers

**Rewrite from scratch with minimal surface:**
- All WearNTear patches
- `StationNetworkBinder` (or replace with proven Jotunn/vanilla pattern)
- `Player.RemovePiece` override
- `ZNetSceneCreateObject` custom path (only if inactive prefab approach kept)

**Do not copy:**
- Accumulated patch stack without pruning
- alpha35–alpha46 experimental fixes layered on each other
