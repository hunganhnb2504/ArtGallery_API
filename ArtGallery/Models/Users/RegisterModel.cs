using System.ComponentModel.DataAnnotations;

namespace ArtGallery.Models.Users
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Please enter phone")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Phone number is not valid")]
        public string phone { get; set; }

        [Required(ErrorMessage = "Please enter fullname")]
        [MinLength(3, ErrorMessage = "Enter at least 3 characters")]
        [MaxLength(255, ErrorMessage = "Enter up to 255 characters")]
        public string fullname { get; set; }

        [Required(ErrorMessage = "Please enter email")]
        [RegularExpression(@"^\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}\b", ErrorMessage = "Email address is not valid")]
        public string email { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [MaxLength(250)]
        [MinLength(6)]
        public string password { get; set; }

       
    }
}
