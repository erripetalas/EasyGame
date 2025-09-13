using EasyGame.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyGame.Services
{
    /// <summary>
    /// Interface defining shopping cart service operations.
    /// Provides contract for cart management and checkout functionality.
    /// </summary>
    public interface ICartService
    {
        Task<List<CartItem>> GetCartItemsAsync(string userId); // retrieves all cart items for a user with product details
        Task AddToCartAsync(string userId, int productId, int quantity); // adds product to cart or updates quantity if item exists
        Task UpdateQuantityAsync(int cartItemId, int newQuantity); // updates cart item quantity or removes if quantity <= 0
        Task RemoveFromCartAsync(int cartItemId); // removes specific cart item
        Task ClearCartAsync(string userId); // clears all cart items for user
        Task<decimal> GetCartTotalAsync(string userId); // calculates total cart value
        Task<Order> CheckoutAsync(string userId); // processes checkout: creates order, reduces stock, clears cart
    }
}