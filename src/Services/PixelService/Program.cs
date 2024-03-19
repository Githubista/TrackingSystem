using MassTransit;
using PixelService.Middleware;
using PixelService.Tracking;
using RabbitMq;
using static PixelService.Resources.ImageLoader;

namespace PixelService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await LoadTrackingImage();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); 
                    // normally an event log as Serilog needs to be configured
                    logging.AddConsole();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    services.AddScoped<IService<TrackingRequestModel, TrackingResponseModel>, TrackingService>();
                    services.AddMassTransit(x =>
                    {
                        var rabbitMqSettings = configuration.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>();
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.Port, "/", h =>
                            {
                                h.Username(rabbitMqSettings.Username);
                                h.Password(rabbitMqSettings.Password);
                            });

                            cfg.ConfigureEndpoints(context);
                        });
                    });

                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure((context, builder) =>
                    {
                        builder.UseRouteResolverMiddleware();
                    });
                });
    }
}