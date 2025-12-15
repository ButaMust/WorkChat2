using System.ComponentModel.DataAnnotations;

namespace WorkChat2.ViewModels
{
    public class AdminResetPasswordVm
    {
        [Required]
        public string UserId { get; set; } = null!;

        public string Email { get; set; } = "";

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
