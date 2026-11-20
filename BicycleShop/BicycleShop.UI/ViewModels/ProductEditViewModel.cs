using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using BicycleShop.Core.Services;
using BicycleShop.Data.Context;
using BicycleShop.Data.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BicycleShop.UI.ViewModels;

public partial class ProductEditViewModel : ObservableObject
{
    private readonly Product? _originalProduct; // null = добавление, не null = редактирование
    private readonly Action _onSaveSuccess;
    private FileInfo? _selectedImage;

    public List<Category> Categories { get; }
    public List<Manufacturer> Manufacturers { get; }
    public List<Supplier> Suppliers { get; }
    public List<Unit> Units { get; }

    [ObservableProperty] private string _title;
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private decimal _price;
    [ObservableProperty] private int _quantityInStock;
    [ObservableProperty] private int _validDiscount;
    [ObservableProperty] private string _article = "";
    
    [ObservableProperty] private Category? _selectedCategory;
    [ObservableProperty] private Manufacturer? _selectedManufacturer;
    [ObservableProperty] private Supplier? _selectedSupplier;
    [ObservableProperty] private Unit? _selectedUnit;

    [ObservableProperty] private string _currentImagePath = ""; // Отображаемое имя файла

    public ProductEditViewModel(Product? product, Action onSaveSuccess)
    {
        _originalProduct = product;
        _onSaveSuccess = onSaveSuccess;

        using var context = new BicycleShopDbContext();
        Categories = context.Categories.ToList();
        Manufacturers = context.Manufacturers.ToList();
        Suppliers = context.Suppliers.ToList();
        Units = context.Units.ToList();

        if (product == null)
        {
            // Режим добавления
            Title = "Добавление товара";
            Id = context.Products.Any() ? context.Products.Max(p => p.Id) + 1 : 1;
            Article = $"ART{Id:D3}"; // Временный артикул
        }
        else
        {
            // Режим редактирования
            Title = "Редактирование товара";
            Id = product.Id;
            Name = product.Name ?? "";
            Description = product.Description ?? "";
            Price = product.Price;
            QuantityInStock = product.QuantityInStock;
            ValidDiscount = product.ValidDiscount;
            Article = product.Article;
            CurrentImagePath = product.Photo ?? "";
            
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == product.CategoryId);
            SelectedManufacturer = Manufacturers.FirstOrDefault(m => m.Id == product.ManufacturerId);
            SelectedSupplier = Suppliers.FirstOrDefault(s => s.Id == product.SupplierId);
            SelectedUnit = Units.FirstOrDefault(u => u.Id == product.UnitId);
        }
    }

    // Вызывается из Code-Behind при выборе нового фото
    public void SetImage(FileInfo file)
    {
        _selectedImage = file;
        // Для предпросмотра берем имя файла, но конвертер будет искать его в bin. 
        // Чтобы показать локальный файл, мы можем использовать полный путь в конвертере (он умеет).
        CurrentImagePath = file.FullName; 
    }

    [RelayCommand]
    private void Save(Window window)
    {
        // Валидация
        if (string.IsNullOrWhiteSpace(Name))
        {
            MessageBox.Show("Введите наименование товара.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        if (Price < 0)
        {
            MessageBox.Show("Цена не может быть отрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        if (QuantityInStock < 0)
        {
            MessageBox.Show("Количество не может быть отрицательным.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            using var context = new BicycleShopDbContext();
            string newPhotoName = _originalProduct?.Photo ?? "";

            // Обработка фото
            if (_selectedImage != null)
            {
                // Удаляем старое фото
                if (!string.IsNullOrWhiteSpace(newPhotoName))
                {
                    FileService.DeleteImage(newPhotoName);
                }

                // Сохраняем новое фото
                newPhotoName = $"{Id}{_selectedImage.Extension}";
                FileService.SaveImage(_selectedImage, newPhotoName);
            }

            if (_originalProduct == null)
            {
                // Добавление
                var newProduct = new Product
                {
                    Id = this.Id,
                    Article = this.Article,
                    Name = this.Name,
                    Description = this.Description,
                    Price = (int)this.Price,
                    QuantityInStock = this.QuantityInStock,
                    ValidDiscount = this.ValidDiscount,
                    CategoryId = SelectedCategory?.Id,
                    ManufacturerId = SelectedManufacturer?.Id,
                    SupplierId = SelectedSupplier?.Id,
                    UnitId = SelectedUnit?.Id,
                    Photo = newPhotoName
                };
                context.Products.Add(newProduct);
            }
            else
            {
                // Редактирование
                var productToUpdate = context.Products.Find(_originalProduct.Id);
                if (productToUpdate != null)
                {
                    productToUpdate.Name = this.Name;
                    productToUpdate.Description = this.Description;
                    productToUpdate.Price = (int)this.Price;
                    productToUpdate.QuantityInStock = this.QuantityInStock;
                    productToUpdate.ValidDiscount = this.ValidDiscount;
                    productToUpdate.CategoryId = SelectedCategory?.Id;
                    productToUpdate.ManufacturerId = SelectedManufacturer?.Id;
                    productToUpdate.SupplierId = SelectedSupplier?.Id;
                    productToUpdate.UnitId = SelectedUnit?.Id;
                    productToUpdate.Photo = newPhotoName;
                }
            }

            context.SaveChanges();
            
            MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            _onSaveSuccess?.Invoke();
            window.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Cancel(Window window) => window.Close();
}