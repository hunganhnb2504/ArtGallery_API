namespace ArtGallery.Models.Offer
{
    public class OfferDetail
    {
        public int Id { get; set; }
        public int ArtWorkId { get; set; }
        public string OfferCode { get; set; }
        public int UserId { get; set; }
        public List<string> ArtWorkNames { get; set; }
        public List<string> ArtWorkImages { get; set; }
        public string UserName { get; set; }
        public decimal OfferPrice { get; set; }
        public decimal ToTal { get; set; }
        public int IsPaid { get; set; }
        public string Address { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int Status { get; set; }
        public List<OfferArtWorkResponse> OfferArtWork { get; set; }


    }
}
