# HexSpiel

Welcome to HexSpiel! A Unity-based 3D hex-and-counter wargame inspired by classic tabletop systems.

## 🛉 Project Overview

HexSpiel is a modular turn-based strategy engine with three main sub-projects:

* **Map Builder**: Create custom hex-grid game boards
* **Army Builder**: Define unit patterns and create army lists
* **Game Player**: Play scenarios with 3D counters and tile-based mechanics

## 🎮 Unity Version

Tested with **Unity 2022.3.49f1 (LTS)**

## 📁 Project Structure

```
Assets/
  Scripts/           # Game logic (HexGridManager, HexCell, etc.)
  Prefabs/           # Hex tile prefabs
  Scenes/            # Map builder, game player, etc.
  Materials/         # Tile materials, overlays
ProjectSettings/     # Unity config files
```

## 🛠️ Getting Started

1. Clone the repo:

   ```bash
   git clone https://github.com/BoyLeibniz/HexSpiel.git
   ```

2. Open in **Unity Hub** → Add project → Select folder

3. (Optional) Open in **VS Code** if you’re editing scripts:

   ```bash
   code .
   ```

## 🤝 Contributing

* Use XML doc comments for all public scripts
* Avoid committing `.vscode/`, `Library/`, and build artifacts
* Keep commits descriptive and atomic

## 🗽 Roadmap (Early Stage)

* [x] Basic grid generation
* [x] Mat placement
* [ ] Tile overlays
* [ ] Unit placement
* [ ] Turn system
* [ ] AI player logic

---

© 2025 BoyLeibniz. This project is currently in prototype phase.
