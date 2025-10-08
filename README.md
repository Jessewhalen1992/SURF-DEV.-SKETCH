# ResidenceSync

ResidenceSync is an AutoCAD .NET plugin that synchronizes residence locations between a scaled planning sketch and a master DWG. The commands use a two-point similarity transform derived from section corner picks to translate, rotate, and scale residence points between the two drawings.

## Features
- **PULLRES** – Reads residences from the master drawing for a given ATS section, maps them into the active scaled sketch, and inserts DBPoints on layer `Z-RESIDENCE`.
- **PUSHRES** – Pushes selected residence blocks from the scaled sketch back to true section space and appends them as DBPoints in the shared master drawing.

## Requirements
- AutoCAD 2025 with .NET API assemblies (`acdbmgd.dll`, `acmgd.dll`).
- Microsoft .NET Framework 4.8.
- Visual Studio 2022.

## Build & Installation
1. Open `ResidenceSync.sln` in Visual Studio 2022.
2. Set the solution configuration to **Release | Any CPU**.
3. Build the solution. A post-build step copies `ResidenceSync.dll` to `%USERPROFILE%\Desktop\ResidenceSync\bin`.
4. In AutoCAD, run `NETLOAD` and browse to `%USERPROFILE%\Desktop\ResidenceSync\bin\ResidenceSync.dll` to load the plugin.

## Usage
1. **PULLRES**
   - Enter the section key values (SEC, TWP, RGE, MER).
   - Pick the Top-Left and Top-Right points on the scaled sketch.
   - Select the polyline that represents the actual ATS section boundary. The plugin identifies true top corners from the polyline vertices (falling back to extents if needed).
   - Residences stored in the master drawing within the section AABB are transformed into the sketch and inserted as DBPoints on layer `Z-RESIDENCE`.

2. **PUSHRES**
   - Enter the section key values (SEC, TWP, RGE, MER).
   - Pick the Top-Left and Top-Right points on the scaled sketch.
   - Select the actual section polyline to determine the true corners.
   - Select residence block references within the sketch. Their insertion points are mapped back to true section space and added to the master drawing as DBPoints on layer `Z-RESIDENCE`.
   - The master drawing is stored at `C:\_CG_SHARED\Master_Residences.dwg` (update the constant in code if your environment differs).

## Similarity Transform Details
- The transform uses two reference corner pairs (true section TL/TR and sketch TL/TR) to compute scale, rotation, and translation.
- Matrices follow `M_master_to_sketch = T2 * S * R * T1`, with the inverse used for pushing residences back to master space.
- Section height is assumed equal to the width to build an axis-aligned bounding box (AABB) for filtering residences during pull operations.

## Testing
- Build succeeds in Release mode and copies the DLL to the desktop bin folder.
- In AutoCAD, `PULLRES` and `PUSHRES` execute without exceptions when provided valid inputs.
- `PULLRES` inserts visible DBPoints on layer `Z-RESIDENCE` in the sketch.
- `PUSHRES` creates/updates `C:\_CG_SHARED\Master_Residences.dwg` and increases the total residence point count.

## Future Enhancements
- Replace DBPoints with a dedicated RESIDENCE block containing attributes (ID/NAME/DATE).
- Auto-detect ATS sections via Object Data from Map 3D (`AB_SEC_N83UTMz11`) instead of manual selection.
- Use a polygon point-in-polygon test instead of the square AABB filter.
- Add a settings UI to configure the master drawing path and persist it in the Named Objects Dictionary.
- Implement de-duplication during `PUSHRES` within a 2 m tolerance.
