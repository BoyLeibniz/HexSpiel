# HexSpiel

Welcome to HexSpiel! A Unity-based 3D hex-and-counter strategy game inspired by classic tabletop systems.

## 🛉 Project Overview

HexSpiel is a modular turn-based strategy engine with three main sub-projects:

* **Map Builder**: Create custom hex-grid game boards
* **Army Builder**: Define unit patterns and create army lists
* **Game Player**: Play scenarios with 3D counters and tile-based mechanics

## 🎮 Unity Version

Developed with **Unity 2022.3.49f1 (LTS)**

## 📁 Project Structure

```
Assets/
  Scripts/           # Game logic (HexGridManager, HexCell, etc.)
  Prefabs/           # Hex tile prefabs
  Scenes/            # Map builder, game player, etc.
  Materials/         # Tile materials, overlays
  SaveSystem/        # Save/load scaffolding (IDataPersistence, MapData)
ProjectSettings/     # Unity config files
Samples/             # Example Data files
```

## 🛠️ Getting Started

If you have the zip archive already:

1. Just unzip to your local folder of choice

Otherwise:

1. Clone the repo:

   ```bash
   git clone https://github.com/BoyLeibniz/HexSpiel.git
   ```

Then:

2. Open in **Unity Hub** → Add project → Select folder

3. (Optional) Open in **VS Code** if you’re editing scripts:

   ```bash
   code .
   ```

## 📦 Required Unity Packages

The project uses the following packages that are usually present with most installations, but
if starting from scratch ensure the following are installed via Unity Package Manager:

- `UI Toolkit` (`com.unity.ui`)
- `TextMeshPro` (`com.unity.textmeshpro`)


## 🧪 Testing

### Loading and Saving

To get started simply Play the HexMap scene.
Maps are saved and loaded from your system's Unity Data folder - the location is presented in the Console 
immediately on Play of the scene, e.g.;
```
[HexGridManager] Default Data Path (Application.persistentDataPath): %APPDATA%/DefaultCompany/HexSpiel
```
Maps can be loaded via the UI by entering a Map Name. Spaces are converted to underscores when saving, and restored when loading.
If no name is provided in the UI a map `default_map.json` will be saved or loaded (if present).
A default map (Samples/default_map.json) and a sample map in the original map format has been provided (Samples/hexmap_v1.json).
Simply copy it to your system's Unity Data folder if needed.

### Current Priority: 

[Issue #005](#🐞-outstanding-issues)
#### Hex Movement Cost updates not applied

Current status:

A recent update disabled the persistence of Hex movement cost edits, only saving default values on selection of Hex type.

Steps to reproduce:

In the HexMap Editor select a hex or group of hexes, modify the default movement cost from it's default value and select the `Apply` button. 
If the problem persists, on reselecting one of the updated hexes you will be presented with the default movement cost rather than the edited value.

Required behaviour:

- Edits to hex movement cost values should be allowed and persisted whether single or multiple hexes are selected.
- If multiple hexes are selected that have the same movement cost, that movement cost should be displayed, otherwise if values differ the placeholder "-- Mixed --" should be displayed.
- In either case for multiple selections, the displayed value should remain editable while hexes are selected and edits should persist on being applied via the `Apply` button.
- The movement cost field should be validated to only accept positive integers or the "-- Mixed --" placeholder

To validate movement cost editing and UI behavior in the Hex Editor:

- ✅ No-selection disables fields 
- ✅ Selection shows correct field state and values
- ✅ "-- Mixed --" appears when appropriate and allows overwrite
- ✅ Only valid positive integers are accepted
- ✅ Apply does nothing unless values differ
- ✅ Tooltip and red highlight appear for invalid input

### Minimal UI Behavior Validation Checklist

1. No Selection
- Open the Hex Editor with no hexes selected.
- 🔒 Verify the movement cost, label, type, and transparency fields are disabled.

2. Single Selection
- Select one hex tile.
- 📝 Confirm movement cost field is enabled and shows the correct value.
- 🟩 Enter a valid positive integer and press Apply.
- ✅ Confirm value is applied to the selected hex.

3. Multiple Selection (Same Values)
- Select multiple hexes that all share the same movement cost.
- 📝 Confirm field shows that value and is editable.
- 🔘 Change the value, press Apply.
- ✅ Confirm all selected hexes were updated.

4. Multiple Selection (Mixed Values)
- Select multiple hexes with different movement costs.
- 📛 Confirm field shows "-- Mixed --" but remains editable.
- 🟩 Enter a valid integer and Apply.
- ✅ Confirm all selected hexes now share the new cost.

5. Validation and Tooltip
- Type an invalid value (e.g. "abc" or "-1") into the cost field.
- 🚫 Confirm background turns red and tooltip shows validation message on hover.
- 🛑 Press Apply — no changes should be applied to any field.
- ✔ Restore a valid value — red highlight clears, tooltip reverts.

These steps validate all movement cost field logic and tooltip feedback.

## ⌨️ Keyboard Shortcuts

While running the project, the following shortcuts are available:

* **Ctrl+A** (or **Cmd+A** on macOS): Select all hex tiles
* **Esc**: Cancel current selection
* **R**: Reset the camera position

These are intended to assist during editing and testing phases.

## 🗽 Roadmap (Early Stage)

* [x] Basic grid generation
* [x] Mat placement and Camera Control
* [x] Hex selection and property editing
* [x] Map Save and Load
* [ ] Tile overlays
* [ ] Unit Pattern maintenance
* [ ] Army List maintenance
* [ ] Game Scenario maintenance
* [ ] Game Setup and presentation
* [ ] Unit placement
* [ ] Turn system
* [ ] AI player logic

## 🤝 Contributing

* Use XML doc comments for all public scripts
* Avoid committing `.vscode/`, `Library/`, and build artifacts
* Keep commits descriptive and atomic

## 🐞 Outstanding Issues

| ID   | Description                                                                 | Status     | Workaround / Notes                                               |
|------|-----------------------------------------------------------------------------|------------|------------------------------------------------------------------|
| #001 | Clicking a selected hex does not always deselect it                         | 🎯 Resolved | Simply add `AddToSelection()` logic to `HexTileVisuals.OnMouseEnter()` |
| #002 | Glow applied on Hex selection is not scaling, lighter colors too high       | ⏳ Open     | Visual quirk only; does not affect save/load behavior           |
| #003 | Changing UI Hex Type after selection updates the color, but new selections revert | 🎯 Resolved | Modify to always respect the current UI Type selection |
| #004 | Process other Hex property updates for "-- Mixed --" selections             | 🔁 Regression | Should also update UI behaviour to visually reflect changes pending |
| #005 | Movement Cost edits are not persisting                                      | 🛠 In Progress | Should behave consistent with other properties for multiple hex selection |

> **Status legend**: ✅ Confirmed • 🧠 Investigating • ⏳ Open • 🛠 In Progress • 🔁 Regression • 🎯 Resolved • ❓ Needs Review • 🚫 Won’t Fix

---

© 2025 BoyLeibniz. This project is currently in prototype phase.
