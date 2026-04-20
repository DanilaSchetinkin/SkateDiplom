using System;
using System.Collections.Generic;
using System.Linq;

namespace SchetinkinDemo
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImagePath { get; set; }
        public decimal Total => Price * Quantity;
    }

    public static class CartManager
    {
        private static readonly List<CartItem> _items = new();

        public static IReadOnlyList<CartItem> Items => _items;

        /// <summary>Срабатывает при любом изменении корзины.</summary>
        public static event Action? CartChanged;

        public static void AddItem(int productId, string productName, decimal price, string? imagePath = null)
        {
            var existing = _items.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
                existing.Quantity++;
            else
                _items.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = productName,
                    Price = price,
                    Quantity = 1,
                    ImagePath = imagePath
                });

            CartChanged?.Invoke();
        }

        public static void RemoveItem(int productId)
        {
            _items.RemoveAll(i => i.ProductId == productId);
            CartChanged?.Invoke();
        }

        public static void UpdateQuantity(int productId, int quantity)
        {
            var item = _items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) return;

            if (quantity <= 0)
                RemoveItem(productId);
            else
            {
                item.Quantity = quantity;
                CartChanged?.Invoke();
            }
        }

        public static void Clear()
        {
            _items.Clear();
            CartChanged?.Invoke();
        }
    }
}
