using System.ComponentModel.DataAnnotations;

namespace ArtGallery.Models.Artist
{
    public class ArtistRequestModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string NameArtist { get; set; }

        [Required(ErrorMessage = "Biography is required")]
        public string Biography { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Image is required")]
        public IFormFile Image { get; set; }

        [Required]
        public string SchoolOfArt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
