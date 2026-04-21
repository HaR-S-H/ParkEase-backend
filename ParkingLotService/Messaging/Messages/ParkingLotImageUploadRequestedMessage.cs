namespace ParkingLotService.Messaging.Messages
{
    public class ParkingLotImageUploadRequestedMessage
    {
        public int LotId { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public required string Base64Content { get; set; }
    }
}