using OptimusFrame.Core.Application.Interfaces;

namespace OptimusFrame.Core.Application.UseCases.DownloadVideo
{
    public class DownloadVideoUseCase
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IMediaService _mediaService;

        public DownloadVideoUseCase(
            IMediaRepository mediaRepository,
            IMediaService mediaService)
        {
            _mediaRepository = mediaRepository;
            _mediaService = mediaService;
        }

        public async Task<string?> Execute(Guid videoId)
        {
            var media = await _mediaRepository.GetByIdAsync(videoId);

            if (media == null)
                throw new InvalidOperationException($"Video with ID {videoId} not found");

            if (string.IsNullOrEmpty(media.OutputUri))
                throw new InvalidOperationException($"Video {videoId} has not been processed yet");

            var downloadUrl = await _mediaService.GenerateDownloadUrlAsync(media.OutputUri, 60);

            return downloadUrl;
        }
    }
}
