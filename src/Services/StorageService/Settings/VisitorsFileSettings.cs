namespace StorageService.Settings
{
    public class VisitorsFileSettings
    {
        private static readonly string DEFAULT_FILE_PATH = "/tmp/visitors.log";

        public string FilePath { get; init; } = DEFAULT_FILE_PATH;
    }
}
