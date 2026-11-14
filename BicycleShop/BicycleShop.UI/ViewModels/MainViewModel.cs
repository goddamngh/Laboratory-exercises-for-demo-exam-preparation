using System.Collections.ObjectModel;
using System.Linq;
using BicycleShop.Data.Context;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore; // Обязательно для Include

namespace BicycleShop.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<ProductViewModel> Products { get; } = new();

    public MainViewModel()
    {
        LoadData();
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
}