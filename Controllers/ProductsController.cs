using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EasyGame.Data;
using EasyGame.Models;

namespace EasyGame.Controllers
{
    [Authorize(Roles = "Owner")]  // Only Owner can access these actions
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        [AllowAnonymous]  // Everyone can view products
        public async Task<IActionResult> Index(string category = null)
        {
            // Start with all products
            var products = from p in _context.Products select p;

            // Filter by category if one is specified
            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category == category);
            }

            // Pass the current category to the view for display purposes
            ViewData["CurrentCategory"] = category;
            ViewData["Title"] = string.IsNullOrEmpty(category) ? "All Products" : $"{category}";

            // Optional: Add breadcrumb or category display
            ViewData["CategoryDisplayName"] = string.IsNullOrEmpty(category) ? "All Products" : category;

            return View(await products.ToListAsync());
        }

        // GET: Products/Details/5
        [AllowAnonymous]  // Everyone can view product details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create - Only Owner
        public IActionResult Create()
        {
            // Add category options for the dropdown
            ViewBag.CategoryOptions = new SelectList(new[] { "Games", "Books", "Toys" });
            return View();
        }

        // POST: Products/Create - Only Owner
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,StockQuantity,Category,ImageUrl")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Re-populate category options if validation fails
            ViewBag.CategoryOptions = new SelectList(new[] { "Games", "Books", "Toys" }, product.Category);
            return View(product);
        }

        // GET: Products/Edit/5 - Only Owner
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Add category options for the dropdown
            ViewBag.CategoryOptions = new SelectList(new[] { "Games", "Books", "Toys" }, product.Category);
            return View(product);
        }

        // POST: Products/Edit/5 - Only Owner
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,StockQuantity,Category,ImageUrl")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Product updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Re-populate category options if validation fails
            ViewBag.CategoryOptions = new SelectList(new[] { "Games", "Books", "Toys" }, product.Category);
            return View(product);
        }

        // GET: Products/Delete/5 - Only Owner
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5 - Only Owner
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                TempData["Message"] = "Product deleted successfully!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}