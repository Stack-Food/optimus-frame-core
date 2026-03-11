using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OptimusFrame.Core.Application.Events
{
    public record VideoProcessingMessage
    {
        /// <summary>
        /// ID único do vídeo a ser processado
        /// </summary>
        [JsonPropertyName("videoId")]
        public string VideoId { get; init; } = string.Empty;

        /// <summary>
        /// Nome do arquivo do vídeo (opcional - se não informado, usa VideoId)
        /// </summary>
        [JsonPropertyName("fileName")]
        public string? FileName { get; init; }

        /// <summary>
        /// ID de correlação para rastreamento (opcional)
        /// </summary>
        [JsonPropertyName("correlationId")]
        public string? CorrelationId { get; init; }
    }
}
