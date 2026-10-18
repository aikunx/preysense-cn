using System.Runtime.Versioning;

namespace PreySense.Helpers
{
    /// <summary>
    /// Predator Sense keyboard RGB modes and AcerService effect mapping.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class RgbProfile
    {
        public const int ModeCount = 37;

        public const int WaveModeIndex = 3;

        public const int StepCount = 5;

        public static readonly string[] UiModeNames =
        {
            // 前 8 项保持原有顺序，兼容旧配置
            "静态", "呼吸", "霓虹", "波浪", "涟漪", "缩放", "蛇形", "迪斯科",
            // 以下为官方 AcerService 支持但原版未暴露的效果
            "蛇形流动", "波纹", "雨滴", "闪电", "聚光", "星光", "火球", "飘雪",
            "心跳", "音乐律动", "屏幕取色", "彩虹", "斜切", "爆破", "堆叠", "流动光点",
            "炫彩", "矩阵", "彩虹蛇", "缩放进入", "动态灯效", "横扫", "竞速", "生长",
            "圆环", "光效秀", "脉冲演示", "横向波浪"
        };

        /// <summary>
        /// AcerService EffectNames index for each UI mode (0..7).
        /// Matches Predator Sense KB_Zone_Default: STATIC, BREATHING, NEON, WAVE,
        /// SHIFTING (UI: Ripple), ZOOM, METEOR (UI: Snake), TWINKLING (UI: Disco).
        /// </summary>
        public static readonly int[] ModeToServiceEffectIndex =
        {
            0, 1, 5, 2, 13, 14, 15, 16,          // 原有 8 项
            3, 4, 6, 7, 8, 9, 10, 11, 12,         // SNAKE, RIPPLE, RAIN_DROP, LIGHTNING, SPOT, STAR, FIREBALL, SNOW, HEARTBEAT
            17, 18, 19, 20, 21, 22, 23, 24, 25,   // MUSIC, SCREEN, RAINBOW, SLASH, BLASTING, STACK, MOTION_POINT, DAZZLING, MATRIX
            26, 27, 28, 29, 30, 31, 32, 33, 34,   // SNAKE_RAINBOW, ZOOM_IN, DYNAMIC_LIGHTING, SWIPING, RACING, SPROUTING, DISCO, PINGPONG, LIGHT_SHOW
            35, 36                                // PULSAR_DEMO, ROWWAVE
        };

        public static int GetServiceEffectIndex(int mode) =>
            mode is >= 0 and < ModeCount ? ModeToServiceEffectIndex[mode] : 0;

        public static string GetServiceEffectName(int mode, string[] serviceEffectNames) =>
            serviceEffectNames[GetServiceEffectIndex(mode)];

        /// <summary>Normalize saved/UI level to AcerService scale (1–5).</summary>
        public static int NormalizeStepLevel(int saved) =>
            saved switch
            {
                > 10 => Math.Clamp((saved + 10) / 20, 1, StepCount),
                > StepCount => Math.Clamp((saved + 1) / 2, 1, StepCount),
                _ => Math.Clamp(saved, 1, StepCount)
            };

        public static int ScaleBrightnessToService(int brightness) =>
            NormalizeStepLevel(brightness);

        /// <summary>Predator Sense subindex zone 2 fallback for device 0 lighting.</summary>
        public static string GetSubindexSecondary(string effect) => effect switch
        {
            "WAVE" => "NEON",
            "SHIFTING" => "STATIC",
            "ZOOM" => "STATIC",
            "METEOR" => "STATIC",
            "TWINKLING" => "BREATHING",
            _ => effect
        };

        /// <summary>Map legacy EffectNames indices saved before mode-index fix.</summary>
        public static int NormalizeSavedMode(int mode, int version = 1)
        {
            if (version >= 2)
            {
                if (mode is >= 0 and < ModeCount)
                    return mode;

                return mode switch
                {
                    5 => 2,   // NEON
                    14 => 5,  // ZOOM
                    32 => 7,  // DISCO
                    _ => 0
                };
            }

            return mode switch
            {
                0 => 0,
                1 => 1,
                2 => 3,   // WAVE
                3 => 6,   // SNAKE
                4 => 4,
                5 => 2,   // NEON
                14 => 5,  // ZOOM
                32 => 7,  // DISCO
                >= 0 and < ModeCount => mode,
                _ => 0
            };
        }
    }
}
