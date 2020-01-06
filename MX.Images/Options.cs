namespace MX.Images
{
    public class Options
        : IOptions
    {
        public Options() =>
            SearchPattern = "*.jpg";

        public string SearchPattern { get; }
    }
}