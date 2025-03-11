namespace ArtGallery.Service.IMG
{
    public class ImgService : IImgService
    {
        public async Task<string> UploadImageAsync(IFormFile avatar, string storageType)
        {
            if (avatar != null && avatar.Length > 0)
            {
                if (!IsImageFile(avatar))
                {
                    throw new Exception("Only image files (png, jpg, etc.) are allowed.");
                }
                string fileName = GenerateUniqueFileName(avatar);

                string uploadDirectory = GetUploadDirectory(storageType);
                string filePath = Path.Combine(uploadDirectory, fileName);

                Directory.CreateDirectory(uploadDirectory);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                return GenerateFileUrl(fileName, storageType);
            }

            return null; // Return null if no image is provided.
        }

        private string GenerateUniqueFileName(IFormFile avatar)
        {
            return $"{Guid.NewGuid()}{Path.GetExtension(avatar.FileName)}";
        }

        private string GetUploadDirectory(string storageType)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", storageType);
        }

        private string GenerateFileUrl(string fileName, string storageType)
        {
            // You will need to provide the base URL here or retrieve it from your configuration.
            string baseUrl = "https://localhost:7270"; // Replace with your actual base URL.
            return $"{baseUrl}/uploads/{storageType}/{fileName}";
        }
        private bool IsImageFile(IFormFile file)
        {
            // Kiểm tra loại tệp tin có phải là ảnh không (png, jpg, jpeg, gif, bmp)
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(fileExtension);
        }
    }
}
