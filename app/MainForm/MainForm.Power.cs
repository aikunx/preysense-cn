using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using PreySense.Dialogs;
using PreySense.Fan;
using PreySense.Gpu;
using PreySense.Helpers;
using PreySense.Mode;
using PreySense.Rgb;
using PreySense.Input;

namespace PreySense
{
    public partial class MainForm
    {
        private void LoadMemory()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\PreySense");
                if (key != null)
                {
                    LoadPersistedGpuState(key);
                    LoadPersistedRefreshState(key);
                    LoadPersistedRgbState(key);
                    LoadPersistedFanState(key);
                }
            }
            catch { }
        }

        private void LoadPersistedGpuState(RegistryKey key)
        {
            bool hasSavedMode = key.GetValue("GPUMode") is int;
            int gpuMode = GetRegistryInt(key, "GPUMode", 1);
            int gpuBattery = GetRegistryInt(key, "GpuBatteryAuto", 0);
            if (gpuMode == 3)
            {
                gpuMode = 1;
                gpuBattery = 1;
                SaveState("GPUMode", 1);
                SaveState("GpuBatteryAuto", 1);
            }

            if (!hasSavedMode)
            {
                bool hasIntegratedGpu = DeviceHelper.HasPresentDisplayDevice("VEN_8086");
                bool hasDiscreteGpu = DeviceHelper.HasPresentDisplayDevice("VEN_10DE");

                gpuMode = hasIntegratedGpu && hasDiscreteGpu
                    ? 1
                    : hasDiscreteGpu ? 2 : 0;

                SaveState("GPUMode", gpuMode);
            }

            _gpuBatteryAuto = (gpuBattery == 1);
            MarkGpuMode(gpuMode);
            HighlightFanMode(GetRegistryInt(key, "Fan", 0x01));
        }

        private void LoadPersistedRefreshState(RegistryKey key)
        {
            int screenAuto = GetRegistryInt(key, "ScreenAuto", 0);
            buttonAutoRefreshRate.Activated = (screenAuto == 1);

            int refresh = GetRegistryInt(key, "RefreshRate", 0);
            int currentHz = GetCurrentRefreshRate();
            _currentRefreshRate = currentHz;
            if (buttonAutoRefreshRate.Activated)
            {
                SyncRefreshRateButtons(currentHz, true);
            }
            else if (refresh > 0)
            {
                RestoreDisplayModeOnLaunch(refresh);
            }
            else
            {
                SyncRefreshRateButtons(currentHz, false);
            }
        }

        private void LoadPersistedRgbState(RegistryKey key)
        {
            _isApplyingSavedRgbState = true;
            try
            {
                int modeVersion = GetRegistryInt(key, "RGB_ModeVersion", 1);
                int rgbMode = RgbProfile.NormalizeSavedMode(GetRegistryInt(key, "RGB_Mode", 0), modeVersion);
                comboRgbLightingMode.SelectedIndex = MapModeToDropdownIndex(rgbMode);
                buttonRgbProfiles.Enabled = (rgbMode == 0);

                int rgbSpeed = RgbProfile.NormalizeStepLevel(GetRegistryInt(key, "RGB_Speed", 3));
                int rgbDirection = GetRegistryInt(key, "RGB_Direction", 1);
                byte direction = rgbDirection == 1 ? (byte)1 : (byte)2;

                int r = GetRegistryInt(key, "RGB_Zone0_R", 0);
                int g = GetRegistryInt(key, "RGB_Zone0_G", 150);
                int b = GetRegistryInt(key, "RGB_Zone0_B", 255);

                Color[] colors = new Color[4];
                for (int i = 0; i < 4; i++)
                {
                    int zR = GetRegistryInt(key, $"RGB_Zone{i}_R", r);
                    int zG = GetRegistryInt(key, $"RGB_Zone{i}_G", g);
                    int zB = GetRegistryInt(key, $"RGB_Zone{i}_B", b);
                    colors[i] = Color.FromArgb(zR, zG, zB);
                }

                pictureBacklightSwatch.BackColor = colors[0];

                byte brightness = GetBrightnessSliderValue();
                _wmi.SetCachedRgbState(rgbMode, (byte)r, (byte)g, (byte)b, brightness, (byte)rgbSpeed, direction, colors);
                Task.Run(() => _wmi.ApplyRgbState());
            }
            finally
            {
                _isApplyingSavedRgbState = false;
            }
        }

        private void LoadPersistedFanState(RegistryKey key)
        {
            labelBatteryStatusLimitTitle.Text = $"电池充电上限：{sliderBatteryChargeLimit.Value}%";

            int calibrated = GetRegistryInt(key, "Fan_Calibrated", 0);
            if (calibrated == 1)
            {
                PreySense.Fan.FanCurveControl.MaxCpuRpmSeen = GetRegistryInt(key, "Fan_MaxCpuRpm", 6400);
                PreySense.Fan.FanCurveControl.MaxGpuRpmSeen = GetRegistryInt(key, "Fan_MaxGpuRpm", 6400);
            }

            byte activePowerMode = GetActivePowerMode();
            var profile = ProfileManager.LoadProfile(activePowerMode);
            _cpuCurve = FanCurveStorage.LoadCpuCurve(activePowerMode);
            _gpuCurve = FanCurveStorage.LoadGpuCurve(activePowerMode);

            int legacyFanEnabled = GetRegistryInt(key, "Fan_CurveEnabled", 0);
            if (legacyFanEnabled == 1 && !profile.ApplyFanCurve)
            {
                profile.ApplyFanCurve = true;
                ProfileManager.SaveProfile(profile);
            }

            _applyCustomFans = profile.ApplyFanCurve;
            _fanRampUp = _applyCustomFans ? profile.FanRampUp : 1;
            _maxFanEnabled = GetRegistryInt(key, "Fan_MaxSpeed", 0) == 1;
            buttonTurboFanModePower.Activated = _applyCustomFans;
            buttonTurboFanModePower.BorderColor = _applyCustomFans ? PreySense.UI.RForm.colorCustom : borderSecond;
        }

        private static int GetRegistryInt(RegistryKey? key, string name, int defaultValue)
        {
            return key != null ? (int)key.GetValue(name, defaultValue)! : defaultValue;
        }

        public void ApplyPowerMode(byte mode)
        {
            ApplyPowerMode(mode, persistHardware: true);
        }

        private void ApplyPowerMode(byte mode, bool persistHardware)
        {
            if (persistHardware)
            {
                if ((DateTime.Now - _lastModeChangeTime).TotalSeconds < 2.0)
                {
                    return;
                }
                _lastModeChangeTime = DateTime.Now;
            }

            if (persistHardware && IsKnownPowerMode(_lastKnownProfile) && mode == _lastKnownProfile)
                return;

            byte previousMode = IsKnownPowerMode(_lastKnownProfile) ? _lastKnownProfile : GetActivePowerMode();
            byte currentMode = 0x01;
            string modeName = mode switch
            {
                0x00 => "静音",
                0x04 => "性能",
                0x05 => "加速",
                0x06 => "节能",
                _ => "均衡"
            };
            try
            {
                using var k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\PreySense");
                if (k != null) currentMode = (byte)(int)k.GetValue("Power", 0x01);
            }
            catch { }

            if (mode == 0x05 && currentMode != 0x05)
            {
                _prevPowerMode = currentMode;
                SaveState("PrevPower", _prevPowerMode);
            }

            // Apply all visual state immediately so the button outline and label
            // update before any blocking hardware call can delay the repaint.
            _lastKnownProfile = mode;
            HighlightPowerBtn(mode);
            UpdatePerformanceModeLabel(modeName);
            fansForm?.SyncActiveMode(mode);

            if (previousMode != mode)
            {
                ShowModeToast(modeName, mode);
            }

            if (!persistHardware)
            {
                // Sync-only path (e.g. hardware poll): no WMI write needed.
                ApplyFanCurvesForMode(mode);
                return;
            }

            // Run the blocking WMI poll and profile apply off the UI thread so the
            // button outline repaints immediately without waiting up to ~600 ms.
            _ = Task.Run(async () =>
            {
                bool applied = _wmi.SetPowerMode(mode);
                if (!applied)
                {
                    BeginInvoke(new Action(() =>
                    {
                        _modeToast?.Dismiss();
                        _lastKnownProfile = previousMode;
                        MarkPowerMode(previousMode);
                    }));
                    return;
                }

                _lastModeChangeTime = DateTime.Now;
                SaveState("Power", mode);
                SaveState(IsOnBatteryPower() ? "PowerBattery" : "PowerAC", mode);


                try
                {
                    await PreySense.Mode.ProfileManager.ApplyProfileAsync(modeName, _wmi);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"自动应用 {modeName} 配置失败：{ex.Message}");
                }

                BeginInvoke(new Action(() => ApplyFanCurvesForMode(mode)));
            });
        }

        private void ShowModeToast(string modeName, byte mode)
        {
            if ((DateTime.Now - _lastModeToastTime).TotalMilliseconds < ModeToastDebounceMs)
            {
                return;
            }

            _lastModeToastTime = DateTime.Now;
            _modeToast ??= new Overlay.ModeToastNotification();
            _modeToast.ShowMode(modeName, GetModeAccentColor(mode), GetModeToastIcon(mode));
        }

        private static Color GetModeAccentColor(byte mode) => mode switch
        {
            0x00 => SilentModeOutlineColor,
            0x06 => colorEco,
            0x04 => PerformanceModeColor,
            0x05 => colorTurbo,
            _ => colorStandard
        };

        private Image? GetModeToastIcon(byte mode) => mode switch
        {
            0x00 or 0x06 => buttonEcoMode.Image,
            0x04 => buttonPerformanceMode.Image,
            0x05 => buttonTurboFanMode.Image,
            _ => buttonBalancedMode.Image
        };

        private void SyncPowerModeFromHardware(byte mode)
        {
            ApplyPowerMode(mode, persistHardware: false);
        }

        private void MarkPowerMode(byte mode)
        {
            _lastKnownProfile = mode;
            HighlightPowerBtn(mode);
            string modeName = mode switch
            {
                0x00 => "静音",
                0x04 => "性能",
                0x05 => "加速",
                0x06 => "节能",
                _ => "均衡"
            };
            UpdatePerformanceModeLabel(modeName);
            fansForm?.SyncActiveMode(mode);
        }

        private void HighlightPowerBtn(byte mode)
        {
            buttonEcoMode.Activated = (mode == 0x00 || mode == 0x06);
            buttonBalancedMode.Activated = (mode == 0x01);
            buttonPerformanceMode.Activated = (mode == 0x04);
            buttonTurboFanMode.Activated = (mode == 0x05);
        }

        private void UpdatePerformanceModeLabel(string modeName, bool batteryAutoEco = false)
        {
            labelPerformanceMode.Text = batteryAutoEco
                ? $"性能模式：{modeName}（电池）"
                : $"性能模式：{modeName}";
        }

        private void ToggleTurboMode()
        {
            if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline) return;
            byte currentMode = _wmi.GetPowerProfile();
            if (currentMode == 0x05)
            {
                byte prev = 0x01;
                try
                {
                    using var k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\PreySense");
                    if (k != null) prev = (byte)(int)k.GetValue("PrevPower", 0x01);
                }
                catch { }
                ApplyPowerMode(prev);
            }
            else
            {
                ApplyPowerMode(0x05);
            }
        }

        private void CheckPowerSourceTransition()
        {
            CheckPowerSourceTransition(null);
        }

        private void CheckPowerSourceTransition(bool? onBatteryOverride)
        {
            bool currentPluggedIn = onBatteryOverride.HasValue ? !onBatteryOverride.Value : !IsOnBatteryPower();
            if (_isPluggedIn != currentPluggedIn)
            {
                _isPluggedIn = currentPluggedIn;
                HandlePowerSourceChanged(currentPluggedIn);
            }
        }

        private void HandlePowerSourceChanged(bool pluggedIn)
        {
            AppLogger.Log($"电源状态变化，已接通电源：{pluggedIn}");

            UpdatePowerUIForBatteryState(!pluggedIn);
            ApplyAutoRefreshRate(pluggedIn);

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\PreySense");
                byte powerMode;
                if (pluggedIn)
                {
                    powerMode = (byte)(int)(key?.GetValue("PowerAC", _lastKnownProfile == 0xFF ? 0x01 : _lastKnownProfile) ?? (_lastKnownProfile == 0xFF ? 0x01 : _lastKnownProfile));
                    ApplyPowerMode(powerMode);
                }
                else
                {
                    powerMode = 0x06;
                    SaveState("PowerBattery", powerMode);
                    ApplyPowerMode(powerMode);
                    UpdatePerformanceModeLabel("节能", batteryAutoEco: true);
                    return;
                }
            }
            catch (Exception ex)
            {
                AppLogger.Log($"电源变化时应用电源模式出错：{ex.Message}");
            }

            if (_gpuBatteryAuto && _gpuMode != 2)
            {
                int targetMode = pluggedIn ? 1 : 0;
                if (_gpuMode != targetMode)
                {
                    ApplyGpuMode(targetMode);
                }
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.StatusChange)
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(CheckPowerSourceTransition));
                }
                else
                {
                    CheckPowerSourceTransition();
                }
            }
            else if (e.Mode == PowerModes.Resume)
            {
                AppLogger.Log("系统已从睡眠恢复，正在重新应用当前性能配置。");
                byte curMode = GetActivePowerMode();
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() => ApplyPowerMode(curMode)));
                }
                else
                {
                    ApplyPowerMode(curMode);
                }
            }
        }

        private void MarkGpuMode(int mode)
        {
            _gpuMode = mode;
            buttonEnduranceMode.Activated = (mode == 0);
            buttonGpuStandardMode.Activated = (mode == 1);
            buttonGpuUltimateMode.Activated = (mode == 2);

            string labelText = mode switch
            {
                0 => "显卡模式：仅核显",
                1 => "显卡模式：混合输出",
                2 => "显卡模式：独显直连",
                _ => "显卡模式"
            };
            labelGpuMode.Text = labelText;
            labelGpuHint.Text = "";

            fansForm?.UpdateGpuOverclockAvailability();
        }

        private void ApplyHardwareGpuTransition(int mode)
        {
            if (mode == 0)
            {
                var control = new NvidiaGpuControl();
                if (control.IsValid)
                {
                    control.KillGPUApps();
                }
                NvidiaWrapper.SetGpuMode(0);
                NvidiaGpuControl.StopNVService();
                DeviceHelper.SetDeviceState("VEN_10DE", false);
            }
            else if (mode == 1)
            {
                DeviceHelper.SetDeviceState("VEN_10DE", true);
                NvidiaGpuControl.RestartNVService();
                NvidiaWrapper.SetGpuMode(1);

                System.Threading.Thread.Sleep(2500); // Let NV service fully restart before writing settings.

                // Re-apply GPU overclock for the active performance profile now that the dGPU is back.
                var profile = PreySense.Mode.ProfileManager.LoadProfile(_lastKnownProfile);
                if (profile.ApplyGpuLimits)
                {
                    AppLogger.Log($"ApplyHardwareGpuTransition: re-applying GPU offsets after iGPU→Standard (core={profile.GpuCoreOffset:+#;-#;0}MHz, mem={profile.GpuMemoryOffset:+#;-#;0}MHz).");
                    NvidiaWrapper.ApplyGpuSettings(profile.GpuCoreOffset, profile.GpuMemoryOffset);
                }
            }
        }

        public void ApplyGpuMode(int mode)
        {
            int oldMode = _gpuMode;
            if (oldMode == mode) return;

            if (mode == 2)
            {
                var result = ConfirmDialog.Show(this,
                    "切换到极致模式需要重启。\n重启前请保存所有打开的文件并关闭程序。",
                    "需要重启");
                if (result != DialogResult.Yes)
                {
                    MarkGpuMode(oldMode);
                    return;
                }

                _gpuMode = mode;
                SaveState("GPUMode", mode);
                MarkGpuMode(mode);

                if (!_wmi.SetGpuMuxMode(WmiController.MapUiGpuModeToMuxMode(mode)))
                {
                    AppLogger.Log("应用显卡模式：AcerService GPU_MODE（独显）失败，已取消极致模式切换。");
                    _gpuMode = oldMode;
                    SaveState("GPUMode", oldMode);
                    MarkGpuMode(oldMode);
                    return;
                }

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "shutdown",
                        Arguments = "/r /t 0",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
                catch (Exception ex)
                {
                    AppLogger.Log($"启动系统重启失败：{ex.Message}");
                }
                return;
            }

            if (oldMode == 2)
            {
                var result = ConfirmDialog.Show(this,
                    "退出极致模式需要重启。\n重启前请保存所有打开的文件并关闭程序。",
                    "需要重启");
                if (result != DialogResult.Yes)
                {
                    MarkGpuMode(2);
                    return;
                }

                _gpuMode = mode;
                SaveState("GPUMode", mode);
                MarkGpuMode(mode);

                if (!_wmi.SetGpuMuxMode(WmiController.MapUiGpuModeToMuxMode(mode)))
                {
                    AppLogger.Log("应用显卡模式：AcerService GPU_MODE（混合）失败，保持极致模式。");
                    MarkGpuMode(2);
                    return;
                }

                ApplyHardwareGpuTransition(mode);

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "shutdown",
                        Arguments = "/r /t 0",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
                catch (Exception ex)
                {
                    AppLogger.Log($"启动系统重启失败：{ex.Message}");
                }
                return;
            }

            _gpuMode = mode;
            _lastModeChangeTime = DateTime.Now;
            MarkGpuMode(mode);
            SaveState("GPUMode", mode);

            Task.Run(() =>
            {
                try
                {
                    ApplyHardwareGpuTransition(mode);
                }
                catch (Exception ex)
                {
                    AppLogger.Log($"应用显卡模式的后台执行失败：{ex.Message}");
                }
            });
        }

    }
}
