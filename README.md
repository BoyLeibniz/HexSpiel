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

Maps are saved and loaded from your system's Unity Data folder - the location is presented in the Console 
immediately on Play of the scene, e.g.;
```
[HexGridManager] Default Data Path (Application.persistentDataPath): %APPDATA%/DefaultCompany/HexSpiel
```
Maps can be loaded via the UI by entering a Map Name. Spaces are converted to underscores when saving, and restored when loading.
If no name is provided in the UI a map `default_map.json` will be saved or loaded (if present).
A sample map in the original map format has been provided (Samples/hexmap_v1.json).
Simply copy it to your system's Unity Data folder if needed.

### Current Priority: 

Tooltip presentation - show the new `HexCell.label` property as a floating tooltip in the scene.

Required behaviour; 
- Tooltips should be presented only over tiles that have a value
- They should of readable size without being so large as to obscure neighbouring tooltips
- The tooltips should always present towards the camera billboard-style, even on camera movement
- The tooltips should be toggled on and off via a keyboard shortcut (e.g. ctrl-T)
- Tooltips should be presented and destroyed appropriately as maps are loaded, edited and destroyed (reset)
- The code should be extensible for additional future properties


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
| #001 | Clicking a selected hex does not always deselect it                        | 🛠 In Progress | Simply add `AddToSelection()` logic to `HexTileVisuals.OnMouseEnter()` |
| #002 | Glow applied on Hex selection is not scaling, lighter colors too high      | ⏳ Open     | Visual quirk only; does not affect save/load behavior           |
| #003 | Changing UI Hex Type after selection updates the color, but new selections revert | ✅ Confirmed | Modify to always respect the current UI Type selection |

> **Status legend**: ✅ Confirmed • 🧠 Investigating • ⏳ Open • 🛠 In Progress • 🔁 Regression • 🎯 Resolved • ❓ Needs Review • 🚫 Won’t Fix

---

© 2025 BoyLeibniz. This project is currently in prototype phase.
