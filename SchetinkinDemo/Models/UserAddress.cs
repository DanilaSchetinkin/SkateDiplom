using System;
using System.Collections.Generic;

namespace SchetinkinDemo.Models;

/// <summary>
/// Адреса доставки, привязанные к пользователям.
/// </summary>
public partial class UserAddress
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? AddressName { get; set; }

    public bool IsDefault { get; set; }

    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Почтовый индекс
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Страна
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Город
    /// </summary>
    public string City { get; set; } = null!;

    /// <summary>
    /// Улица
    /// </summary>
    public string Street { get; set; } = null!;

    /// <summary>
    /// Номер дома, корпуса
    /// </summary>
    public string House { get; set; } = null!;

    /// <summary>
    /// Номер квартиры или офиса (необязательно)
    /// </summary>
    public string? Apartment { get; set; }

    public virtual User User { get; set; } = null!;
}
