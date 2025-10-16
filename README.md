<p align="center">
  <img src="https://raw.githubusercontent.com/deadwronggames/ZSharedAssets/main/Banner_Zombie.jpg" alt="ZCommon Banner" style="width: 100%; max-width: 1200px; height: auto;">
</p>

# ZTools

ZTools provides general-purpose Unity editor utilities for project setup and workflow enhancements.

## Installation

Install via Unity Package Manager using the Git URL: https://github.com/deadwronggames/ZTools
Include in your code (when needed) via the namespace:
```csharp 
using DeadWrongGames.ZTools;
```

## Features

- **New Project Initialization**: Automatically installs essential Unity and ZPackages dependencies, imports asset packages, and creates a default project folder structure.
- **CaptureTextureToPng**: MonoBehaviour to capture camera output to a PNG file at configurable resolution.


## Usage Examples

### New Project Initialization
1. Open Unity Editor.
2. Install ZTools as described above
2. Go to menu: ZTools → New project initialization.
3. Run in order:
   - 1. Install essential packages
   - 2. Import essential assets
   - 3. Create default project structure

This sets up a ready-to-go project with standard folders and dependencies.

### CaptureTextureToPng
1. Attach `CaptureTextureToPng` to a Camera in your scene.
2. Configure width, height, and filename in the Inspector.
3. Play the scene and press `P` to save a PNG capture.
4. Output is saved to `Assets/Output_CaptureTexture`.


## Notes

- Editor-only utilities (some classes only compile in the Unity Editor).
- Developed for usage with **Odin Inspector**.
- **Work in progress**, API and features may change.
