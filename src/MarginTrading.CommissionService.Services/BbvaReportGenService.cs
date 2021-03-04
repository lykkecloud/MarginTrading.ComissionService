// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Common;
using iTextSharp.text.pdf;
using jsreport.Client;
using jsreport.Types;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using Microsoft.AspNetCore.Hosting;

namespace MarginTrading.CommissionService.Services
{
    public class BbvaReportGenService : IReportGenService
    {
        private readonly IHostingEnvironment _environment;
        private readonly IKidScenariosService _kidScenariosService;
        private readonly IProductsCache _productsCache;
        private readonly CommissionServiceSettings _serviceSettings;
        private string _assetsPath = Path.Combine("ReportAssets", "CostsAndCharges");

        public BbvaReportGenService(IHostingEnvironment environment, 
            IKidScenariosService kidScenariosService,
            IProductsCache productsCache,
            CommissionServiceSettings serviceSettings)
        {
            _environment = environment;
            _kidScenariosService = kidScenariosService;
            _productsCache = productsCache;
            _serviceSettings = serviceSettings;
        }

        public async Task<byte[]> GenerateBafinCncReport(IEnumerable<CostsAndChargesCalculation> calculations)
        {
            var pdfs = await Task.WhenAll(calculations.Select(async c => await GenerateBafinCncForOneCalc(c)));
            var reportsQueue = new Queue<byte[]>(pdfs);

            var result = reportsQueue.Dequeue();
            while (reportsQueue.Count > 0)
            {
                result = MergeTwoPdfs(result, reportsQueue.Dequeue());
            }

            return result;
        }

        private async Task<byte[]> GenerateBafinCncForOneCalc(CostsAndChargesCalculation calculation)
        {
            var rs = new ReportingService(_serviceSettings.JSReportUrl);

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
                            },
                        }
                    }
                },
                Data = await GetData(calculation),
            });

            return await report.Content.ToBytesAsync();
        }

        private async Task<object> GetData(CostsAndChargesCalculation costsAndChargesCalculation)
        {
            var product = _productsCache.GetById(costsAndChargesCalculation.Instrument);
            var isin = costsAndChargesCalculation.Direction == OrderDirection.Buy ? product.IsinLong : product.IsinShort;
            var kidScenario = await _kidScenariosService.GetByIdAsync(isin);
            if (kidScenario.IsFailed
                || !kidScenario.Value.KidModerateScenario.HasValue
                || !kidScenario.Value.KidModerateScenarioAvreturn.HasValue)
                throw new Exception(
                    $"KID scenario not found or null for isin {isin} and calculation {costsAndChargesCalculation.Id}");

            var theoreticalNetReturn = kidScenario.Value.KidModerateScenario.Value +
                                   costsAndChargesCalculation.TotalCosts.ValueInEur;
            
            return new
            {
                Data = costsAndChargesCalculation,
                EntryExitCommission = new CostsAndChargesValue(
                    costsAndChargesCalculation.EntryCommission.ValueInEur +
                    costsAndChargesCalculation.ExitCommission.ValueInEur,
                    costsAndChargesCalculation.EntryCommission.ValueInPercent +
                    costsAndChargesCalculation.ExitCommission.ValueInPercent),
                EntryExitCost = new CostsAndChargesValue(
                    costsAndChargesCalculation.EntryCost.ValueInEur + costsAndChargesCalculation.ExitCost.ValueInEur,
                    costsAndChargesCalculation.EntryCost.ValueInPercent +
                    costsAndChargesCalculation.ExitCost.ValueInPercent),
                KidScenario = new CostsAndChargesValue(kidScenario.Value.KidModerateScenario.Value,
                    kidScenario.Value.KidModerateScenarioAvreturn.Value),
                TheoreticalNetReturn = new CostsAndChargesValue(Math.Round(theoreticalNetReturn, 2), Math.Round(theoreticalNetReturn / costsAndChargesCalculation.Volume, 2))
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

        private byte[] MergeTwoPdfs(byte[] pdf1, byte[] pdf2)
        {
            var finalStream = new MemoryStream();
            var copy = new PdfCopyFields(finalStream);

            var ms1 = new MemoryStream(pdf1) {Position = 0};
            copy.AddDocument(new PdfReader(ms1));
            ms1.Dispose();

            var ms2 = new MemoryStream(pdf2) {Position = 0};
            copy.AddDocument(new PdfReader(ms2));
            ms2.Dispose();
            copy.Close();

            return finalStream.GetBuffer();
        }
    }
}