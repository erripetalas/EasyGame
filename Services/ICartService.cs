using EasyGame.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyGame.Services
{
    public interface ICartService
    {
        Task<List<CartItem>> GetCartItemsAsync(string userId);
        Task AddToCartAsync(string userId, int productId, int quantity);
        Task UpdateQuantityAsync(int cartItemId, int newQuantity);
        Task RemoveFromCartAsync(int cartItemId);
        Task ClearCartAsync(string userId);
        Task<decimal> GetCartTotalAsync(string userId);
        Task<Order> CheckoutAsync(string userId);
    }
}