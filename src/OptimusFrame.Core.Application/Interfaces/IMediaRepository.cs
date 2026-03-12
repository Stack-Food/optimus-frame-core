using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimusFrame.Core.Domain.Entities;
using OptimusFrame.Core.Domain.Enums;

namespace OptimusFrame.Core.Application.Interfaces
{
    public interface IMediaRepository
    {
        Task<Media> CreateAsync(Media media);
        Task<IEnumerable<Media>> GetByUserNameAsync(string userName);
        Task<Media?> GetByIdAsync(Guid mediaId);
        Task UpdateStatusAsync(Guid mediaId, MediaStatus status, string? outputUri = null, string? errorMessage = null);
    }
}
