using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimusFrame.Core.Application.Interfaces;
using OptimusFrame.Core.Domain.Entities;

namespace OptimusFrame.Core.Infrastructure.Repositories
{
    public class MediaRepository : IMediaRepository
    {
        public Task<Media> CreateAsync(Media media)
        {
            if (media == null)
            {
                throw new ArgumentNullException(nameof(media));
            }

            var created = new Media
            {
                MediaId = Guid.NewGuid(),
                UserName = media.UserName,
                Base64 = media.Base64,
                Status = media.Status,
                CreatedAt = media.CreatedAt
            };

            return Task.FromResult(created);
        }
    }
}
