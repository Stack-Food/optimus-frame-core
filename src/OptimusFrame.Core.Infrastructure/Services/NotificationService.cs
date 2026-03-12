using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OptimusFrame.Core.Application.Interfaces;

namespace OptimusFrame.Core.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public NotificationService(
            ILogger<NotificationService> logger,
            IAmazonSimpleEmailService sesClient,
            IConfiguration configuration)
        {
            _logger = logger;
            _sesClient = sesClient;
            _fromEmail = configuration["AWS:SES:FromEmail"] ?? "noreply@optimusframe.com";
            _fromName = configuration["AWS:SES:FromName"] ?? "OptimusFrame";
        }

        public async Task NotifyProcessingFailureAsync(
            string videoId,
            string? errorMessage,
            string? correlationId)
        {
            _logger.LogWarning(
                "Sending failure notification - VideoId: {VideoId}, CorrelationId: {CorrelationId}, Error: {Error}",
                videoId,
                correlationId,
                errorMessage);

            try
            {
                // Para ambiente de desenvolvimento ou quando SES não está configurado,
                // apenas loga a notificação
                var toEmail = $"user-{videoId}@example.com"; // Em produção, buscar email real do usuário

                var sendRequest = new SendEmailRequest
                {
                    Source = $"{_fromName} <{_fromEmail}>",
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { toEmail }
                    },
                    Message = new Message
                    {
                        Subject = new Content($"Erro no processamento do vídeo {videoId}"),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = "UTF-8",
                                Data = $@"
                                    <html>
                                    <body>
                                        <h2>OptimusFrame - Erro no Processamento</h2>
                                        <p>Houve um erro ao processar seu vídeo.</p>
                                        <p><strong>ID do Vídeo:</strong> {videoId}</p>
                                        <p><strong>ID de Correlação:</strong> {correlationId}</p>
                                        <p><strong>Erro:</strong> {errorMessage}</p>
                                        <br/>
                                        <p>Por favor, tente novamente ou entre em contato com o suporte.</p>
                                        <br/>
                                        <p>Atenciosamente,<br/>Equipe OptimusFrame</p>
                                    </body>
                                    </html>"
                            },
                            Text = new Content
                            {
                                Charset = "UTF-8",
                                Data = $@"
OptimusFrame - Erro no Processamento

Houve um erro ao processar seu vídeo.

ID do Vídeo: {videoId}
ID de Correlação: {correlationId}
Erro: {errorMessage}

Por favor, tente novamente ou entre em contato com o suporte.

Atenciosamente,
Equipe OptimusFrame"
                            }
                        }
                    }
                };

                var response = await _sesClient.SendEmailAsync(sendRequest);

                _logger.LogInformation(
                    "Email notification sent successfully. MessageId: {MessageId}, VideoId: {VideoId}",
                    response.MessageId,
                    videoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send email notification for VideoId: {VideoId}. Falling back to log-only notification.",
                    videoId);

                // Fallback: apenas loga se o envio de email falhar
                _logger.LogWarning(
                    """
                    NOTIFICATION (EMAIL FAILED - LOG ONLY)
                    VideoId: {VideoId}
                    CorrelationId: {CorrelationId}
                    Error: {Error}
                    """,
                    videoId,
                    correlationId,
                    errorMessage);
            }
        }
    }
}
