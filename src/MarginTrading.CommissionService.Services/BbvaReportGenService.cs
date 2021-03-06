// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common;
using jsreport.Client;
using jsreport.Types;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using Microsoft.AspNetCore.Hosting;

namespace MarginTrading.CommissionService.Services
{
    public class BbvaReportGenService : IReportGenService
    {
        private readonly IHostingEnvironment _environment;
        private readonly CommissionServiceSettings _serviceSettings;
        private string _assetsPath = Path.Combine("ReportAssets", "CostsAndCharges");
        private string _content;
        private string _layout;

        public BbvaReportGenService(IHostingEnvironment environment,
            CommissionServiceSettings serviceSettings)
        {
            _environment = environment;
            _serviceSettings = serviceSettings;

            var reportAssets = new Dictionary<string, string>()
            {
                {"benton-font", GetBase64Asset("BentonSansBBVA-Light.woff2")},
                {"bbva-logo", GetBase64Asset("bbva_logo.png")},
                {"bell", GetBase64Asset("bell.png")},
                {"contact", GetBase64Asset("contact.jpeg")},
            };

            _content = InjectAssets(GetAssetText("content.html"), reportAssets);
            _layout = InjectAssets(GetAssetText("layout.html"), reportAssets);
        }

        private string InjectAssets(string template, Dictionary<string, string> assets)
        {
            var sb = new StringBuilder(template);

            foreach (var asset in assets)
            {
                sb.Replace($"[[{asset.Key}]]", asset.Value);
            }

            return sb.ToString();
        }

        public async Task<byte[]> GenerateBafinCncReport(CostsAndChargesCalculation calculation)
        {
            return await GenerateBafinCncForOneCalc(calculation);
        }

        private async Task<byte[]> GenerateBafinCncForOneCalc(CostsAndChargesCalculation calculation)
        {
            var rs = new ReportingService(_serviceSettings.JSReportUrl);

            var report = await rs.RenderAsync(new RenderRequest
            {
                Template = new Template()
                {
                    Content = _content,
                    Engine = Engine.Handlebars,
                    Recipe = Recipe.ChromePdf,
                    Chrome = new Chrome
                    {
                        MarginTop = "3cm",
                        MarginLeft = "2cm",
                        MarginRight = "1.5cm",
                        MarginBottom = "2.5cm",
                    },
                    PdfOperations = new List<PdfOperation>()
                    {
                        new PdfOperation()
                        {
                            Type = PdfOperationType.Merge,
                            Template = new Template
                            {
                                Content = _layout,
                                Engine = Engine.Handlebars,
                                Recipe = Recipe.ChromePdf,
                                Chrome = new Chrome
                                {
                                    MarginLeft = "2cm",
                                    MarginRight = "1.5cm",
                                    MarginBottom = "1cm",
                                },
                            },
                            RenderForEveryPage = true,
                        }
                    }
                },
                Data = await GetData(calculation),
            });

            return await report.Content.ToBytesAsync();
        }

        private async Task<object> GetData(CostsAndChargesCalculation costsAndChargesCalculation)
        {
            return costsAndChargesCalculation;
        }

        private string GetBase64Asset(string asset)
        {
            var path = Path.Combine(_environment.ContentRootPath, _assetsPath, asset);
            var bytes = File.ReadAllBytes(path);
            var base64 = Convert.ToBase64String(bytes);

            return base64;
        }

        private string GetAssetText(string asset)
        {
            return File.ReadAllText(Path.Combine(_environment.ContentRootPath, _assetsPath, asset));
        }
    }
}