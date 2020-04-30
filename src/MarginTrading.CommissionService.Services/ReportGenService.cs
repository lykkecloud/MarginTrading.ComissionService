// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text.pdf;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;


namespace MarginTrading.CommissionService.Services
{
    public class ReportGenService : IReportGenService
    {
        private readonly IAssetsCache _assetsCache;
        private readonly string _fontPath;
        private readonly string _timeZonePartOfTheName;

        public ReportGenService(IAssetsCache assetsCache, string fontPath, string timeZonePartOfTheName)
        {
            _assetsCache = assetsCache;
            _fontPath = fontPath;
            _timeZonePartOfTheName = timeZonePartOfTheName;
        }

        public byte[] GenerateBafinCncReport(IEnumerable<CostsAndChargesCalculation> calculations)
        {
            var reportsQueue = new Queue<byte[]>(calculations.Select(GenerateBafinCncForOneCalc));

            var result = reportsQueue.Dequeue();
            while (reportsQueue.Count > 0)
            {
                result = MergeTwoPdfs(result, reportsQueue.Dequeue());
            }

            return result;
        }

        public byte[] GenerateBafinCncForOneCalc(CostsAndChargesCalculation calculation)
        {
            var assetName = _assetsCache.GetName(calculation.Instrument);
            var time = ConvertToReportTimeZone(calculation.Timestamp);
            var accountPrefix = !string.IsNullOrEmpty(calculation.AccountId) ? calculation.AccountId + " - " : "";

            return GenerateOnePart(new[]
                {
                    new BafinCncReportData("Aufstellung der Kostenpositionen".ToUpper(), "EUR", "%"),
                    new BafinCncReportData($"Die Kostenpositionen werden dargestellt als einmalige Ein- und Ausstiegskosten und laufende Kosten für die Haltedauer bis zum nächsten Handelstag. Dabei wird in Produkt-, Dienstleistungs- und Fremdwährungskosten untergliedert. Alle Werte sind in Euro und als prozentualer Anteil am Anlagebetrag von 5000 EUR angegeben.", "", ""),
                    new BafinCncReportData("", "", ""),
                    new BafinCncReportData("Einstiegskosten (einmalig):", calculation.EntrySum),
                    new BafinCncReportData("Produktkosten", calculation.EntryCost),
                    new BafinCncReportData("Dienstleistungskosten", calculation.EntryCommission),
                    new BafinCncReportData("..davon Zuwendungen an Consorsbank", calculation.EntryConsorsDonation),
                    new BafinCncReportData("Fremdwährungskosten", calculation.EntryForeignCurrencyCosts),
                    new BafinCncReportData("", "", ""),

                    new BafinCncReportData("Laufende Kosten (bis zum nächsten Handelstag):", calculation.RunningCostsSum),
                    new BafinCncReportData("Produktkosten", calculation.RunningCostsProductReturnsSum),
                    new BafinCncReportData("..Overnight-Kosten", calculation.OvernightCost),
                    new BafinCncReportData("..Referenzzinsbetrag", calculation.ReferenceRateAmount),
                    new BafinCncReportData("..Leihekosten ", calculation.RepoCost),
                    new BafinCncReportData("Dienstleistungskosten", calculation.RunningCommissions),
                    new BafinCncReportData("..davon Zuwendungen an Consorsbank", calculation.RunningCostsConsorsDonation),
                    new BafinCncReportData("Fremdwährungskosten", calculation.RunningCostsForeignCurrencyCosts),
                    new BafinCncReportData("", "", ""),

                    new BafinCncReportData("Ausstiegskosten (einmalig):", calculation.ExitSum),
                    new BafinCncReportData("Produktkosten", calculation.ExitCost),
                    new BafinCncReportData("Dienstleistungskosten", calculation.ExitCommission),
                    new BafinCncReportData("..davon Zuwendungen an Consorsbank", calculation.ExitConsorsDonation),
                    new BafinCncReportData("Fremdwährungskosten", calculation.ExitForeignCurrencyCosts),
                    new BafinCncReportData("", "", ""),

                    new BafinCncReportData("-----------------------", "", ""),

                    new BafinCncReportData("Zusammenfassung der Kosten für eine Haltedauer bis zum nächsten Handelstag".ToUpper(), "EUR", "%"),
                    new BafinCncReportData("Die Gesamtkosten werden als Summe von Produkt-, Dienstleistungs und Fremdwährungskosten für eine angenommene Haltedauer von einem Tag gebildet.", "", ""),
                    new BafinCncReportData("", "", ""),
                    new BafinCncReportData("Produktkosten:", calculation.ProductsReturn),
                    new BafinCncReportData("Dienstleistungskosten", calculation.ServiceCost),
                    new BafinCncReportData("..davon Zuwendungen an Consorsbank", calculation.ProductsReturnConsorsDonation),
                    new BafinCncReportData("Fremdwährungskosten", calculation.ProductsReturnForeignCurrencyCosts),
                    new BafinCncReportData("", "", ""),
                    new BafinCncReportData("Gesamtkosten".ToUpper(), calculation.TotalCosts),
                    new BafinCncReportData("", "", ""),

                    new BafinCncReportData("-----------------------", "", ""),

                    new BafinCncReportData("Auswirkung der Kosten auf die Wertentwicklung".ToUpper(), "EUR", "%"),
                    new BafinCncReportData($"Die Renditeauswirkungen der Kosten für die Anlagesumme von 5000 EUR werden im Folgenden für einen Tag dargestellt. Die einzelnen Kosten reduzieren die individuelle Wertentwicklung einer Anlage. Bei der angenommenen Haltedauer von einem Tag, machen sich vor allem Ein- und Ausstiegskosten bemerkbar. Diese fallen bei jeder Transaktion in gleicher Höhe an. Damit führt eine hohe Handelsaktivität zu höheren Ein- und Ausstiegskosten. Eine längere Haltedauer der Position erhöht die laufenden Kosten für jeden Tag in gleicher Höhe.", "", ""),
                    new BafinCncReportData("", "", ""),
                    new BafinCncReportData("1 Tag", calculation.OneTag),
                }, $"{accountPrefix}{assetName} - {calculation.Direction.ToString()} - {time:F}");
        }

        private byte[] GenerateOnePart<T>(IReadOnlyCollection<T> data, string header)
            where T : class
        {
            return new PdfReport().DocumentPreferences(doc =>
            {
                doc.RunDirection(PdfRunDirection.LeftToRight);
                doc.Orientation(PageOrientation.Portrait);
                doc.PageSize(PdfPageSize.A4);
                doc.DocumentMetadata(new DocumentMetadata
                {
                    Author = "Lykke Business",
                    Application = nameof(CommissionService),
                    Keywords = "Lykke BNP Paribas",
                    Subject = "ExAnte",
                    Title = "ExAnte"
                });
                doc.Compression(new CompressionSettings
                {
                    EnableCompression = true,
                    EnableFullCompression = true
                });
                doc.PrintingPreferences(new PrintingPreferences
                {
                    ShowPrintDialogAutomatically = false
                });
            })
                .DefaultFonts(fonts =>
                {
                    fonts.Path(Path.Combine(_fontPath, "OpenSans-Regular.ttf"),
                        Path.Combine(_fontPath, "Raleway-Regular.ttf"));
                    fonts.Size(9);
                    fonts.Color(System.Drawing.Color.Black);
                })
                .MainTableTemplate(template => { template.BasicTemplate(BasicTemplate.ClassicTemplate); })
                .MainTablePreferences(table => { table.ColumnsWidthsType(TableColumnWidthType.Relative); })
                .MainTableDataSource(dataSource => { dataSource.StronglyTypedList(data); })
                .MainTableColumns(columns =>
                {
                    columns.AddColumn(column =>
                    {
                        column.PropertyName(nameof(BafinCncReportData.Name));
                        column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                        column.IsVisible(true);
                        column.Order(0);
                        column.Width(6);
                        column.HeaderCell(header, mergeHeaderCell: true);
                    });
                    columns.AddColumn(column =>
                    {
                        column.PropertyName(nameof(BafinCncReportData.Eur));
                        column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                        column.IsVisible(true);
                        column.Order(1);
                        column.Width(1);
                        column.HeaderCell("", mergeHeaderCell: true);
                    });
                    columns.AddColumn(column =>
                    {
                        column.PropertyName(nameof(BafinCncReportData.Percent));
                        column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                        column.IsVisible(true);
                        column.Order(2);
                        column.Width(1);
                    });
                })
                .GenerateAsByteArray();
        }

        private DateTime ConvertToReportTimeZone(DateTime sourceDt)
        {
            var timeZoneFromSettings = string.IsNullOrWhiteSpace(_timeZonePartOfTheName)
                ? null
                : TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(x => x.Id.Contains(_timeZonePartOfTheName));

            if (timeZoneFromSettings == null)
            {
                if (sourceDt.Kind == DateTimeKind.Utc)
                {
                    return sourceDt;
                }

                return TimeZoneInfo.ConvertTime(sourceDt, TimeZoneInfo.Utc);
            }

            return TimeZoneInfo.ConvertTime(sourceDt, timeZoneFromSettings);
        }

        private byte[] MergeTwoPdfs(byte[] pdf1, byte[] pdf2)
        {
            var finalStream = new MemoryStream();
            var copy = new PdfCopyFields(finalStream);

            var ms1 = new MemoryStream(pdf1) { Position = 0 };
            copy.AddDocument(new PdfReader(ms1));
            ms1.Dispose();

            var ms2 = new MemoryStream(pdf2) { Position = 0 };
            copy.AddDocument(new PdfReader(ms2));
            ms2.Dispose();
            copy.Close();

            return finalStream.GetBuffer();
        }
    }
}
