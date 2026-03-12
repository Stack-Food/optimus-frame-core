using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OptimusFrame.Core.Application.DTOs.Request;
using OptimusFrame.Core.Application.Events;
using OptimusFrame.Core.Application.Interfaces;
using OptimusFrame.Core.Domain.Entities;
using OptimusFrame.Core.Domain.Enums;

namespace OptimusFrame.Core.Application.UseCases.UploadMedia
{
    public class UploadMediaUseCase
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IMediaService _mediaService;
        private readonly IVideoEventPublisher _publisher;
        private readonly IConfiguration _configuration;

        public UploadMediaUseCase(IMediaRepository mediaRepository, IMediaService mediaService, IVideoEventPublisher videoEventPublisher, IConfiguration configuration)
        {
            _mediaRepository = mediaRepository;
            _mediaService = mediaService;
            _publisher = videoEventPublisher;
            _configuration = configuration;
        }

        public async Task UploadVideoToS3(byte[] videoBytes, UploadVideoBase64Request request)
        {
            var uploadFile = new Media
            {
                Base64 = videoBytes.ToString(),
                CreatedAt = DateTime.Now,
                UserName = request.UserName,
                Status = MediaStatus.Uploaded,
            };

            var response = _mediaRepository.CreateAsync(uploadFile);
            var idMedia = response.Result.MediaId;
            var bucketName = _configuration["AWS:S3:BucketName"];

            var s3Key = await _mediaService.UploadVideoAsync(
                videoBytes,
                idMedia,
                request.UserName,
                bucketName);

            await _publisher.Publish(new VideoProcessingMessage
            {
                VideoId = idMedia.ToString(),
                FileName = request.FileName,
                CorrelationId = Guid.NewGuid().ToString()
            });
        }
    }
}
