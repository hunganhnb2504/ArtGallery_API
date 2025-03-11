

namespace ArtGallery.Entities
{
    public class ArtistArtWork
    {
        public int Id { get; set; }
        public int ArtistId { get; set; }

        public int ArtWorkId { get; set; }

        public virtual Artist Artist { get; set; } = null!;

        public virtual ArtWork ArtWork { get; set; } = null!;
    }
}
