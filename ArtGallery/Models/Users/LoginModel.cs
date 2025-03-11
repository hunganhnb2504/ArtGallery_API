using System.ComponentModel.DataAnnotations;

namespace ArtGallery.Models.Users
{
    public class LoginModel
    {

        [Required(ErrorMessage = "Please enter email")]
        [RegularExpression(@"^\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}\b", ErrorMessage = "Email address is not valid")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Please enter password")]
        [MaxLength(250)]
        public string Password { get; set; }
    }
}
