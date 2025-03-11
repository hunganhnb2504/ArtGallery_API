using ArtGallery.Entities;
using ArtGallery.Models.Artist;
using ArtGallery.Models.Offer;
using ArtGallery.Models.SchoolOfArt;

namespace ArtGallery.DTOs
{
    public class ArtWorkDTO : AbstractDTO<ArtWorkDTO>
    {
        public string Name { get; set; } = null!;
        public string ArtWorkImage { get; set; } = null!; 
        public string Medium { get; set; } = null!;
        public string Materials { get; set; } = null!;
        public string Size { get; set; } = null!;
        public string Condition { get; set; } = null!;
        public string Signature { get; set; } = null!;
        public string Rarity { get; set; } = null!;
        public string CertificateOfAuthenticity { get; set; } = null!;
        public string Frame { get; set; } = null!;
        public string Series { get; set; } = null!;
        public decimal Price { get; set; }
        public int FavoriteCount { get; set; }
        public List<SchoolOfArtResponse>? SchoolOfArts { get; set; }

        public List<ArtistResponse>? Artists { get; set; }

        public List<OfferResponse>? Offers { get; set; }


    }
}