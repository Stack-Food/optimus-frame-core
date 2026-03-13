using Amazon.S3;
using Amazon.SimpleEmail;
using Microsoft.EntityFrameworkCore;
using OptimusFrame.Core.Application.Interfaces;
using OptimusFrame.Core.Application.UseCases.DownloadVideo;
using OptimusFrame.Core.Application.UseCases.GetUserVideos;
using OptimusFrame.Core.Application.UseCases.UploadMedia;
using OptimusFrame.Core.Infrastructure.Data;
using OptimusFrame.Core.Infrastructure.Messaging;
using OptimusFrame.Core.Infrastructure.Messaging.Consumers;
using OptimusFrame.Core.Infrastructure.Repositories;
using OptimusFrame.Core.Infrastructure.Services;
using OptimusFrame.Core.API.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Diagnostics.CodeAnalysis;

namespace OptimusFrame.Core.API
{
[ExcludeFromCodeCoverage]
public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
            builder.Services.AddAWSService<IAmazonS3>();
            builder.Services.AddAWSService<IAmazonSimpleEmailService>();

            builder.Services.Configure<RabbitMqSettings>(
                builder.Configuration.GetSection("RabbitMQ"));

            builder.Services.AddSingleton<RabbitMqConnection>();

            builder.Services.AddSingleton<VideoPublisher>();

            builder.Services.AddScoped<IMediaService, MediaService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IMediaRepository, MediaRepository>();
            builder.Services.AddScoped<IVideoEventPublisher, VideoPublisher>();
            builder.Services.AddHostedService<VideoProcessingCompletedConsumer>();
            builder.Services.AddScoped<UploadMediaUseCase>();
            builder.Services.AddScoped<GetUserVideosUseCase>();
            builder.Services.AddScoped<DownloadVideoUseCase>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=production_db;Username=postgres;Password=postgres";

            builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

            // Configure Health Checks
            var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMqSettings>()
                ?? new RabbitMqSettings();

            var rabbitHost = rabbitMqSettings.HostName?.Trim();
            if (string.IsNullOrEmpty(rabbitHost))
                throw new InvalidOperationException(
                    "RabbitMQ HostName não está configurado. Verifique a variável de ambiente RabbitMQ__HostName.");

            var rabbitFactory = new RabbitMQ.Client.ConnectionFactory
            {
                HostName = rabbitHost,
                Port = rabbitMqSettings.Port,
                UserName = rabbitMqSettings.UserName?.Trim() ?? "guest",
                Password = rabbitMqSettings.Password ?? "",
                VirtualHost = string.IsNullOrEmpty(rabbitMqSettings.VirtualHost) ? "/" : rabbitMqSettings.VirtualHost
            };

            builder.Services.AddHealthChecks()
                .AddNpgSql(
                    connectionString,
                    name: "postgresql",
                    tags: new[] { "db", "sql", "postgresql" })
                .AddRabbitMQ(
                    sp => rabbitFactory.CreateConnectionAsync(),
                    name: "rabbitmq",
                    tags: new[] { "messaging", "rabbitmq" });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    dbContext.Database.Migrate();
                    Console.WriteLine("Migrations applied successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error applying migrations: {ex.Message}");
                    throw;
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.MapControllers();

            // Health Check Endpoints
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponseWriter.WriteHealthCheckResponse
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("db") || check.Tags.Contains("messaging"),
                ResponseWriter = HealthCheckResponseWriter.WriteHealthCheckResponse
            });

            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false, // Liveness - just checks if app is running
                ResponseWriter = HealthCheckResponseWriter.WriteHealthCheckResponse
            });

            app.Run();
        }
    }
}