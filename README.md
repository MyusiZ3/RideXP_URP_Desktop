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

This project supports real-time hardware integration with stationary exercise bikes via Arduino / ESP microcontrollers.

#### Serial Communication & Controller Script Locations

1. **Ardity Serial Framework**:
   - `Assets/Ardity/`: Contains the complete Ardity communication library (`SerialController.cs`, `SerialThread.cs`).
2. **Active Serial Integration Code**:
   - [`Assets/2 Script/GamePlay/RespawnManager.cs`](file:///d:/Projects/Unity/RideXP_URP%20-%20Bicycle%20-%20Main/Assets/2%20Script/GamePlay/RespawnManager.cs): Reads incoming serial messages from Arduino using `serialController.ReadSerialMessage()`. Expected serial payload format:
     ```txt
     POT:XXX, SPD:YYY, BTN1:Z, BTN2:W
     ```
   - [`Assets/2 Script/Setting/SerialOverlayTrigger.cs`](file:///d:/Projects/Unity/RideXP_URP%20-%20Bicycle%20-%20Main/Assets/2%20Script/Setting/SerialOverlayTrigger.cs): Dedicated script for direct serial communication via `System.IO.Ports.SerialPort` (COM4, 115200 baud) that listens for ESP signals (e.g. string `"mulai"`).
3. **Backup Script References**:
   - `Assets/2 Script/BicycleBackup.txt` & `Assets/2 Script/BicycleControllModif.txt`: Preserved code backups of the bicycle controller logic.

#### Switching Between Input Modes

- **Desktop Keyboard Mode (Default)**:
  - `Assets/2 Script/BicycleController.cs` and `Assets/2 Script/GamePlay/SteerBicycle.cs` use standard Unity input axes (`Input.GetAxis("Horizontal")`, `Input.GetAxis("Vertical")`).
- **Arduino Hardware Mode**:
  - Connect your micro-controller via USB and verify the assigned port in Windows Device Manager.
  - In Unity, select the **SerialController** GameObject in the scene hierarchy.
  - Set **Port Name** (e.g., `COM3` or `COM4`) and **Baud Rate** to `115200`.
  - Pass parsed speed/potentiometer data from `SerialController` into `BicycleController.cs` or `SteerBicycle.cs` using the implementation pattern in `RespawnManager.cs`.

#### Complete Arduino / ESP Microcontroller Sketch (`.ino`)

Flash the following C++ code to your Arduino UNO, Nano, or ESP board. It reads steering potentiometer, hall/reed pulse speed, jump button, and respawn button, sending formatted data at 115200 baud:

```cpp
/*
  RideXP Bicycle Controller Firmware (Arduino / ESP32)
  
  Hardware Pinout:
  - Handlebar Steering Potentiometer : Analog Pin A0 (POT: 0-1023)
  - Wheel Cadence/Speed Sensor       : Digital Pin 2 (Reed Switch / Hall Sensor with Interrupt)
  - Jump / BunnyHop Button           : Digital Pin 3 (Active LOW with INPUT_PULLUP)
  - Respawn Button                   : Digital Pin 4 (Active LOW with INPUT_PULLUP)

  Baud Rate: 115200
  Payload Format: "POT:<0-1023>, SPD:<RPM>, BTN1:<0/1>, BTN2:<0/1>"
*/

#define STEER_POT_PIN    A0
#define REED_SENSOR_PIN  2
#define JUMP_BTN_PIN     3
#define RESPAWN_BTN_PIN  4

volatile unsigned long lastPulseTime = 0;
volatile unsigned long pulseInterval = 0;

unsigned long lastSerialTime = 0;
const unsigned long SERIAL_INTERVAL = 33; // Send data at ~30 FPS (33ms)

void IRAM_ATTR pulseISR() {
  unsigned long now = millis();
  if (now - lastPulseTime > 40) { // 40ms debounce filter
    pulseInterval = now - lastPulseTime;
    lastPulseTime = now;
  }
}

void setup() {
  Serial.begin(115200);

  pinMode(STEER_POT_PIN, INPUT);
  pinMode(REED_SENSOR_PIN, INPUT_PULLUP);
  pinMode(JUMP_BTN_PIN, INPUT_PULLUP);
  pinMode(RESPAWN_BTN_PIN, INPUT_PULLUP);

  attachInterrupt(digitalPinToInterrupt(REED_SENSOR_PIN), pulseISR, FALLING);
}

void loop() {
  unsigned long currentMillis = millis();

  if (currentMillis - lastSerialTime >= SERIAL_INTERVAL) {
    lastSerialTime = currentMillis;

    // 1. Read handlebar steering potentiometer (0 - 1023)
    int potValue = analogRead(STEER_POT_PIN);

    // 2. Calculate wheel speed / RPM from interrupt interval
    int speedValue = 0;
    if (currentMillis - lastPulseTime < 2000 && pulseInterval > 0) {
      speedValue = (int)(60000.0 / pulseInterval); // Convert to RPM
    } else {
      speedValue = 0; // Bike is stationary
    }

    // 3. Read digital buttons (Pressed = 1, Released = 0)
    int jumpState = (digitalRead(JUMP_BTN_PIN) == LOW) ? 1 : 0;      // BTN1 (Jump/BunnyHop)
    int respawnState = (digitalRead(RESPAWN_BTN_PIN) == LOW) ? 1 : 0; // BTN2 (Respawn)

    // 4. Output serial payload expected by Unity RespawnManager & SerialController
    Serial.print("POT:");
    Serial.print(potValue);
    Serial.print(", SPD:");
    Serial.print(speedValue);
    Serial.print(", BTN1:");
    Serial.print(jumpState);
    Serial.print(", BTN2:");
    Serial.println(respawnState);
  }
}
```

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
