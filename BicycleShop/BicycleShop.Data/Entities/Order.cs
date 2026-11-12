using System;
using System.Collections.Generic;

namespace BicycleShop.Data.Entities;

public partial class Order
{
    public int Id { get; set; }

    public int OrderNumber { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public int CodeForObtainig { get; set; }

    public int? OrderStatusId { get; set; }

    public int? OrderPickupPointId { get; set; }

    public int? UserId { get; set; }

    public virtual OrderPickupPoint? OrderPickupPoint { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual OrderStatus? OrderStatus { get; set; }

    public virtual User? User { get; set; }
}
