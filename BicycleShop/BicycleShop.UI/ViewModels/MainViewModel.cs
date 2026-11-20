using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using BicycleShop.Core.Session;
using BicycleShop.Data.Context;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace BicycleShop.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<ProductViewModel> Products { get; } = new();
    public ICollectionView ProductsView { get; }
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private string _selectedSort = "Без сортировки";
    
    [ObservableProperty]
    private string _selectedFilter = "Все диапазоны";
    
    public string[] SortOptions { get; } = 
    { 
        "Без сортировки", 
        "Количество (по возрастанию)", 
        "Количество (по убыванию)", 
        "Цена (по возрастанию)", 
        "Цена (по убыванию)", 
        "Скидка (по возрастанию)", 
        "Скидка (по убыванию)" 
    };

    public string[] FilterOptions { get; } = 
    { 
        "Все диапазоны", 
        "0 - 11,99%", 
        "12 - 18,99%", 
        "19% и более" 
    };
    
    public bool IsAdminOrManager => UserSession.RoleName == "Администратор" || UserSession.RoleName == "Менеджер";
    public bool IsAdmin => UserSession.RoleName == "Администратор";

    public MainViewModel()
    {
        LoadData();
        
        ProductsView = CollectionViewSource.GetDefaultView(Products);
        ProductsView.Filter = FilterProducts;
        ApplySorting();
    }

    private void LoadData()
    {
        using var context = new BicycleShopDbContext();
        var productsFromDb = context.Products
            .Include(p => p.Category)
            .Include(p => p.Manufacturer)
            .Include(p => p.Supplier)
            .Include(p => p.Unit)
            .ToList();

        foreach (var product in productsFromDb)
        {
            Products.Add(new ProductViewModel(product));
        }
    }
    
    partial void OnSearchTextChanged(string value) => ProductsView.Refresh();
    partial void OnSelectedFilterChanged(string value) => ProductsView.Refresh();
    
    partial void OnSelectedSortChanged(string value) => ApplySorting();

    private bool FilterProducts(object obj)
    {
        if (obj is not ProductViewModel product) return false;
        
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string search = SearchText.ToLower();
            bool matchesSearch = 
                (product.Name?.ToLower().Contains(search) ?? false) ||
                (product.Description?.ToLower().Contains(search) ?? false) ||
                (product.CategoryName?.ToLower().Contains(search) ?? false) ||
                (product.ManufacturerName?.ToLower().Contains(search) ?? false) ||
                (product.SupplierName?.ToLower().Contains(search) ?? false) ||
                (product.Article?.ToLower().Contains(search) ?? false);

            if (!matchesSearch) return false;
        }
        
        if (SelectedFilter != "Все диапазоны")
        {
            int discount = product.ValidDiscount;
            switch (SelectedFilter)
            {
                case "0 - 11,99%":
                    if (discount < 0 || discount > 11) return false;
                    break;
                case "12 - 18,99%":
                    if (discount < 12 || discount > 18) return false;
                    break;
                case "19% и более":
                    if (discount < 19) return false;
                    break;
            }
        }

        return true;
    }

    private void ApplySorting()
    {
        ProductsView.SortDescriptions.Clear();

        if (SelectedSort == "Без сортировки") return;

        string propertyName = SelectedSort switch
        {
            "Количество (по возрастанию)" or "Количество (по убыванию)" => nameof(ProductViewModel.QuantityInStock),
            "Цена (по возрастанию)" or "Цена (по убыванию)" => nameof(ProductViewModel.FinalPrice),
            "Скидка (по возрастанию)" or "Скидка (по убыванию)" => nameof(ProductViewModel.ValidDiscount),
            _ => nameof(ProductViewModel.Name)
        };

        ListSortDirection direction = SelectedSort.Contains("убыванию") 
            ? ListSortDirection.Descending 
            : ListSortDirection.Ascending;

        ProductsView.SortDescriptions.Add(new SortDescription(propertyName, direction));
    }
}