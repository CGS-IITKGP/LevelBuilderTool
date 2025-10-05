# 🧱 3D Level Creation Tool for Unity

A powerful Unity Editor extension designed to simplify and speed up **level creation** by replacing manual object placement with an intuitive, grid-based workflow.  
This tool enables artists and designers to create structured, randomized, and layered levels with ease — while supporting prefab grouping, procedural touches, and flexible editing tools.

---

## 🚀 Key Features

### 🧩 Grid System
- **Customizable Grid Size** — e.g., `Cell Width = 2.5m`
- Adjustable **Y-axis grid movement** to add height variation
- Supports **multiple grid layers** for complex 3D layouts

---

### 🧰 Prefab Management Panel
- Central panel for storing and organizing all **level prefabs** (e.g., floor tiles, props)
- **Grouping System** — combine multiple prefabs into a *Group* and randomize placement  
- Create new prefab groups directly from selected prefabs
- Supports **offsets** and **random offsets** for varied positioning
- Define **number of prefabs per cell** (e.g., grass clusters, prop scatter)

---

### 🎨 Brush Tools
Flexible placement tools for different workflows:
- **Single Placement** → Place one prefab at a time  
- **Fill** → Fill an entire grid area with a chosen prefab  
- **Brush** → Paint prefabs freely across the grid  
- **Line** → Draw prefab lines for fences, walls, etc.  
- **Erase** → Toggle removal of prefabs from grid cells  

---

### 🧭 Selection & Editing
- Select multiple grid cells or prefabs by **layer** or **box selection**
- Move, rotate, and scale selected prefabs while keeping grid alignment
- Support for **vertical selection** (↑ / ↓ to select in Y direction)
- **Copy / Paste** selected areas to reuse sections quickly

---

### 🗂️ Layer System
- Organize prefabs by **layers**
- Toggle visibility per layer for easy management
- Each layer acts as a separate editing context  

---

### 🔁 Undo & Redo
Full support for Unity’s Undo/Redo system for safe experimentation.

---

### 💾 Saving & Loading
- Save and load **grid level data**
- Export data for external tools or procedural systems

---

### ✨ Future Plans (If Time Allows)
- Add **procedural generation** for specific tile types inspired by *Tiny Glade* and *TileWorldCreator 4*  
- Procedurally select:
  - Edge walls  
  - Corner walls  
  - Top face walls  

---

## 🧠 Implementation Notes
- Built entirely in Unity’s Editor using **EditorWindows**, **SerializedObjects**, and **GUI/IMGUI**  
- Grid data stored in **ScriptableObjects** for easy saving/loading  
- Prefab previews generated via `AssetPreview.GetAssetPreview()`  
- Designed for **extensibility** — developers can add new brush types, placement rules, or randomization logic  

---

## 🏗️ Installation

1. Copy the tool folder (including the `Editor` scripts) into your Unity project under `Assets/`.  
2. Open the editor window from the menu:  
