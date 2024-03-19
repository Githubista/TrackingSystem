using System.Text;

namespace StorageService
{
    public class FileWriter : IFileWriter
    {
        public async Task AppendToFile(string filePath, string input)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentNullException(nameof(input));


            byte[] jsonBytes = Encoding.UTF8.GetBytes(input);

            var directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var fileStreamOptions = new FileStreamOptions
            {
                Access = FileAccess.Write,
                Mode = FileMode.Append,
                Share = FileShare.Write,
                Options = FileOptions.Asynchronous
            };

            await using var sourceStream = File.Open(filePath, fileStreamOptions);
            await sourceStream.WriteAsync(jsonBytes);
        }
    }
}
