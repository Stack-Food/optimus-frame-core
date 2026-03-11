using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimusFrame.Core.Domain.Enums;

namespace OptimusFrame.Core.Domain.Entities
{
    public class Media
    {
        public Guid MediaId { get; set; } = Guid.NewGuid();
        public string UserName { get; set; }
        public string Base64 { get; set; }
        public string UrlBucket { get; set; }
        public DateTime CreatedAt { get; set; }
        public MediaStatus Status { get; set; }
    }
}
