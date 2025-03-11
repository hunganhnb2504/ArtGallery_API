using ArtGallery.DTOs;
using ArtGallery.Entities;
using ArtGallery.Models.Artist;
using Microsoft.EntityFrameworkCore;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace ArtGallery.Service.Artists
{
    public class ArtistService : IArtistService
    {
        private readonly ArtGalleryApiContext _context;
        public ArtistService(ArtGalleryApiContext context)
        {
            _context = context;
        }

        public async Task<List<ArtistDTO>> GetAllArtistAsync()
        {
            List<Artist> artist = await _context.Artist.Where(m => m.DeletedAt == null).OrderByDescending(m => m.Id).ToListAsync();
            List<ArtistDTO> result = new List<ArtistDTO>();
            foreach (Artist a in artist)
            {
                result.Add(new ArtistDTO
                {
                    Id = a.Id,
                    Name = a.Name,
                    Biography = a.Biography,
                    Image = a.Image,
                    createdAt = a.CreatedAt,
                    updatedAt = a.UpdatedAt,
                    deletedAt = a.DeletedAt,
                });
            }
            return result;
        }

        public Task<ArtistDTO> GetArtistByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ArtistDTO> CreateArtistAsync(CreateArtistModel model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateArtistAsync(EditArtistModel model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteArtistAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
