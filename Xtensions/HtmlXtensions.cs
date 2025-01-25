namespace Buratino.Xtensions
{
    public static class HtmlXtensions
    {
        public static string AsHtmlDateTime(this DateTime? source)
        {
            return source?.ToString("s") ?? "";
        }
    }
}