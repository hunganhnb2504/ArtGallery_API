using System.ComponentModel.DataAnnotations;

namespace ArtGallery.Models.Users
{
    public class ChangePasswordModel
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required, MinLength(6, ErrorMessage = "Please enter at least 6 characters, dude!")]
        public string NewPassword { get; set; }

        [Required, Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}
