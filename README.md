# 掠夺者控制台（PreySense 中文版）

[![](https://img.shields.io/badge/powered_by-dsh-4D6BFE?style=flat-square&logo=deepseek&logoColor=white)](https://github.com/deepseek-ai/deepseek-harness)
![platform Windows](https://img.shields.io/badge/platform-Windows-0078D6?style=flat-square&logo=windows&logoColor=white) ![C# WinForms](https://img.shields.io/badge/C%23-WinForms-239120?style=flat-square&logo=csharp&logoColor=white) ![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white) ![RAM 902 MB → 129 MB](https://img.shields.io/badge/RAM-902_MB_→_129_MB-brightgreen?style=flat-square) ![locale zh-CN](https://img.shields.io/badge/locale-zh--CN-grey?style=flat-square)

[**简体中文**](README.md) | [English](README.en.md)

---

## 📑 目录

- [🔥 这是什么？](#-这是什么)
- [⚡ 30 秒快速开始](#-30-秒快速开始)
- [💡 为什么值得换](#-为什么值得换)
- [🎯 能做什么](#-能做什么)
- [🌏 汉化说明](#-汉化说明)
- [🔧 精简官方软件（可选）](#-精简官方软件可选)
- [🛠️ 编译](#️-编译)
- [❓ 常见问题](#-常见问题)
- [🙏 致谢](#-致谢)
- [🤖 关于本项目](#-关于本项目)
- [⚠️ 免责声明](#️-免责声明)

---

## 🔥 这是什么？

**把 785 MB 的 Chromium 壳，换成 62 MB 的原生控制台。**

这是 [PreySense](https://github.com/hammadzaigham/PreySense) 的**完整中文化版本**。
PreySense 是一个第三方工具，用来替代宏碁 Predator 笔记本自带的 PredatorSense。

### 一眼看懂差距

| 对比项 | 官方 PredatorSense | PreySense 中文版 |
|---|---|---|
| 内存占用 | **902 MB** | **129 MB** |
| 磁盘占用 | 219 MB | 10 MB |
| 内核 | Chromium（libcef 197 MB） | 纯 WinForms |
| 开机进程 | 4 个 | 1 个 |
| 界面语言 | 中文（机翻感重） | **中文（全部重译）** |
| 可读性 | 压缩后的 bundle | **完整开源源码** |

> 省下 85% 的内存，功能一个不少。这不是阉割版，是**轻量版**。

我做的事情只有两件：

1. **把界面全部翻译成中文**，技术缩写加上中文注释
2. **写清楚怎么把官方软件安全精简掉**（保留必要的驱动服务，不破坏硬件能力）

核心功能全部来自原作者，我只做中文化与整理。

---

## 30 秒快速开始

### 一、安装

1. 下载 [Releases](https://github.com/aikunx/preysense-cn/releases/latest) 里的 `PreySense.exe`
2. **右键 → 以管理员身份运行**（必须，否则读不到硬件）
3. 完成

### 二、需要先装的东西

| 需要什么 | 为什么 | 怎么装 |
|---|---|---|
| [.NET 10 桌面运行时](https://dotnet.microsoft.com/download/dotnet/10.0) | 程序运行环境 | 下载安装包，一路下一步 |
| [PawnIO](https://pawnio.eu/) | 读写 CPU 功耗 / 风扇需要底层驱动 | 下载安装，重启 |

不装 PawnIO 也能用，只是「CPU 功耗」和「风扇曲线」两个功能会灰掉。

### 三、想卸掉官方软件？

看下面「[精简官方软件](#精简官方软件可选)」那一节。**先看完再动手**，有坑。

---

## 为什么值得换

先看数字：

| 项目 | 官方 PredatorSense | PreySense |
|---|---|---|
| 内存占用 | **902 MB** | **129 MB** |
| 磁盘占用 | 219 MB | 10 MB |
| 开机启动 | 4 个进程 | 1 个进程 |
| 界面语言 | 中文（但翻译质量一般） | **中文（全部重译）** |

> 902 MB 里，光 `libcef.dll`（Chromium 内核）就占 197 MB。

换过去**内存省 85%**，功能一个不少。

---

## 能做什么

### 性能与散热

- **性能模式**：节能 / 静音 / 均衡 / 性能 / 加速 / 极致
- **风扇控制**：自定义风扇曲线（按住 Ctrl 吸附到整十度）
- **CPU 功耗**：PL1 / PL2 功耗墙、CPU 加速档位
- **显卡超频**：核心/显存偏移、温度功耗实时读取
- **显卡模式**：独显直连 / 混合输出 / 仅核显

### 显示与电池

- **刷新率**：自动切换、LCD 超频（提响应速度）
- **颜色配置**：按刷新率保存不同的色彩方案
- **电池**：充电上限（延长电池寿命）

### 灯效与覆盖层

- **键盘灯效**：37 种模式（深空 / 熔岩 / 极光 / 晚霞 / 冰寒…）
- **游戏覆盖层**：实时显示 FPS、CPU/GPU 温度、风扇转速、功耗曲线

### 其他

- **Predator 键**：物理模式切换键 + 数字键 1-5 快捷切模式
- **开机自启**：计划任务方式，不弹 UAC

---

## 汉化说明

### 翻译了什么

| 类别 | 例子 |
|---|---|
| 性能模式 | 静音 / 均衡 / 性能 / 加速 / 极致 / 节能 / 续航 |
| 风扇 | 风扇与功耗 / 风扇转速 / 风扇升速 / 风扇全速 |
| 功耗 | 功耗限制 / PL1 功耗 (W) / PL2 功耗 (W) / CPU 加速 |
| 显卡 | 显卡模式：独显直连 / 混合输出 / 仅核显 / 核心偏移 / 显存偏移 |
| 灯效 | 深空 / 熔岩 / 极光 / 晚霞 / 冰寒（共 37 种） |
| 显示 | 笔记本屏幕 / 自动 / 超频 / 电池充电上限 |
| 服务 | LCD 超频 / Win 键锁定 / 粘滞键 / 开机音 / 屏幕防撕裂 / 音效模式 |
| 覆盖层 | 处理器 / 显卡 / 转速 / 使用率 / 温度 / 频率 / 功耗 |

### 缩写怎么处理

保留原文 + 括号中文，一眼就能看懂：

| 显示 | 意思 |
|---|---|
| **CPU（处理器）** | 中央处理器 |
| **GPU（显卡）** | 图形处理器 |
| **FPPT（快速功耗限制）** | 短时爆发功耗上限 |
| **SPPT（持续功耗限制）** | 长时间持续功耗上限 |
| **FHD（全高清）** | 1920×1080 分辨率 |
| **RGB（灯效）** | 彩色灯效 |

### 没翻译的

- **技术标识**：MSR、WMI、PL1、PL2、NVAPI、PawnIO、ETW、FPS
- **品牌名**：Acer、NVIDIA、Ryzen、Predator
- **日志文件**（在 `%LOCALAPPDATA%\PreySense\logs`）
- **注册表键名**、**命令行参数**

这些翻译了反而看不懂。

---

## 精简官方软件（可选）

> ⚠️ **动手前必读**：以下操作会让你失去官方的 PredatorSense 界面，但**保留所有硬件能力**。
> 操作前建议先创建系统还原点。

### 第一步：搞清楚哪些能删、哪些不能删

| 组件 | 能不能删 | 删了会怎样 |
|---|---|---|
| PredatorSense UI（4 个进程，785 MB） | ✅ 能删 | 界面没了，用 PreySense 代替 |
| PredatorSense 安装目录（277 MB） | ✅ 能删 | 省磁盘 |
| 开机自启任务 | ✅ 能禁 | 开机不再弹官方界面 |
| Acer Care Center / Quick Access | ✅ 能禁 | 这些是别的软件的，跟 Predator 无关 |
| **AcerLightingService** | ❌ **绝对不能删** | 灯效全废 |
| **AcerServiceSvc** | ❌ **绝对不能删** | 风扇、性能模式、音效、LCD 全废 |
| **AcerDeviceEnablingServiceV2** | ❌ **绝对不能删** | WMI 接口没了，PreySense 读不到硬件 |

**关键理解**：官方的 219 MB 里，780 MB 内存都是那个 Chromium 界面。真正的硬件控制是三个驱动服务，它们只占 66 MB。
**我们要做的是「换界面、留服务」**。

### 第二步：正确做法

**❌ 不要用 MSI 卸载 PredatorSense**

我试过。MSI 卸载会**连带删除驱动层服务**（`AcerLightingService`、`AcerServiceSvc`），
删完之后灯效和风扇全废，而且恢复需要重装整个软件。

**✅ 正确做法**：

```powershell
# 1. 结束官方界面进程（服务不要动）
Get-Process PredatorSense, SenseOverlay -EA 0 | Stop-Process -Force

# 2. 禁用开机自启
Disable-ScheduledTask -TaskName 'PredatorSenseLauncher'

# 3. 删掉界面目录（服务在 DriverStore，不受影响）
Remove-Item 'C:\Program Files\PredatorSense' -Recurse -Force
```

**第三步：验证服务还在**

```powershell
Get-Service AcerLightingService, AcerServiceSvc, AcerDeviceEnablingServiceV2
```

三个都显示 `Running` 就对了。

> 如果是 Store 版（装在 `WindowsApps` 里），还要额外执行：
> `Get-AppxPackage *PredatorSense* | Remove-AppxPackage`

### 第四步：省更多（可选）

这 6 个是 Acer Care Center 的配套进程，跟 Predator 核心无关：

```
AcerAgentService          21 MB
AcerSysHardwareService    23 MB
AcerHardwareService       20 MB
AcerSysMonitorService     18 MB
AcerSystemCentralService   7 MB
AcerCentralService         7 MB
```

全停能再省 97 MB。不过我建议**先不动**——它们按需加载，停掉后某些 WMI 查询可能受影响。

---

## 编译

需要 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)。

```powershell
dotnet publish app\PreySense.csproj -c Release -r win-x64 ^
    --self-contained false ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -o publish-zh
```

---

## 常见问题

**Q：会不会影响保修？**
A：不会。我们只改软件设置，不动 BIOS、不刷固件、不拆机。

**Q：为什么必须管理员运行？**
A：读写 CPU 的 MSR 寄存器和风扇控制需要内核级权限。官方软件也一样。

**Q：装完以后官方软件还能用吗？**
A：能，但两者抢同一个硬件接口，建议只留一个。

**Q：我的 Acer 型号不是 PHN16-71 能用吗？**
A：大概率可以。PreySense 依赖的是 Acer 通用的 WMI 接口。但灯效模式数量、风扇档位可能不同。

**Q：改 CPU 功耗有风险吗？**
A：有。设太高会过热降频，设太低会卡。建议从默认值小幅调整，观察温度再决定。

---

## 致谢

这个项目的核心工作来自别人：

| 项目 | 作者 | 作用 |
|---|---|---|
| [PreySense](https://github.com/hammadzaigham/PreySense) | [@hammadzaigham](https://github.com/hammadzaigham) | **汉化与精简的基础，核心功能全部来自他** |
| [G-Helper](https://github.com/seerge/g-helper) | [@seerge](https://github.com/seerge) | PreySense 的上游项目 |
| [PawnIO](https://pawnio.eu/) | namazso | Intel MSR / EC 驱动 |
| [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) | LibreHardwareMonitor 团队 | 硬件传感器读取 |
| [NvAPIWrapper](https://github.com/falahati/NvAPIWrapper) | [@falahati](https://github.com/falahati) | NVIDIA GPU 控制 |

**特别感谢 [DeepSeek](https://www.deepseek.com/)** 提供模型能力——本项目的汉化、文档和精简方案，由 AI 完成。

---

## 关于本项目

**本仓库的全部内容由 AI 辅助生成。**

| 项 | 说明 |
|---|---|
| 生成模型 | [DeepSeek](https://www.deepseek.com/) |
| 运行环境 | DeepSeek Harness |
| 人的角色 | 提出目标、真机执行验证、反馈现象 |

内存数字、服务名称、操作步骤都经过真机验证；但代码与文字确实由 AI 生成。请把它当作「AI 生成、人类验证」的产物。

---

## ⚠️ 免责声明

**使用本项目的任何代码、文档或方法，风险自负。**

1. **硬件风险**：修改 DPI、灯效、功耗、按键映射涉及与设备固件交互。操作不当（例如发送格式错误的 HID 报文）可能导致设备进入异常状态，需要重新插拔甚至复位才能恢复。
2. **保修**：卸载官方软件、修改设备设置可能影响厂商保修。是否违反保修条款，请咨询设备厂商。
3. **合法性**：本项目仅用于**你自己拥有的设备**。请勿用于修改他人设备、绕过计费、或在未经授权的环境中使用。
4. **无担保**：代码按「原样」提供，不提供任何明示或暗示的担保（包括但不限于适销性、特定用途适用性）。作者不对任何直接或间接损失负责。
5. **固件**：本项目**不做固件烧写**。如果你自行尝试刷写固件，风险完全由你承担——刷错会变砖。
6. **反作弊**：宏功能使用标准 `SendInput` 模拟按键，原理与 AutoHotkey 相同。部分竞技游戏可能将此类工具判定为违规，请自行判断是否使用。
7. **AI 生成**：本项目内容由 AI 生成、人类验证。虽然经过真机测试，但仍可能存在未发现的错误。执行任何命令前请先理解它的作用。

---
## 许可

基于 [PreySense](https://github.com/hammadzaigham/PreySense)（**MIT 许可**），同样遵循 MIT。

原项目作者：**hammadzaigham**。汉化与精简方案由本项目作者完成。

---

<p align="center">
  <i>留驱动，换掉壳 · 内存省 85% 而功能不减</i>
</p>
