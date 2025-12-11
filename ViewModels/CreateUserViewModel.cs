using System.ComponentModel.DataAnnotations;

namespace WorkChat2.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Display(Name = "UserName")]
        public string UserName { get; set; } = null!;

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; } = null!;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password do not match")]
        public string ConfirmPassword { get; set; } = null!;

        [Display(Name = "IsAdmin")]
        public bool IsAdmin { get; set; }
    }
}
