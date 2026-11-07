# CircleLooks for Virt-A-Mate

Performance-optimized appearance preset cycler for Virt-A-Mate (VaM).

This plugin allows you to organize character appearances into separate folder collections (Looksets) and cycle through them or switch between target characters using responsive screen-overlay hotkeys, completely independent of the VaM UI window.

---

## Key Features

* **Folder-Based Collections (Looksets):** Automatically groups .json appearance presets based on the subfolders they are placed in.
* **On-Screen Helper Overlay:** Auto-fading text alerts pop up directly in the simulation space when cycling via hotkeys, keeping you informed even when all menus are hidden.
* **Smart Performance Optimization:** No heavy background disk scanning or frame drops during normal gameplay. Directories are scanned cleanly upon initialization or manual refresh.
* **Closing UI Freedom:** Designed specifically for players who prefer to cycle looks using hotkeys without keeping the Session Plugins window open.

---

## How It Works (The Concept)

You can organize your look presets into folders inside the standard VaM directory:
Saves/Person/Appearance/

Each top-level subfolder automatically becomes an individual Lookset collection available for rotation. For example:
* Saves/Person/Appearance/Female/ -> Will act as the "Female" lookset collection.
* Saves/Person/Appearance/Male/ -> Will act as the "Male" lookset collection.
* Saves/Person/Appearance/Cyberpunk/ -> Will act as the "Cyberpunk" lookset collection.

---

## Installation and Setup

1. Download the latest .var package from releases or compile the script locally.
2. Ensure the script is located under: Custom/Scripts/GRomVAM/CircleLooks/CircleLooks.cs
3. Launch Virt-A-Mate.
4. Add CircleLooks to your Session Plugins list.

---

## Controls and Hotkeys

The user interface is split into a Management and Setup Panel (left side) and a Customizable Hotkeys Setup Panel (right side).

| Action | Default Keyboard Key | Description |
| :--- | :---: | :--- |
| **Next Look** | L | Cycles to the next appearance preset in the active folder. |
| **Prev Look** | K | Cycles to the previous appearance preset in the active folder. |
| **Next Lookset** | O | Switches to the next available folder collection. |
| **Next Person** | P | Switches to the next Person atom found in the scene. |

Note: Hotkeys can be fully customized or disabled (None) directly from the plugin's UI panel.

---

## License and Credits

* **License:** Distributed under the Creative Commons Attribution-ShareAlike (CC BY-SA) license.
* **Credits:** Special thanks to MeshedVR for Virt-A-Mate and Acidbubbles for the project setup template.
