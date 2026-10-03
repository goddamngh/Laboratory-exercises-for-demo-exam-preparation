using System;
using System.Collections.Generic;

namespace MakrupProject.Data.Entities;

public partial class Supplier
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
