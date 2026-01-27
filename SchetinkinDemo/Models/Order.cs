using System;
using System.Collections.Generic;

namespace SchetinkinDemo.Models;

public partial class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string? ShippingAddress { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}
