using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PreySense.UI
{
    public class RCheckBox : CheckBox
    {
        private int borderRadius = 5;
        public int BorderRadius
        {
            get => borderRadius;
            set => borderRadius = value;
        }

        private bool showBorder = true;
        public bool ShowBorder
        {
            get => showBorder;
            set => showBorder = value;
        }

        public RCheckBox()
        {
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            if (!Enabled)
            {
                PaintDisabled(pevent);
                return;
            }

            if (BackColor == Color.Transparent && Parent != null)
            {
                pevent.Graphics.Clear(Parent.BackColor);
            }
            else
            {
                pevent.Graphics.Clear(BackColor);
            }

            float ratio = pevent.Graphics.DpiX / 192.0f;
            int radius = (int)Math.Round(ratio * borderRadius, MidpointRounding.AwayFromZero);

            if (showBorder && Parent != null)
            {
                Rectangle outerRect = new Rectangle(0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
                using (GraphicsPath roundedPath = RComboBox.RoundedRect(outerRect, radius, radius))
                using (GraphicsPath cutoutPath = new GraphicsPath())
                using (Brush parentBrush = new SolidBrush(Parent.BackColor))
                {
                    cutoutPath.AddRectangle(ClientRectangle);
                    cutoutPath.AddPath(roundedPath, false);
                    cutoutPath.FillMode = FillMode.Alternate;
                    pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    pevent.Graphics.FillPath(parentBrush, cutoutPath);
                }

                Color sideColor = FlatAppearance.BorderColor.A > 0 ? FlatAppearance.BorderColor : RForm.borderSecond;
                ControlHelper.DrawGradientBorder(pevent.Graphics, outerRect, sideColor, radius, 1f, PenAlignment.Center);
            }

            int boxSize = Math.Max(12, (int)Math.Round(14 * ratio));
            int boxY = Math.Max(0, (ClientSize.Height - boxSize) / 2);
            int boxX = Padding.Left;
            Rectangle borderRect = new Rectangle(boxX, boxY, boxSize, boxSize);

            bool isLight = UiTheme.IsLightTheme();
            Color boxBg = Checked ? UiTheme.Accent : (isLight ? Color.FromArgb(255, 255, 255, 255) : Color.FromArgb(255, 30, 30, 30));
            Color boxBorder = Checked ? UiTheme.Accent : (isLight ? Color.FromArgb(255, 180, 180, 180) : Color.FromArgb(255, 75, 75, 75));

            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath roundedPath = RComboBox.RoundedRect(borderRect, Math.Max(1, radius / 2), Math.Max(1, radius / 2)))
            using (Brush fill = new SolidBrush(boxBg))
            using (Pen pen = new Pen(boxBorder, 1f))
            {
                pevent.Graphics.FillPath(fill, roundedPath);
                pevent.Graphics.DrawPath(pen, roundedPath);
            }

            if (Checked)
            {
                Color checkmarkColor = Color.White;
                using var pen = new Pen(checkmarkColor, Math.Max(1.5f, ratio * 2f))
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };
                int inset = Math.Max(2, (int)Math.Round(3 * ratio));
                pevent.Graphics.DrawLine(pen, borderRect.Left + inset, borderRect.Top + borderRect.Height / 2,
                    borderRect.Left + borderRect.Width / 2, borderRect.Bottom - inset);
                pevent.Graphics.DrawLine(pen, borderRect.Left + borderRect.Width / 2, borderRect.Bottom - inset,
                    borderRect.Right - inset, borderRect.Top + inset);
            }

            int textX = borderRect.Right + Math.Max(6, (int)Math.Round(8 * ratio));
            Rectangle textRect = new Rectangle(textX, 0, Math.Max(0, ClientSize.Width - textX), ClientSize.Height);
            TextRenderer.DrawText(pevent.Graphics, Text, Font, textRect, ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private void PaintDisabled(PaintEventArgs pevent)
        {
            if (BackColor == Color.Transparent && Parent != null)
            {
                pevent.Graphics.Clear(Parent.BackColor);
            }
            else
            {
                pevent.Graphics.Clear(BackColor);
            }

            float ratio = pevent.Graphics.DpiX / 192.0f;
            int radius = (int)Math.Round(ratio * borderRadius, MidpointRounding.AwayFromZero);

            if (showBorder && Parent != null)
            {
                Rectangle outerRect = new Rectangle(0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
                using (GraphicsPath roundedPath = RComboBox.RoundedRect(outerRect, radius, radius))
                using (GraphicsPath cutoutPath = new GraphicsPath())
                using (Brush parentBrush = new SolidBrush(Parent.BackColor))
                {
                    cutoutPath.AddRectangle(ClientRectangle);
                    cutoutPath.AddPath(roundedPath, false);
                    cutoutPath.FillMode = FillMode.Alternate;
                    pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    pevent.Graphics.FillPath(parentBrush, cutoutPath);
                }

                Color sideColor = FlatAppearance.BorderColor.A > 0 ? FlatAppearance.BorderColor : RForm.borderSecond;
                Color disabledOuterBorder = Color.FromArgb(120, sideColor);
                ControlHelper.DrawGradientBorder(pevent.Graphics, outerRect, disabledOuterBorder, radius, 1f, PenAlignment.Center);
            }

            int boxSize = Math.Max(12, (int)Math.Round(14 * ratio));
            int boxY = Math.Max(0, (ClientSize.Height - boxSize) / 2);
            int boxX = Padding.Left;
            Rectangle borderRect = new Rectangle(boxX, boxY, boxSize, boxSize);
            Color disabledBorder = Color.FromArgb(255, 95, 95, 95);
            Color disabledText = Color.FromArgb(255, 140, 140, 140);

            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath roundedPath = RComboBox.RoundedRect(borderRect, Math.Max(1, radius / 2), Math.Max(1, radius / 2)))
            using (Brush fill = new SolidBrush(Color.FromArgb(255, 40, 40, 40)))
            using (Pen pen = new Pen(disabledBorder, 1f))
            {
                pevent.Graphics.FillPath(fill, roundedPath);
                pevent.Graphics.DrawPath(pen, roundedPath);
            }

            if (Checked)
            {
                using var pen = new Pen(disabledText, Math.Max(1f, ratio * 1.5f))
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };
                int inset = Math.Max(2, (int)Math.Round(3 * ratio));
                pevent.Graphics.DrawLine(pen, borderRect.Left + inset, borderRect.Top + borderRect.Height / 2,
                    borderRect.Left + borderRect.Width / 2, borderRect.Bottom - inset);
                pevent.Graphics.DrawLine(pen, borderRect.Left + borderRect.Width / 2, borderRect.Bottom - inset,
                    borderRect.Right - inset, borderRect.Top + inset);
            }

            int textX = borderRect.Right + Math.Max(6, (int)Math.Round(8 * ratio));
            Rectangle textRect = new Rectangle(textX, 0, Math.Max(0, ClientSize.Width - textX), ClientSize.Height);
            TextRenderer.DrawText(pevent.Graphics, Text, Font, textRect, disabledText,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
    }
}
