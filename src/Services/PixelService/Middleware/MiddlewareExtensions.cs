namespace PixelService.Middleware
{
    public static class MiddlewareExtensions
    {
        public static void UseRouteResolverMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<RouteResolverMiddleware>();
        }
    }
}
