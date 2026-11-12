using System;
using System.Collections.Generic;

namespace BicycleShop.Data.Entities;

public partial class Product
{
    public int Id { get; set; }

    public string Article { get; set; } = null!;

    public string? Name { get; set; }

    public int? UnitId { get; set; }

    public int Price { get; set; }

    public int? SupplierId { get; set; }

    public int? ManufacturerId { get; set; }

    public int? CategoryId { get; set; }

    public int ValidDiscount { get; set; }

    public int QuantityInStock { get; set; }

    public string Description { get; set; } = null!;

    public string? Photo { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Manufacturer? Manufacturer { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual Supplier? Supplier { get; set; }

    public virtual Unit? Unit { get; set; }
}
