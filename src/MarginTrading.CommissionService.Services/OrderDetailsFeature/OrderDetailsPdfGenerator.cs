// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using MarginTrading.CommissionService.Core.Domain.OrderDetailFeature;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Services.OrderDetailsFeature;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper.HtmlToPdf;
using PdfRpt.FluentInterface;

namespace MarginTrading.CommissionService.Services.OrderDetailsFeature
{
    public class OrderDetailsPdfGenerator : IOrderDetailsPdfGenerator
    {
        private readonly IFontProvider _fontProvider;
        private string _header;
        private string _footer;

        public OrderDetailsPdfGenerator(IFontProvider fontProvider)
        {
            _fontProvider = fontProvider;
            var assetsPath = "./ReportAssets/OrderDetails";
            _header = File.ReadAllText(Path.Combine(assetsPath, "header.html"));
            _footer = File.ReadAllText(Path.Combine(assetsPath, "footer.html"));
        }

        public byte[] GenerateReport(IReadOnlyCollection<OrderDetailsReportRow> data, ReportProperties properties)
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
                .MainTableDataSource(dataSource => { dataSource.StronglyTypedList(data); })
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
                        providerBuilder.AddPageHeader(headerData => string.Format(_header, 
                            properties.AccountId,
                            properties.OrderId
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
                        if (properties.IncludeManualConfirmationFooter) events.DocumentClosing(args => { AddFooter(args.PdfDoc, args.PdfFont); });
                    }
                )
                .GenerateAsByteArray();
        }

        private void AddFooter(Document argsPdfDoc, IPdfFont eventsPdfFont)
        {
            var table = new PdfGrid(1)
            {
                RunDirection = (int) PdfRunDirection.LeftToRight,
                WidthPercentage = 100,
                SpacingBefore = 25,
            };

            var htmlCell = new HtmlWorkerHelper
            {
                PdfFont = eventsPdfFont,
                HorizontalAlignment = HorizontalAlignment.Left,
                Html = _footer,
                RunDirection = PdfRunDirection.LeftToRight,
            }.RenderHtml();
            htmlCell.Border = 0;
            table.AddCell(htmlCell);

            argsPdfDoc.Add(table);
        }
    }
}