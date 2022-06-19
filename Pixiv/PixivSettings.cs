namespace unbooru.Pixiv
{
    public class PixivSettings
    {
        public string Token { get; set; }
        public string AccessToken { get; set; }
        public int MaxImagesToDownloadImport { get; set; } = 700;
        public int MaxImagesToDownloadService { get; set; } = 300;
        public string ContinueFrom { get; set; }
        public int MaxExistingFilesBeforeExit { get; set; } = 5;
    }
}
