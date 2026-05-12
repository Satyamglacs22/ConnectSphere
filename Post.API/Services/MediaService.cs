using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Post.API.Services.Interfaces;

namespace Post.API.Services
{
    public class MediaService : IMediaService
    {
        private readonly IConfiguration _config;

        public MediaService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> UploadMedia(IFormFile file)
        {
            var connStr = _config["Azure:BlobConnectionString"];
            var containerName = _config["Azure:PostContainer"] ?? "posts";

            if (string.IsNullOrEmpty(connStr))
            {
                throw new Exception("Azure:BlobConnectionString is missing in configuration.");
            }

            var blobClient = new BlobContainerClient(connStr, containerName);
            await blobClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blob = blobClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, overwrite: true);

            return blob.Uri.ToString();
        }
    }
}
