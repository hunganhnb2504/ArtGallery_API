
namespace ArtGallery.Entities
{
    public class ArtistSchoolOfArt
    {
        public int Id { get; set; }

        public int ArtistId { get; set; }
        public int SchoolOfArtId { get; set; }

        public virtual Artist Artist { get; set; } = null!;

        public virtual SchoolOfArt SchoolOfArt { get; set; } = null!;
    }
}
