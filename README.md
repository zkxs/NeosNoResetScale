# NeosNoResetScale

A [NeosModLoader](https://github.com/zkxs/NeosModLoader) mod for [Neos VR](https://neos.com/) that removes the Reset Scale functionality from the built in context menu button. This allows you to lock your scale whenever you want. 

Relevant Neos issue: [#3505](https://github.com/Neos-Metaverse/NeosPublic/issues/3505)

Restoring the reset scale functionality is left as an exercise for the user. If you wish to do it via creating a context menu button, here are the details to replicate the vanilla behavior:
| Field | Value |
| - | - |
| Label | `Interaction.ResetScale` (locale string) or `Reset Scale` (English) |
| Icon | `neosdb:///38c8ebd9590dbefd2589189cf7e25ea20e25a78d7c675ff613d566bdd5f5afc9.png` |
| Color (RGB) | (1.0, 0.4, 0.2) |
| Transition Time | 0.25 seconds |

## Installation
1. Install [NeosModLoader](https://github.com/zkxs/NeosModLoader).
2. Place [NeosNoResetScale.dll](https://github.com/zkxs/NeosNoResetScale/releases/latest/download/NeosNoResetScale.dll) into your `nml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods` for a default install. You can create it if it's missing, or if you launch the game once with NeosModLoader installed it will create the folder for you.
3. Start the game. If you want to verify that the mod is working you can check your Neos logs.
