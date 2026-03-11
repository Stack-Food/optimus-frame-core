using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimusFrame.Core.Application.Interfaces;
using OptimusFrame.Core.Domain.Entities;
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
    }
}
