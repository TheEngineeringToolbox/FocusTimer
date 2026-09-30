using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;

namespace FocusTimer.Services;

internal sealed class TrayIconRenderer : IDisposable
{
    private const int IconSize = 32;
    private readonly Bitmap _baseImage;

    public TrayIconRenderer()
    {
        var resource = System.Windows.Application.GetResourceStream(
            new Uri("pack://application:,,,/FocusTimer;component/Assets/FocusTimerTrayBase.png"))
            ?? throw new InvalidOperationException("Het ingebedde tray-icoon ontbreekt.");

        using (resource.Stream)
        using (var loaded = new Bitmap(resource.Stream))
        {
            _baseImage = new Bitmap(loaded);
        }
    }

    public Icon CreateIcon(TimerPhase phase, double remainingProgress, bool isRunning)
    {
        remainingProgress = Math.Clamp(remainingProgress, 0, 1);
        using var bitmap = new Bitmap(IconSize, IconSize, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.Transparent);
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var imageBounds = new Rectangle(5, 5, 22, 22);
            graphics.DrawImage(_baseImage, imageBounds);

            using var trackPen = new Pen(Color.FromArgb(105, 220, 225, 232), 2.75f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            var ringBounds = new RectangleF(2.25f, 2.25f, 27.5f, 27.5f);
            graphics.DrawArc(trackPen, ringBounds, -90, 360);

            var phaseColor = phase switch
            {
                TimerPhase.Focus => Color.FromArgb(59, 167, 255),
                TimerPhase.ShortBreak => Color.FromArgb(78, 203, 113),
                _ => Color.FromArgb(166, 107, 255)
            };
            if (!isRunning)
                phaseColor = Color.FromArgb(165, phaseColor.R, phaseColor.G, phaseColor.B);

            using var progressPen = new Pen(phaseColor, 3.25f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            graphics.DrawArc(progressPen, ringBounds, -90, (float)(360 * remainingProgress));
        }

        var nativeHandle = bitmap.GetHicon();
        try
        {
            using var temporary = Icon.FromHandle(nativeHandle);
            return (Icon)temporary.Clone();
        }
        finally
        {
            DestroyIcon(nativeHandle);
        }
    }

    public void Dispose() => _baseImage.Dispose();

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr handle);
}
