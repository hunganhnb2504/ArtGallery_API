namespace ArtGallery.DTOs
{
    public class ProfileDTO : AbstractDTO<ProfileDTO>
    {
        public string fullname { get; set; } = null!;

        public DateTime? birthday { get; set; }

        public string email { get; set; } = null!;

        public string? phone { get; set; }

        public string? address { get; set; } 
    }
}
