using System.ComponentModel.DataAnnotations;

namespace ArtGallery.Models.Users
{
    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "Please enter email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter new password")]
        [MinLength(6)] // Độ dài tối thiểu của mật khẩu
        public string NewPassword { get; set; }
    }
}
