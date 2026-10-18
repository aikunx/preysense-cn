using System;
using System.IO;
using PreySense;

namespace PreySense.Gpu
{
    public class NvidiaWrapper
    {
        /// <summary>
        /// Applies the NVIDIA GPU core and memory clock offsets.
        /// </summary>
        public static void ApplyGpuSettings(int coreOffsetMHz, int memoryOffsetMHz)
        {
            try
            {
                if (coreOffsetMHz < 0) coreOffsetMHz = 0;
                if (memoryOffsetMHz < 0) memoryOffsetMHz = 0;
                if (coreOffsetMHz > 500) coreOffsetMHz = 500;
                if (memoryOffsetMHz > 3000) memoryOffsetMHz = 3000;

                var control = new NvidiaGpuControl();
                if (control.IsValid)
                {
                    control.SetClocks(coreOffsetMHz, memoryOffsetMHz);

                    AppLogger.Log($"应用显卡设置：核心 +{coreOffsetMHz}MHz、显存 +{memoryOffsetMHz}MHz 成功。");
                }
                else
                {
                    AppLogger.Log("应用显卡设置：NvidiaGpuControl 无效或未找到 NVIDIA 显卡。");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Log($"应用显卡设置失败：{ex.Message}");
            }
        }

        /// <summary>
        /// Optional display registry profile for Optimus transitions (Eco/Standard).
        /// Ultimate mode is handled via WMI MUX + reboot, not here.
        /// </summary>
        public static bool SetGpuMode(int mode)
        {
            if (mode is 0 or 1)
            {
                ApplyRegistryDisplayProfile("optimus_mode.reg");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Imports a registry display configuration profile and runs pnputil to scan for device changes.
        /// </summary>
        public static void ApplyRegistryDisplayProfile(string filename)
        {
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PreySense");
                string appDataPath = Path.Combine(appDataDir, filename);
                string tempPath = Path.Combine(Path.GetTempPath(), filename);
                string fallbackPath = Path.Combine(appDir, filename);
                string targetPath = "";

                if (File.Exists(appDataPath))
                {
                    targetPath = appDataPath;
                }
                else if (File.Exists(tempPath))
                {
                    targetPath = tempPath;
                }
                else if (File.Exists(fallbackPath))
                {
                    try
                    {
                        if (!Directory.Exists(appDataDir))
                        {
                            Directory.CreateDirectory(appDataDir);
                        }
                        File.Copy(fallbackPath, appDataPath, true);
                        AppLogger.Log($"已将注册表显示配置复制到 LocalAppData：{appDataPath}");
                        targetPath = appDataPath;
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Log($"复制注册表显示配置到 AppData 失败：{ex.Message}");
                        targetPath = fallbackPath;
                    }
                }

                if (string.IsNullOrEmpty(targetPath))
                {
                    AppLogger.Log($"在 AppData、Temp 或程序目录中找不到显示配置 '{filename}'。");
                    return;
                }

                AppLogger.Log($"正在导入注册表显示配置：{targetPath}");

                using (var regProcess = new System.Diagnostics.Process())
                {
                    regProcess.StartInfo.FileName = "reg.exe";
                    regProcess.StartInfo.Arguments = $"import \"{targetPath}\"";
                    regProcess.StartInfo.CreateNoWindow = true;
                    regProcess.StartInfo.UseShellExecute = false;
                    regProcess.StartInfo.RedirectStandardOutput = true;
                    regProcess.Start();
                    regProcess.WaitForExit();
                }

                AppLogger.Log("正在通过 pnputil 扫描设备变更…");
                using (var pnpProcess = new System.Diagnostics.Process())
                {
                    pnpProcess.StartInfo.FileName = "pnputil.exe";
                    pnpProcess.StartInfo.Arguments = "/scan-devices";
                    pnpProcess.StartInfo.CreateNoWindow = true;
                    pnpProcess.StartInfo.UseShellExecute = false;
                    pnpProcess.StartInfo.RedirectStandardOutput = true;
                    pnpProcess.Start();
                    pnpProcess.WaitForExit();
                }

                AppLogger.Log("显示配置已应用，设备扫描完成。");
            }
            catch (Exception ex)
            {
                AppLogger.Log($"应用注册表显示配置失败：{ex.Message}");
            }
        }
    }
}
