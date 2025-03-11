namespace ArtGallery.Entities
{
    public class ArtistRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string NameArtist { get; set; }
        public string Image { get; set; }
        public string Biography { get; set; }
        public string? SchoolOfArt { get; set; }
        public int StatusRequest { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }


    }
}
