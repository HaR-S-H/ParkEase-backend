namespace AuthService.Services
{
    public interface IStorageService
    {
        Task<string> UploadProfilePicture(Stream fileStream, string fileName, string contentType, int userId);
    }
}
