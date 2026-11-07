using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using MakrupProject.Data;
using MakrupProject.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarkupChallenge.UI;

public partial class ProductEditorWindow : Window
{
    private readonly bool _isEdit;
    private readonly int _productId;

    public ProductEditorWindow()
    {
        InitializeComponent();
        _isEdit = false;
        IdLabel.Visibility = Visibility.Collapsed;
        IdBox.Visibility = Visibility.Collapsed;
        DeleteButton.Visibility = Visibility.Collapsed;
        InitializeCombos();
        PriceBox.Text = "0";
        CountBox.Text = "0";
        DiscountBox.Text = "0";
    }

    public ProductEditorWindow(int productId)
    {
        InitializeComponent();
        _isEdit = true;
        _productId = productId;
        InitializeCombos();
        LoadProduct(productId);
    }

    private void InitializeCombos()
    {
        using var db = new MarkupChallengeDbContext();

        CategoryCombo.SelectedValuePath = "Id";
        ManufacturerCombo.SelectedValuePath = "Id";
        SupplierCombo.SelectedValuePath = "Id";

        CategoryCombo.ItemsSource     = db.Categories.OrderBy(c => c.Name).ToList();
        ManufacturerCombo.ItemsSource = db.Manufacturers.OrderBy(m => m.Name).ToList();
        SupplierCombo.ItemsSource     = db.Suppliers.OrderBy(s => s.Name).ToList();
    }

    private void LoadProduct(int id)
    {
        using var db = new MarkupChallengeDbContext();
        var product = db.Products.Include(p => p.Name).FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            MessageBox.Show("Товар не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }

        IdBox.Text          = product.Id.ToString();
        NameBox.Text        = product.Name?.Name ?? "";
        DescriptionBox.Text = product.Description ?? "";
        PriceBox.Text       = (product.Price ?? 0).ToString(CultureInfo.InvariantCulture);
        CountBox.Text       = (product.Count ?? 0).ToString(CultureInfo.InvariantCulture);
        DiscountBox.Text    = (product.Discount ?? 0).ToString(CultureInfo.InvariantCulture);

        CategoryCombo.SelectedValue     = product.CategoryId;
        ManufacturerCombo.SelectedValue = product.ManufacturerId;
        SupplierCombo.SelectedValue     = product.SupplierId;

        var inOrders = db.OrderArticles.Any(oa => oa.ProductId == id);
        DeleteButton.IsEnabled = !inOrders;
        if (inOrders)
            DeleteButton.ToolTip = "Товар присутствует в заказе и не может быть удалён.";
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!ValidateInput(out var price, out var count, out var discount)) return;

        var name = NameBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show("Введите наименование товара.", "Ошибка валидации",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var db = new MarkupChallengeDbContext();

        Product product;
        if (_isEdit)
        {
            var existing = db.Products.FirstOrDefault(p => p.Id == _productId);
            if (existing == null)
            {
                MessageBox.Show("Товар не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            product = existing;
        }
        else
        {
            var nextId = (db.Products.Max(p => (int?)p.Id) ?? 0) + 1;
            product = new Product { Id = nextId };
            db.Products.Add(product);
        }

        var productName = db.ProductNames.FirstOrDefault(n => n.Name == name);
        if (productName == null)
        {
            var nextNameId = (db.ProductNames.Max(n => (int?)n.Id) ?? 0) + 1;
            productName = new ProductName { Id = nextNameId, Name = name };
            db.ProductNames.Add(productName);
        }

        product.Name         = productName;
        product.NameId       = productName.Id;
        product.CategoryId   = CategoryCombo.SelectedValue as int?;
        product.ManufacturerId = ManufacturerCombo.SelectedValue as int?;
        product.SupplierId   = SupplierCombo.SelectedValue as int?;
        product.Description  = DescriptionBox.Text.Trim();
        product.Price        = (double)price;
        product.Count        = (double)count;
        product.Discount     = (double)discount;

        db.SaveChanges();

        DialogResult = true;
        Close();
    }

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!_isEdit) return;

        var confirm = MessageBox.Show(
            "Вы уверены, что хотите удалить товар?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        using var db = new MarkupChallengeDbContext();

        if (db.OrderArticles.Any(oa => oa.ProductId == _productId))
        {
            MessageBox.Show(
                "Товар присутствует в заказе и не может быть удалён.",
                "Удаление запрещено", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var product = db.Products.FirstOrDefault(p => p.Id == _productId);
        if (product == null) return;

        db.Products.Remove(product);
        db.SaveChanges();

        DialogResult = true;
        Close();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private bool ValidateInput(out decimal price, out decimal count, out decimal discount)
    {
        price = count = discount = 0;

        if (!TryParseDecimal(PriceBox.Text, out price) || price < 0)
        {
            MessageBox.Show("Цена должна быть неотрицательным числом.",
                "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        if (decimal.Round(price, 2) != price)
        {
            MessageBox.Show("Цена может содержать не более двух знаков после запятой.",
                "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        if (!TryParseDecimal(CountBox.Text, out count) || count < 0)
        {
            MessageBox.Show("Количество должно быть неотрицательным числом.",
                "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        if (!TryParseDecimal(DiscountBox.Text, out discount) || discount < 0 || discount > 100)
        {
            MessageBox.Show("Скидка должна быть числом от 0 до 100.",
                "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }

    private static bool TryParseDecimal(string? text, out decimal value)
    {
        text = (text ?? "").Trim().Replace(',', '.');
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }
}