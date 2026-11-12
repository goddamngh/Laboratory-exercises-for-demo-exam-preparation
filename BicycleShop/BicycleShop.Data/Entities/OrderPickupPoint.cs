using System;
using System.Collections.Generic;

namespace BicycleShop.Data.Entities;

public partial class OrderPickupPoint
{
    public int Id { get; set; }

    public string Address { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
