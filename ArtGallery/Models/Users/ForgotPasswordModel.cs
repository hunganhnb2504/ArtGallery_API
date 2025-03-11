using System.ComponentModel.DataAnnotations;

namespace ArtGallery.Models.Users
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Please enter email")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
