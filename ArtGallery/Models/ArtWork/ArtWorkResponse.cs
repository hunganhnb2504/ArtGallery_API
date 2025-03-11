using ArtGallery.Models.Offer;

namespace ArtGallery.Models.ArtWork
{
    public class ArtWorkResponse
    {
        public int Id { get; set; }

        public int artWorkId { get; set; }
        public string Name { get; set; }
        public string ArtWorkImage { get; set; }
        public string Medium { get; set; }
        public string Materials { get; set; }
        public string Size { get; set; }
        public string Condition { get; set; }
        public string Signature { get; set; }
        public string Rarity { get; set; }
        public string CertificateOfAuthenticity { get; set; }
        public string Frame { get; set; }
        public string Series { get; set; }
        public decimal Price { get; set; }
        public int FavoriteCount { get; set; }
        public List<OfferResponse> Offers { get; set; }
    }

}
