using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimusFrame.Core.Application.Interfaces
{
    public interface IMediaService
    {
        Task<string> UploadVideoAsync(byte[] fileBytes, Guid MediaId, string userName,string bucketName);
    }
}
