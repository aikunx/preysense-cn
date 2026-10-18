# <img src="app/appicon.svg" alt="logo" width="42" height="42" align="left"> 掠夺者控制台 · PreySense 中文版

<p align="center">
  <b>官方 PredatorSense 太臃肿？换掉它</b><br>
  <i>完整中文化 · 保留驱动层能力 · 内存从 902 MB 降到 129 MB</i>
</p>

<p align="center">
  <a href="https://github.com/aikunx/preysense-zh"><img src="https://img.shields.io/badge/Memory--85%25-00ff88?style=flat-square" alt="Memory" /></a>
  <a href="https://github.com/aikunx/preysense-zh"><img src="https://img.shields.io/badge/UI-100%25%20%E4%B8%AD%E6%96%87-ff6b35?style=flat-square" alt="Chinese" /></a>
  <a href="https://github.com/aikunx/preysense-zh"><img src="https://img.shields.io/badge/Device-Acer%20Predator%20PHN16--71-83b81a?style=flat-square" alt="Device" /></a>
  <a href="https://dotnet.microsoft.com/download/dotnet/10.0"><img src="https://img.shields.io/badge/.NET-10%20Desktop-512BD4?style=flat-square&logo=dotnet" alt=".NET" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-green.svg?style=flat-square" alt="License" /></a>
</p>

---

## 📖 项目简介

基于 [PreySense](https://github.com/hammadzaigham/PreySense)（fork 自 G-Helper）的 **完整中文化** 版本，
并附带一套经过实测验证的 **官方 PredatorSense 精简方案**。

官方 PredatorSense 本质上是 CefSharp（Chromium）套壳：

| 组件 | 内存 | 磁盘 |
|---|---|---|
| PredatorSense UI（4 进程） | **784.9 MB** | 219 MB |
| Acer 后台服务（12 进程） | 117.5 MB | — |
| **合计** | **902.4 MB** | 219 MB |

换成 PreySense 后：

| 组件 | 内存 |
|---|---|
| PreySense | 62.8 MB |
| Acer 核心服务（3 个） | 66.5 MB |
| **合计** | **129.3 MB** |

> **省 773 MB（−85.7%），功能一个不少。**

---

## ✨ 功能

- **性能模式**：节能 / 静音 / 均衡 / 性能 / 加速 / 极致，支持 Predator 键 + 1-5 快捷切换
- **风扇控制**：自定义风扇曲线（Ctrl 吸附）、风扇全速、升速速率
- **CPU 调优**：PL1 / PL2 功耗限制、CPU 加速档位（经 PawnIO 直写 MSR）
- **显卡调优**：NVIDIA 核心/显存偏移、温度/功耗/频率实时读取
- **显卡模式**：独显直连 / 混合输出 / 仅核显，电池自动切核显
- **显示配置**：刷新率自动切换、LCD 超频、刷新率颜色配置
- **电池管理**：充电上限
- **键盘灯效**：37 种模式（深空 / 熔岩 / 极光 / 晚霞 / 冰寒 …）
- **硬件覆盖层**：实时 CPU/GPU 温度、风扇转速、功耗、内存占用、FPS、功耗曲线

---

## 🌏 汉化范围

### 已翻译

| 类别 | 例子 |
|---|---|
| 性能模式 | 静音 / 均衡 / 性能 / 加速 / 极致 / 节能 / 续航 |
| 风扇 | 风扇与功耗 / 风扇转速 / 风扇升速 / 风扇全速 |
| 功耗 | 功耗限制 / PL1 功耗 (W) / PL2 功耗 (W) / CPU 加速 |
| 显卡 | 显卡模式：独显直连 / 混合输出 / 仅核显 / 核心偏移 / 显存偏移 |
| 灯效 | 深空 / 熔岩 / 极光 / 晚霞 / 冰寒 / 共 37 种 |
| 显示 | 笔记本屏幕 / 自动 / 超频 / 电池充电上限 |
| 服务 | LCD 超频 / Win 键锁定 / 粘滞键 / 开机音 / 屏幕防撕裂 / 音效模式 |
| 覆盖层 | 处理器 / 显卡 / 转速 / 覆盖层设置 / 使用率 / 温度 / 频率 / 功耗 |
| 缩写注释 | CPU（处理器）/ GPU（显卡）/ FPPT（快速功耗限制）/ SPPT（持续功耗限制）/ FHD（全高清） |

### 保留英文

- **技术标识**：MSR、WMI、PL1、PL2、NVAPI、PawnIO、ETW、FPS
- **品牌**：Acer、NVIDIA、Ryzen、Predator
- **日志**（`%LOCALAPPDATA%\PreySense\logs`）
- **注册表键名 / 命令行参数**

---

## 🔧 官方软件精简方案

### 可以卸掉的

| 项目 | 操作 |
|---|---|
| PredatorSense AppX（Store 版） | `Remove-AppxPackage` |
| PredatorSense MSI（277 MB） | 删目录 + 卸注册表项 |
| 应用执行别名 `PredatorSense.exe` | 随 AppX 移除 |
| 开机任务 PredatorSenseLauncher | 禁用并删除 |
| Acer Care Center / Quick Access / Device Info | 停止并禁用 |

### ⚠️ 必须保留的（PreySense 依赖）

| 服务 | 作用 |
|---|---|
| AcerLightingService | 37 种灯效 |
| AcerServiceSvc | 风扇 / 性能 / 音效 / LCD / 防撕裂 |
| AcerDeviceEnablingServiceV2 | WMI 接口 |

这些服务的可执行文件都在 `C:\Windows\System32\DriverStore\`，删 UI 目录不影响它们。

### 🚨 重要教训

**不要用 MSI 卸载 PredatorSense。** 实测会连带删除驱动层服务（`AcerLightingService`、`AcerServiceSvc`），
导致灯效和风扇控制失效，而且恢复需要重装整个软件。

正确做法：**只删 UI 目录、只禁 UI 自启，保留驱动服务**。

---

## 🛠️ 构建

需要 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)。

```powershell
dotnet publish app\PreySense.csproj -c Release -r win-x64 `
    --self-contained false `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o publish-zh
```

## 📋 运行要求

- **系统**：Windows 10 / 11 x64
- **运行时**：.NET 10 Windows Desktop Runtime x64
- **权限**：管理员（读写 MSR / EC，走 PawnIO）
- **驱动**：[PawnIO](https://pawnio.eu/)（读写 Intel MSR 必需）

---

## 📊 能力对照

| 能力 | 通路 | 状态 |
|---|---|---|
| 性能模式 | WMI `SetGamingProfile` | ✅ |
| 风扇转速 / 曲线 | WMI + AcerService | ✅ |
| CPU 功耗 / 加速 | PawnIO 直写 MSR | ✅ |
| GPU 超频 / 温度 / 功耗 | NVAPI | ✅ |
| 37 种灯效 | AcerService | ✅ |
| 音效 / LCD 超频 / 屏幕防撕裂 | AcerService | ✅ |
| 显示刷新率 / 颜色配置 | WMI | ✅ |
| 电池充电上限 | WMI | ✅ |
| 游戏覆盖层（FPS / 占用 / 图表） | ETW + GDI+ | ✅ |
| 开机自启（最高权限，无 UAC） | 计划任务 | ✅ |

---

## 🙏 致谢

本项目建立在以下开源项目与作者的工作之上，在此郑重致谢：

| 项目 | 作者 | 用途 | 许可 |
|---|---|---|---|
| [PreySense](https://github.com/hammadzaigham/PreySense) | [@hammadzaigham](https://github.com/hammadzaigham) | 汉化与精简的基础 | MIT |
| [G-Helper](https://github.com/seerge/g-helper) | [@seerge](https://github.com/seerge) | PreySense 的上游项目 | MIT |
| [PawnIO](https://pawnio.eu/) | namazso | Intel MSR / EC 驱动 | — |
| [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) | LibreHardwareMonitor 团队 | 硬件传感器读取 | MPL 2.0 |
| [NvAPIWrapper](https://github.com/falahati/NvAPIWrapper) | [@falahati](https://github.com/falahati) | NVIDIA GPU 控制 | BSD 3-Clause |

> PreySense 是 hammadzaigham 从 G-Helper fork 并重构的成果，
> 本项目只是在此之上做中文化与官方精简方案，核心逻辑全部来自原作者。

---

## ⚖️ 许可

本项目基于 [PreySense](https://github.com/hammadzaigham/PreySense)（**MIT**）汉化，同样遵循 **MIT**。

原项目作者：hammadzaigham。汉化与精简方案由本项目作者完成。

---

<p align="center">
  <i>留驱动，换掉壳 · 内存省 85% 而功能不减</i>
</p>