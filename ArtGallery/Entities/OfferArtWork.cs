namespace ArtGallery.Entities
{
    public class OfferArtWork
    {
        public int Id { get; set; }

        public int OfferId { get; set; }

        public int ArtWorkId { get; set; }

        public decimal Price { get; set; }


        public virtual ArtWork ArtWork { get; set; } = null!;

        public virtual Offer Offer { get; set; } = null!;

        //public virtual User User { get; set; } = null!;  
    }
}
