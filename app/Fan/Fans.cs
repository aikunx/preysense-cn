using System;
using System.Drawing;
using System.Runtime.Versioning;
using System.Windows.Forms;
using PreySense.Helpers;
using PreySense.Mode;
using PreySense.UI;

namespace PreySense.Fan
{
    [SupportedOSPlatform("windows")]
    public partial class Fans : RForm
    {
        private readonly MainForm _mainForm;
        private readonly WmiController _wmi;

        private PointF[] _cpuCurve = Array.Empty<PointF>();
        private PointF[] _gpuCurve = Array.Empty<PointF>();
        private System.Windows.Forms.Timer _telemetryTimer = null!;
        private PerformanceProfile _currentProfile = null!;
        private bool _isUpdatingUi = false;
        private FanCurveControl _curveCpu = null!;
        private FanCurveControl _curveGpu = null!;
        private SectionCardControl _cpuCurveCard = null!;
        private SectionCardControl _gpuCurveCard = null!;
        private Panel _curveHost = null!;
        private Panel _curveFrame = null!;
        private RCheckBox checkApplyGpuLimits = null!;
        private RCheckBox checkMaxFans = null!;
        private RButton _buttonSaveSettings = null!;
        private RButton _buttonResetDefaults = null!;
        private RButton _buttonApplySettings = null!;
        private RNumericUpDown numPl1 = null!;
        private RNumericUpDown numPl2 = null!;
        private bool _enforcingPlOrder;
        private RComboBox comboPerfMode = null!;
        private RComboBox comboCpuBoost = null!;
        private Label labelCpuBoostTitle = null!;
        private Panel _perfModeHost = null!;
        private Panel _powerModeHost = null!;
        private byte _activeMode;
        private byte _editingMode;
        private RNumericUpDown numGpuCoreOffset = null!;
        private RNumericUpDown numGpuMemoryOffset = null!;
        private int _pl1MaxW = 200;
        private int _pl2MaxW = 200;
        private RNumericUpDown numFanRampUp = null!;
        private Label labelFanRampUp = null!;

        private static readonly byte[] PerfModes = { 0x06, 0x00, 0x01, 0x04, 0x05 };
        private static readonly string[] PerfModeNames = { "节能", "静音", "均衡", "性能", "加速" };

        public Fans(MainForm mainForm, WmiController wmi)
        {
            _mainForm = mainForm;
            _wmi = wmi;

            InitializeComponent();
            CreateMaxFanCheck();

            labelPowerModeTitle.Text = "Windows 电源模式";
            panelCpuLimitsGraph.Visible = false;
            panelGpuOffsetsSection.Visible = false;
            panelPl1.Visible = true;
            panelPl2.Visible = true;
            labelLeftPl1.Text = "PL1";
            labelLeftPl2.Text = "PL2";

            CreateCpuLimitEditors();
            trackGpuCoreOffset.Minimum = -1000;
            trackGpuCoreOffset.Maximum = 1000;
            trackGpuMemoryOffset.Minimum = -1000;
            trackGpuMemoryOffset.Maximum = 3000;
            CreateGpuRuntimeControls();



            _cpuCurve = FanCurveStorage.DefaultCurve();
            _gpuCurve = FanCurveStorage.DefaultCurve();
            CreateFanCurveCards();

            comboWindowsPowerMode.Items.Clear();
            comboWindowsPowerMode.Items.AddRange(new object[] { "最佳能效", "均衡", "最佳性能" });

            ConfigureFansLayout();

            LoadStates();
            WireEvents();
            InitTheme(true);
        }

        private static int IndexFromPerfMode(byte mode)
        {
            int i = Array.IndexOf(PerfModes, mode);
            if (i >= 0) return i;
            return Array.IndexOf(PerfModes, (byte)0x01);
        }



        public void RefreshCurves()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefreshCurves));
                return;
            }

            _curveCpu?.Invalidate();
            _curveGpu?.Invalidate();
            _cpuCurveCard?.Invalidate();
            _gpuCurveCard?.Invalidate();
        }
    }
}
