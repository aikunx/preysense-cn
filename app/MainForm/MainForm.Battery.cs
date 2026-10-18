using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using PreySense.Battery;

namespace PreySense
{
    public partial class MainForm
    {
        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint CallNtPowerInformation(
            int InformationLevel,
            IntPtr InputBuffer,
            uint InputBufferLength,
            IntPtr OutputBuffer,
            uint OutputBufferLength);

        private const int SystemBatteryStateInformation = 5;

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_BATTERY_STATE
        {
            [MarshalAs(UnmanagedType.U1)] public bool AcOnLine;
            [MarshalAs(UnmanagedType.U1)] public bool BatteryPresent;
            [MarshalAs(UnmanagedType.U1)] public bool Charging;
            [MarshalAs(UnmanagedType.U1)] public bool Discharging;
            public byte Spare1;
            public byte Spare2;
            public byte Spare3;
            public byte Spare4;
            public uint MaxCapacity;
            public uint RemainingCapacity;
            public int Rate;
            public uint EstimatedTime;
            public uint DefaultAlert1;
            public uint DefaultAlert2;
        }

        private unsafe string GetBatteryRateString()
        {
            try
            {
                SYSTEM_BATTERY_STATE state = default;
                uint status = CallNtPowerInformation(
                    SystemBatteryStateInformation,
                    IntPtr.Zero,
                    0,
                    (IntPtr)(&state),
                    (uint)sizeof(SYSTEM_BATTERY_STATE));

                if (status != 0)
                    return "充电：--%";

                if (!state.BatteryPresent)
                    return "充电：--%";

                int rate = state.Rate;
                if (rate > 0)
                    return $"充电中：{rate / 1000.0:F1}W";

                if (!state.AcOnLine && rate < 0)
                    return $"放电中：{Math.Abs(rate) / 1000.0:F1}W";

                if (state.AcOnLine && rate == 0)
                    return "已接通电源";

                return "充电：--%";
            }
            catch
            {
                return "充电：--%";
            }
        }

        public void ApplyBatteryMode(int mode)
        {
            BatteryControl.SetBatteryLimit(_wmi, mode);
            UpdateBatteryLimitUi(mode);
            RefreshBatteryRateLabel();
            _ = RefreshBatteryRateLabelSoonAsync();
        }

        public void UpdateBatteryHighlight(int mode)
        {
            if (sliderBatteryChargeLimit == null) return;

            _isApplyingSavedBatteryLimit = true;
            try
            {
                sliderBatteryChargeLimit.Value = mode == 1 ? 80 : 100;
                labelBatteryStatusLimitTitle.Text = $"电池充电上限：{sliderBatteryChargeLimit.Value}%";
                UpdateBatteryLimitButtonFromValue(sliderBatteryChargeLimit.Value);
            }
            finally
            {
                _isApplyingSavedBatteryLimit = false;
            }
        }

        private void UpdateBatteryLimitButtonFromValue(int value)
        {
            bool batteryLimitEnabled = value == 80;
            buttonBatteryFull.Activated = batteryLimitEnabled;
            buttonBatteryFull.BackColor = batteryLimitEnabled ? colorStandard : buttonSecond;
            buttonBatteryFull.ForeColor = batteryLimitEnabled ? SystemColors.ControlLightLight : SystemColors.ControlDark;
            buttonBatteryFull.Text = batteryLimitEnabled ? "80%" : "100%";
        }

        public void UpdateCurrentCharge()
        {
            RefreshBatteryRateLabel();
        }

        private void RefreshBatteryRateLabel()
        {
            if (!_showBatteryTelemetry) return;
            if (labelBatteryStatus == null || labelBatteryStatus.IsDisposed) return;
            labelBatteryStatus.Text = GetBatteryRateString();
        }

        private async Task RefreshBatteryRateLabelSoonAsync()
        {
            await Task.Delay(250);
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefreshBatteryRateLabel));
                return;
            }

            RefreshBatteryRateLabel();
        }

        private void SetBatteryTelemetryVisible(bool visible)
        {
            _showBatteryTelemetry = visible;
            if (visible)
            {
                RefreshBatteryRateLabel();
            }
        }

        private void UpdateBatteryLimitUi(int mode)
        {
            Program.settingsForm?.UpdateBatteryHighlight(mode);
            UpdateBatteryLimitButtonFromValue(mode == 1 ? 80 : 100);
        }

        private void ApplySavedBatteryLimitAsync()
        {
            Task.Run(() =>
            {
                int mode = BatteryControl.GetBatteryLimit();
                int sliderValue = mode == 1 ? 80 : 100;

                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (IsDisposed) return;

                        _isApplyingSavedBatteryLimit = true;
                        try
                        {
                            if (sliderBatteryChargeLimit.Value != sliderValue)
                            {
                                sliderBatteryChargeLimit.Value = sliderValue;
                            }

                            labelBatteryStatusLimitTitle.Text = $"电池充电上限：{sliderBatteryChargeLimit.Value}%";
                            UpdateBatteryLimitButtonFromValue(sliderBatteryChargeLimit.Value);
                        }
                        finally
                        {
                            _isApplyingSavedBatteryLimit = false;
                        }
                    }));
                }
                catch
                {
                    return;
                }

                BatteryControl.ApplyBatteryLimit(_wmi, mode);

                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (!IsDisposed) UpdateBatteryLimitUi(mode);
                    }));
                }
                catch { }
            });
        }
    }
}
