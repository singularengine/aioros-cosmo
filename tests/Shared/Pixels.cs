using SkiaSharp;

namespace Aioros.Cosmo.Testing;

/// <summary>The pixel verdict of the fidelity criteria: the share of pixels whose three channels each differ by at most a tolerance.</summary>
internal static class Pixels
{
    public const int Tolerance = 16;

    /// <summary>The share of matching pixels over the union of both images (a pixel only one image has counts as different).</summary>
    public static double Matched(SKBitmap ours, SKBitmap reference)
    {
        var width = Math.Max(ours.Width, reference.Width);
        var height = Math.Max(ours.Height, reference.Height);
        var same = 0;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (x < ours.Width && y < ours.Height && x < reference.Width && y < reference.Height && Near(ours.GetPixel(x, y), reference.GetPixel(x, y)))
                {
                    same++;
                }
            }
        }

        return (double)same / (width * height);
    }

    /// <summary>shadcn/ui's capture of a fixture in a theme.</summary>
    public static SKBitmap Reference(string fixture, string theme) => SKBitmap.Decode(Repository.Path("tools", "shadcn", "out", $"{fixture}.{theme}.png"));

    private static bool Near(SKColor a, SKColor b) =>
        Math.Abs(a.Red - b.Red) <= Tolerance && Math.Abs(a.Green - b.Green) <= Tolerance && Math.Abs(a.Blue - b.Blue) <= Tolerance;
}
