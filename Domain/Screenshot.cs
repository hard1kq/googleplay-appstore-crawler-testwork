namespace StoreParsers.Domain
{
    public class Screenshot
    {
        public int ScreenshotId{ get; set; }
        public string Url{ get; set; }
        public int ApplicationId { get; set; }
        public Application Application { get; set; }
    }
}
