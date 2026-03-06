using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public UploadMediaUseCase(IMediaRepository mediaRepository, IMediaService mediaService, IVideoEventPublisher videoEventPublisher)
        {
            _mediaRepository = mediaRepository;
            _mediaService = mediaService;
            _publisher = videoEventPublisher;
        }

        public async Task UploadVideoToS3(byte[] videoBytes, UploadVideoBase64Request request)
        {
            //salvar no s3
            var bucketName = "bucketmedianame";
            //var bucketName = "bucketmedianame";

            var s3Key = await _mediaService.UploadVideoAsync(
                videoBytes,
                request.FileName,
                request.UserName,
                bucketName);
            //var s3Key = await _mediaService.UploadVideoAsync(
            //    videoBytes,
            //    request.FileName,
            //    request.UserName,
            //    bucketName);

            //enviar para uma fila

            await _publisher.Publish(new VideoProcessingMessage
            {
                VideoId = Guid.NewGuid().ToString(),
                FileName = request.FileName,
                CorrelationId = Guid.NewGuid().ToString()
            });

            //salvar no banco de dados
            var uploadFile = new Media
            {
                Base64 = videoBytes.ToString(),
                CreatedAt = DateTime.Now,
                UserName = request.UserName,
                Status = MediaStatus.Uploaded,
            };

            await _mediaRepository.CreateAsync(uploadFile);
        }
    }
}
