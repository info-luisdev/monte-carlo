using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

namespace Epidemia.Benchmark;

public static class AnimationGenerator
{
    private static readonly Rgba32 Background = new(20, 20, 20);

    public static void Generate(
        string outputPath,
        List<byte[]> leftFrames,
        List<byte[]> rightFrames,
        int gridWidth,
        int gridHeight,
        int scaleFactor = 4)
    {
        int panelW = gridWidth / scaleFactor;
        int panelH = gridHeight / scaleFactor;
        const int gap = 6;
        int totalW = panelW * 2 + gap;
        int totalH = panelH;
        int frameCount = Math.Min(leftFrames.Count, rightFrames.Count);

        using var gif = new Image<Rgba32>(totalW, totalH, Background);
        gif.Metadata.GetGifMetadata().RepeatCount = 0;

        for (int f = 0; f < frameCount; f++)
        {
            using var frame = new Image<Rgba32>(totalW, totalH, Background);

            RenderPanel(frame, leftFrames[f], gridWidth, gridHeight, 0, 0, scaleFactor);
            RenderPanel(frame, rightFrames[f], gridWidth, gridHeight, panelW + gap, 0, scaleFactor);

            frame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = 12;
            gif.Frames.AddFrame(frame.Frames.RootFrame);
        }

        gif.Frames.RemoveFrame(0);
        gif.SaveAsGif(outputPath);
    }

    private static void RenderPanel(
        Image<Rgba32> image,
        byte[] grid,
        int gridW, int gridH,
        int offsetX, int offsetY,
        int scale)
    {
        for (int gy = 0; gy < gridH; gy += scale)
        for (int gx = 0; gx < gridW; gx += scale)
        {
            int px = offsetX + gx / scale;
            int py = offsetY + gy / scale;

            if (px >= image.Width || py >= image.Height)
                continue;

            image[px, py] = ToColor(grid[gy * gridW + gx]);
        }
    }

    private static Rgba32 ToColor(byte state) => state switch
    {
        1 => new Rgba32(244, 67, 54),
        2 => new Rgba32(33, 150, 243),
        3 => new Rgba32(97, 97, 97),
        _ => new Rgba32(76, 175, 80)
    };
}
