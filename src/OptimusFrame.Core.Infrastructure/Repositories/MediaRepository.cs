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
            throw new NotImplementedException();
        }
    }
}
