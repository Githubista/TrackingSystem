using MassTransit;
using PixelService.Middleware;
using PixelService.Settings;
using PixelService.Tracking;

namespace PixelService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IService<TrackingRequestModel,TrackingResponseModel>, TrackingService>();
            services.AddMassTransit(x =>
            {
                var rabbitMqSettings = Configuration.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>();
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
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            app.UseRouteResolverMiddleware();
        }
    }
}
