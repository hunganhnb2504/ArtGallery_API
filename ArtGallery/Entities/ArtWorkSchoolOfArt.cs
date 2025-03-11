namespace ArtGallery.Entities
{
    public class ArtWorkSchoolOfArt
    {
        public int Id { get; set; }

        public int SchoolOfArtId { get; set; }

        public int ArtWorkId { get; set; }

        public virtual ArtWork ArtWork { get; set; } = null!;

        public virtual SchoolOfArt SchoolOfArt { get; set; } = null!;
    }
}
