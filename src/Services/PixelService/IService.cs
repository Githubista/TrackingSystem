namespace PixelService
{
    public interface IService<in TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        Task<TResponse> GetAsync(TRequest request, HttpContext context, CancellationToken cancellationToken);
    }
}
