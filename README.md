# CircleLooks

**CircleLooks** is a lightweight utility plugin for **Virt-A-Mate (VaM)** that allows you to organize `.vap` appearance presets into subfolder collections (**Looksets**) and seamlessly cycle through them on any `Person` atom in a scene—without breaking scene logic, animations, physics, or triggers.

## Key Features

* **Folder-Based Looksets:** Automatically scans top-level subfolders inside `Custom/Atom/Person/Appearance/` and treats each folder as an independent lookset collection.
* **Non-Destructive Switching:** Uses VaM's native `AppearancePresets` (`LoadPresetWithPath`) to swap character appearance, hair, and clothing while keeping poses, animations, timelines, and scene triggers intact.
* **Gallery-Style Navigation:** Cycle forward and backward through looks cleanly without double-step delays when reversing direction.
* **Visual Target Feedback:** Briefly blinks the selected `Person` atom in the 3D scene (`tempDisableRender`) so you always know which character is active.
* **3D Screen Overlay:** Displays unobtrusive, auto-fading status notifications directly in your field of view (Desktop & VR compatible).
* **Full Trigger & Hotkey Support:** Control everything via UI buttons, customizable keyboard shortcuts, or native VaM triggers.

## Folder Structure Setup

Organize your `.vap` appearance presets into subfolders inside your VaM installation directory:

```text
VaM/
└── Custom/
    └── Atom/
        └── Person/
            └── Appearance/
                ├── Casual/
                │   ├── Preset_Anna.vap
                │   └── Preset_Kate.vap
                ├── SciFi/
                │   └── Preset_Nova.vap
                └── Evening/
                    └── Preset_Bella.vap
```

> **Tip:** Save your character looks using the **Appearance Presets** tab (`Create New Preset`) rather than full `Atom Presets` so they remain compatible with any scene.

## Installation & Setup

### Option 1: Session Plugin (Recommended)
1. Download the latest `.var` package (`GRomVAM.CircleLooks.X.var`) and place it into your `AddonPackages/` folder.
2. Open **Main Menu → Session Plugins** and click **Add Plugin**.
3. Select the **`CircleLooks`** `.var` package and load **`CircleLooks.cslist`**.
4. Click **Save As Default** in the **Session Plugins** tab so the plugin automatically loads into every scene you open.

### Option 2: Scene / Atom Plugin
1. Open **Scene Plugins** (or select any atom's **Plugins** tab) and click **Add Plugin**.
2. Select **`CircleLooks.cslist`** from the package (important: select the `.cslist` file, not individual `.cs` files).
3. Choose an **Active Lookset** and a **Target Person** from the dropdown menus, then start cycling looks using the UI buttons, hotkeys, or VaM triggers.


## Default Hotkeys & Trigger Actions

Hotkeys are automatically ignored while typing inside any UI input field. You can rebind or disable (`None`) any key in the plugin UI.

| Action | Default Key | Trigger Action Name | Description |
| :--- | :---: | :--- | :--- |
| **Next Look** | `L` | `NextLook` | Applies the next appearance preset in the active lookset. |
| **Prev Look** | `K` | `PrevLook` | Applies the previous appearance preset in the active lookset. |
| **Cycle Lookset** | `O` | `CycleLookset` | Switches to the next lookset folder. |
| **Cycle Person** | `P` | `CyclePersons` | Switches to the next `Person` atom in the scene. |

## Credits & License

* **Author:** [GRomVAM](https://github.com/GRomVAM)
* **Special Thanks:** [atani](https://hub.virtamate.com/members/atani.6611) for valuable advice and helpful recommendations during development.
* **License:** Distributed under the Creative Commons Attribution-ShareAlike (CC BY-SA) license.
