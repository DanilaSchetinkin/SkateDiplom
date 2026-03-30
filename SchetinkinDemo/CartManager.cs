using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchetinkinDemo
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImagePath { get; set; }
        public decimal Total => Price * Quantity;
    }

    public static class CartManager
    {
        private static List<CartItem> _items = new List<CartItem>();

        public static IReadOnlyList<CartItem> Items => _items;

        public static void AddItem(int productId, string productName, decimal price, string? imagePath = null)
        {
            var existing = _items.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                _items.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = productName,
                    Price = price,
                    Quantity = 1,
                    ImagePath = imagePath
                });
            }
        }

        public static void RemoveItem(int productId)
        {
            _items.RemoveAll(i => i.ProductId == productId);
        }

        public static void UpdateQuantity(int productId, int quantity)
        {
            var item = _items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                    RemoveItem(productId);
                else
                    item.Quantity = quantity;
            }
        }

        public static void Clear()
        {
            _items.Clear();
        }
    }
}
