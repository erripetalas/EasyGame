using Microsoft.AspNetCore.Identity;

namespace EasyGame.Services
{
    /// <summary>
    /// Manages application roles and default admin setup.
    /// Handles role creation and automatic role assignment for users.
    /// </summary>
    public class Roles
    {
        public static async Task CreateRoles(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager) // creates Owner and Customer roles and sets up default admin
        {
            string[] roleNames = { "Owner", "Customer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            await CreateDefaultOwner(userManager);
        }

        public static async Task FixAllUsersWithoutRoles(UserManager<IdentityUser> userManager) // assigns Customer role to users without roles (except admin)
        {
            var allUsers = userManager.Users.ToList();

            foreach (var user in allUsers)
            {
                var roles = await userManager.GetRolesAsync(user);

                // Assign Customer role if user has no roles and isn't admin
                if (!roles.Any() && user.Email != "Admin@hotmail.com")
                {
                    await userManager.AddToRoleAsync(user, "Customer");
                }
            }
        }

        public static async Task EnsureNewUserHasRole(UserManager<IdentityUser> userManager, string email) // ensures new user has Customer role assigned
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var roles = await userManager.GetRolesAsync(user);
                if (!roles.Any() && email != "Admin@hotmail.com")
                {
                    await userManager.AddToRoleAsync(user, "Customer");
                }
            }
        }

        private static async Task CreateDefaultOwner(UserManager<IdentityUser> userManager) // creates default admin account with Owner role
        {
            string ownerEmail = "Admin@hotmail.com";
            string ownerPassword = "Admin123!";

            if (await userManager.FindByEmailAsync(ownerEmail) == null)
            {
                var ownerUser = new IdentityUser
                {
                    UserName = ownerEmail,
                    Email = ownerEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(ownerUser, ownerPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(ownerUser, "Owner");
                }
            }
        }
    }
}