namespace Post.API.Services.Interfaces
{
    public interface IMediaService
    {
        Task<string> UploadMedia(IFormFile file);
    }
}
