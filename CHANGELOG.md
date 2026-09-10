# Changelog

## 1.0.0

- **Valheim 1.0 / Unity 6:** Recompiled against Valheim 1.0.7 managed assemblies and BepInExPack 5.4.2350.
- **Fixed:** Implemented `Hoverable.GetHoverOffset()` on station and portal signs (stops TypeLoadException spam).
- **Fixed:** Wood piece prefab names for 1.0 (`woodwall`, `woodwallhalf`, `woodwallquarter`, `wood_pole_2`).
- **Fixed:** Inactive destination signs use `DefaultPortalDescription`; the **active** destination (matching the linked portal tag) uses `DefaultHighlightColor`.
- **Changed:** Depends on Jotunn **2.30.0** (1.0-ready).
- **Changed:** Removed redundant prefab re-registration warning from Jotunn.
- **Version:** First 1.0.0 release — feature-complete and Valheim 1.0 compliant.

## 0.3.2

- Added BepInEx config (`DisplayNameMaxLength`, craft costs, default/highlight colors).
- Player-facing README with screenshots; Thunderstore packaging via `build.ps1`.

## 0.3.1

- Portal sign hover localization; hammer icon apply; mod artwork.

## 0.3.0

- Portal linking, station config UI, destination activation, naming cleanup from Runic Portals rebuild.
