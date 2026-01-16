# Scroll Zoom

Scroll Zoom is a client-side camera modification for **Among Us** that enables adjustable zoom using the mouse scroll wheel.

The mod directly modifies the in-game camera, altering the visible area rendered to the player. It is intended for **modded environments, development, testing, recording, and private lobbies**.

---

## Showcase

![showcasegif](https://github.com/user-attachments/assets/f37c146a-ec8a-4607-b762-2f585d505443)

---

## Features

* Scroll wheel zoom control
* Active only during gameplay (matches and Freeplay)
* Automatically resets during meetings and body reports
* Zoom state is cleared when a round ends or when the map unloads

Zoom is automatically reset:

* During meetings and body reports
* When leaving a game or when a round ends

> [!IMPORTANT]
> Recommended use cases:
>
> * Private or modded lobbies
> * Development and testing
> * Freeplay
> * Recording or cinematic use

---

## Compatibility

| Mod                                                                |   Status  | Notes                          |
| ------------------------------------------------------------------ | :-------: | ------------------------------ |
| [Submerged](https://github.com/SubmergedAmongUs/Submerged/)        | Supported | Fully compatible               |
| [LevelImpostor](https://github.com/DigiWorm0/LevelImposter)        | Supported | Fully compatible               |
| [Town of Us: Mira](https://github.com/AU-Avengers/TOU-Mira/)       |  Partial  | Includes similar functionality |
| [Endless Host Roles](https://github.com/Gurge44/EndlessHostRoles/) |  Unknown  | Not tested                     |

> [!CAUTION]
> Mods that modify camera behavior, shadows, or rendering systems may introduce compatibility issues.

---

## Disclaimer

> [!WARNING]
> This mod modifies the camera’s orthographic size. As a result, it may:
>
> * Extend the visible area beyond normal player vision
> * Bypass fog, lighting, and shadow masking
> * Reveal areas and players that would not normally be visible

> [!IMPORTANT]
> This project is **not affiliated with Innersloth**.
> Use at your own discretion, particularly in public or restricted lobbies.
> The developer assumes no responsibility for misuse or violations of server or community rules.

---

## Credits

| Source                                                       | Credit              |
| ------------------------------------------------------------ | ------------------- |
| [Town Of Us: Mira](https://github.com/AU-Avengers/TOU-Mira/) | Main Zoom Code      |
| [jukixyo](https://github.com/jukixyo)                        | Bugfixes & Patches  |
