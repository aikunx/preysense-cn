using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using PreySense.Input;

namespace PreySense
{
    public partial class MainForm
    {
        private static readonly Color SilentModeOutlineColor = Color.FromArgb(210, 210, 210);

        private void ConfigureStartupWindow()
        {
            ApplyAppIcon();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MinimizeBox = false;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "掠夺者控制台 - v1.4.0";
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        private void ConfigureStartupButtons()
        {
            buttonTurboFanMode.Secondary = false;
            buttonTurboFanModePower.Secondary = false;

            buttonEcoMode.Text = "静音";
            buttonBalancedMode.Text = "均衡";
            buttonPerformanceMode.Text = "性能";
            buttonTurboFanMode.Text = "加速";

            buttonEnduranceMode.Text = "续航";
            buttonGpuStandardMode.Text = "标准";
            buttonGpuUltimateMode.Text = "极致";
            buttonTurboFanModePower.Text = "风扇与功耗";

            buttonEcoMode.BorderColor = SilentModeOutlineColor;
            buttonBalancedMode.BorderColor = colorStandard;
            buttonPerformanceMode.BorderColor = PerformanceModeColor;
            buttonTurboFanMode.BorderColor = TurboModeColor;
            buttonEnduranceMode.BorderColor = colorEco;
            buttonGpuStandardMode.BorderColor = colorStandard;
            buttonGpuUltimateMode.BorderColor = colorTurbo;
            buttonTurboFanModePower.BorderColor = borderSecond;
            button60Hz.BorderColor = AccentColor;
            button120Hz.BorderColor = AccentColor;
            buttonMaxRefreshRate.BorderColor = AccentColor;
            buttonAutoRefreshRate.BorderColor = colorGray;
            buttonRgbLighting.BorderColor = colorGray;

            buttonColorProfiles.Text = "显示";
            buttonColorProfiles.TabStop = false;
            buttonColorProfiles.Borderless = false;
            buttonColorProfiles.Secondary = true;
            buttonColorProfiles.BackColor = buttonSecond;
            buttonColorProfiles.ForeColor = foreMain;
            buttonColorProfiles.FlatAppearance.BorderColor = borderSecond;
            buttonColorProfiles.BorderColor = colorGray;
            buttonColorProfiles.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            buttonColorProfiles.TextAlign = ContentAlignment.MiddleCenter;
            buttonColorProfiles.Width = 80;
            buttonColorProfiles.Margin = new Padding(0, 4, 0, 4);

            button120Hz.Text = "120Hz + 超频";
            buttonBatteryFull.Text = "100%";
            labelBatteryStatus.Visible = true;
            UpdatePowerButtonAccentForPowerSource(SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline);
        }

        private void ConfigureStartupPanels()
        {
            panelScreen.Visible = true;
            panelRgb.Visible = false;
            panelBattery.Visible = true;
            panelStartup.Visible = true;
            MatchSectionTitleColors();
        }

        private void UpdatePowerButtonAccentForPowerSource(bool onBattery)
        {
            buttonEcoMode.BorderColor = onBattery ? colorEco : SilentModeOutlineColor;
        }

        private void ConfigureStartupActions()
        {
            checkRunOnStartup.ShowBorder = false;
            checkRunOnStartup.BackColor = Color.Transparent;
            checkRunOnStartup.ForeColor = foreMain;
            checkRunOnStartup.UseVisualStyleBackColor = false;

            checkAutoGpuBattery.ShowBorder = false;
            checkAutoGpuBattery.BackColor = Color.Transparent;
            checkAutoGpuBattery.Checked = IsGpuBatteryAutoEnabled();
            checkAutoGpuBattery.ForeColor = foreMain;
            checkAutoGpuBattery.UseVisualStyleBackColor = false;
            checkAutoGpuBattery.CheckedChanged += (_, _) => SetGpuBatteryAuto(checkAutoGpuBattery.Checked);
        }

        private void ConfigureStartupInfrastructure()
        {
            ConfigureTrayIcon();

            _timer.Interval = 1000;
            _timer.Tick += UpdateTelemetry;
            _timer.Start();

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            _keyboardHook = new KeyboardHook(ToggleAppVisibility, null, HandleModeNumberKey, OpenMetricsOverlay);
            _wmiHotkeyWatcher = new WmiHotkeyWatcher(OnHotkeyEvent);
            _quickAccessModeWatcher = new QuickAccessModeWatcher(HandleModeKeyPressed);
        }

        private void QueueStartupWork()
        {
            Task.Run(() => ClosePredatorSenseUiProcesses());
            ApplySavedBatteryLimitAsync();
        }


        private static void ClosePredatorSenseUiProcesses()
        {
            string[] processNames =
            [
                "PredatorSense",
            ];

            int currentProcessId = Environment.ProcessId;
            foreach (string processName in processNames)
            {
                foreach (Process process in Process.GetProcessesByName(processName))
                {
                    using (process)
                    {
                        try
                        {
                            if (process.Id == currentProcessId)
                                continue;

                            AppLogger.Log($"启动：正在关闭冲突的 Predator Sense 进程 '{process.ProcessName}'（PID {process.Id}）。");
                            if (!process.CloseMainWindow() || !process.WaitForExit(1500))
                            {
                                process.Kill(entireProcessTree: true);
                            }
                        }
                        catch (Exception ex)
                        {
                            AppLogger.Log($"启动：关闭 Predator Sense 进程 '{processName}' 失败：{ex.Message}");
                        }
                    }
                }
            }
        }

        private void StartDeferredStartupInitialization()
        {
            _ = Task.Run(() =>
            {
                int maxHz = GetMaxRefreshRate();
                int currentHz = GetCurrentRefreshRate();
                bool runOnStartup = IsRunOnStartupEnabled();

                BeginInvoke(new Action(() =>
                {
                    if (IsDisposed) return;

                    _maxHz = maxHz;
                    _currentRefreshRate = currentHz;
                    button120Hz.Text = $"{_maxHz}Hz + 超频";
                    UpdatePowerUIForBatteryState(SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline);
                    ConfigureRefreshRateButtons();
                    LoadMemory();
                    SyncRefreshRateButtons(currentHz, buttonAutoRefreshRate.Activated);
                    UpdateScreenCardTitle();
                    _lastRefreshRate = currentHz;

                    checkRunOnStartup.Checked = runOnStartup;
                    checkRunOnStartup.CheckedChanged += (_, _) => SetRunOnStartup(checkRunOnStartup.Checked);

                    LoadCurrentHardwarePowerMode();
                }));

                // Apply gamma LUT on the background thread — SetDeviceGammaRamp doesn't require UI thread
                // and doing it here avoids blocking the first paint with a GPU display pipeline stall.
                ApplyGammaForRefreshRate(currentHz);
            });
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("PreySense-Updater");
                string json = await client.GetStringAsync("https://api.github.com/repos/hammadzaigham/PreySense/releases/latest");
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("tag_name", out var tagProperty))
                {
                    string? latestTag = tagProperty.GetString();
                    if (!string.IsNullOrEmpty(latestTag))
                    {
                        string latestVersionStr = latestTag.TrimStart('v', 'V');
                        string currentVersionStr = Program.VersionString;

                        if (Version.TryParse(latestVersionStr, out Version? latestVersion) &&
                            Version.TryParse(currentVersionStr, out Version? currentVersion))
                        {
                            if (latestVersion > currentVersion)
                            {
                                BeginInvoke(new Action(() =>
                                {
                                    DialogResult result = Dialogs.ConfirmDialog.Show(
                                        this,
                                        $"掠夺者控制台有新版本 {latestTag} 可用。\n\n" +
                                        "是否打开 GitHub 仓库下载？",
                                        "掠夺者控制台 - 有新版本",
                                        "Download",
                                        "Later");

                                    if (result == DialogResult.Yes)
                                    {
                                        try
                                        {
                                            Process.Start(new ProcessStartInfo
                                            {
                                                FileName = "https://github.com/hammadzaigham/PreySense/releases",
                                                UseShellExecute = true
                                            });
                                        }
                                        catch (Exception ex)
                                        {
                                            AppLogger.Log($"打开更新链接失败：{ex.Message}");
                                        }
                                    }
                                }));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Log($"检查更新失败：{ex.Message}");
            }
        }
    }
}
