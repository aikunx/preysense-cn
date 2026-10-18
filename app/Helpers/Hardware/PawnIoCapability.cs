using System.IO;
using System.Runtime.Versioning;
using PreySense.Mode;

namespace PreySense.Helpers;

[SupportedOSPlatform("windows")]
public sealed record PawnIoCapability
{
    public bool Installed { get; init; }
    public bool Accessible { get; init; }
    public bool AdminRequired { get; init; }
    public bool ModuleLoaded { get; init; }
    public bool CanReadMsr { get; init; }
    public bool CanWriteMsr { get; init; }
    public bool LockLikelyPresent { get; init; }
    public string StatusText { get; init; } = string.Empty;
    public string DetailText { get; init; } = string.Empty;

    public static PawnIoCapability Probe()
    {
        var report = new PawnIoCapability();

        try
        {
            if (!PawnIO.IntelMsr.IsPawnIoAvailable(out string reason))
            {
                return report with
                {
                    Installed = false,
                    Accessible = false,
                    AdminRequired = reason.Contains("Administrator", StringComparison.OrdinalIgnoreCase),
                    StatusText = "PawnIO 未安装",
                    DetailText = reason
                };
            }

            using var msr = new PawnIO.IntelMsr();
            string resourceName = typeof(PowerLimitController).Assembly.GetName().Name + ".IntelMSR.bin";
            using var moduleStream = typeof(PowerLimitController).Assembly.GetManifestResourceStream(resourceName);
            if (moduleStream == null)
            {
                return report with
                {
                    Installed = true,
                    Accessible = true,
                    StatusText = "PawnIO 已安装",
                    DetailText = $"找不到内嵌资源 '{resourceName}'。"
                };
            }

            using var ms = new MemoryStream();
            moduleStream.CopyTo(ms);

            if (!msr.TryInitialize(ms.ToArray(), out string initError))
            {
                return report with
                {
                    Installed = true,
                    Accessible = true,
                    AdminRequired = initError.Contains("Administrator", StringComparison.OrdinalIgnoreCase),
                    StatusText = "PawnIO 不可用",
                    DetailText = initError
                };
            }

            bool readOk = false;
            ulong unitRaw = 0;
            ulong currentVal = 0;
            if (msr.ReadMsr(0x606, out unitRaw) && msr.ReadMsr(0x610, out currentVal))
                readOk = true;
            bool writeOk = false;
            bool lockLikely = false;

            if (readOk)
            {
                writeOk = msr.WriteMsr(0x610, currentVal) && msr.ReadMsr(0x610, out ulong verifyVal) && verifyVal == currentVal;
                lockLikely = !writeOk;
            }

            return report with
            {
                Installed = true,
                Accessible = true,
                ModuleLoaded = true,
                CanReadMsr = readOk,
                CanWriteMsr = writeOk,
                LockLikelyPresent = lockLikely,
                StatusText = writeOk ? "PawnIO 就绪" : readOk ? "MSR 只读" : "PawnIO 部分可用",
                DetailText = writeOk
                    ? "CPU 功耗调节可用"
                    : readOk
                        ? "MSR 可读，但写入被阻止或锁定"
                        : "CPU MSR 访问不可用"
            };
        }
        catch (Exception ex)
        {
            return report with
            {
                StatusText = "PawnIO 检查失败",
                DetailText = ex.Message
            };
        }
    }
}
