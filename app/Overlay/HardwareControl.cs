using System;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using PreySense.Gpu;
using PreySense.Helpers;
using System.Runtime.InteropServices;

namespace PreySense.Overlay
{
    public static class HardwareControl
    {
        public static IGpuControl? GpuControl;
        public static string CpuName { get; private set; } = "";
        public static string CpuShortName { get; private set; } = "";
        public static string GpuShortName { get; private set; } = "";

        public static float? cpuTemp = -1;
        public static float? gpuTemp = -1;

        public static float? cpuPower;
        public static float? gpuPower;

        public static int? cpuFanRPM;
        public static int? gpuFanRPM;

        public static int? cpuUsage;
        public static int? gpuUsage;
        public static int? vramUsage;
        public static int? ramUsage;
        public static int? vramUsedMb;
        public static int? ramUsedMb;

        // Extra metrics
        public static int?   cpuMhz;
        public static int?   gpuMhz;
        public static float? cpuVoltage;
        public static float? gpuVoltage;
        public static int?   ramSpeedMhz;
        public static int?   vramSpeedMhz;

        private static readonly LibreHardwareMonitor.Hardware.Computer _lhm = new()
        {
            IsCpuEnabled = true,
            IsGpuEnabled = false,
            IsMemoryEnabled = false,
            IsMotherboardEnabled = false,
            IsControllerEnabled = false,
            IsNetworkEnabled = false,
            IsStorageEnabled = false
        };
        private static bool _lhmOpened = false;
        private static float? _lhmCpuTemp;
        private static void EnsureLhmOpen()
        {
            if (_lhmOpened) return;
            try
            {
                _lhm.Open();
                _lhmOpened = true;
            }
            catch (Exception ex)
            {
                AppLogger.Log("打开 LibreHardwareMonitor 失败：" + ex.Message);
            }
        }

        static HardwareControl()
        {
            try
            {
                GpuControl = new NvidiaGpuControl();
            }
            catch (Exception ex)
            {
                AppLogger.Log($"硬件控制显卡初始化失败：{ex.Message}");
            }

            CpuName = ReadCpuName();
            CpuShortName = ShortCpuName(CpuName);
            RefreshGpuName();
        }

        public static void RefreshGpuName() => GpuShortName = ShortGpuName(GpuControl?.FullName);

        private static string ReadCpuName()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                return key?.GetValue("ProcessorNameString")?.ToString()?.Trim() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static string ShortGpuName(string? full)
        {
            if (string.IsNullOrEmpty(full)) return "";
            foreach (string tag in new[] { "RTX", "GTX", "RX", "Arc" })
            {
                int i = full.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
                if (i < 0) continue;
                string[] p = full[i..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return p.Length >= 2 ? p[0] + " " + p[1] : p[0];
            }
            return full;
        }

        private static string ShortCpuName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";

            var m = Regex.Match(name, @"i[3579]-\w+");
            if (m.Success) return m.Value;

            m = Regex.Match(name, @"Ultra\s+\d+\s+(\w*\d\w*)");
            if (m.Success) return "Ultra " + m.Groups[1].Value;

            if (name.Contains("Ryzen", StringComparison.OrdinalIgnoreCase))
            {
                m = Regex.Match(name, @"(?:[A-Z]{2,}\s+)?\d{3,}\w*");
                return m.Success ? "Ryzen " + m.Value : "Ryzen";
            }

            return name.Split(' ', StringSplitOptions.RemoveEmptyEntries) is { Length: > 0 } t ? t[0] : "";
        }

        public static void ResetCPUPowerCounter()
        {
        }

        public static void InitCPUPowerAsync()
        {
        }

        public static int GetCpuMhz()
        {
            try
            {
                EnsureLhmOpen();

                float maxCpuClock = 0;
                cpuVoltage = null;
                _lhmCpuTemp = null;

                foreach (var hardware in _lhm.Hardware)
                {
                    hardware.Update();

                    if (hardware.HardwareType == LibreHardwareMonitor.Hardware.HardwareType.Cpu)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == LibreHardwareMonitor.Hardware.SensorType.Clock &&
                                sensor.Value.HasValue &&
                                sensor.Name.IndexOf("Bus", StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                if (sensor.Value.Value > maxCpuClock)
                                    maxCpuClock = sensor.Value.Value;
                            }
                            else if (sensor.SensorType == LibreHardwareMonitor.Hardware.SensorType.Voltage && sensor.Value.HasValue)
                            {
                                if (sensor.Name.IndexOf("Core", StringComparison.OrdinalIgnoreCase) >= 0 || 
                                    sensor.Name.IndexOf("Vcore", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    cpuVoltage == null)
                                {
                                    cpuVoltage = sensor.Value.Value;
                                }
                            }
                            else if (sensor.SensorType == LibreHardwareMonitor.Hardware.SensorType.Temperature && sensor.Value.HasValue)
                            {
                                if (sensor.Name.IndexOf("Package", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    _lhmCpuTemp == null)
                                {
                                    _lhmCpuTemp = sensor.Value.Value;
                                }
                            }
                        }
                    }
                }
                return (int)Math.Round(maxCpuClock);
            }
            catch (Exception ex)
            {
                AppLogger.Log("LHM 读取 CPU 频率与电压出错：" + ex.Message);
                return 0;
            }
        }

        public static float? GetCPUPower()
        {
            return null;
        }

        private static long _cpuLastIdle, _cpuLastKernel, _cpuLastUser, _cpuLastTick;
        private static bool _cpuUsageBaseline;

        public static int? GetCPUUsage()
        {
            if (!NativeMethods.GetSystemTimes(out long idle, out long kernel, out long user)) return null;

            long now = Environment.TickCount64;

            if (!_cpuUsageBaseline || now - _cpuLastTick > 2000)
            {
                _cpuLastIdle = idle; _cpuLastKernel = kernel; _cpuLastUser = user; _cpuLastTick = now;
                _cpuUsageBaseline = true;
                return null;
            }

            long deltaIdle = idle - _cpuLastIdle;
            long deltaTotal = (kernel - _cpuLastKernel) + (user - _cpuLastUser);

            _cpuLastIdle = idle; _cpuLastKernel = kernel; _cpuLastUser = user; _cpuLastTick = now;

            if (deltaTotal <= 0) return 0;
            return Math.Clamp((int)Math.Round((1.0 - (double)deltaIdle / deltaTotal) * 100), 0, 100);
        }

        public static (int percent, int usedMb)? GetRAMInfo()
        {
            var status = new NativeMethods.MEMORYSTATUSEX { dwLength = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.MEMORYSTATUSEX>() };
            if (!NativeMethods.GlobalMemoryStatusEx(ref status)) return null;
            int usedMb = (int)((status.ullTotalPhys - status.ullAvailPhys) / (1024 * 1024));
            return ((int)status.dwMemoryLoad, usedMb);
        }

        public static float? GetGPUPower()
        {
            try
            {
                float? power = GpuControl?.GetGpuPower();
                if (power is not null) return power.Value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("读取显卡功耗失败：" + ex.Message);
            }
            return null;
        }

        public static void ReadSensorsOverlay()
        {
            cpuMhz = GetCpuMhz(); // Call early to update LHM and populate cpuVoltage and _lhmCpuTemp

            var wmi = Program.settingsForm?.Wmi;

            cpuFanRPM = wmi?.CpuFanRpm;
            gpuFanRPM = wmi?.GpuFanRpm;

            cpuTemp = wmi?.CpuTemp ?? -1;
            if (cpuTemp <= 0 && _lhmCpuTemp.HasValue && _lhmCpuTemp.Value > 0)
            {
                cpuTemp = _lhmCpuTemp.Value;
            }
            gpuTemp = wmi?.GpuTemp ?? -1;

            if (GpuControl != null && GpuControl.IsValid)
            {
                var nvidTemp = GpuControl.GetCurrentTemperature();
                if (nvidTemp.HasValue && nvidTemp.Value > 0)
                {
                    gpuTemp = nvidTemp.Value;
                }
            }

            cpuUsage = GetCPUUsage();
            try { gpuUsage = GpuControl?.GetGpuUse(); } catch { gpuUsage = null; }

            var ram = GetRAMInfo();
            ramUsage = ram?.percent;
            ramUsedMb = ram?.usedMb;

            try
            {
                if (GpuControl?.GetVramInfo() is { } v && v.totalMb > 0)
                {
                    vramUsedMb = (int)v.usedMb;
                    vramUsage = (int)Math.Clamp(v.usedMb * 100 / v.totalMb, 0, 100);
                }
                else { vramUsedMb = null; vramUsage = null; }
            }
            catch { vramUsedMb = null; vramUsage = null; }

            cpuPower = null;
            try
            {
                var msr = PreySense.Mode.PowerLimitController.GetMsr();
                if (msr != null)
                {
                    float? newCpu = msr.GetPackagePower();
                    if (newCpu.HasValue && newCpu.Value > 0)
                    {
                        cpuPower = newCpu;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Log($"PawnIO 读取 CPU 功耗失败：{ex.Message}");
            }



            gpuPower = GetGPUPower();
            try { gpuMhz = GpuControl?.GetGpuClock(); } catch { gpuMhz = null; }
            
            ramSpeedMhz = GetRamSpeed();
            try { vramSpeedMhz = GpuControl?.GetGpuMemoryClock(); } catch { vramSpeedMhz = null; }
            try { gpuVoltage = GpuControl?.GetGpuVoltage(); } catch { gpuVoltage = null; }
        }

        private static int? _cachedRamSpeed;
        public static int? GetRamSpeed()
        {
            if (_cachedRamSpeed.HasValue) return _cachedRamSpeed;
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Speed, ConfiguredClockSpeed FROM Win32_PhysicalMemory");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var speed = obj["ConfiguredClockSpeed"] ?? obj["Speed"];
                    if (speed != null)
                      {
                          int val = Convert.ToInt32(speed);
                          if (val > 0)
                          {
                              _cachedRamSpeed = val;
                              return val;
                          }
                      }
                  }
              }
              catch { }
              return null;
          }
    }
}
