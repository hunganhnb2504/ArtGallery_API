namespace ArtGallery.Entities
{
    public partial class Artist
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Image {  get; set; }  
        public string? Biography { get; set; }
        public int? FollowCount { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        //public virtual ViewingRooms? ViewingRooms { get; set; } = null;
        public virtual ICollection<ArtistArtWork> ArtistArtWorks { get; set; } = new List<ArtistArtWork>();
        public virtual ICollection<ArtistSchoolOfArt> ArtistSchoolOfArts { get; set; } = new List<ArtistSchoolOfArt>();
        public virtual ICollection<UserArtist> UserArtists  { get; set; } = new List<UserArtist>();

        public virtual ICollection<Follow> Follow { get; set; } = new List<Follow>();
    }
}
