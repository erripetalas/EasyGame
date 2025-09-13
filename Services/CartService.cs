using EasyGame.Data;
using EasyGame.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGame.Services
{
    /// <summary>
    /// Service class responsible for managing shopping cart operations and checkout processes.
    /// Implements the ICartService interface to provide dependency injection support.
    /// Handles all cart-related business logic including item management, calculations, and order creation.
    /// </summary> 
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CartItem>> GetCartItemsAsync(string userId) // gets all cart items for a specific user 
        {
            return await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> AddToCartAsync(string userId, int productId, int quantity) // adds a product to the cart or updates quantity if it already exists
        {
            // Get product to check stock
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return false;

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            int newTotalQuantity;
            if (existingItem != null)
            {
                newTotalQuantity = existingItem.Quantity + quantity;
            }
            else
            {
                newTotalQuantity = quantity;
            }

            // Check if we have enough stock
            if (newTotalQuantity > product.StockQuantity)
            {
                throw new InvalidOperationException($"Only {product.StockQuantity} items available in stock.");
            }

            if (existingItem != null)
            {
                existingItem.Quantity = newTotalQuantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    DateAdded = DateTime.Now
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateQuantityAsync(int cartItemId, int newQuantity) // updates the quantity of a specific cart item or removes it if quantity is zero or less
        {
            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == cartItemId);

            if (cartItem == null)
                return false;

            if (newQuantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                // Check if we have enough stock for the new quantity
                if (newQuantity > cartItem.Product.StockQuantity)
                {
                    throw new InvalidOperationException($"Only {cartItem.Product.StockQuantity} items available in stock.");
                }

                cartItem.Quantity = newQuantity;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task RemoveFromCartAsync(int cartItemId) // removes a specific item from the cart
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(string userId) // clears all items from a user's cart
        {
            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetCartTotalAsync(string userId) // calculates the total cost of all items in a user's cart
        {
            var cartItems = await GetCartItemsAsync(userId);
            return cartItems.Sum(item => item.Product.Price * item.Quantity);
        }

        public async Task<Order> CheckoutAsync(string userId) // processes the checkout, creates an order, reduces stock, and clears the cart
        {
            var cartItems = await GetCartItemsAsync(userId);

            if (!cartItems.Any())
                return null;

            // CRITICAL: Check stock availability before processing order
            foreach (var cartItem in cartItems)
            {
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {cartItem.ProductId} not found.");
                }

                if (cartItem.Quantity > product.StockQuantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for {product.Name}. Only {product.StockQuantity} available, but {cartItem.Quantity} requested.");
                }
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = await GetCartTotalAsync(userId),
                Status = "Completed",
                OrderItems = new List<OrderItem>()
            };

            foreach (var cartItem in cartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product.Price
                });

                // Reduce stock quantity (we already validated stock above)
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= cartItem.Quantity;
                }
            }

            _context.Orders.Add(order);
            await ClearCartAsync(userId);
            await _context.SaveChangesAsync();

            return order;
        }

        // Helper method to validate stock for a specific product
        public async Task<bool> IsStockAvailableAsync(int productId, int requestedQuantity)
        {
            var product = await _context.Products.FindAsync(productId);
            return product != null && product.StockQuantity >= requestedQuantity;
        }

        // Helper method to get available stock for a product
        public async Task<int> GetAvailableStockAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            return product?.StockQuantity ?? 0;
        }
    }
}