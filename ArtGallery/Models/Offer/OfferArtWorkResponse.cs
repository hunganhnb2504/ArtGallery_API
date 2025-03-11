namespace ArtGallery.Models.Offer
{
    public class OfferArtWorkResponse
    {
        public int Id { get; set; }
        public int OfferId { get; set; }
        public int ArtWorkId { get; set; }
        public string ArtWorkName { get; set; }
        public string ArtWorkImage { get; set; }

        public decimal OfferPrice { get; set; }
    }
}
