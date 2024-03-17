using PixelService.Middleware;
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
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            app.UseRouteResolverMiddleware();
        }
    }
}
