namespace ArtGallery.Entities
{
    public partial class Offer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ArtWorkId { get; set; }
        public string OfferCode { get; set; } = null!;
        public decimal OfferPrice { get; set; }
        public decimal Total { get; set; }
        public string? Address { get; set; }
        public int IsPaid { get; set; }
        public int Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual ICollection<OfferArtWork> OfferArtWorks { get; set; } = new List<OfferArtWork>();

        public virtual ICollection<UserArtist> UserArtists { get; set; } = new List<UserArtist>();

    }  
}
