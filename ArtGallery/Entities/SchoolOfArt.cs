namespace ArtGallery.Entities
{
    public partial class SchoolOfArt
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; 
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

    }
}
