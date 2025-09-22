# PSVR2-Gamepad

A small Windows app that lets your PSVR2 Sense controllers act like a single Xbox 360 controller. It reads the inputs via HidSharp and sends them to a virtual Xbox controller using ViGEm.

Think of it as a bridge so your PSVR2 controllers can work in pretty much any game that supports XInput.

* **Language/Runtime**: C# / .NET 9
* **HID handling**: [HidSharp](https://github.com/IntergatedCircuits/HidSharp)
* **Virtual gamepad**: [Nefarius.ViGEm.Client](https://github.com/ViGEm/ViGEm.NET)
* **Mapping ideas/reference**: community research by [AwesomeTornado](https://github.com/AwesomeTornado/PSVR2-controller-explorer)

---

## Requirements

* Windows 10 or 11
* [ViGEm Bus Driver](https://github.com/nefarius/ViGEmBus/releases/latest) installed
* PSVR2 controllers (**Bluetooth ONLY**)
* .NET 9 SDK

> ViGEm must be installed for the virtual Xbox 360 controller to work.
> USB support is not yet implemented, only Bluetooth connections are supported.

---

## Download the latest release

1. Install the latest [ViGEm Bus Driver](https://github.com/nefarius/ViGEmBus/releases/latest)
2. Go to the [releases page](https://github.com/BlueberryWolf/PSVR2-Gamepad/releases/latest)
3. Download the `.zip` for the latest version
4. Extract it anywhere you like
5. Run `PSVR2-Gamepad.exe` (ensure ViGEm is installed and your PSVR2 controllers are connected)

---

## Build from source

1. make sure you have [.net 9 sdk](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) installed
2. clone the repo

```powershell
git clone https://github.com/yourusername/PSVR2-Gamepad.git
cd PSVR2-Gamepad\PSVR2-Gamepad\PSVR2-Gamepad
```

3. restore dependencies

```powershell
dotnet restore
```

4. build the release version

```powershell
dotnet build -c Release
```

5. the compiled executable will be in `bin\Release\net9.0\`. run `PSVR2-Gamepad.exe` from there (ensure ViGEm is installed and your PSVR2 controllers are connected)

---

## Button / Stick Mapping (to Xbox 360/XInput)

* **Sticks (LS/RS)**: left/right, Y inverted
* **Triggers (LT/RT)**: trigger pull percent
* **Grips (LB/RB)**: left/right grip click
* **Face buttons (A/B/X/Y)**: mapped around Cross, Circle, Square, Triangle
* **Back**: left Option
* **Start**: right Option
* **Guide**: right Menu
* **Stick click**: LS/RS press
* **D-Pad**: optional Fake D-Pad from left stick (toggle with left PS Button/Menu)

---

## Fake D-Pad

* Turns your left stick into D-Pad presses
* **Toggle**: left Menu button
* **Default**: off at startup
* **Env var tweaks**:

  * `PSVR2_FAKE_DPAD_THRESHOLD` (0–1, default 0.5)
* Only works if the left controller is connected

---

## Rumble behavior

* Games that use XInput rumble will vibrate your Sense controllers
* Two controllers: Large motor -> left, Small motor -> right
* Single controller: combines both channels and sends to that one

---

## Troubleshooting


* **No virtual controller?** Make sure ViGEm Bus Driver is installed and app is running
* **No PSVR2 detected?** Pair via Bluetooth, then try again
* **No rumble on Bluetooth?** Check connection and try a test app (Steam or XInput tester)
* **Opening the app tries to open ms-gamebar** This is a windows feature, Game Bar is opened when an XInput device is detected. You can disable the gamebar entirely [here](https://github.com/AveYo/Gaming/blob/main/ms-gamebar-annoyance.bat)

---

## Project layout

<details>
<summary>PSVR2-Gamepad: .NET 9 console app</summary>

* `Program.cs`: app entry point; device discovery and main loop

* `Bridge/ViGEmBridge.cs`: XInput emulation and rumble routing to PSVR2

* `Communication/RumbleProtocol.cs`: USB/Bluetooth rumble packets (0x02/0x31, CRC32)

* `Hardware/PSVR2Controller.cs`: HID open/read/write; connection (USB/BLE)

* `Hardware/HidDeviceExtensions.cs`: helpers for HidSharp

* `Parsing/ReportParser.cs`: parse input reports to model

* `Models/PSVR2Report.cs`: strongly-typed controller state

* `Mapping/Xbox360Mapping.cs`: map PSVR2 state to Xbox 360 buttons/axes

* `Features/FakeDpad.cs`: dominant-axis D-Pad from left stick

* `Features/FakeDpadConfig.cs`: configuration and env binding

* `Constants/PSVR2Constants.cs`: vendor/product IDs, report IDs, sizes, bit masks

* `UI/ConsoleDisplay.cs`: console HUD and runtime status

* `PSVR2-Gamepad.csproj`: project metadata and NuGet dependencies

* `PSVR2-Gamepad/PSVR2-Gamepad.sln`: solution file

</details>

---

## Roadmap / future ideas

* Configurable rumble mirroring and scaling
* Controller UI for visualization and configuration  
* Advanced haptics
* Fix USB Support 

---

## Credits

* HID parsing inspired by [AwesomeTornado](https://github.com/AwesomeTornado)
* Virtual controller powered by [ViGEm](https://github.com/nefarius/ViGEmBus/releases/latest)
