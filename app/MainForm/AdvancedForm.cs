using System;
using System.Drawing;
using System.Windows.Forms;
using PreySense.Gpu;
using PreySense.Helpers;
using PreySense.Mode;
using PreySense.UI;
using PreySense.UI.Controls;

namespace PreySense
{
    /// <summary>
    /// 高级硬件控制：CPU 功耗/加速、GPU 超频、风扇手动转速、LCD 超频。
    /// 这些能力此前只在后端实现，未在主界面暴露。
    /// </summary>
    public class AdvancedForm : RForm
    {
        private readonly WmiController _wmi;
        private readonly NvidiaGpuControl? _gpu;
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 2000 };

        private TrackBar _cpuBoost = null!;
        private NumericUpDown _pl1 = null!;
        private NumericUpDown _pl2 = null!;
        private NumericUpDown _coreOff = null!;
        private NumericUpDown _memOff = null!;
        private TrackBar _fanCpu = null!;
        private TrackBar _fanGpu = null!;
        private RCheckBox _lcdOverdrive = null!;
        private RCheckBox _winKeyLock = null!, _stickyKey = null!, _bootSound = null!;
        private ComboBox _panelDfr = null!, _soundMode = null!;
        private Label _cpuInfo = null!;
        private Label _gpuInfo = null!;
        private Label _fanInfo = null!;

        public AdvancedForm(WmiController wmi)
        {
            _wmi = wmi;
            try { _gpu = new NvidiaGpuControl(); } catch { _gpu = null; }

            InitTheme(true);
            Text = "高级硬件控制";
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = true;
            ClientSize = new Size(600, 660);
            BackColor = formBack;
            ForeColor = foreMain;
            ShowIcon = false;

            var fontLabel = new Font("Microsoft YaHei UI", 9F);
            var fontHeader = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(12)
            };
            Controls.Add(flow);

            // ---------------- CPU ----------------
            var cpuCard = new SectionCardControl { SectionTitle = "CPU 控制", Width = 552, Height = 168, Margin = new Padding(0, 0, 0, 10) };
            var cpuTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 4, Padding = new Padding(10, 26, 10, 8) };
            cpuTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            cpuTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            cpuTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));

            _cpuBoost = new TrackBar { Minimum = 0, Maximum = 3, TickStyle = TickStyle.None, Width = 300 };
            _pl1 = new NumericUpDown { Minimum = 5, Maximum = 200, Value = 45, Width = 90 };
            _pl2 = new NumericUpDown { Minimum = 5, Maximum = 250, Value = 65, Width = 90 };

            cpuTable.Controls.Add(MakeLabel("CPU 加速", fontLabel), 0, 0);
            cpuTable.Controls.Add(_cpuBoost, 1, 0);
            cpuTable.Controls.Add(MakeValueLabel("0=禁用 3=激进", fontLabel), 2, 0);
            cpuTable.Controls.Add(MakeLabel("PL1 功耗 (W)", fontLabel), 0, 1);
            cpuTable.Controls.Add(_pl1, 1, 1);
            cpuTable.Controls.Add(MakeValueLabel("长效功耗", fontLabel), 2, 1);
            cpuTable.Controls.Add(MakeLabel("PL2 功耗 (W)", fontLabel), 0, 2);
            cpuTable.Controls.Add(_pl2, 1, 2);
            cpuTable.Controls.Add(MakeValueLabel("短时功耗", fontLabel), 2, 2);

            var applyCpu = new RButton { Text = "应用 CPU 设置", Width = 160, Height = 30, BorderRadius = 6 };
            applyCpu.Click += (s, e) => ApplyCpu();
            _cpuInfo = MakeValueLabel("", fontLabel);
            cpuTable.Controls.Add(applyCpu, 1, 3);
            cpuTable.Controls.Add(_cpuInfo, 2, 3);
            cpuCard.Controls.Add(cpuTable);
            flow.Controls.Add(cpuCard);

            // ---------------- GPU ----------------
            var gpuCard = new SectionCardControl { SectionTitle = "GPU 控制", Width = 552, Height = 158, Margin = new Padding(0, 0, 0, 10) };
            var gpuTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 4, Padding = new Padding(10, 26, 10, 8) };
            gpuTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            gpuTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            gpuTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));

            _coreOff = new NumericUpDown { Minimum = -200, Maximum = 200, Value = 0, Width = 90 };
            _memOff = new NumericUpDown { Minimum = -500, Maximum = 1500, Value = 0, Width = 90 };

            gpuTable.Controls.Add(MakeLabel("核心偏移", fontLabel), 0, 0);
            gpuTable.Controls.Add(_coreOff, 1, 0);
            gpuTable.Controls.Add(MakeValueLabel("MHz", fontLabel), 2, 0);
            gpuTable.Controls.Add(MakeLabel("显存偏移", fontLabel), 0, 1);
            gpuTable.Controls.Add(_memOff, 1, 1);
            gpuTable.Controls.Add(MakeValueLabel("MHz", fontLabel), 2, 1);

            var applyGpu = new RButton { Text = "应用 GPU 超频", Width = 160, Height = 30, BorderRadius = 6 };
            applyGpu.Click += (s, e) => ApplyGpu();
            _gpuInfo = MakeValueLabel("", fontLabel);
            gpuTable.Controls.Add(applyGpu, 1, 2);
            gpuTable.Controls.Add(_gpuInfo, 2, 2);
            gpuCard.Controls.Add(gpuTable);
            flow.Controls.Add(gpuCard);

            // ---------------- FAN ----------------
            var fanCard = new SectionCardControl { SectionTitle = "风扇控制", Width = 552, Height = 158, Margin = new Padding(0, 0, 0, 10) };
            var fanTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3, Padding = new Padding(10, 26, 10, 8) };
            fanTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            fanTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            fanTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));

            _fanCpu = new TrackBar { Minimum = 20, Maximum = 100, Value = 50, TickStyle = TickStyle.None, Width = 300 };
            _fanGpu = new TrackBar { Minimum = 20, Maximum = 100, Value = 50, TickStyle = TickStyle.None, Width = 300 };

            fanTable.Controls.Add(MakeLabel("CPU 风扇", fontLabel), 0, 0);
            fanTable.Controls.Add(_fanCpu, 1, 0);
            fanTable.Controls.Add(MakeValueLabel("%", fontLabel), 2, 0);
            fanTable.Controls.Add(MakeLabel("GPU 风扇", fontLabel), 0, 1);
            fanTable.Controls.Add(_fanGpu, 1, 1);
            fanTable.Controls.Add(MakeValueLabel("%", fontLabel), 2, 1);

            var applyFan = new RButton { Text = "应用风扇转速", Width = 160, Height = 30, BorderRadius = 6 };
            applyFan.Click += (s, e) => ApplyFan();
            _fanInfo = MakeValueLabel("", fontLabel);
            fanTable.Controls.Add(applyFan, 1, 2);
            fanTable.Controls.Add(_fanInfo, 2, 2);
            fanCard.Controls.Add(fanTable);
            flow.Controls.Add(fanCard);

            // ---------------- SERVICE ----------------
            var svcCard = new SectionCardControl { SectionTitle = "显示与服务（官方服务接口）", Width = 552, Height = 230, Margin = new Padding(0, 0, 0, 10) };
            var svcTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, Padding = new Padding(10, 26, 10, 8) };
            svcTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            svcTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

            _lcdOverdrive = new RCheckBox { Text = "LCD 超频", AutoSize = true };
            _lcdOverdrive.CheckedChanged += (s, e) => SafeSvc(() => _wmi.ServiceClient?.SetLcdOverdrive(_lcdOverdrive.Checked));

            _winKeyLock = new RCheckBox { Text = "锁定 Win 键", AutoSize = true };
            _winKeyLock.CheckedChanged += (s, e) => SafeSvc(() => _wmi.ServiceClient?.SetWinKeyLock(_winKeyLock.Checked));

            _stickyKey = new RCheckBox { Text = "粘滞键", AutoSize = true };
            _stickyKey.CheckedChanged += (s, e) => SafeSvc(() => _wmi.ServiceClient?.SetStickyKey(_stickyKey.Checked));

            _bootSound = new RCheckBox { Text = "开机音", AutoSize = true };
            _bootSound.CheckedChanged += (s, e) => SafeSvc(() => _wmi.ServiceClient?.SetBootSound(_bootSound.Checked));

            svcTable.Controls.Add(_lcdOverdrive, 0, 0);
            svcTable.Controls.Add(_winKeyLock, 0, 1);
            svcTable.Controls.Add(_stickyKey, 0, 2);
            svcTable.Controls.Add(_bootSound, 0, 3);

            _panelDfr = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
            _panelDfr.Items.AddRange(new object[] { "屏幕防撕裂：关", "屏幕防撕裂：开", "屏幕防撕裂：自动" });
            _panelDfr.SelectedIndex = 0;
            _panelDfr.SelectedIndexChanged += (s, e) => SafeSvc(() => _wmi.ServiceClient?.SetPanelDfrMode(_panelDfr.SelectedIndex));

            _soundMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
            _soundMode.Items.AddRange(new object[] { "音效：标准", "音效：音乐", "音效：电影", "音效：游戏" });
            _soundMode.SelectedIndex = 0;
            _soundMode.SelectedIndexChanged += (s, e) => SafeSvc(() => _wmi.ServiceClient?.SetSoundMode(_soundMode.SelectedIndex));

            svcTable.Controls.Add(MakeLabel("屏幕", fontLabel), 1, 4);
            svcTable.Controls.Add(_panelDfr, 1, 5);
            svcTable.Controls.Add(MakeLabel("音效", fontLabel), 0, 4);
            svcTable.Controls.Add(_soundMode, 0, 5);
            svcCard.Controls.Add(svcTable);
            flow.Controls.Add(svcCard);

            _timer.Tick += (s, e) => RefreshInfo();
            _timer.Start();
            RefreshInfo();
        }

        private void SafeSvc(Action action)
        {
            try { action(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "服务调用失败", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private static Label MakeLabel(string text, Font font) => new()
        {
            Text = text, Font = font, AutoSize = true, Anchor = AnchorStyles.Left,
            ForeColor = Color.FromArgb(200, 200, 200)
        };

        private static Label MakeValueLabel(string text, Font font) => new()
        {
            Text = text, Font = font, AutoSize = true, Anchor = AnchorStyles.Left,
            ForeColor = Color.FromArgb(160, 160, 160)
        };

        private void ApplyCpu()
        {
            try
            {
                PowerLimitController.SetCpuBoost(_cpuBoost.Value);
                PowerLimitController.SetCpuPowerLimits((int)_pl1.Value, (int)_pl2.Value, true);
                _cpuInfo.Text = "已应用";
            }
            catch (Exception ex) { _cpuInfo.Text = "失败"; MessageBox.Show(ex.Message, "CPU", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void ApplyGpu()
        {
            if (_gpu == null || !_gpu.IsValid) { _gpuInfo.Text = "无 NVIDIA GPU"; return; }
            try
            {
                _gpu.GetClocks(out int baseCore, out int baseMem, false);
                _gpu.SetClocks(baseCore + (int)_coreOff.Value, baseMem + (int)_memOff.Value);
                _gpuInfo.Text = "已应用";
            }
            catch (Exception ex) { _gpuInfo.Text = "失败"; MessageBox.Show(ex.Message, "GPU", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void ApplyFan()
        {
            try
            {
                _wmi.SetCpuFanSpeed(_fanCpu.Value);
                _wmi.SetGpuFanSpeed(_fanGpu.Value);
                _fanInfo.Text = "已应用";
            }
            catch (Exception ex) { _fanInfo.Text = "失败"; MessageBox.Show(ex.Message, "风扇", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void RefreshInfo()
        {
            try
            {
                int cpuFan = _wmi.GetCpuFanSpeed();
                int gpuFan = _wmi.GetGpuFanSpeed();
                _fanInfo.Text = "CPU（处理器）" + cpuFan + " / GPU " + gpuFan;
            }
            catch { }

            try
            {
                if (_gpu != null && _gpu.IsValid)
                {
                    _gpuInfo.Text = _gpu.GetCurrentTemperature() + "°C  " + Math.Round(_gpu.GetGpuPower() ?? 0f, 1) + "W";
                }
            }
            catch { }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Stop();
            _gpu?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
