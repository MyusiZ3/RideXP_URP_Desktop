# RideXP URP - Interactive Bicycle Simulator & Racing Game

RideXP URP is an interactive 3D bicycle simulation and racing game built with Unity Universal Render Pipeline (URP). It includes player bicycle physics, AI competitors, checkpoint tracking, 3D leaderboards, and real-time serial hardware integration for stationary exercise bikes via Arduino.

---

## Technical Specifications & Requirements

- **Unity Editor Version**: Unity 6 (`6000.0.35f1`) or higher
- **Render Pipeline**: Universal Render Pipeline (URP 17.0+)
- **Operating System**: Windows 10/11 (for Arduino Serial communication)
- **Hardware Integration**: Stationary bike / exercise equipment connected via Arduino (USB Serial CH340/FTDI driver)

---

## Step-by-Step Installation Guide

### Step 1: Fork and Clone the Repository

1. Click the **Fork** button at the top-right of this GitHub repository to create your own copy.
2. Clone your forked repository to your local machine:

```bash
git clone https://github.com/YOUR_USERNAME/RideXP_URP-Bicycle-Main.git
cd RideXP_URP-Bicycle-Main
```

---

### Step 2: Open the Project in Unity Hub

1. Open **Unity Hub**.
2. Click **Add** -> **Add project from disk**.
3. Select the cloned repository root directory (`RideXP_URP - Bicycle - Main`).
4. Ensure the editor version is set to **Unity 6 (`6000.0.35f1`)** or compatible Unity 6 release.
5. Click the project to open it in Unity Editor. (Allow Unity to import initial package dependencies).

---

### Step 3: Package Dependencies (Automatic via Package Manager)

The project relies on standard Unity Package Manager dependencies specified in `Packages/manifest.json`. Unity Hub will automatically resolve these upon opening:

- `com.unity.render-pipelines.universal` (URP 17.0.3)
- `com.unity.inputsystem` (New Input System 1.11.2)
- `com.unity.ai.navigation` (NavMesh AI Navigation 2.0.7)
- `com.unity.cinemachine` (Cinemachine 3.1.3)
- `com.unity.visualeffectgraph` (VFX Graph 17.0.3)
- `com.unity.terrain-tools` (Terrain Tools 5.1.2)

If packages are missing, open **Window** -> **Package Manager** -> **In Project** and verify all dependencies are loaded.

---

### Step 4: Optional Third-Party Asset Packages (Environment & Weather)

The core codebase, bicycle physics, UI, hardware integration, and AI systems are tracked directly in Git. Heavy third-party environment asset packages are ignored by default to keep the repository lightweight.

If you wish to restore full environment textures and weather graphics, import the following Unity Asset Store packages into `Assets/`:

1. **Meadow Environment Dynamic Nature** (NatureManufacture) -> import into `Assets/7 Environment/`
2. **River Auto Material** (NatureManufacture) -> import into `Assets/7 Environment/`
3. **Gaia Pro** (Procedural Worlds) -> import into `Assets/Procedural Worlds/`
4. **UniStorm Weather System** -> import into `Assets/UniStorm Weather System/`
5. **Volumetric Lights** -> import into `Assets/VolumetricLights/`
6. **Animals Full Pack** -> import into `Assets/ANIMALS FULL PACK/`

---

### Step 5: Hardware & Arduino Setup (Optional for Physical Bikes)

1. Connect your stationary exercise bike / Arduino module via USB.
2. Ensure the correct VCP driver (CH340 / FTDI) is installed on your operating system.
3. Open Device Manager on Windows and note the assigned COM port (e.g., `COM3`).
4. In Unity, select the **SerialController** game object in your active scene (`Assets/Ardity/Scripts/SerialController.cs`).
5. Set **Port Name** to your assigned COM port and set **Baud Rate** to match your Arduino sketch (e.g., `9600` or `115200`).

---

### Step 6: Running the Game

1. In the Project window, navigate to `Assets/3 Scenes/`.
2. Open the main menu scene or race scene (`NatureV2.unity`).
3. Press **Play** in the Unity Editor to start the simulation.

---

## Project Architecture Summary

- `Assets/1 UI/`: UI assets, sprites, icons, and menus.
- `Assets/2 Script/`: Core gameplay scripts (`BicycleController.cs`, `NPCBicycleAIController.cs`, `RaceCountdownManager.cs`, `LeaderboardManager.cs`).
- `Assets/3 Scenes/`: Main race tracks and menu scenes.
- `Assets/3D/`: 3D models and track prefabs.
- `Assets/4 Sound/`: Audio clips and sound effects.
- `Assets/5 Prefabs/`: Assembled game object prefabs.
- `Assets/6 VFX/`: Visual effect graphs and particle systems.
- `Assets/Ardity/`: Arduino hardware serial communication library.
- `Assets/Simple Bicycle Physics/`: Core bicycle physics subsystem.
