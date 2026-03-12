using Amazon.S3;
using OptimusFrame.Core.Application.Interfaces;
using OptimusFrame.Core.Application.UseCases.UploadMedia;
using OptimusFrame.Core.Application.UseCases.GetUserVideos;
using OptimusFrame.Core.Application.UseCases.DownloadVideo;
using OptimusFrame.Core.Infrastructure.Messaging;
using OptimusFrame.Core.Infrastructure.Messaging.Consumers;
using OptimusFrame.Core.Infrastructure.Services;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using OptimusFrame.Core.Infrastructure.Repositories;
using OptimusFrame.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace OptimusFrame.Core.API
{
public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
            builder.Services.AddAWSService<IAmazonS3>();

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

            builder.Services.AddHealthChecks().AddNpgSql(connectionString);

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

            app.Run();
        }
    }
}