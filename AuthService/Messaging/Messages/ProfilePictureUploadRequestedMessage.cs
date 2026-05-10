namespace AuthService.Messaging.Messages
{
    public class ProfilePictureUploadRequestedMessage
    {
        public int UserId { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public required string Base64Content { get; set; }
    }
}