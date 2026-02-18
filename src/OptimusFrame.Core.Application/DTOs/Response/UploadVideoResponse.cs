using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimusFrame.Core.Application.DTOs.Response
{
    public class UploadVideoResponse
    {
        public string FileName { get; set; } = default!;
        public long SizeInBytes { get; set; }
        public string Message { get; set; } = default!;
    }
}
