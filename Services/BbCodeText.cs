using System.Text.RegularExpressions;

namespace Teron_Addon_Manager.Services
{
    public readonly record struct DescriptionSegment(bool IsImage, string Value);

    public static class BbCodeText
    {
        private static readonly Regex ImgPattern = new(@"\[img\](.*?)\[/img\]", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex UrlWithTextPattern = new(@"\[url=(.*?)\](.*?)\[/url\]", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex UrlPattern = new(@"\[url\](.*?)\[/url\]", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex ListItemPattern = new(@"[ \t]*\r?\n?\[\*\]\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex TagPattern = new(@"\[/?[a-z][^\]]*\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ExtraBlankLinesPattern = new(@"(\r?\n){3,}", RegexOptions.Compiled);

        public static List<DescriptionSegment> ToSegments(string? bbcode)
        {
            var segments = new List<DescriptionSegment>();
            if (string.IsNullOrWhiteSpace(bbcode))
            {
                return segments;
            }

            var normalized = bbcode.Replace("\r\n", "\n");

            // ImgPattern has one capture group, so Regex.Split interleaves [text, imageUrl, text, imageUrl, ...].
            var parts = ImgPattern.Split(normalized);
            for (var i = 0; i < parts.Length; i++)
            {
                if (i % 2 == 1)
                {
                    var url = parts[i].Trim();
                    if (url.Length > 0)
                    {
                        segments.Add(new DescriptionSegment(true, url));
                    }
                }
                else
                {
                    var text = StripTags(parts[i]).Trim();
                    if (text.Length > 0)
                    {
                        segments.Add(new DescriptionSegment(false, text));
                    }
                }
            }

            return segments;
        }

        private static string StripTags(string text)
        {
            text = UrlWithTextPattern.Replace(text, "$2 ($1)");
            text = UrlPattern.Replace(text, "$1");
            text = ListItemPattern.Replace(text, "\n  - ");
            text = TagPattern.Replace(text, "");
            text = ExtraBlankLinesPattern.Replace(text, "\n\n");
            return text;
        }
    }
}
