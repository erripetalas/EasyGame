using EasyGame.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EasyGame.Services
{
    /// <summary>
    /// Custom UserStore that automatically assigns Customer role to new users.
    /// Extends Identity UserStore to inject role assignment logic during user creation.
    /// </summary>
    public class AutoRoleAssignmentUser : UserStore<IdentityUser>
    {
        public AutoRoleAssignmentUser(ApplicationDbContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
        }

        public override async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken = default) // creates user and auto-assigns Customer role (except for admin account)
        {
            var result = await base.CreateAsync(user, cancellationToken);

            // Auto-assign Customer role to non-admin users
            if (result.Succeeded && user.Email != "Admin@hotmail.com")
            {
                var roleManager = Context.GetService<RoleManager<IdentityRole>>();
                var userManager = Context.GetService<UserManager<IdentityUser>>();

                await userManager.AddToRoleAsync(user, "Customer");
            }

            return result;
        }
    }
}