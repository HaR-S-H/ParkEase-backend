namespace ParkingLotService.Services
{
    public interface IStorageService
    {
        Task<string> UploadParkingLotImage(Stream fileStream, string fileName, string contentType, int lotId);
    }
}