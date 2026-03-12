using OptimusFrame.Core.Domain.Enums;

namespace OptimusFrame.Core.Application.UseCases.GetUserVideos
{
    public class GetUserVideosResponse
    {
        public Guid VideoId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public MediaStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? OutputUri { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
