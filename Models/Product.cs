using System;
using System.Collections.Generic;

namespace ABCRetail.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductCategory { get; set; } = null!;

    public string? ProductDescription { get; set; }

    public string ProductName { get; set; } = null!;

    public int ProductStock { get; set; }

    public decimal ProductPrice { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
