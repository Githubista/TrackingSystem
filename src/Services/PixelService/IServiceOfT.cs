namespace PixelService
{
    public interface IService<in TRequest, TResponse>
    {
        Task<TResponse> GetAsync(TRequest request, HttpContext context);
    }
}
