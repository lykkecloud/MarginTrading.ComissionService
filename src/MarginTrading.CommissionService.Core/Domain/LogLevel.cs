using System;

namespace MarginTrading.CommissionService.Core.Domain
{
    /// <summary>Specifies log level</summary>
    [Flags]
    public enum LogLevel
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Error = 4,
        FatalError = 8,
        Monitoring = 16, // 0x00000010
        All = Monitoring | FatalError | Error | Warning | Info, // 0x0000001F
    }
}