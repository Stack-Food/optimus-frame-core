using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using OptimusFrame.Core.Application.DTOs.Request;
using OptimusFrame.Core.Application.DTOs.Response;
using OptimusFrame.Core.Application.UseCases.UploadMedia;
namespace optimus_frame_core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ExcludeFromCodeCoverage]
    public class MediaController : ControllerBase
    {
        private readonly UploadMediaUseCase _uploadMediaUseCase;

        public MediaController(
            UploadMediaUseCase uploadMediaUseCase)
        {
            _uploadMediaUseCase = uploadMediaUseCase;
        }

        [HttpPost("upload")]
        [ProducesResponseType(typeof(UploadVideoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UploadVideoBase64([FromBody] UploadVideoBase64Request request)
        {
            if (string.IsNullOrWhiteSpace(request.Base64))
                return BadRequest("Base64 não informado.");

            if (string.IsNullOrWhiteSpace(request.FileName))
                return BadRequest("FileName não informado.");

            try
            {
                var base64Data = request.Base64.Contains(",")
                    ? request.Base64.Split(',')[1]
                    : request.Base64;

                byte[] videoBytes = Convert.FromBase64String(base64Data);

                // envia o video
                 _uploadMediaUseCase.UploadVideoToS3(videoBytes, request);

                var response = new UploadVideoResponse
                {
                    FileName = request.FileName,
                    SizeInBytes = videoBytes.Length,
                    Message = $"Vídeo {request.FileName} processado com sucesso."
                };

                return Ok(response);
            }
            catch (FormatException)
            {
                return BadRequest("Base64 inválido.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno ao processar o vídeo.");
            }
        }
    }
}
