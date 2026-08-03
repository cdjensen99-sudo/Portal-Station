# Assets

## Station_Icon.png

Hammer build menu icon for the Portal Station piece.

**You saved your icon here.** When starting the new mod:

1. Place the PNG in the new project's `art/Station_Icon.png`
2. Embed in the DLL (see old `RunicPortals.csproj` `EmbeddedResource` entry)
3. Copy to plugin folder on deploy as `Station_Icon.png` (fallback if embed fails)
4. Load in `StationHammerIcon.Apply(Piece)` — match `pixelsPerUnit` to vanilla portal icon

**Recommended size:** Match vanilla piece icons (typically 64×64 or 128×128; old code used portal piece's `pixelsPerUnit`).

## Optional 3D art (old repo)

The abandoned `RunicPortals/art/` folder had OBJ meshes (`RunicStand_*.obj`, `RuneStone_*.obj`) — **not used** in the final station build. The shipped station uses **vanilla pole/beam/wall/sign prefabs** assembled in code, not custom meshes.
