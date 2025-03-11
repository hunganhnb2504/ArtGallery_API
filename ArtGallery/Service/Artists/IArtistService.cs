using ArtGallery.DTOs;
using ArtGallery.Models.Artist;

namespace ArtGallery.Service.Artists
{
    public interface IArtistService
    {
        Task<List<ArtistDTO>> GetAllArtistAsync();
        Task<ArtistDTO> GetArtistByIdAsync(int id);
        Task<ArtistDTO> CreateArtistAsync(CreateArtistModel model);
        Task<bool> UpdateArtistAsync(EditArtistModel model);
        Task<bool> DeleteArtistAsync(int id);

    }
}
