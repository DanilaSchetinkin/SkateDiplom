using System;
using System.Collections.Generic;

namespace SchetinkinDemo.Models;

/// <summary>
/// Персональная корзина пользователя.
/// </summary>
public partial class Cart
{
    public int Id { get; set; }

    /// <summary>
    /// У одного пользователя - одна постоянная корзина.
    /// </summary>
    public int UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User User { get; set; } = null!;
}
