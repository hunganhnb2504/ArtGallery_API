using System.ComponentModel.DataAnnotations;
namespace ArtGallery.Models.ArtWork
{
    public class CreateArtWorkModel
    {

        public string Name { get; set; }
        public IFormFile ArtWorkImage { get; set; }
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

        [Required]
        public List<int> SchoolOfArtIds { get; set; }

        //public List<int> ArtistId { get; set; }
    }
}