// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using MarginTrading.CommissionService.Core.Domain.OrderDetailFeature;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Services.OrderDetailsFeature;
using Microsoft.AspNetCore.Hosting;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper.HtmlToPdf;
using PdfRpt.FluentInterface;

namespace MarginTrading.CommissionService.Services.OrderDetailsFeature
{
    public class OrderDetailsPdfGenerator : IOrderDetailsPdfGenerator
    {
        private readonly IFontProvider _fontProvider;
        private readonly IHostingEnvironment _environment;
        private string _assetsPath = Path.Combine("ReportAssets", "OrderDetails");

        public OrderDetailsPdfGenerator(IFontProvider fontProvider, IHostingEnvironment environment)
        {
            _fontProvider = fontProvider;
            _environment = environment;
        }

        public byte[] GenerateReport(IReadOnlyCollection<OrderDetailsReportRow> rows, ReportProperties props)
        {
            return new PdfReport().DocumentPreferences(doc =>
                {
                    doc.RunDirection(PdfRunDirection.LeftToRight);
                    doc.Orientation(PageOrientation.Portrait);
                    doc.PageSize(PdfPageSize.A4);
                    doc.DocumentMetadata(new DocumentMetadata
                    {
                        Author = "BNP Paribas",
                        Application = nameof(CommissionService),
                        Keywords = "BNP Paribas",
                        Subject = "Order Details",
                        Title = "Order Details"
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
                    fonts.Path(_fontProvider.OpenSansRegular, _fontProvider.RalewayRegular);
                    fonts.Size(9);
                    fonts.Color(System.Drawing.Color.Black);
                })
                .MainTableTemplate(template => { template.BasicTemplate(BasicTemplate.ClassicTemplate); })
                .MainTablePreferences(table =>
                {
                    table.ColumnsWidthsType(TableColumnWidthType.Relative);
                    table.ShowHeaderRow(false);
                })
                .MainTableDataSource(dataSource => { dataSource.StronglyTypedList(rows); })
                .PagesHeader(builder =>
                {
                    builder.HtmlHeader(providerBuilder =>
                    {
                        providerBuilder.PageHeaderProperties(new HeaderBasicProperties()
                        {
                            RunDirection = PdfRunDirection.LeftToRight,
                            ShowBorder = false,
                            PdfFont = builder.PdfFont,
                            HorizontalAlignment = HorizontalAlignment.Left,
                        });
                        providerBuilder.AddPageHeader(headerData => string.Format(GetAssetText("header.html"),
                            GetAsset("bbva_logo.png")
                        ));
                    });
                })
                .MainTableColumns(columns =>
                    {
                        columns.AddColumn(column =>
                        {
                            column.PropertyName(
                                $"{nameof(OrderDetailsReportRow.LeftGroup)}.{nameof(OrderDetailsReportRow.LeftGroup.Name)}");
                            column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                            column.Font(fonts =>
                                {
                                    fonts.Path(_fontProvider.OpenSansBold, _fontProvider.RalewayBold);
                                    fonts.Size(9);
                                    fonts.Style(DocumentFontStyle.Italic);
                                    fonts.Color(System.Drawing.Color.Black);
                                }
                            );
                            column.IsVisible(true);
                            column.Order(0);
                            column.Width(3);
                        });

                        columns.AddColumn(column =>
                        {
                            column.PropertyName(
                                $"{nameof(OrderDetailsReportRow.LeftGroup)}.{nameof(OrderDetailsReportRow.LeftGroup.Value)}");
                            column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                            column.IsVisible(true);
                            column.Order(1);
                            column.Width(3);
                            column.PaddingRight(50);
                        });

                        columns.AddColumn(column =>
                        {
                            column.PropertyName(
                                $"{nameof(OrderDetailsReportRow.RightGroup)}.{nameof(OrderDetailsReportRow.LeftGroup.Name)}");
                            column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                            column.Font(fonts =>
                                {
                                    fonts.Path(_fontProvider.OpenSansBold, _fontProvider.RalewayBold);
                                    fonts.Size(9);
                                    fonts.Style(DocumentFontStyle.Italic);
                                    fonts.Color(System.Drawing.Color.Black);
                                }
                            );
                            column.IsVisible(true);
                            column.Order(2);
                            column.Width(3);
                            column.PaddingLeft(50);
                            ;
                        });

                        columns.AddColumn(column =>
                        {
                            column.PropertyName(
                                $"{nameof(OrderDetailsReportRow.RightGroup)}.{nameof(OrderDetailsReportRow.LeftGroup.Value)}");
                            column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                            column.IsVisible(true);
                            column.Order(3);
                            column.Width(3);
                        });
                    }
                )
                .MainTableEvents(events =>
                    {
                        events.MainTableCreated(args =>
                        {
                            AddHtml(args.PdfDoc, args.PdfFont, GetAssetText("heading.html"), HorizontalAlignment.Center,
                                props.OrderId);
                            AddHtml(args.PdfDoc, args.PdfFont, GetAssetText("account.html"), HorizontalAlignment.Left,
                                props.AccountName);
                        });


                        events.DocumentClosing(args =>
                        {
                            if (!props.EnableAllWarnings) return;
                            
                            if (props.EnableProductComplexityWarning)
                                AddHtml(args.PdfDoc, args.PdfFont, GetAssetText("handwritten_warning.html"),
                                    HorizontalAlignment.Left);

                            if (props.EnableTotalCostPercentWarning)
                                AddHtml(args.PdfDoc, args.PdfFont, GetAssetText("percent_warning.html"),
                                    HorizontalAlignment.Left,
                                    props.TotalCostPercentWarning);

                            if (props.EnableLossRatioWarning)
                                AddHtml(args.PdfDoc, args.PdfFont, GetAssetText("loss_ratio_warning.html"),
                                    HorizontalAlignment.Left,
                                    props.ProductName,
                                    props.LossRatioMin,
                                    props.LossRatioMax);

                            AddHtml(args.PdfDoc, args.PdfFont, GetAssetText("statement_warning.html"),
                                HorizontalAlignment.Left);
                            AddHtml(args.PdfDoc, args.PdfFont, GetAssetText("general_warning.html"),
                                HorizontalAlignment.Left);
                        });
                    }
                )
                .GenerateAsByteArray();
        }

        private string GetAsset(string asset)
        {
            return Path.Combine(_environment.ContentRootPath, _assetsPath, asset);
        }

        private string GetAssetText(string asset)
        {
            return File.ReadAllText(Path.Combine(_environment.ContentRootPath, _assetsPath, asset));
        }

        private void AddHtml(Document argsPdfDoc,
            IPdfFont eventsPdfFont,
            string html,
            HorizontalAlignment alignment,
            params object[] format)
        {
            var table = new PdfGrid(1)
            {
                RunDirection = (int) PdfRunDirection.LeftToRight,
                WidthPercentage = 100,
                SpacingBefore = 5,
                SpacingAfter = 5,
            };

            var htmlCell = new HtmlWorkerHelper
            {
                PdfFont = eventsPdfFont,
                HorizontalAlignment = alignment,
                Html = string.Format(html, format),
                RunDirection = PdfRunDirection.LeftToRight,
            }.RenderHtml();
            htmlCell.Border = 0;
            table.AddCell(htmlCell);

            argsPdfDoc.Add(table);
        }
    }
}