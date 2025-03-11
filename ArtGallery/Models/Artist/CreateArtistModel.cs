using System.ComponentModel.DataAnnotations;

namespace ArtGallery.Models.Artist
{
    public class CreateArtistModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Biography is required")]
        public string Biography { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Image is required")]
        public IFormFile ImagePath { get; set; }

        [Required]
        public List<int> SchoolOfArtIds { get; set; }
    }
}
