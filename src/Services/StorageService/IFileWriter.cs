namespace StorageService
{
    public interface IFileWriter
    {
        Task AppendToFile(string filePath, string input);
    }
}
