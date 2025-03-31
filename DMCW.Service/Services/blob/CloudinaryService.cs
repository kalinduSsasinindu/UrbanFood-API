using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace DMCW.Service.Services.blob
{



    public class CloudinaryService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        private string _clientId => Helper.Utility.GetUserIdFromClaims(_httpContextAccessor);
       

        public CloudinaryService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _config = configuration;
            _httpContextAccessor = httpContextAccessor;

            var account = new Account(
                _config["Cloudinary:CloudName"],
                _config["Cloudinary:ApiKey"],
                _config["Cloudinary:ApiSecret"]
            );

            _cloudinary = new CloudinaryDotNet.Cloudinary(account);
        }

        public async Task<string> UploadToCloudinaryAsync(string base64ImageString)
        {
            byte[] imageBytes = Convert.FromBase64String(base64ImageString);

            using var memoryStream = new MemoryStream(imageBytes);

            string folderPath = $"{_clientId}";

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription($"{Guid.NewGuid()}", memoryStream),
                Folder = folderPath,
                Format = "webp",
         
                Transformation = new Transformation()
                    .Quality(75)
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }

        public async Task<List<string>> UploadMedia(List<string> images)
        {
            List<string> imageUrls = new List<string>();

            if (images == null || !images.Any())
            {
                return imageUrls;
            }

            foreach (var image in images)
            {
                var cloudinaryUrl = await UploadToCloudinaryAsync(image);
                imageUrls.Add(cloudinaryUrl);
            }

            return imageUrls;
        }
    }
}

