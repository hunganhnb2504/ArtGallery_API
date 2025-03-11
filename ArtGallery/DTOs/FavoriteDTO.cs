namespace ArtGallery.DTOs
{
    internal class FavoriteDTO : AbstractDTO<FavoriteDTO>
    {
        public int ArtWorkId { get; set; }
        public string ArtWorkName { get; set; } = null!;
        public string ArtWorkImage { get; set; } = null!;

        public decimal ArtWorkAmount { get; set; }

        public string Series { get; set; } = null!;

        public int UserId { get; set; }
    }
}