using EasyGame.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EasyGame.Services
{
    public class AutoRoleAssignmentUser : UserStore<IdentityUser>
    {
        public AutoRoleAssignmentUser(ApplicationDbContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
        }

        public override async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            var result = await base.CreateAsync(user, cancellationToken);

            if (result.Succeeded && user.Email != "Admin@hotmail.com")
            {
                // Get role manager from context
                var roleManager = Context.GetService<RoleManager<IdentityRole>>();
                var userManager = Context.GetService<UserManager<IdentityUser>>();

                await userManager.AddToRoleAsync(user, "Customer");
            }

            return result;
        }
    }
}