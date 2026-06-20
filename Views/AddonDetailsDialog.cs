using Teron_Addon_Manager.Services;

namespace Teron_Addon_Manager
{
    public partial class AddonDetailsDialog : Form
    {
        private const int DescriptionContentWidth = 400;
        private const int DescriptionImageHeight = 225;
        private const int ScrollStep = 60;

        private readonly FlowLayoutPanel _scrollContent;
        private readonly VScrollBar _scrollBar;

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

            _scrollContent.SizeChanged += (_, _) => UpdateScrollRange();
            contentPanel.SizeChanged += (_, _) => UpdateScrollRange();
            contentPanel.MouseWheel += ContentPanel_MouseWheel;

            UpdateScrollRange();
        }

        private void UpdateScrollRange()
        {
            var viewHeight = contentPanel.ClientSize.Height;
            var contentHeight = _scrollContent.Height;

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
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
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

                valueControl.MaximumSize = new Size(300, 0);
                valueControl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                table.Controls.Add(valueControl, 1, i);
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
                Control control;
                if (segment.IsImage)
                {
                    control = BuildImageControl(segment.Value);
                }
                else if (IsDividerText(segment.Value))
                {
                    control = BuildDividerControl();
                }
                else
                {
                    control = BuildTextControl(segment.Value);
                }
                section.Controls.Add(control);
            }

            return section;
        }

        // ESOUI authors often use a run of underscores/dashes as a plain-text section divider. Those are
        // long unbreakable (no-space) runs, so a word-wrapping RichTextBox can't size itself correctly for
        // them; render them as an actual rule instead of trying to lay them out as text.
        private static bool IsDividerText(string text)
        {
            var trimmed = text.Trim();
            return trimmed.Length >= 5 && trimmed.All(c => c is '_' or '-' or '=');
        }

        private static Control BuildDividerControl()
        {
            return new Panel
            {
                Width = DescriptionContentWidth,
                Height = 2,
                BackColor = SystemColors.ControlDark,
                Margin = new Padding(0, 6, 0, 10)
            };
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

        private Control BuildTextControl(string text)
        {
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
                Width = DescriptionContentWidth,
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
                    // Don't rely solely on this resize bubbling up through SizeChanged on every ancestor —
                    // recalculate the scroll range directly so the range never goes stale relative to a
                    // textbox that only just learned its real wrapped height (this is what was causing
                    // description text to get cut off with no way to scroll further to see the rest).
                    UpdateScrollRange();
                }
            };

            return textBox;
        }

        private Control BuildImageControl(string url)
        {
            var pictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(DescriptionContentWidth, DescriptionImageHeight),
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
                pictureBox.Size = ScaledImageSize(pictureBox.Image);
                // See the matching comment in BuildTextControl's ContentsResized handler: recalculate the
                // scroll range directly rather than counting on this resize to bubble up reliably.
                UpdateScrollRange();

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

        private static Size ScaledImageSize(Image image)
        {
            var scale = Math.Min((double)DescriptionContentWidth / image.Width, (double)DescriptionImageHeight / image.Height);
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

            // Zoom in noticeably relative to how the image is displayed in its thumbnail box, rather than
            // capping at the image's native resolution (which left small/already-fit images no bigger, or
            // even smaller, than their thumbnail).
            var thumbnailScale = Math.Min((double)DescriptionContentWidth / image.Width, (double)DescriptionImageHeight / image.Height);
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
