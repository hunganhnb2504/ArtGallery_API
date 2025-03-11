namespace ArtGallery.DTOs
{
    public class OfferDTO : AbstractDTO<OfferDTO>
    {

            public int UserId { get; set; }
            public string UserName { get; set; }
            public int ArtWorkId { get; set; }
            public string? Address { get; set; }
            //public string? ArtWorkName { get; set; }
            //public string? ArtWorkImage { get; set; }
            public decimal OfferPrice { get; set; }
            public string OfferCode { get; set; } = null!;
            public int IsPaid { get; set; }
            public decimal ToTal {  get; set; }
            public int Status { get; set; }
            public List<string> ArtWorkNames { get; set; }
            public List<string> ArtWorkImages { get; set; }
            






    }
}
