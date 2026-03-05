using Amazon.S3;
using Amazon.S3.Model;
using OptimusFrame.Core.Application.Interfaces;

namespace OptimusFrame.Core.Infrastructure.Services
{
    public class MediaService : IMediaService
    {
        private readonly IAmazonS3 _s3Client;

        public MediaService(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task<string> UploadVideoAsync(byte[] fileBytes, string fileName, string userName, string bucketName)
        {
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var key = $"bucket_upload/{userName}/{fileName}_{date}.mp4";

            using var stream = new MemoryStream(fileBytes);

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = stream,
                ContentType = "video/mp4"
            };

            await _s3Client.PutObjectAsync(request);

            return key;
        }
    }
}
