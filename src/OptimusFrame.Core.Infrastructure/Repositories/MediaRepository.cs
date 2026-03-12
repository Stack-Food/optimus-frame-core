using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OptimusFrame.Core.Application.Interfaces;
using OptimusFrame.Core.Domain.Entities;
using OptimusFrame.Core.Domain.Enums;
using OptimusFrame.Core.Infrastructure.Data;

namespace OptimusFrame.Core.Infrastructure.Repositories
{
    public class MediaRepository : IMediaRepository
    {
        private readonly AppDbContext _context;
        public MediaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Media> CreateAsync(Media media)
        {
            if (media == null)
                throw new ArgumentNullException(nameof(media));

            media.MediaId = Guid.NewGuid();

            _context.Media.Add(media);

            await _context.SaveChangesAsync();

            return media;
        }

        public async Task<IEnumerable<Media>> GetByUserNameAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("UserName cannot be null or empty", nameof(userName));

            return await _context.Media
                .Where(m => m.UserName == userName)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<Media?> GetByIdAsync(Guid mediaId)
        {
            return await _context.Media
                .FirstOrDefaultAsync(m => m.MediaId == mediaId);
        }

        public async Task UpdateStatusAsync(Guid mediaId, MediaStatus status, string? outputUri = null, string? errorMessage = null)
        {
            var media = await _context.Media.FindAsync(mediaId);

            if (media == null)
                throw new InvalidOperationException($"Media with ID {mediaId} not found");

            media.Status = status;

            if (outputUri != null)
                media.OutputUri = outputUri;

            if (errorMessage != null)
                media.ErrorMessage = errorMessage;

            if (status == MediaStatus.Completed || status == MediaStatus.Failed)
                media.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
