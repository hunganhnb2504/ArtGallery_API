using System.ComponentModel.DataAnnotations;

namespace ArtGallery.Models.Users
{
    public class UpdateProfile
    {
        [Required]
        public int id { get; set; }

        [Required]
        public DateTime birthday { get; set; }

        public string? phone { get; set; }

        public string? address { get; set; }
    } 
}
