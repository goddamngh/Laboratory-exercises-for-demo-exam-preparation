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
    
    public bool IsEditMode => _originalProduct != null; //для отображения кнопки удаления 

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
    private void Delete(Window window)
    {
        // 1. Проверяем, что это не режим добавления (нельзя удалить то, чего нет в БД)
        if (_originalProduct == null) return;

        // 2. Запрашиваем подтверждение
        var result = MessageBox.Show(
            $"Вы действительно хотите удалить товар '{Name}'?\nЭто действие нельзя отменить.",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            using var context = new BicycleShopDbContext();

            // 3. Проверяем, есть ли товар в заказах
            bool isInOrders = context.OrderProducts.Any(op => op.ProductId == _originalProduct.Id);
            if (isInOrders)
            {
                MessageBox.Show(
                    "Невозможно удалить товар, так как он присутствует в заказах.\nСначала удалите связанные заказы.",
                    "Ошибка удаления",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // 4. Находим товар в БД для удаления
            var productToDelete = context.Products.Find(_originalProduct.Id);
            if (productToDelete == null) return;

            // 5. Запоминаем имя картинки
            string photoName = productToDelete.Photo ?? "";

            // 6. Удаляем из БД
            context.Products.Remove(productToDelete);
            context.SaveChanges();

            // 7. Удаляем физический файл (из корня и из bin)
            FileService.DeleteImage(photoName);

            MessageBox.Show("Товар успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            // 8. Обновляем главный список и закрываем окно
            _onSaveSuccess?.Invoke();
            window.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Save(Window window)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            MessageBox.Show("Поле 'Наименование' не может быть пустым.\nПожалуйста, введите название товара.", 
                            "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (string.IsNullOrWhiteSpace(Description))
        {
            MessageBox.Show("Поле 'Описание' не может быть пустым.\nПожалуйста, добавьте описание товара.", 
                            "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (Price < 0)
        {
            MessageBox.Show("Цена не может быть отрицательной.\nПожалуйста, введите положительное число.", 
                            "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (QuantityInStock < 0)
        {
            MessageBox.Show("Количество на складе не может быть отрицательным.\nПожалуйста, введите положительное число.", 
                            "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (ValidDiscount < 0 || ValidDiscount > 100)
        {
            MessageBox.Show("Размер скидки должен быть в диапазоне от 0 до 100%.\nПожалуйста, исправьте значение.", 
                            "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (Article.Length > 6)
        {
            MessageBox.Show($"Артикул не может быть длиннее 6 символов.\nВаш артикул: {Article} (длина {Article.Length}).\nПожалуйста, сократите артикул.", 
                            "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (SelectedCategory == null || SelectedManufacturer == null || SelectedSupplier == null || SelectedUnit == null)
        {
            MessageBox.Show("Пожалуйста, выберите категорию, производителя, поставщика и единицу измерения из выпадающих списков.", 
                            "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    Price = this.Price,
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
                    productToUpdate.Price = this.Price;
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