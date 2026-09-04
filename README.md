# Z32

C# Nissan 300ZX (Z32) ECU serial diagnostics: Consult 9600 baud, live sensors, DTC read/clear, and WinForms hosts. `Z32.Comms.ECU` opens a `SerialPort` at 9600 8N1 (no handshake, ISO-8859-1 encoding), initialises with `FF FF EF`, then reads ECU part number (`0xD0`), three diagnostic trouble-code sets, and up to 20 live sensors using Nissan Consult register maps (coolant, O2, speed, battery, throttle, CAS RPM, MAF, injector pulse, A/F alpha). `Fault` maps hex codes 11-55 to FSM diagnostic procedures, repair priority, and regional applicability (California / twin-turbo / A/T). `Test` is a COM-port picker that dumps part number (`23710-` prefix), DTCs, and a sensor snapshot; `Graphing` polls coolant temperature every 500 ms onto an analog `AGauge` (30-120 °C) via hardcoded `COM5`. `AndroidComms` is an unfinished Mono for Android stub of `ECU` (fields only; `Dispose` throws) and is not listed in `Z32.sln`.

**Source last updated:** 2020-09-02 · **Language:** C# · **Target:** .NET Framework 2.0 (`Comms`, `Test`), .NET Framework 4.0 (`Graphing`), Mono for Android / `v4.0.3` (`AndroidComms`) · **Output:** class libraries (`Comms`, `AndroidComms`) + WinForms exes (`Test`, `Graphing`)

## Solution structure

| Project | Language | Type | Purpose |
|---------|----------|------|---------|
| `Comms` (`Comms/Comms.csproj`) | C# | Class library (`Z32.Comms`) | Nissan Z32 Consult serial protocol: `ECU` connect/disconnect, `ReadDTC` / `ResetCodes`, `ReadSensors` (max 20), coolant/injection/ignition test commands (several `TEST_*` stubs empty). |
| `Test` (`Test/Test.csproj`) | C# | WinForms exe (`Z32.Test`) | Port combo + list box: connect, show part number, read DTC set 1, dump CAS/injector/MAF/speed/AAC sensors, then disconnect. |
| `Graphing` (`Graphing/Graphing.csproj`) | C# | WinForms exe | Timer-driven coolant-temp analog gauge (`AGaugeApp.AGauge`); references `Comms` and sibling `..\Serial\AGauge\AGauge.csproj`. |
| `AndroidComms` (`AndroidComms/AndroidComms.csproj`) | C# | Mono for Android class library | Incomplete `AndroidComms.ECU` port (command/sensor byte tables only). Not in `Z32.sln`. |
| `AGauge` (`..\Serial\AGauge\AGauge.csproj`) | C# | Class library (not in this tree) | Analog gauge WinForms control used by `Graphing`. Clone [VaderConsulting/Serial](https://github.com/VaderConsulting/Serial) as a sibling `Serial` folder. |

TFS `$tf/` cache, `bin/`, `obj/`, `.vs/`, and `*.suo` are gitignored. `Z32.sln` TFS server URL was redacted (`vaderconsulting.visualstudio.com` → `org.example.visualstudio.com`).

## How to open

Open `Z32.sln` in Visual Studio 2019 or later (solution format 12.00 / Visual Studio Version 16). `Comms` and `Test` target .NET Framework 2.0; `Graphing` targets .NET Framework 4.0 and needs the sibling AGauge project from Historical Dev `Serial`. `AndroidComms` uses `Novell.MonoDroid.CSharp.targets` (Mono for Android / Xamarin) and is opened separately via `AndroidComms/AndroidComms.csproj`. Bindings remain for Azure DevOps TFVC (`SccProvider` SAK on the `.csproj` files).

## Requirements

- Visual Studio 2019, .NET Framework 2.0, .NET Framework 4.0, .NET Framework 4.0.3

## Attribution and provenance

Working copy from Dave Robinson's OneDrive Historical Dev folder `Z32`. All four assemblies: `AssemblyCopyright` Copyright © 2012, `AssemblyVersion` 1.0.0.0, empty `AssemblyCompany` / `AssemblyDescription`. Titles/products: Comms, Test, Graphing, AndroidComms. Solution still lists a TFVC binding for AGauge under a SkyDrive `New Apps\Graphing\AGauge` path; the `.csproj` reference is `..\Serial\AGauge\AGauge.csproj`. `Z32.v11.suo` (VS 2012) was present in the zip and is gitignored.

## License

MIT © 2026 VaderConsulting for Dave Robinson's code. See `LICENSE`. AGauge is third-party analog-gauge source living in the sibling Serial tree; it is referenced only and is not bundled here.
