# CircleLooks

**CircleLooks** is a lightweight, easy-to-use appearance preset switcher for Virt-A-Mate supporting both **Desktop** and **VR** modes. 

Stop digging through menus every time you want to test different outfits, hairstyles, or character looks in a scene. Simply organize your saved appearances into subfolders, and smoothly cycle through them—or switch between characters—using UI buttons or customizable keyboard hotkeys.

---

### Key Features

* **Folder-Based Collections (Looksets):** Automatically turns your subfolders inside `Saves/Person/appearance/` into ready-to-use collections with preset counters.
* **Safe Character Switching:** Keeps your character's scene name intact when loading new appearances, ensuring your scene logic, triggers, and animations stay unbroken.
* **Desktop & VR On-Screen Notifications:** Clean, auto-fading text popups appear right in your view when switching looks or characters, so you always know what is selected even with all menus hidden.
* **Trigger Support:** All cycling actions (`NextLook`, `PrevLook`, `CycleLookset`, `CyclePersons`) can be bound to in-game UI buttons, timers, or VR triggers.
* **Smart Load Protection:** Prevents accidental double-clicks or spam while heavy outfits and hair are still loading.

---

### How to Organize Your Looks

Place your saved appearance presets into subfolders inside your standard Virt-A-Mate appearance directory:
`Saves/Person/appearance/`

Each subfolder automatically appears in the plugin as a separate Lookset:
* `Saves/Person/appearance/Female/` → **Female (12)**
* `Saves/Person/appearance/Male/` → **Male (8)**
* `Saves/Person/appearance/Cyberpunk/` → **Cyberpunk (15)**

---

### Recommended Setup (Session Plugin)

1. Place `GRomVAM.CircleLooks.1.var` into your `AddonPackages` folder.
2. Launch Virt-A-Mate, open **Main Menu** → **Session Plugins**, and click **Add Plugin**.
3. Select the `CircleLooks` package and load `CircleLooks.cs`.
4. Click **Save As Default** in the Session Plugins tab so the plugin automatically loads into every scene you open.

---

### Default Hotkeys

All hotkeys can be customized or disabled (`None`) directly in the plugin UI:

| Action | Default Key | Description |
| :--- | :---: | :--- |
| **Next Look** | `L` | Applies the next look from the active folder to the selected character. |
| **Prev Look** | `K` | Applies the previous look from the active folder to the selected character. |
| **Cycle Lookset** | `O` | Switches to the next available folder collection. |
| **Cycle Person** | `P` | Switches the target selection to the next character in the scene. |

---

### License & Credits

* **License:** Creative Commons Attribution-ShareAlike (CC BY-SA)
* **Credits:** Special thanks to MeshedVR for Virt-A-Mate and Acidbubbles for the plugin setup template.

---

### Links & Support

* **Source Code & Documentation:** [GitHub Repository](https://github.com/GRomVAM/CircleLooks)
* **Bug Reports & Suggestions:** [GitHub Issues](https://github.com/GRomVAM/CircleLooks/issues)
