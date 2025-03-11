namespace ArtGallery.Models.Artist
{
    public class EditArtistModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Biography { get; set; }

        public string? Description { get; set; }
        public IFormFile? ImagePath { get; set; }
    }
}