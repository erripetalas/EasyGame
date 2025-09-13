using EasyGame.Data;
using EasyGame.Models;
using EasyGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGame.Controllers
{
    [Authorize] // Requires user to be logged in
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CartController(ICartService cartService, UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _cartService = cartService;
            _userManager = userManager;
            _context = context;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _cartService.GetCartItemsAsync(userId);
            var total = await _cartService.GetCartTotalAsync(userId);

            ViewBag.CartTotal = total;
            return View(cartItems);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var success = await _cartService.AddToCartAsync(userId, productId, quantity);

                if (success)
                {
                    TempData["Message"] = "Item added to cart!";
                }
                else
                {
                    TempData["Error"] = "Unable to add item to cart.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message; // This will show "Only X items available in stock."
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while adding item to cart.";
                // Log the exception here if you have logging set up
            }

            return RedirectToAction("Index", "Products");
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                var success = await _cartService.UpdateQuantityAsync(cartItemId, quantity);

                if (success)
                {
                    if (quantity <= 0)
                    {
                        TempData["Message"] = "Item removed from cart.";
                    }
                    else
                    {
                        TempData["Message"] = "Cart updated successfully.";
                    }
                }
                else
                {
                    TempData["Error"] = "Unable to update cart item.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message; // This will show "Only X items available in stock."
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating cart.";
                // Log the exception here if you have logging set up
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            try
            {
                await _cartService.RemoveFromCartAsync(cartItemId);
                TempData["Message"] = "Item removed from cart";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while removing item from cart.";
                // Log the exception here if you have logging set up
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/Clear
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                await _cartService.ClearCartAsync(userId);
                TempData["Message"] = "Cart cleared";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while clearing cart.";
                // Log the exception here if you have logging set up
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Cart/Checkout
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _cartService.GetCartItemsAsync(userId);

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction(nameof(Index));
            }

            // Check if all items in cart are still available in sufficient quantities
            bool hasStockIssues = false;
            foreach (var item in cartItems)
            {
                var availableStock = await _cartService.GetAvailableStockAsync(item.ProductId);
                if (item.Quantity > availableStock)
                {
                    hasStockIssues = true;
                    TempData["Error"] = $"Insufficient stock for {item.Product.Name}. Only {availableStock} available, but you have {item.Quantity} in cart.";
                    break;
                }
            }

            if (hasStockIssues)
            {
                return RedirectToAction(nameof(Index));
            }

            var total = await _cartService.GetCartTotalAsync(userId);
            ViewBag.CartTotal = total;

            return View(cartItems);
        }

        // POST: Cart/ProcessOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var order = await _cartService.CheckoutAsync(userId);

                if (order == null)
                {
                    TempData["Error"] = "Unable to process order - your cart is empty";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Message"] = "Order processed successfully!";
                return RedirectToAction(nameof(OrderConfirmation), new { orderId = order.Id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message; // This will show stock-related errors
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing your order. Please try again.";
                // Log the exception here if you have logging set up
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Cart/OrderConfirmation
        public IActionResult OrderConfirmation(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        // GET: Cart/OrderHistory
        public async Task<IActionResult> OrderHistory()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // AJAX endpoint to check stock availability
        [HttpGet]
        public async Task<IActionResult> CheckStock(int productId, int quantity)
        {
            try
            {
                var isAvailable = await _cartService.IsStockAvailableAsync(productId, quantity);
                var availableStock = await _cartService.GetAvailableStockAsync(productId);

                return Json(new
                {
                    isAvailable = isAvailable,
                    availableStock = availableStock,
                    message = isAvailable ? "Stock available" : $"Only {availableStock} items available"
                });
            }
            catch (Exception ex)
            {
                return Json(new { isAvailable = false, error = "Error checking stock" });
            }
        }
    }
}