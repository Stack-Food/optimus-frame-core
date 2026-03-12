using OptimusFrame.Core.Application.Interfaces;

namespace OptimusFrame.Core.Application.UseCases.GetUserVideos
{
    public class GetUserVideosUseCase
    {
        private readonly IMediaRepository _mediaRepository;

        public GetUserVideosUseCase(IMediaRepository mediaRepository)
        {
            _mediaRepository = mediaRepository;
        }

        public async Task<IEnumerable<GetUserVideosResponse>> Execute(string userName)
        {
            var videos = await _mediaRepository.GetByUserNameAsync(userName);

            return videos.Select(v => new GetUserVideosResponse
            {
                VideoId = v.MediaId,
                FileName = v.FileName,
                Status = v.Status,
                CreatedAt = v.CreatedAt,
                OutputUri = v.OutputUri,
                CompletedAt = v.CompletedAt
            });
        }
    }
}
