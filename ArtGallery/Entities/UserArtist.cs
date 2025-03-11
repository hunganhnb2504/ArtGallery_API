namespace ArtGallery.Entities
{
    public class UserArtist
    {
        public int Id { get; set; }
        public int ArtistId { get; set; }

        public int UserId { get; set; }

        public virtual Artist Artist { get; set; } = null!;

        public virtual User User { get; set; } = null!;
    }
}
