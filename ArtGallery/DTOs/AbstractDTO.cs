namespace ArtGallery.DTOs
{
    public class AbstractDTO<T>
    {
        public int Id { get; set; }

        public DateTime? createdAt { get; set; }

        public DateTime? updatedAt { get; set; }

        public DateTime? deletedAt { get; set; }
    }
}
