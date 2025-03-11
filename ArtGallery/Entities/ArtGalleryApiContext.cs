using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Entities;
namespace ArtGallery.Entities
{
    public partial class ArtGalleryApiContext : DbContext 
    {

        public ArtGalleryApiContext() 
        {
        }
        public ArtGalleryApiContext(DbContextOptions<ArtGalleryApiContext> options)
           : base(options)
        {
        }
        public DbSet<ArtGallery.Entities.Artist> Artist { get; set; } /*= default!;*/
        public DbSet<ArtGallery.Entities.ArtWork> ArtWork { get; set; } = default!;
        public DbSet<ArtGallery.Entities.SchoolOfArt> SchoolOfArt { get; set; } = default!;
        public DbSet<ArtGallery.Entities.Offer> Offer { get; set; } = default!;
        public DbSet<ArtGallery.Entities.Favorite> Favorite { get; set; } = default!;
        public DbSet<ArtGallery.Entities.Specialists> Specialists { get; set; } = default!;
        public DbSet<ArtGallery.Entities.ArtistArtWork> ArtistArtWork { get; set; } = default!;
        public DbSet<ArtGallery.Entities.ArtistSchoolOfArt> ArtistSchoolOfArt { get; set; } = default!;
        public DbSet<ArtGallery.Entities.ArtWorkSchoolOfArt> ArtWorkSchoolOfArt { get; set; } = default!;
        public DbSet<ArtGallery.Entities.OfferArtWork> OfferArtWork { get; set; } = default!;
        public DbSet<ArtGallery.Entities.User> Users { get; set; }
        
        public DbSet<ArtGallery.Entities.ArtistRequest> ArtistRequests { get; set; }
        public DbSet<ArtGallery.Entities.UserArtist> UserArtist { get; set; } = default!;
        public DbSet<ArtGallery.Entities.Follow> Follow { get; set; } = default!;

    }
}
