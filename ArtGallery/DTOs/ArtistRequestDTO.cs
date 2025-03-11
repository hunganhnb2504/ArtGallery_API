namespace ArtGallery.DTOs
{
    public class ArtistRequestDTO : AbstractDTO<ArtistRequestDTO>
    {
        public string NameArtist { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string Image { get; set; }
        public string Biography { get; set; }
        public string SchoolOfArt { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
