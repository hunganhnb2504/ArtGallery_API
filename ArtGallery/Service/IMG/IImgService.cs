namespace ArtGallery.Service.IMG
{
    public interface IImgService
    {
        Task<string> UploadImageAsync(IFormFile img, string storageType);
    }
}
