using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace DeathCounter;

internal sealed class CounterDisplayControl : Control
{
    private const int AnimationDurationMs = 160;
    private const int AnimationIntervalMs = 12;

    private readonly System.Windows.Forms.Timer animationTimer;
    private string currentText = "0";
    private string previousText = "0";
    private string nextText = "0";
    private int animationDirection = 1;
    private int firstChangedIndex = -1;
    private DateTime animationStartedAt;
    private bool isAnimating;

    public CounterDisplayControl()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = SystemColors.Control;
        ForeColor = Color.White;
        OutlineColor = Color.Black;
        CounterFontFamily = "Segoe UI";
        CounterFontSize = 96f;
        CounterFontBold = true;
        ShowOutline = true;

        animationTimer = new System.Windows.Forms.Timer
        {
            Interval = AnimationIntervalMs,
        };
        animationTimer.Tick += AnimationTimer_Tick;
    }

    public Color OutlineColor { get; set; }

    public string CounterFontFamily { get; set; }

    public float CounterFontSize { get; set; }

    public bool CounterFontBold { get; set; }

    public bool EnableCounterAnimation { get; set; } = true;

    public bool ShowOutline { get; set; } = true;

    public string DisplayText => currentText;

    public void SetCounterText(string text, bool animate, int direction)
    {
        text ??= "0";

        if (!animate || !EnableCounterAnimation || currentText == text)
        {
            animationTimer.Stop();
            isAnimating = false;
            currentText = text;
            previousText = text;
            nextText = text;
            Invalidate();
            return;
        }

        previousText = currentText;
        nextText = text;
        animationDirection = direction >= 0 ? 1 : -1;
        firstChangedIndex = GetFirstChangedIndex(previousText, nextText);

        if (firstChangedIndex < 0)
        {
            currentText = text;
            previousText = text;
            nextText = text;
            Invalidate();
            return;
        }

        isAnimating = true;
        animationStartedAt = DateTime.UtcNow;
        animationTimer.Start();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        using var font = CreateFontSafe();
        var lineHeight = GetLineHeight(font);

        if (!isAnimating || firstChangedIndex < 0)
        {
            DrawCenteredCounterText(e.Graphics, currentText, 0f, font, lineHeight);
            return;
        }

        var alignedTexts = AlignTexts(previousText, nextText);
        var oldAligned = alignedTexts.oldAligned;
        var newAligned = alignedTexts.newAligned;
        var prefix = oldAligned[..firstChangedIndex];
        var oldSuffix = oldAligned[firstChangedIndex..];
        var newSuffix = newAligned[firstChangedIndex..];

        var progress = GetAnimationProgress();
        var oldOffset = animationDirection > 0 ? progress * lineHeight : -progress * lineHeight;
        var newOffset = animationDirection > 0 ? (-lineHeight + (progress * lineHeight)) : (lineHeight - (progress * lineHeight));

        var prefixWidth = MeasureTextWidth(e.Graphics, prefix, font);
        var suffixWidth = Math.Max(
            MeasureTextWidth(e.Graphics, oldSuffix, font),
            MeasureTextWidth(e.Graphics, newSuffix, font));
        var totalWidth = prefixWidth + suffixWidth;
        var baseX = (Width - totalWidth) / 2f;
        var baseY = (Height - lineHeight) / 2f;

        DrawCounterText(e.Graphics, prefix, baseX, baseY, font);
        DrawCounterText(e.Graphics, oldSuffix, baseX + prefixWidth, baseY + oldOffset, font);
        DrawCounterText(e.Graphics, newSuffix, baseX + prefixWidth, baseY + newOffset, font);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            animationTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        if (GetAnimationProgress() >= 1f)
        {
            animationTimer.Stop();
            isAnimating = false;
            currentText = nextText;
            previousText = nextText;
            Invalidate();
            return;
        }

        Invalidate();
    }

    private float GetAnimationProgress()
    {
        var elapsed = (float)(DateTime.UtcNow - animationStartedAt).TotalMilliseconds;
        return Math.Clamp(elapsed / AnimationDurationMs, 0f, 1f);
    }

    private void DrawCenteredCounterText(Graphics graphics, string text, float verticalOffset, Font font, float lineHeight)
    {
        var baseX = (Width - MeasureTextWidth(graphics, text, font)) / 2f;
        var baseY = ((Height - lineHeight) / 2f) + verticalOffset;
        DrawCounterText(graphics, text, baseX, baseY, font);
    }

    private void DrawCounterText(Graphics graphics, string text, float x, float y, Font font)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        foreach (var ch in text)
        {
            var charText = ch.ToString();
            var charWidth = MeasureTextWidth(graphics, charText, font);

            if (ch != ' ')
            {
                using var path = new GraphicsPath();
                path.AddString(
                    charText,
                    font.FontFamily,
                    (int)font.Style,
                    graphics.DpiY * font.SizeInPoints / 72f,
                    new PointF(x, y),
                    StringFormat.GenericTypographic);

                using var fillBrush = new SolidBrush(ForeColor);
                if (ShowOutline)
                {
                    using var outlinePen = new Pen(OutlineColor, Math.Max(2f, CounterFontSize / 12f))
                    {
                        LineJoin = LineJoin.Round,
                    };
                    graphics.DrawPath(outlinePen, path);
                }

                graphics.FillPath(fillBrush, path);
            }

            x += charWidth;
        }
    }

    private float MeasureTextWidth(Graphics graphics, string text, Font font)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0f;
        }

        var size = graphics.MeasureString(text, font, PointF.Empty, StringFormat.GenericTypographic);
        return size.Width;
    }

    private float GetLineHeight(Font font) => font.GetHeight();

    private Font CreateFontSafe()
    {
        var style = CounterFontBold ? FontStyle.Bold : FontStyle.Regular;

        try
        {
            return new Font(CounterFontFamily, CounterFontSize, style, GraphicsUnit.Point);
        }
        catch
        {
            return new Font("Segoe UI", CounterFontSize, style, GraphicsUnit.Point);
        }
    }

    private static int GetFirstChangedIndex(string oldText, string newText)
    {
        var aligned = AlignTexts(oldText, newText);

        for (var i = 0; i < aligned.oldAligned.Length; i++)
        {
            if (aligned.oldAligned[i] != aligned.newAligned[i])
            {
                return i;
            }
        }

        return -1;
    }

    private static (string oldAligned, string newAligned) AlignTexts(string oldText, string newText)
    {
        var maxLength = Math.Max(oldText.Length, newText.Length);
        return (oldText.PadLeft(maxLength), newText.PadLeft(maxLength));
    }
}
