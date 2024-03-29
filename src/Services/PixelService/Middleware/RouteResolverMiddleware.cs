﻿using PixelService.Tracking;

namespace PixelService.Middleware
{
    public class RouteResolverMiddleware
    {
        private readonly RequestDelegate _next;
        
        private const string CONTENT_TYPE_HEADER_NAME = "Content-Type";
        private const string REFERER_HEADER_NAME = "Referer";
        private const string USER_AGENT_HEADER_NAME = "User-Agent";
        private const string IMAGE_TYPE = "image/gif";
        private const string GET_METHOD_NAME = "GET";
        private const string TRACK_METHOD_NAME = "/track";

        private readonly ILogger<RouteResolverMiddleware> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RouteResolverMiddleware(RequestDelegate next,
            ILogger<RouteResolverMiddleware> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;

            if (context.Request.Method.Equals(GET_METHOD_NAME, StringComparison.OrdinalIgnoreCase) &&
                path.StartsWithSegments(TRACK_METHOD_NAME))
            {
                try
                {
                    var referrer = context.Request.Headers[REFERER_HEADER_NAME].ToString();
                    var userAgent = context.Request.Headers[USER_AGENT_HEADER_NAME].ToString();
                    var ipAddress = context.Connection.RemoteIpAddress?.ToString();

                    var requestModel = new TrackingRequestModel(referrer, userAgent, ipAddress);

                    context.Response.Headers.Add(CONTENT_TYPE_HEADER_NAME, IMAGE_TYPE);

                    using var scope = _serviceScopeFactory.CreateScope();
                    var trackingService = scope.ServiceProvider.GetRequiredService<IService<TrackingRequestModel, TrackingResponseModel>>();
                    var responseModel = await trackingService.GetAsync(requestModel);
                    var imageBytes = responseModel.ImageBytes;
                    await context.Response.Body.WriteAsync(imageBytes, 0, imageBytes.Length);
                }
                catch (Exception exc)
                {
                    _logger.LogError(exc, "Error when handling request for endpoint {endpoint}", TRACK_METHOD_NAME);
                    await WriteError(context);
                }
            }
            else
            {
                await _next(context);
            }
        }

        private static async Task WriteError(HttpContext context)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error");
        }
    }
}
