using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using EasyGame.ViewModels;

namespace EasyGame.Controllers
{
    [Authorize(Roles = "Owner")]  // Changed from Admin to Owner
    public class AdminUserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: AdminUser
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = string.Join(", ", roles)
                });
            }

            return View(userViewModels);
        }

        // GET: AdminUser/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                EmailConfirmed = user.EmailConfirmed,
                Roles = string.Join(", ", roles)
            };

            return View(viewModel);
        }

        // GET: AdminUser/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_roleManager.Roles.Select(r => r.Name));
            return View();
        }

        // POST: AdminUser/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = model.EmailConfirmed
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    }
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = new SelectList(_roleManager.Roles.Select(r => r.Name));
            return View(model);
        }

        // GET: AdminUser/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                EmailConfirmed = user.EmailConfirmed,
                CurrentRole = userRoles.FirstOrDefault() ?? "Customer",  // Changed default to Customer
                Roles = new SelectList(allRoles)
            };

            return View(viewModel);
        }

        // POST: AdminUser/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.UserName = model.UserName;
                user.EmailConfirmed = model.EmailConfirmed;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Update role
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    }

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            model.Roles = new SelectList(allRoles);
            return View(model);
        }

        // GET: AdminUser/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting the owner account - update with your owner email
            if (user.Email == "Admin@hotmail.com")  
            {
                return BadRequest("Cannot delete the owner account.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                EmailConfirmed = user.EmailConfirmed,
                Roles = string.Join(", ", roles)
            };

            return View(viewModel);
        }

        // POST: AdminUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Prevent deleting the owner account - update with your owner email
                if (user.Email == "admin@example.com")  // Update this to match your owner email from Roles.cs
                {
                    return BadRequest("Cannot delete the owner account.");
                }

                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}