using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimusFrame.Core.Application.DTOs.Request
{
    public class UploadVideoBase64Request
    {
        public string FileName { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Base64 { get; set; } = default!;
        public DateTime CreatedAt { get; set; }

        public UploadVideoBase64Request(string fileName, string userName, string base64, DateTime createdAt)
        {
            FileName = fileName;
            UserName = userName;
            Base64 = base64;
            CreatedAt = createdAt;
        }

        public UploadVideoBase64Request()
        {
        }
    }
}
