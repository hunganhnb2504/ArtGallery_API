namespace ArtGallery.Entities
{
    public partial class Follow
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int ArtistId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual Artist Artist { get; set; } = null!;
    }
}
