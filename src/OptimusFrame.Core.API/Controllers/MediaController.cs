using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using OptimusFrame.Core.Application.DTOs.Request;
using OptimusFrame.Core.Application.DTOs.Response;
using OptimusFrame.Core.Application.UseCases.UploadMedia;
using OptimusFrame.Core.Application.UseCases.GetUserVideos;
using OptimusFrame.Core.Application.UseCases.DownloadVideo;
namespace optimus_frame_core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ExcludeFromCodeCoverage]
    public class MediaController : ControllerBase
    {
        private readonly UploadMediaUseCase _uploadMediaUseCase;
        private readonly GetUserVideosUseCase _getUserVideosUseCase;
        private readonly DownloadVideoUseCase _downloadVideoUseCase;

        public MediaController(
            UploadMediaUseCase uploadMediaUseCase,
            GetUserVideosUseCase getUserVideosUseCase,
            DownloadVideoUseCase downloadVideoUseCase)
        {
            _uploadMediaUseCase = uploadMediaUseCase;
            _getUserVideosUseCase = getUserVideosUseCase;
            _downloadVideoUseCase = downloadVideoUseCase;
        }

        [HttpPost("upload")]
        [ProducesResponseType(typeof(UploadVideoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadVideoBase64Async([FromBody] UploadVideoBase64Request request)
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

                base64Data = base64Data
                    .Trim()
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Replace(" ", "");

                byte[] videoBytes = Convert.FromBase64String(base64Data);

                await _uploadMediaUseCase.UploadVideoToS3(videoBytes, request);

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

        [HttpGet("user/{userName}/videos")]
        [ProducesResponseType(typeof(IEnumerable<GetUserVideosResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUserVideos(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return BadRequest("UserName não informado.");

            try
            {
                var videos = await _getUserVideosUseCase.Execute(userName);
                return Ok(videos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno ao buscar vídeos do usuário.");
            }
        }

        [HttpGet("{videoId}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DownloadVideo(Guid videoId)
        {
            try
            {
                var downloadUrl = await _downloadVideoUseCase.Execute(videoId);
                return Ok(new { url = downloadUrl, expiresIn = 3600 });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno ao gerar URL de download.");
            }
        }
    }
}
