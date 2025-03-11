namespace ArtGallery.Models.Artist
{
    public class ArtistResponse
    {

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Image { get; set; }
        public string? Biography { get; set; }

        public string? Description { get; set; }
    }
}
