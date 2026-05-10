using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace AuthService.Services
{
    public class S3StorageService : IStorageService
    {
        private readonly IConfiguration _configuration;

        public S3StorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadProfilePicture(Stream fileStream, string fileName, string contentType, int userId)
        {
            if (!fileStream.CanRead)
            {
                throw new ArgumentException("Profile picture stream is not readable.");
            }

            var accessKey = _configuration["AWS:AccessKey"];
            var secretKey = _configuration["AWS:SecretKey"];
            var region = _configuration["AWS:Region"];
            var bucket = _configuration["AWS:S3:BucketName"];

            if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(bucket))
            {
                throw new InvalidOperationException("AWS S3 configuration is incomplete.");
            }

            var extension = Path.GetExtension(fileName);
            var key = $"{userId}/{Guid.NewGuid():N}{extension}";

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            using var s3Client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(region));

            var putRequest = new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                InputStream = fileStream,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
            };

            await s3Client.PutObjectAsync(putRequest);

            return $"https://{bucket}.s3.{region}.amazonaws.com/{key}";
        }
    }
}
