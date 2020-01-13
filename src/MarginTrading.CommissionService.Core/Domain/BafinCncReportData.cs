// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain
{
    public class BafinCncReportData
    {
        public BafinCncReportData(string name, params string[] eurAndPercent)
        {
            Name = name;
            Eur = eurAndPercent[0];
            Percent = eurAndPercent[1];
        }

        public BafinCncReportData(string name, CostsAndChargesValue data)
        {
            Name = name;
            Eur = data == null ? "" : $"{data.ValueInEur}";
            Percent = data == null ? "" : $"{data.ValueInPercent}%";
        }

        public string Name { get; set; }

        public string Eur { get; set; }

        public string Percent { get; set; }
    }
}
