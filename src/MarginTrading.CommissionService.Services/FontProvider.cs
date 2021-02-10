// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.IO;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class FontProvider : IFontProvider
    {
        private readonly string _fontPath;

        public FontProvider(string fontPath)
        {
            _fontPath = fontPath;
        }

        public string OpenSansRegular => GetFont("OpenSans", "Regular");

        public string OpenSansBold => GetFont("OpenSans", "Bold");

        public string RalewayRegular => GetFont("Raleway", "Regular");

        public string RalewayBold => GetFont("Raleway", "Bold");

        private string GetFont(string family, string style)
        {
            return Path.Combine(_fontPath, $"{family}-{style}.ttf");
        }
    }
}