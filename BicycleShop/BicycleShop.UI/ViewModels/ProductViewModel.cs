using BicycleShop.Data.Entities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BicycleShop.UI.ViewModels;

public class ProductViewModel: ObservableObject
{
    private readonly Product _product;

    public ProductViewModel(Product product)
    {
        _product = product;
    }
    
    
    public int Id => _product.Id;
    public string Article => _product.Article;
    public string Name => _product.Name ?? "Без названия";
    public string Description => _product.Description  ?? "Нет описания";
    public string CategoryName => _product.Category?.CategoryName ?? "Не указана";
    public string ManufacturerName => _product.Manufacturer?.ManufacturerName ?? "Не указан";
    public string SupplierName => _product.Supplier?.SupplierName ?? "Не указан";
    public string UnitName => _product.Unit?.UnitName ?? "Не указано";
    public int QuantityInStock => _product.QuantityInStock;
    public int ValidDiscount => _product.ValidDiscount;
    public string DiscountDisplay => $"{ValidDiscount}%";
    public bool IsHighDiscount => ValidDiscount > 15;
    public decimal Price => _product.Price;
    
    public decimal FinalPrice => _product.Price - (_product.Price * (decimal)_product.ValidDiscount / 100);
    
    public bool HasDiscount => _product.ValidDiscount > 0;
    
    public bool IsOutOfStock => _product.QuantityInStock == 0;

    public string PhotoPath
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_product.Photo)) 
                return "/Images/picture.png"; 
            return $"/Images/{_product.Photo}";
        }
    }

}