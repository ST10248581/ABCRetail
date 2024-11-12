using System;
using System.Collections.Generic;

namespace ABCRetail.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string CustomerFullName { get; set; } = null!;

    public string CustomerEmail { get; set; } = null!;

    public string CustomerPhoneNumber { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
