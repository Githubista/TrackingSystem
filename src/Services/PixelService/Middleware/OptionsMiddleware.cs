namespace PixelService.Middleware
{
    public class OptionsMiddleware
    {
        public OptionsMiddleware(RequestDelegate next)
        {
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await Task.CompletedTask;
            SetHeaders(context);
            if (context.Request.Path.StartsWithSegments("/track"))
                context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
        }

        public static void SetHeaders(HttpContext context)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
            context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        }
    }
}
