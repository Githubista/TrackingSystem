namespace StorageService
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public string GetUtcNow() => DateTime.UtcNow.ToString("O");
    }
}
