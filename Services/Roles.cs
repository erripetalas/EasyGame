using Microsoft.AspNetCore.Identity;

namespace EasyGame.Services
{
    public class Roles
    {
        public static async Task CreateRoles(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            // Create Owner and Customer roles
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

        public static async Task FixAllUsersWithoutRoles(UserManager<IdentityUser> userManager)
        {
            var allUsers = userManager.Users.ToList();

            foreach (var user in allUsers)
            {
                var roles = await userManager.GetRolesAsync(user);

                // If user has no roles and isn't the owner, assign Customer role
                if (!roles.Any() && user.Email != "Admin@hotmail.com")
                {
                    await userManager.AddToRoleAsync(user, "Customer");
                }
            }
        }

        public static async Task EnsureNewUserHasRole(UserManager<IdentityUser> userManager, string email)
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

        private static async Task CreateDefaultOwner(UserManager<IdentityUser> userManager)
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