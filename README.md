# CircleLooks for Virt-A-Mate

An easy-to-use appearance preset switcher for Virt-A-Mate (VaM).

This plugin lets you organize your character appearances into separate folders (Looksets) and smoothly cycle through them or switch between characters using UI buttons or keyboard hotkeys.

---

## Key Features

* **Folder-Based Collections (Looksets):** Automatically turns your appearance subfolders into ready-to-use collections.
* **Safe Character Switching:** Keeps your character's name intact when loading new appearances, ensuring your scene setup and animations stay unbroken.
* **Trigger Support:** All cycling actions can be bound to in-game buttons.

---

## How to Organize Your Looks

Place your saved appearance presets into subfolders inside your standard Virt-A-Mate appearance folder:
`Saves/Person/appearance/`

Each subfolder automatically appears in the plugin as a separate Lookset with the total number of looks shown next to its name:
* `Saves/Person/appearance/Female/` → **Female (12)**
* `Saves/Person/appearance/Male/` → **Male (8)**
* `Saves/Person/appearance/Cyberpunk/` → **Cyberpunk (15)**

---

## Installation

1. Place the `.var` file into your Virt-A-Mate `AddonPackages` folder.
2. Launch Virt-A-Mate.
3. Open **Main Menu** → **Session Plugins** and click **Add Plugin**.
4. Select the `CircleLooks` `.var` package and load the script.
5. Click **Save As Default** in the Session Plugins tab so the plugin automatically loads into every scene you open.

---

## User Interface & Controls

* **Target Person:** Dropdown menu to select which character in the scene will receive the new look.
* **Refresh Scene Persons:** Updates the character list if you added or removed characters in the scene.
* **Active Lookset:** Dropdown menu to choose which folder collection you want to browse.
* **Refresh Looksets:** Rescans your appearance folders if you added new presets while the game is running.
* **Next Look / Prev Look:** Buttons to manually step forward or backward through the active collection.

You can customize or disable (`None`) any hotkey using the dropdown menus on the right side of the plugin UI:

| Action | Default Key | What It Does |
| :--- | :---: | :--- |
| **Next Look** | `L` | Applies the next look from the active folder to the selected character. |
| **Prev Look** | `K` | Applies the previous look from the active folder to the selected character. |
| **Cycle Lookset** | `O` | Switches to the next available folder collection. |
| **Cycle Person** | `P` | Switches the target selection to the next character in the scene. |

---

## License and Credits

* **License:** Distributed under the Creative Commons Attribution-ShareAlike (CC BY-SA) license.
* **Credits:** Special thanks to MeshedVR for Virt-A-Mate and Acidbubbles for the project setup template.
