using System.Text.RegularExpressions;

namespace TeronAddonManager.Services
{
    public enum DescriptionSegmentKind
    {
        Text,
        Image,
        Quote,
        Heading,
        List
    }

    // Extra is a kind-specific parameter: for Heading, 0 means a standalone [B]-only line (bold, body-size
    // text) and 3+ is the addon's [SIZE="N"] BBCode value (rendered bold and progressively larger). For List,
    // it's 1 for a numbered [LIST=N] and 0 for a plain bulleted [LIST]. Value holds the list's items joined by
    // ListItemSeparator.
    public readonly record struct DescriptionSegment(DescriptionSegmentKind Kind, string Value, int Extra = 0);

    public static class BbCodeText
    {
        public const char ListItemSeparator = '';

        // [CODE], [QUOTE] and [HIGHLIGHT="Lua"] (vBulletin's syntax-highlight tag, used for pasted source far
        // more than plain [CODE] in practice) all render on the website as the same bordered, monospaced
        // <pre class="bbquote"> block. We don't replicate per-token syntax coloring, but all three still get
        // the same monospace/bordered treatment instead of being flattened into a normal paragraph.
        private static readonly Regex ImgPattern = new(@"\[img\](.*?)\[/img\]", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex QuotePattern = new(@"\[(?:quote(?:=[^\]]*)?|code|highlight(?:=[^\]]*)?)\](.*?)\[/(?:quote|code|highlight)\]", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex ListBlockPattern = new(@"\[list(?:=([^\]]*))?\](.*?)\[/list\]", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex ListItemSplitPattern = new(@"\[\*\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SizePattern = new(@"\[size=[""']?(\d+)[""']?\](.*?)\[/size\]", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex BoldOnlyLinePattern = new(@"^\[b\](.*)\[/b\]$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex UrlWithTextPattern = new(@"\[url=(.*?)\](.*?)\[/url\]", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex UrlPattern = new(@"\[url\](.*?)\[/url\]", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        // A whitelist of actual BBCode tag names, rather than a catch-all "[a-z][^\]]*" pattern. ESOUI
        // descriptions often contain literal bracketed placeholder text in usage examples (e.g. "[argument]",
        // "[seconds]") that isn't BBCode at all — a catch-all would silently delete that text. Built from the
        // tags actually observed across multiple real addon descriptions (b, center, code, color, font,
        // highlight, i, img, list, quote, size, u, url), plus a few other standard vBulletin tags.
        private static readonly Regex TagPattern = new(
            @"\[/?(?:b|i|u|s|strike|center|left|right|color|colour|size|font|url|img|list|code|highlight|quote|email|sub|sup|indent)(?:=[^\]]*)?\]",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
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
            var imageParts = ImgPattern.Split(normalized);
            for (var i = 0; i < imageParts.Length; i++)
            {
                if (i % 2 == 1)
                {
                    var url = imageParts[i].Trim();
                    if (url.Length > 0)
                    {
                        segments.Add(new DescriptionSegment(DescriptionSegmentKind.Image, url));
                    }
                    continue;
                }

                // Within each non-image chunk, further split out [CODE], [QUOTE]/[QUOTE=Author] and
                // [HIGHLIGHT="Lang"] blocks so they get distinct, monospaced rendering instead of being
                // flattened into the surrounding paragraph like plain formatting tags.
                var quoteParts = QuotePattern.Split(imageParts[i]);
                for (var j = 0; j < quoteParts.Length; j++)
                {
                    if (j % 2 == 1)
                    {
                        // Only strip literal BBCode tags here — skip the URL/blank-line reflow rules, since
                        // quoted content (especially code) should keep its original text as-is.
                        var quoteText = TagPattern.Replace(quoteParts[j], "").Trim('\n');
                        if (quoteText.Trim().Length > 0)
                        {
                            segments.Add(new DescriptionSegment(DescriptionSegmentKind.Quote, quoteText));
                        }
                    }
                    else
                    {
                        AddListSegments(segments, quoteParts[j]);
                    }
                }
            }

            return segments;
        }

        // [LIST]/[LIST=N]...[/LIST] blocks get pulled out and rendered as their own indented, hanging-indent
        // control (see AddonDetailsDialog.BuildListControl) instead of just turning each [*] into an inline
        // "- " prefix within the surrounding paragraph, which had no real indentation and didn't keep wrapped
        // lines aligned under the item's text.
        private static void AddListSegments(List<DescriptionSegment> segments, string chunk)
        {
            var lastIndex = 0;
            foreach (Match match in ListBlockPattern.Matches(chunk))
            {
                AddTextAndHeadingSegments(segments, chunk[lastIndex..match.Index]);

                var isNumbered = !string.IsNullOrEmpty(match.Groups[1].Value);
                var items = ListItemSplitPattern.Split(match.Groups[2].Value)
                    .Select(item => TagPattern.Replace(item, "").Trim())
                    .Where(item => item.Length > 0)
                    .ToList();
                if (items.Count > 0)
                {
                    segments.Add(new DescriptionSegment(DescriptionSegmentKind.List, string.Join(ListItemSeparator, items), isNumbered ? 1 : 0));
                }

                lastIndex = match.Index + match.Length;
            }
            AddTextAndHeadingSegments(segments, chunk[lastIndex..]);
        }

        // Addon authors consistently use [SIZE="N"]Title[/SIZE] for section titles ("Features", "API
        // Reference", etc., rendered as <font size="N"> on the website) rather than mid-sentence styling, so
        // splitting every occurrence out into its own bold, progressively-larger heading line — instead of
        // just stripping the tag and leaving plain inline text — gets us much closer to the website's actual
        // visual structure.
        private static void AddTextAndHeadingSegments(List<DescriptionSegment> segments, string chunk)
        {
            var lastIndex = 0;
            foreach (Match match in SizePattern.Matches(chunk))
            {
                AddPlainTextSegment(segments, chunk[lastIndex..match.Index]);

                var size = int.Parse(match.Groups[1].Value);
                var headingText = TagPattern.Replace(match.Groups[2].Value, "").Trim();
                if (headingText.Length > 0)
                {
                    segments.Add(new DescriptionSegment(DescriptionSegmentKind.Heading, headingText, size));
                }

                lastIndex = match.Index + match.Length;
            }
            AddPlainTextSegment(segments, chunk[lastIndex..]);
        }

        private static void AddPlainTextSegment(List<DescriptionSegment> segments, string chunk)
        {
            // A line that's entirely [B]...[/B] (no other text on it) is being used the same way as [SIZE] —
            // as a standalone sub-heading — just without changing the font size, so give it the same bold
            // heading treatment rather than letting the generic tag-strip flatten it into plain text.
            var trimmedRaw = chunk.Trim();
            var boldMatch = BoldOnlyLinePattern.Match(trimmedRaw);
            if (boldMatch.Success)
            {
                var boldText = TagPattern.Replace(boldMatch.Groups[1].Value, "").Trim();
                if (boldText.Length > 0 && !boldText.Contains('\n'))
                {
                    segments.Add(new DescriptionSegment(DescriptionSegmentKind.Heading, boldText));
                    return;
                }
            }

            var text = StripTags(chunk).Trim();
            if (text.Length > 0)
            {
                segments.Add(new DescriptionSegment(DescriptionSegmentKind.Text, text));
            }
        }

        private static string StripTags(string text)
        {
            text = UrlWithTextPattern.Replace(text, "$2 ($1)");
            text = UrlPattern.Replace(text, "$1");
            // Safety net for a stray [*] that ends up outside any matched [LIST]...[/LIST] (e.g. mismatched
            // nesting confusing the non-greedy list-block match) — TagPattern's whitelist won't touch it
            // since "*" isn't a letter, so it would otherwise show up as literal "[*]" text.
            text = ListItemSplitPattern.Replace(text, "\n  - ");
            text = TagPattern.Replace(text, "");
            text = ExtraBlankLinesPattern.Replace(text, "\n\n");
            return text;
        }
    }
}
