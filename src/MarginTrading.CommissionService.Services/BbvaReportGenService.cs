// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common;
using jsreport.Client;
using jsreport.Types;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;
using Microsoft.AspNetCore.Hosting;

namespace MarginTrading.CommissionService.Services
{
    public class BbvaReportGenService : IReportGenService
    {
        private readonly IHostingEnvironment _environment;
        private string _assetsPath = Path.Combine("ReportAssets", "CostsAndCharges");

        public BbvaReportGenService(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<byte[]> GenerateBafinCncReport(IEnumerable<CostsAndChargesCalculation> calculations)
        {
            var rs = new ReportingService("http://localhost:5488");

            var report = await rs.RenderAsync(new RenderRequest
            {
                Template = new Template()
                {
                    Content = GetAssetText("content.html"),
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
                                Content = GetAssetText("layout.html"),
                                Engine = Engine.Handlebars,
                                Recipe = Recipe.ChromePdf,
                                Helpers = GetAssetText("layout-helpers.js"),
                                Chrome = new Chrome
                                {
                                    MarginLeft = "2cm",
                                    MarginRight = "1.5cm",
                                    MarginBottom = "1cm",
                                },
                            }
                        }
                    }
                },
                Data = GetData(),
            });

            return await report.Content.ToBytesAsync();
        }

        private object GetData()
        {
            return new
            {
                instrument = "APPLE_INC"
            };
        }

        private string GetAsset(string asset)
        {
            return Path.Combine(_environment.ContentRootPath, _assetsPath, asset);
        }

        private string GetAssetText(string asset)
        {
            return File.ReadAllText(Path.Combine(_environment.ContentRootPath, _assetsPath, asset));
        }
    }
}