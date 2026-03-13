using Amazon.S3;
using Amazon.S3.Model;
using OptimusFrame.Core.Application.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace OptimusFrame.Core.Infrastructure.Services
{
    [ExcludeFromCodeCoverage]
    public class MediaService : IMediaService
    {
        private readonly IAmazonS3 _s3Client;

        public MediaService(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task<string> UploadVideoAsync(byte[] fileBytes, Guid MediaId, string userName, string bucketName)
        {
            var key = $"input/{MediaId}.mp4";

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

        public async Task<string> GenerateDownloadUrlAsync(string s3Key, int expirationMinutes = 60)
        {
            var bucketName = "optimus-frame-core-bucket";

            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = s3Key,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            var url = await Task.FromResult(_s3Client.GetPreSignedURL(request));

            return url;
        }
    }
}
