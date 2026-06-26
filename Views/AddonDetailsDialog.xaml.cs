using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TeronAddonManager.Services;
using Brushes = System.Windows.Media.Brushes;
using Cursors = System.Windows.Input.Cursors;
using MediaColor = System.Windows.Media.Color;
using MediaFontFamily = System.Windows.Media.FontFamily;
using WpfImage = System.Windows.Controls.Image;

namespace TeronAddonManager
{
    public partial class AddonDetailsDialog : Window
    {
        // Images and code/quote blocks deliberately stay a fixed size regardless of how wide the dialog is
        // resized — unlike wrapped text, scaling them up doesn't make them more readable (a screenshot
        // doesn't gain detail, and code only needs enough width for ~80 columns).
        private const int DescriptionImageHeight = 225;
        private const int FixedMediaWidth = 400;
        private const int FieldLabelColumnWidth = 140;

        private static readonly Regex UrlDetectPattern = new(@"https?://[^\s)]+", RegexOptions.Compiled);

        public AddonDetailsDialog(string title, string? subtitle, IEnumerable<(string Label, string Value)> fields, string? description = null)
        {
            InitializeComponent();

            Title = title;
            headerText.Text = title;
            subtitleText.Text = subtitle ?? "";
            subtitleText.Visibility = string.IsNullOrWhiteSpace(subtitle) ? Visibility.Collapsed : Visibility.Visible;

            contentPanel.Children.Add(BuildFieldTable(fields));
            if (!string.IsNullOrWhiteSpace(description))
            {
                contentPanel.Children.Add(BuildDescriptionSection(description));
            }
        }

        private static FrameworkElement BuildFieldTable(IEnumerable<(string Label, string Value)> fields)
        {
            var fieldList = fields.ToList();
            var grid = new Grid { Margin = new Thickness(20, 8, 20, 8) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(FieldLabelColumnWidth) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            for (var i = 0; i < fieldList.Count; i++)
            {
                var (label, value) = fieldList[i];
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var labelBlock = new TextBlock
                {
                    Text = label,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 4, 8, 4),
                    VerticalAlignment = System.Windows.VerticalAlignment.Top
                };
                Grid.SetRow(labelBlock, i);
                Grid.SetColumn(labelBlock, 0);
                grid.Children.Add(labelBlock);

                FrameworkElement valueControl = value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                                 value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                    ? CreateLinkValue(value)
                    : new TextBlock { Text = value, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 4) };
                Grid.SetRow(valueControl, i);
                Grid.SetColumn(valueControl, 1);
                grid.Children.Add(valueControl);
            }

            return grid;
        }

        private FrameworkElement BuildDescriptionSection(string description)
        {
            var section = new StackPanel { Margin = new Thickness(20, 4, 20, 8) };

            section.Children.Add(new TextBlock
            {
                Text = "Description",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 4, 0, 4)
            });

            foreach (var segment in BbCodeText.ToSegments(description))
            {
                FrameworkElement control = segment.Kind switch
                {
                    DescriptionSegmentKind.Image => BuildImageControl(segment.Value),
                    DescriptionSegmentKind.Quote => BuildQuoteControl(segment.Value),
                    DescriptionSegmentKind.Heading => BuildHeadingControl(segment.Value, segment.Extra),
                    DescriptionSegmentKind.List => BuildListControl(segment.Value, segment.Extra),
                    _ when IsDividerText(segment.Value) => BuildDividerControl(),
                    _ => BuildTextControl(segment.Value)
                };
                section.Children.Add(control);
            }

            return section;
        }

        // Mirrors the website's <ul>/<ol> indentation: a narrow bullet/number column plus a wrapped-text
        // column. Unlike the WinForms version, the text column's wrap width never needs manual recalculation
        // on resize — a Star-sized Grid column just reflows on its own.
        private static FrameworkElement BuildListControl(string value, int extra)
        {
            var isNumbered = extra == 1;
            var items = value.Split(BbCodeText.ListItemSeparator);

            var list = new StackPanel { Margin = new Thickness(16, 4, 0, 8) };
            for (var i = 0; i < items.Length; i++)
            {
                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(22) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var bullet = new TextBlock
                {
                    Text = isNumbered ? $"{i + 1}." : "•",
                    Margin = new Thickness(0, 2, 4, 2)
                };
                Grid.SetColumn(bullet, 0);
                row.Children.Add(bullet);

                var itemText = (TextBlock)BuildTextControl(items[i]);
                itemText.Margin = new Thickness(0, 2, 0, 2);
                Grid.SetColumn(itemText, 1);
                row.Children.Add(itemText);

                list.Children.Add(row);
            }

            return list;
        }

        // Mirrors how the website renders [SIZE="N"] section titles as progressively larger headers, and
        // [B]-only lines (HeadingSize 0) as bold sub-headers at the body text size.
        private static FrameworkElement BuildHeadingControl(string text, int headingSize)
        {
            var fontSize = headingSize switch
            {
                <= 2 => 13,
                3 => 14,
                4 => 15,
                _ => 17
            };

            return new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = fontSize,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, headingSize == 0 ? 4 : 10, 0, 4)
            };
        }

        // ESOUI authors often use a run of underscores/dashes as a plain-text section divider.
        private static bool IsDividerText(string text)
        {
            var trimmed = text.Trim();
            return trimmed.Length >= 5 && trimmed.All(c => c is '_' or '-' or '=');
        }

        private static FrameworkElement BuildDividerControl()
        {
            return new Border
            {
                Height = 1,
                Background = Brushes.Gray,
                Opacity = 0.4,
                Margin = new Thickness(0, 6, 0, 10)
            };
        }

        private static IEnumerable<Inline> BuildInlineRuns(string text)
        {
            var lastIndex = 0;
            foreach (Match match in UrlDetectPattern.Matches(text))
            {
                if (match.Index > lastIndex)
                {
                    yield return new Run(text[lastIndex..match.Index]);
                }

                var url = match.Value;
                var link = new Hyperlink(new Run(url)) { NavigateUri = new Uri(url) };
                link.RequestNavigate += (_, e) =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });
                    e.Handled = true;
                };
                yield return link;

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < text.Length)
            {
                yield return new Run(text[lastIndex..]);
            }
        }

        private static FrameworkElement BuildTextControl(string text)
        {
            var block = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 6)
            };
            foreach (var inline in BuildInlineRuns(text))
            {
                block.Inlines.Add(inline);
            }
            return block;
        }

        // [QUOTE] blocks render on the website as a bordered, monospaced <pre> box (most often pasted addon
        // source code). No wrap, matching the website's own "overflow: auto" behavior — wrapped in a
        // horizontally-scrolling ScrollViewer so it naturally sizes to the code's own longest line, only
        // shrinking (with its own scrollbar taking over) if the dialog is narrower than the content needs.
        private static FrameworkElement BuildQuoteControl(string text)
        {
            var block = new TextBlock
            {
                Text = text,
                FontFamily = new MediaFontFamily("Consolas"),
                FontSize = 12,
                TextWrapping = TextWrapping.NoWrap
            };

            var border = new Border
            {
                Child = block,
                Background = new SolidColorBrush(MediaColor.FromArgb(20, 128, 128, 128)),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(6)
            };

            return new ScrollViewer
            {
                Content = border,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Margin = new Thickness(0, 4, 0, 8)
            };
        }

        private FrameworkElement BuildImageControl(string url)
        {
            var image = new WpfImage
            {
                Stretch = Stretch.Uniform,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Margin = new Thickness(0, 2, 0, 6),
                Width = FixedMediaWidth,
                Height = DescriptionImageHeight,
                Cursor = Cursors.Hand
            };

            var bitmap = new BitmapImage();
            bitmap.DownloadCompleted += (_, _) =>
            {
                var (width, height) = ScaledImageSize(bitmap.PixelWidth, bitmap.PixelHeight, FixedMediaWidth);
                image.Width = width;
                image.Height = height;
            };
            bitmap.DownloadFailed += (_, _) => image.Visibility = Visibility.Collapsed;
            image.MouseLeftButtonUp += (_, _) => ShowImagePreview(image);

            try
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                image.Source = bitmap;
            }
            catch
            {
                image.Visibility = Visibility.Collapsed;
            }

            return image;
        }

        private static (double Width, double Height) ScaledImageSize(double nativeWidth, double nativeHeight, double maxWidth)
        {
            var scale = Math.Min(maxWidth / nativeWidth, (double)DescriptionImageHeight / nativeHeight);
            return (nativeWidth * scale, nativeHeight * scale);
        }

        private void ShowImagePreview(WpfImage source)
        {
            if (source.Source is not BitmapSource bitmap)
            {
                return;
            }

            var screen = SystemParameters.WorkArea;
            var maxWidth = screen.Width * 0.85;
            var maxHeight = screen.Height * 0.85;

            // Zoom in noticeably relative to how the image is currently displayed in its thumbnail box,
            // rather than capping at the image's native resolution.
            var thumbnailScale = Math.Min(source.Width / bitmap.PixelWidth, source.Height / bitmap.PixelHeight);
            var scale = thumbnailScale * 2.5;
            var width = bitmap.PixelWidth * scale;
            var height = bitmap.PixelHeight * scale;

            if (width > maxWidth || height > maxHeight)
            {
                var shrink = Math.Min(maxWidth / width, maxHeight / height);
                width *= shrink;
                height *= shrink;
            }

            width = Math.Max(200, width);
            height = Math.Max(150, height);

            var previewImage = new WpfImage
            {
                Source = bitmap,
                Stretch = Stretch.Uniform,
                Cursor = Cursors.Hand
            };

            var previewWindow = new Window
            {
                Title = "Image Preview",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = Brushes.Black,
                Width = width,
                Height = height,
                ShowInTaskbar = false,
                Content = previewImage,
                Owner = this
            };
            previewImage.MouseLeftButtonUp += (_, _) => previewWindow.Close();
            previewWindow.KeyDown += (_, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    previewWindow.Close();
                }
            };

            previewWindow.ShowDialog();
        }

        private static FrameworkElement CreateLinkValue(string url)
        {
            var block = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 4) };
            var link = new Hyperlink(new Run(url)) { NavigateUri = new Uri(url) };
            link.RequestNavigate += (_, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });
                e.Handled = true;
            };
            block.Inlines.Add(link);
            return block;
        }
    }
}
