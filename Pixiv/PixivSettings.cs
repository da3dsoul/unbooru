namespace ImageInfrastructure.Pixiv
{
    public class PixivSettings
    {
        public string Token { get; set; }
        public string AccessToken { get; set; }
        public int MaxImagesToDownload { get; set; } = 10;
    }
}