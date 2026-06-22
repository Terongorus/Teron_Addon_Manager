using Teron_Addon_Manager.Services;

namespace Teron_Addon_Manager
{
    public partial class AddonDetailsDialog : Form
    {
        private const int DescriptionImageHeight = 225;
        // Images and code/quote blocks deliberately stay this fixed width regardless of how wide the dialog
        // is resized — unlike wrapped text, scaling them up doesn't make them more readable (a screenshot
        // doesn't gain detail, and code only needs enough width for ~80 columns), so there's no upside to
        // letting them grow, and a code block scaling independently of its actual content looks odd.
        private const int FixedMediaWidth = 400;
        private const int ScrollStep = 60;
        private const int MinContentWidth = 200;

        private readonly FlowLayoutPanel _scrollContent;
        private readonly VScrollBar _scrollBar;

        // Every width-dependent control (text wrap width, image max width, field value column, etc.)
        // registers a callback here instead of just being sized once at construction time, so the whole
        // dialog actually reflows to use the available width when the user resizes or maximizes it, rather
        // than staying pinned to its initial width with the extra space left blank.
        private readonly List<Action<int>> _contentWidthHandlers = new();
        private int _contentWidth = 400;

        public AddonDetailsDialog(string title, string? subtitle, IEnumerable<(string Label, string Value)> fields, string? description = null)
        {
            InitializeComponent();

            Text = title;
            headerLabel.Text = title;
            subtitleLabel.Text = subtitle ?? "";
            subtitleLabel.Visible = !string.IsNullOrWhiteSpace(subtitle);

            _scrollContent = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 12)
            };
            _scrollContent.Controls.Add(BuildFieldTable(fields));
            if (!string.IsNullOrWhiteSpace(description))
            {
                _scrollContent.Controls.Add(BuildDescriptionSection(description));
            }

            // contentPanel intentionally does not use Panel.AutoScroll: with this dialog's dynamically-built,
            // AutoSize content, something in WinForms' AutoScroll machinery resets the scroll position back
            // to the top on any click anywhere in the dialog (confirmed even on controls never touched here,
            // like headerLabel) — scrolling is positioned manually instead via a plain VScrollBar.
            contentPanel.Controls.Add(_scrollContent);

            _scrollBar = new VScrollBar { Dock = DockStyle.Right, SmallChange = ScrollStep };
            _scrollBar.Scroll += (_, e) => PositionContent(e.NewValue);
            contentPanel.Controls.Add(_scrollBar);

            // Layout fires for any change that could affect how much vertical space the content needs —
            // a child resizing, becoming visible/invisible, being added/removed — which is a more complete
            // signal than SizeChanged alone (e.g. SizeChanged never fires for the case where an image fails
            // to load and gets hidden, since hiding a fixed-size control changes the *parent's* size, not
            // its own). This is now the single source of truth for keeping the scroll range in sync, instead
            // of needing every content-building method to remember to call UpdateScrollRange itself.
            _scrollContent.Layout += (_, _) => UpdateScrollRange();
            contentPanel.SizeChanged += (_, _) => OnContentPanelResized();
            contentPanel.MouseWheel += ContentPanel_MouseWheel;

            UpdateContentWidth();
            UpdateScrollRange();
        }

        private void OnContentPanelResized()
        {
            UpdateContentWidth();
            UpdateScrollRange();
        }

        private void UpdateContentWidth()
        {
            var available = contentPanel.ClientSize.Width - _scrollBar.Width - 24;
            var newWidth = Math.Max(MinContentWidth, available);
            if (newWidth == _contentWidth)
            {
                return;
            }

            _contentWidth = newWidth;
            foreach (var handler in _contentWidthHandlers)
            {
                handler(newWidth);
            }
        }

        private void UpdateScrollRange()
        {
            var viewHeight = contentPanel.ClientSize.Height;
            // GetPreferredSize computes the height fresh from the current children right now, rather than
            // trusting _scrollContent.Height — which only reflects whatever the last completed layout pass
            // produced, and isn't guaranteed to already be up to date at the moment a resize notification
            // fires. That staleness was the root cause of descriptions sometimes getting cut off (range
            // computed too small) or leaving extra blank space after the content (range computed too large).
            var contentHeight = _scrollContent.GetPreferredSize(Size.Empty).Height;

            if (contentHeight <= viewHeight)
            {
                _scrollBar.Visible = false;
                PositionContent(0);
                return;
            }

            _scrollBar.Visible = true;
            var max = contentHeight - viewHeight;
            _scrollBar.Minimum = 0;
            _scrollBar.LargeChange = Math.Max(1, viewHeight);
            _scrollBar.Maximum = max + _scrollBar.LargeChange - 1;
            if (_scrollBar.Value > max)
            {
                _scrollBar.Value = max;
            }
            PositionContent(_scrollBar.Value);
        }

        private void PositionContent(int offset)
        {
            _scrollContent.Location = new Point(0, -offset);
        }

        private void ContentPanel_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (!_scrollBar.Visible)
            {
                return;
            }

            if (e is HandledMouseEventArgs handled)
            {
                handled.Handled = true;
            }

            var notches = e.Delta / SystemInformation.MouseWheelScrollDelta;
            var max = _scrollBar.Maximum - _scrollBar.LargeChange + 1;
            var newValue = Math.Clamp(_scrollBar.Value - (notches * ScrollStep), _scrollBar.Minimum, max);
            _scrollBar.Value = newValue;
            PositionContent(newValue);
        }

        private const int FieldLabelColumnWidth = 140;

        private Control BuildFieldTable(IEnumerable<(string Label, string Value)> fields)
        {
            var fieldList = fields.ToList();
            var table = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(12, 8, 12, 8),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, FieldLabelColumnWidth));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            for (var i = 0; i < fieldList.Count; i++)
            {
                var (label, value) = fieldList[i];
                table.RowCount++;
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var labelControl = new Label
                {
                    Text = label,
                    Font = new Font(Font, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(0, 4, 8, 4),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top
                };
                table.Controls.Add(labelControl, 0, i);

                Control valueControl = value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                        value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                    ? CreateLinkValue(value)
                    : new Label { Text = value, AutoSize = true, Margin = new Padding(0, 4, 0, 4) };

                valueControl.MaximumSize = new Size(_contentWidth - FieldLabelColumnWidth, 0);
                valueControl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                table.Controls.Add(valueControl, 1, i);

                _contentWidthHandlers.Add(width => valueControl.MaximumSize = new Size(Math.Max(80, width - FieldLabelColumnWidth), 0));
            }

            return table;
        }

        private Control BuildDescriptionSection(string description)
        {
            var section = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(12, 4, 12, 8)
            };

            section.Controls.Add(new Label
            {
                Text = "Description",
                Font = new Font(Font, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 4)
            });

            foreach (var segment in BbCodeText.ToSegments(description))
            {
                Control control = segment.Kind switch
                {
                    DescriptionSegmentKind.Image => BuildImageControl(segment.Value),
                    DescriptionSegmentKind.Quote => BuildQuoteControl(segment.Value),
                    DescriptionSegmentKind.Heading => BuildHeadingControl(segment.Value, segment.Extra),
                    DescriptionSegmentKind.List => BuildListControl(segment.Value, segment.Extra),
                    _ when IsDividerText(segment.Value) => BuildDividerControl(),
                    _ => BuildTextControl(segment.Value)
                };
                section.Controls.Add(control);
            }

            return section;
        }

        // Mirrors the website's <ul>/<ol> indentation: a narrow bullet/number column plus a wrapped-text
        // column, with the whole block indented from the surrounding paragraph's left margin, so wrapped
        // lines align under the item's text rather than under the bullet (a real hanging indent) instead of
        // the previous flat "\n  - " inline prefix that had no actual indentation.
        private Control BuildListControl(string value, int extra)
        {
            var isNumbered = extra == 1;
            var items = value.Split(BbCodeText.ListItemSeparator);

            const int bulletColumnWidth = 22;
            const int indent = 16;
            var list = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Margin = new Padding(indent, 4, 0, 8),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            list.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, bulletColumnWidth));
            list.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            for (var i = 0; i < items.Length; i++)
            {
                list.RowCount++;
                list.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var bulletLabel = new Label
                {
                    Text = isNumbered ? $"{i + 1}." : "•",
                    AutoSize = true,
                    Margin = new Padding(0, 2, 4, 2),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top
                };
                list.Controls.Add(bulletLabel, 0, i);

                var itemControl = BuildTextControl(items[i], width => Math.Max(50, width - bulletColumnWidth - indent));
                itemControl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                list.Controls.Add(itemControl, 1, i);
            }

            return list;
        }

        // Mirrors how the website renders [SIZE="N"] section titles as progressively larger <font> headers,
        // and [B]-only lines (HeadingSize 0) as bold sub-headers at the body text size.
        private Control BuildHeadingControl(string text, int headingSize)
        {
            var fontSize = headingSize switch
            {
                <= 2 => Font.Size,
                3 => Font.Size + 1,
                4 => Font.Size + 2,
                _ => Font.Size + 4
            };

            var label = new Label
            {
                Text = text,
                Font = new Font(Font.FontFamily, fontSize, FontStyle.Bold),
                AutoSize = true,
                MaximumSize = new Size(_contentWidth, 0),
                Margin = new Padding(0, headingSize == 0 ? 4 : 10, 0, 4)
            };
            _contentWidthHandlers.Add(width => label.MaximumSize = new Size(width, 0));
            return label;
        }

        // ESOUI authors often use a run of underscores/dashes as a plain-text section divider. Those are
        // long unbreakable (no-space) runs, so a word-wrapping RichTextBox can't size itself correctly for
        // them; render them as an actual rule instead of trying to lay them out as text.
        private static bool IsDividerText(string text)
        {
            var trimmed = text.Trim();
            return trimmed.Length >= 5 && trimmed.All(c => c is '_' or '-' or '=');
        }

        private Control BuildDividerControl()
        {
            var divider = new Panel
            {
                Width = _contentWidth,
                Height = 2,
                BackColor = SystemColors.ControlDark,
                Margin = new Padding(0, 6, 0, 10)
            };
            _contentWidthHandlers.Add(width => divider.Width = width);
            return divider;
        }

        // A RichTextBox used purely for read-only display (link detection) still shows a blinking caret and
        // takes keyboard focus on click like a real edit control. The wrapped native RichEdit control grabs
        // OS-level focus on click regardless of ControlStyles.Selectable, so swallow WM_SETFOCUS before it
        // reaches the native control or WinForms' own focus handling. Link clicks still work, since RichEdit
        // hit-tests links independently of focus.
        private sealed class NonInteractiveRichTextBox : RichTextBox
        {
            private const int WM_SETFOCUS = 0x0007;

            public NonInteractiveRichTextBox()
            {
                SetStyle(ControlStyles.Selectable, false);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_SETFOCUS)
                {
                    return;
                }
                base.WndProc(ref m);
            }
        }

        // widthSelector maps the dialog's current content width to this control's own width — identity for a
        // normal top-level paragraph, or a reduced width for a list item that needs to leave room for its
        // bullet column. Without this, BuildListControl's own narrower-width handler and a generic
        // full-width handler registered here would fight over the same control on every resize.
        private Control BuildTextControl(string text, Func<int, int>? widthSelector = null)
        {
            widthSelector ??= width => width;

            var textBox = new NonInteractiveRichTextBox
            {
                Text = text,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                DetectUrls = true,
                ScrollBars = RichTextBoxScrollBars.None,
                BackColor = SystemColors.Window,
                Cursor = Cursors.Default,
                HideSelection = true,
                Width = widthSelector(_contentWidth),
                Height = TextRenderer.MeasureText("A", new Font(Font, FontStyle.Regular)).Height + 6,
                Margin = new Padding(0, 2, 0, 6),
                TabStop = false
            };
            textBox.LinkClicked += (_, e) =>
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.LinkText!) { UseShellExecute = true });
            textBox.MouseWheel += (_, e) => ContentPanel_MouseWheel(textBox, e);
            // Let the real RichEdit layout engine (which can hard-wrap even unbreakable runs) report the
            // size it actually needs, rather than approximating with TextRenderer.MeasureText.
            textBox.ContentsResized += (_, e) =>
            {
                var newHeight = e.NewRectangle.Height + 6;
                if (textBox.Height != newHeight)
                {
                    textBox.Height = newHeight;
                }
            };
            _contentWidthHandlers.Add(width => textBox.Width = widthSelector(width));

            return textBox;
        }

        // [QUOTE] blocks render on the website as a bordered, monospaced <pre> box (most often pasted addon
        // source code) — give them the same visual treatment instead of flattening them into a plain
        // paragraph. WordWrap is off, matching the website's own "overflow: auto" behavior for long lines, so
        // height is just line count * line height rather than something that needs a resize notification.
        //
        // Width is sized to the code's own longest line, not the dialog's full content width — a code block
        // that needs 300px of width shouldn't stretch to fill a maximized window, it should just stay 300px
        // (or shrink below that, with the box's own horizontal scrollbar taking over, if the dialog is
        // narrower than the content needs).
        private Control BuildQuoteControl(string text)
        {
            var font = new Font("Consolas", 9F);
            var lines = text.Split('\n');
            var lineHeight = TextRenderer.MeasureText("A", font).Height;
            var naturalWidth = lines.Max(line => TextRenderer.MeasureText(line, font).Width) + 8;

            var textBox = new NonInteractiveRichTextBox
            {
                Text = text,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                DetectUrls = true,
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.Horizontal,
                Font = font,
                BackColor = Color.FromArgb(245, 245, 245),
                Cursor = Cursors.Default,
                HideSelection = true,
                Width = Math.Min(naturalWidth, _contentWidth),
                Height = (lines.Length * lineHeight) + 10,
                Margin = new Padding(0, 4, 0, 8),
                TabStop = false
            };
            textBox.LinkClicked += (_, e) =>
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.LinkText!) { UseShellExecute = true });
            textBox.MouseWheel += (_, e) => ContentPanel_MouseWheel(textBox, e);
            _contentWidthHandlers.Add(width => textBox.Width = Math.Min(naturalWidth, width));

            return textBox;
        }

        private Control BuildImageControl(string url)
        {
            var pictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(FixedMediaWidth, DescriptionImageHeight),
                Margin = new Padding(0, 2, 0, 6),
                WaitOnLoad = false
            };
            pictureBox.LoadCompleted += (_, e) =>
            {
                if (e.Error is not null || pictureBox.Image is null)
                {
                    pictureBox.Visible = false;
                    return;
                }

                // Size the box to exactly match the scaled image instead of leaving it letterboxed inside a
                // fixed-size box, so every image's left edge lines up with the description text above it.
                pictureBox.Size = ScaledImageSize(pictureBox.Image, FixedMediaWidth);

                pictureBox.Cursor = Cursors.Hand;
                pictureBox.Click += (_, _) => ShowImagePreview(pictureBox);
            };

            try
            {
                pictureBox.LoadAsync(url);
            }
            catch
            {
                pictureBox.Visible = false;
            }

            return pictureBox;
        }

        private static Size ScaledImageSize(Image image, int maxWidth)
        {
            var scale = Math.Min((double)maxWidth / image.Width, (double)DescriptionImageHeight / image.Height);
            return new Size((int)(image.Width * scale), (int)(image.Height * scale));
        }

        private void ShowImagePreview(PictureBox source)
        {
            var image = source.Image;
            if (image is null)
            {
                return;
            }

            var screen = Screen.FromControl(source).WorkingArea;
            var maxWidth = (int)(screen.Width * 0.85);
            var maxHeight = (int)(screen.Height * 0.85);

            // Zoom in noticeably relative to how the image is currently displayed in its thumbnail box,
            // rather than capping at the image's native resolution (which left small/already-fit images no
            // bigger, or even smaller, than their thumbnail).
            var thumbnailScale = Math.Min((double)source.Width / image.Width, (double)source.Height / image.Height);
            var scale = thumbnailScale * 2.5;
            var width = (int)(image.Width * scale);
            var height = (int)(image.Height * scale);

            if (width > maxWidth || height > maxHeight)
            {
                var shrink = Math.Min((double)maxWidth / width, (double)maxHeight / height);
                width = (int)(width * shrink);
                height = (int)(height * shrink);
            }

            width = Math.Max(200, width);
            height = Math.Max(150, height);

            using var previewForm = new Form
            {
                Text = "Image Preview",
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.Black,
                ClientSize = new Size(width, height),
                ShowIcon = false,
                ShowInTaskbar = false,
                KeyPreview = true
            };

            var previewPictureBox = new PictureBox
            {
                Image = image,
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };
            previewPictureBox.Click += (_, _) => previewForm.Close();
            previewForm.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    previewForm.Close();
                }
            };
            // The thumbnail PictureBox still owns and will dispose this Image; detach our reference first.
            previewForm.FormClosed += (_, _) => previewPictureBox.Image = null;

            previewForm.Controls.Add(previewPictureBox);
            previewForm.ShowDialog(source.FindForm());
        }

        private static LinkLabel CreateLinkValue(string url)
        {
            var link = new LinkLabel { Text = url, AutoSize = true, Margin = new Padding(0, 4, 0, 4) };
            link.LinkClicked += (_, _) =>
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            return link;
        }
    }
}
