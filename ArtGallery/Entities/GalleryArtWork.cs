namespace ArtGallery.Entities
{
    public partial class GalleryArtWork
    {
        public int Id { get; set; }

        public int ArtWorkId { get; set; }

        public string ImagePath { get; set; } = null!;

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public virtual ArtWork ArtWork { get; set; } = null!;
    }
}
