using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EasyGame.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Roles")]
        public string Roles { get; set; }  // Changed to string to match your controller
    }

    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Role")]
        public string SelectedRole { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Current Role")]
        public string CurrentRole { get; set; }

        [Display(Name = "Role")]
        public string SelectedRole { get; set; }

        public SelectList Roles { get; set; }  // Changed from AllRoles to match your controller
    }
}