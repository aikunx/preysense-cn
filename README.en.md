# Predator Control Center (PreySense Chinese Edition)

> Full Chinese localization of [PreySense](https://github.com/hammadzaigham/PreySense), plus a slim replacement for the official PredatorSense.

[**English**](README.en.md) | [简体中文](README.md)

## What this is

A Chinese edition of the Acer Predator laptop control app [PreySense](https://github.com/hammadzaigham/PreySense):

- **100% localized UI**: main window, secondary dialogs, dropdowns, message boxes, OSD overlay
- **Technical abbreviations with Chinese notes**: CPU (处理器), GPU (显卡), FPPT (快速功耗限制), SPPT (持续功耗限制)
- **Coexistence with official software**: keep only driver services, drop the 785 MB Chromium UI

## Why

The official PredatorSense is a CefSharp (Chromium) shell:

| Component | Memory | Disk |
|---|---|---|
| PredatorSense UI (4 processes) | 784.9 MB | 219 MB |
| Acer background services (12) | 117.5 MB | — |
| **Total** | **902.4 MB** | 219 MB |

After switching to PreySense:

| Component | Memory |
|---|---|
| PreySense | 62.8 MB |
| Acer core services (3) | 66.5 MB |
| **Total** | **129.3 MB** |

**Saves 773 MB (−85.7%)** with no feature loss.

## Localization scope

### Translated

| Category | Examples |
|---|---|
| Performance modes | Silent / Balanced / Performance / Turbo / Ultimate / Eco / Endurance |
| Fans | Fans + Power / Fan Speed / Ramp Up / Max Fans |
| Power | Power Limits / PL1 (W) / PL2 (W) / CPU Boost |
| GPU | GPU Mode: dGPU only / Hybrid / iGPU only / Core Offset / Memory Offset |
| Lighting | Deep Space / Magma Flow / Nordic Aurora / Sunset Glow / Ice Cold / 37 modes |
| Display | Laptop Screen / Auto / Overdrive / Battery Charge Limit |
| Services | LCD Overdrive / Win Key Lock / Sticky Key / Boot Sound / Panel DFR / Sound Mode |
| OSD | CPU (处理器) / GPU (显卡) / RPM (转速) / Overlay Settings / Usage / Temperature / Clock / Power |
| Abbreviations | CPU (处理器) / GPU (显卡) / FPPT (快速功耗限制) / SPPT (持续功耗限制) / FHD (全高清) |

### Kept in English

- **Technical identifiers**: MSR, WMI, PL1, PL2, NVAPI, PawnIO, ETW, FPS
- **Brands**: Acer, NVIDIA, Ryzen, Predator
- **Logs** (in `%LOCALAPPDATA%\PreySense\logs`)
- **Registry keys / CLI args**

## Slimming the official software

### Safe to remove

| Item | Action |
|---|---|
| PredatorSense AppX (Store version) | `Remove-AppxPackage` |
| PredatorSense MSI (277 MB) | delete directory + unregister |
| App execution alias `PredatorSense.exe` | removed with AppX |
| Scheduled task PredatorSenseLauncher | disable and delete |
| Acer Care Center / Quick Access / Device Info | stop and disable |

### Must keep (PreySense depends on them)

| Service | Purpose |
|---|---|
| AcerLightingService | 37 lighting effects |
| AcerServiceSvc | fans / performance / sound / LCD / panel DFR |
| AcerDeviceEnablingServiceV2 | WMI interface |

Their executables live in `C:\Windows\System32\DriverStore\`; deleting the UI directory does not affect them.

### Warning

**Do not uninstall PredatorSense via MSI.** We measured that it also deletes driver-layer services (`AcerLightingService`, `AcerServiceSvc`), breaking lighting and fan control.

Correct approach: delete only the UI directory and disable UI autostart; keep driver services.

## Build

Requires .NET 10 SDK.

```powershell
dotnet publish app\PreySense.csproj -c Release -r win-x64 `
    --self-contained false `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o publish-zh
```

## Requirements

- Windows 10/11 x64
- .NET 10 Desktop Runtime
- **Administrator** (MSR / EC access via PawnIO)
- [PawnIO](https://pawnio.eu/) driver for Intel MSR access

## Capability matrix

| Capability | Path | Status |
|---|---|---|
| Performance mode | WMI `SetGamingProfile` | ✅ |
| Fan speed / curve | WMI + AcerService | ✅ |
| CPU power / boost | PawnIO MSR | ✅ |
| GPU overclock / temp / power | NVAPI | ✅ |
| 37 lighting effects | AcerService | ✅ |
| Sound / LCD overdrive / panel DFR | AcerService | ✅ |
| Refresh rate / color profiles | WMI | ✅ |
| Battery charge limit | WMI | ✅ |
| In-game overlay (FPS / usage / chart) | ETW + GDI+ | ✅ |
| Autostart (highest privilege, no UAC) | Scheduled task | ✅ |

## License

Based on [PreySense](https://github.com/hammadzaigham/PreySense) (GPL v3); this edition is also GPL v3.

Original author: hammadzaigham. Localization and slimming guide by this project's author.
