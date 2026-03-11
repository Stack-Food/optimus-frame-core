using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimusFrame.Core.Domain.Entities;

namespace OptimusFrame.Core.Application.Interfaces
{
    public interface IMediaRepository
    {
        Task<Media> CreateAsync(Media media);
    }
}
