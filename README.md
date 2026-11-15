[README.md](https://github.com/user-attachments/files/34864697/README.md)
# QuickCustomizer

A lightweight, client-side wardrobe and profile customizer mod for **Among Us** (BepInEx IL2CPP). Change your colors, hats, visors, skins, pets, nameplates, and display name on the fly using a clean in-game menu or quick chat commands—complete with automatic saving and Fortegreen support!

---

## ✨ Features

* **In-Game Wardrobe GUI (`F1`):** Press **F1** anytime in a lobby or Freeplay to browse and equip your unlocked cosmetics, colors, and name presets.
* **Chat Commands:** Quickly update your look without opening a menu using slash commands directly in the chat box.
* **Fortegreen & Hand Fix:** Equip Fortegreen (`ID 18` or `/color 31`) from the menu or chat, complete with an automatic runtime patch that fixes the hand-tinting issue.
* **Blank Name Support:** Easily equip an invisible/blank display name via the Profile tab or with `/blank`.
* **Persistent Settings:** Automatically saves your selected color, cosmetics, and display name using `PlayerPrefs` and reapplies them whenever you join a lobby.

---

## 🎮 Controls & Chat Commands

### Keybinds
* **`F1`** — Toggle the QuickCustomizer Wardrobe Menu

### Chat Commands
Type any of the following commands into the in-game chat box and press Enter (commands are intercepted locally and won't spam the lobby chat):

| Command | Description | Example |
| :--- | :--- | :--- |
| `/color <id>` | Set your player color by ID (`0–18`, or `31` for Fortegreen) | `/color 31` |
| `/name <text>` | Change your display name (`blank` for invisible name) | `/name Crewmate` |
| `/blank` or `/noname` | Set your display name to a blank character | `/blank` |
| `/hat <query>` | Search and equip an unlocked hat by partial ID/name | `/hat top` |
| `/visor <query>` | Search and equip an unlocked visor by partial ID/name | `/visor glasses` |
| `/skin <query>` | Search and equip an unlocked skin by partial ID/name | `/skin suit` |
| `/pet <query>` | Search and equip an unlocked pet by partial ID/name | `/pet dog` |
| `/nameplate <query>` | Search and equip an unlocked nameplate by partial ID/name | `/nameplate galaxy` |
| `/random` | Randomize your color and display name | `/random` |
| `/help` or `/info` | Display the command list in your local chat | `/help` |

---

## 📥 Installation

1. **Install BepInEx:** Ensure you have **BepInEx 6 (Unity.IL2CPP)** installed in your *Among Us* game directory. (Launch the game once after installing BepInEx so it generates the required folders).
2. **Download the Mod:** Grab the latest `QuickCustomizer.dll` from the [Releases](../../releases) page.
3. **Install the Plugin:** Place `QuickCustomizer.dll` into your `Among Us/BepInEx/plugins/` folder.
4. **Play:** Launch *Among Us*, join a lobby or Freeplay, and press **`F1`**!

---

## 🛠️ Building from Source

1. Clone this repository.
2. Open `QuickCustomizer.csproj` and update `<AmongUsPath>` to point to your local *Among Us* installation folder.
3. Run the build command in your terminal:
   ```bash
   dotnet build -c Release

   This mod is not affiliated with Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.
