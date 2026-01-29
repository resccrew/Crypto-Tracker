using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using Desktop_Crypto_Portfolio_Tracker.ViewModels;
using System.Linq;
using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Desktop_Crypto_Portfolio_Tracker.Views
{
    public partial class PortfolioView : UserControl
    {
        public PortfolioView()
        {
            InitializeComponent();
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private async void OnAddTransactionClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                var availableCoins = viewModel.MarketCoins.ToList();
                var dialog = new AddTransactionWindow(availableCoins);
                var topLevel = TopLevel.GetTopLevel(this) as Window;
                
                if (topLevel != null)
                {
                    var result = await dialog.ShowDialog<PortfolioDisplayItem>(topLevel);
                    
                    if (result != null)
                    {
                        var db = new DatabaseService();
                        long newDbId = await db.AddTransactionAsync(1, result.CoinId ?? "", "Buy", (double)result.Amount, (double)result.Price);
                        
                        if (newDbId > 0)
                        {
                            result.DbId = newDbId;
                            
                            var existingItem = viewModel.MyPortfolio.FirstOrDefault(x => x.Name == result.Name);

                            if (existingItem != null)
                            {

                                decimal totalCostOld = existingItem.Price * existingItem.Amount;
                                decimal totalCostNew = result.Price * result.Amount;
                                decimal newTotalAmount = existingItem.Amount + result.Amount;

                                if (newTotalAmount > 0)
                                {
                                    existingItem.Price = (totalCostOld + totalCostNew) / newTotalAmount;
                                }

                                existingItem.Amount = newTotalAmount;
                            }
                            else
                            {
                                viewModel.MyPortfolio.Add(result);
                            }

                            viewModel.RecalculateBalance();
                        }
                    }
                }
            }
        }

        private async void OnDeleteClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && 
                button.DataContext is PortfolioDisplayItem itemToDelete &&
                DataContext is MainWindowViewModel viewModel)
            {
                var db = new DatabaseService();
                if (await db.DeleteTransactionAsync(itemToDelete.DbId))
                {
                    viewModel.MyPortfolio.Remove(itemToDelete);
                    viewModel.RecalculateBalance();
                }
            }
        }

        private async void OnPrintClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel viewModel) return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Portfolio PDF",
                DefaultExtension = "pdf",
                SuggestedFileName = $"Portfolio_Report_{DateTime.Now:yyyy-MM-dd}",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("PDF Document") { Patterns = new[] { "*.pdf" } }
                }
            });

            if (file is not null)
            {
                string filePath = file.Path.LocalPath;
                GeneratePdf(filePath, viewModel);
            }
        }

        private void GeneratePdf(string filePath, MainWindowViewModel viewModel)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("Crypto Portfolio Report")
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().Text($"Date: {DateTime.Now:g}");
                            x.Item().Text($"Total Balance: ${viewModel.TotalBalance:N2}").Bold().FontSize(16).FontColor(Colors.Green.Medium);
                            
                            x.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(); 
                                    columns.ConstantColumn(80); 
                                    columns.ConstantColumn(80); 
                                    columns.ConstantColumn(100); 
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Asset");
                                    header.Cell().Element(CellStyle).Text("Price");
                                    header.Cell().Element(CellStyle).Text("Amount");
                                    header.Cell().Element(CellStyle).Text("Total Value");

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                    }
                                });

                                foreach (var item in viewModel.MyPortfolio)
                                {
                                    table.Cell().Element(CellStyle).Text(item.Name ?? "Unknown");
                                    table.Cell().Element(CellStyle).Text($"${item.Price:N2}");
                                    table.Cell().Element(CellStyle).Text($"{item.Amount}");
                                    table.Cell().Element(CellStyle).Text($"${item.TotalValue:N2}");

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                    }
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            })
            .GeneratePdf(filePath);
        }
    }
}