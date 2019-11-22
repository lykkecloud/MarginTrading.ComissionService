using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace MarginTrading.CommissionService.Core.Settings
{
    [UsedImplicitly]
    public class RequestLoggerSettings
    {
        public bool Enabled { get; set; }

        public bool EnabledForGet { get; set; }

        public int MaxPartSize { get; set; }
    }
}
