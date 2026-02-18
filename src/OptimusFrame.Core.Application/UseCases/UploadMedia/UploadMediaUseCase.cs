using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimusFrame.Core.Application.DTOs.Request;
using OptimusFrame.Core.Application.Interfaces;
using OptimusFrame.Core.Domain.Entities;
using OptimusFrame.Core.Domain.Enums;

namespace OptimusFrame.Core.Application.UseCases.UploadMedia
{
    public class UploadMediaUseCase
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IMediaService _mediaService;

        public UploadMediaUseCase(IMediaRepository mediaRepository, IMediaService mediaService)
        {
            _mediaRepository = mediaRepository;
            _mediaService = mediaService;
        }
        public async Task UploadVideoToS3(byte[] videoBytes, UploadVideoBase64Request request)
        {
            //salvar no s3
            var bucketName = "bucketmedianame";

            var s3Key = await _mediaService.UploadVideoAsync(
                videoBytes,
                request.FileName,
                request.UserName,
                bucketName);

            //enviar para uma fila


            //salvar no banco de dados
            var uploadFile = new Media
            {
                Base64 = videoBytes.ToString(),
                CreatedAt = DateTime.Now,
                UserName=request.UserName,
                Status=MediaStatus.Uploaded,
            };
            throw new NotImplementedException();
        }
    }
}
