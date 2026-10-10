using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MakrupProject.Data;
using Microsoft.EntityFrameworkCore;

namespace MarkupChallenge.UI;

public partial class MainWindow : Window
{
    private List<ProductRow> _allProducts = new();

    public MainWindow()
    {
        InitializeComponent();
        LoadProducts();
    }
    
    private void LoadProducts()
    {
        using var db = new MarkupChallengeDbContext();

        _allProducts = db.Products
            .Include(p => p.Name)
            .Include(p => p.Category)
            .Include(p => p.Manufacturer)
            .Include(p => p.Supplier)
            .OrderBy(p => p.Id)
            .Select(p => new ProductRow
            {
                Article          = p.Article ?? "",
                Title            = (p.Category != null ? p.Category.Name + " | " : "")
                                    + (p.Name != null ? p.Name.Name : ""),
                Description      = p.Description ?? "",
                ManufacturerName = p.Manufacturer != null ? p.Manufacturer.Name : "",
                SupplierName     = p.Supplier != null ? p.Supplier.Name : "",
                Price            = p.Price ?? 0,
                Unit             = p.Unit ?? "",
                Count            = p.Count ?? 0,
                Discount         = p.Discount ?? 0
            })
            .ToList();

        ApplyFilter();
    }
    
    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = SearchBox.Text?.Trim() ?? "";

        var filtered = string.IsNullOrEmpty(query)
            ? _allProducts
            : _allProducts
                .Where(p =>
                    p.Article.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    p.ManufacturerName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    p.SupplierName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

        ProductsList.ItemsSource = filtered;
    }
    
}

public class ProductRow
{
    public string Article { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string ManufacturerName { get; set; } = "";
    public string SupplierName { get; set; } = "";
    public double Price { get; set; }
    public string Unit { get; set; } = "";
    public double Count { get; set; }
    public double Discount { get; set; }

    public string DescriptionText => $"Описание: {Description}";
    public string ManufacturerText => $"Производитель: {ManufacturerName}";
    public string SupplierText => $"Поставщик: {SupplierName}";
    public string CountText => $"Количество на складе: {Count}";
    
    public string OldPriceText => $"Цена: {Price:N0}";
    public string NewPriceText => Discount > 0
        ? $"{Price * (1 - Discount / 100):N0}"
        : "";
    
    public bool IsHighlighted => Discount > 10;
}