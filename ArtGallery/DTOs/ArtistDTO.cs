using ArtGallery.Entities;
using ArtGallery.Models.ArtWork;
using ArtGallery.Models.SchoolOfArt;

namespace ArtGallery.DTOs
{
    public class ArtistDTO : AbstractDTO<ArtistDTO>
    {


        public string Name { get; set; }
        public string Biography { get; set; }

        public string Image { get; set; }

        //public string ArtistImages { get; set; }
        public string? Description { get; set; }
        public List<ArtWorkResponse>? ArtWork { get; set; }
        public List<SchoolOfArtResponse>? SchoolOfArts { get; set; }


    }
}