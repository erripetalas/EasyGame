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

        public async Task AddToCartAsync(string userId, int productId, int quantity) // adds a product to the cart or updates quantity if it already exists
        {
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
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
        }

        public async Task UpdateQuantityAsync(int cartItemId, int newQuantity) // updates the quantity of a specific cart item or removes it if quantity is zero or less
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                if (newQuantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = newQuantity;
                }
                await _context.SaveChangesAsync();
            }
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

                // Reduce stock quantity
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
    }
}