using ArtGallery.Entities;
using ArtGallery.Models.ArtWork;

namespace ArtGallery.Models.Offer
{
    public class CreateOffer
    {
        public int ArtWorkId { get; set; }
        public decimal Total { get; set; }
        

        //public string paymentMethod { get; set; } 
    }
}
