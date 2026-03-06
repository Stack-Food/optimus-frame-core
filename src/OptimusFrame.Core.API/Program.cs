using System.Diagnostics.CodeAnalysis;
using Amazon.S3;
using OptimusFrame.Core.Application.Interfaces;
using OptimusFrame.Core.Application.UseCases.UploadMedia;
using OptimusFrame.Core.Infrastructure.Messaging;
using OptimusFrame.Core.Infrastructure.Repositories;
using OptimusFrame.Core.Infrastructure.Services;
using System.Diagnostics.CodeAnalysis;

namespace OptimusFrame.Core.API
{
public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Services
            builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
            builder.Services.AddAWSService<IAmazonS3>();

            builder.Services.AddSingleton<RabbitMqConnection>();
            builder.Services.AddSingleton<VideoPublisher>();

            builder.Services.AddScoped<IMediaService, MediaService>();
            builder.Services.AddScoped<IMediaRepository, MediaRepository>();
            builder.Services.AddScoped<IVideoEventPublisher, VideoPublisher>();
            builder.Services.AddScoped<UploadMediaUseCase>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Middleware / HTTP request pipeline
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