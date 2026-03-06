using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimusFrame.Core.Application.Events;

namespace OptimusFrame.Core.Application.Interfaces
{
    public interface IVideoEventPublisher
    {
        Task Publish(VideoProcessingMessage evt);
    }
}
