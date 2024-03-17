namespace PixelService
{
    public interface IService<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
    {
        Task<TResponse> GetAsync(TRequest request, HttpContext context);
    }
}
