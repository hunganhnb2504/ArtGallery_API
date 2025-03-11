namespace ArtGallery.Entities
{
    public class ArtWork
    {
        public int Id { get; set; }
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
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        //public virtual ViewingRooms ViewingRooms { get; set; }
        public virtual ICollection<ArtWorkSchoolOfArt> ArtWorkSchoolOfArts { get; set; } = new List<ArtWorkSchoolOfArt>();

        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();

        public virtual ICollection<ArtistArtWork> ArtistArtWorks { get; set; } = new List<ArtistArtWork>();
        public virtual ICollection<OfferArtWork> OfferArtWork { get; set; } = new List<OfferArtWork>();
        //public virtual ICollection<GalleryArtWork> GalleryArtWorks { get; set; } = new List<GalleryArtWork>();
    }
}
