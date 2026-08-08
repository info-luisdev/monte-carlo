using System.Globalization;
using System.Text;

namespace Epidemia.Benchmark;

public static class SpeedupChartGenerator
{
    public static void GenerateSvg(string outputPath, List<(int Threads, double Speedup)> data)
    {
        const int width = 640;
        const int height = 420;
        const int marginLeft = 65;
        const int marginRight = 40;
        const int marginTop = 45;
        const int marginBottom = 55;

        int plotW = width - marginLeft - marginRight;
        int plotH = height - marginTop - marginBottom;
        int maxThreads = data.Max(d => d.Threads);
        double maxSpeedup = Math.Ceiling(Math.Max(data.Max(d => d.Speedup), maxThreads) + 0.5);

        var sb = new StringBuilder();
        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\">");
        sb.AppendLine("<rect width=\"100%\" height=\"100%\" fill=\"white\"/>");

        AppendTitle(sb, width);
        AppendAxes(sb, marginLeft, marginTop, marginBottom, width, height, marginRight);
        AppendGridLines(sb, maxSpeedup, plotH, marginLeft, marginBottom, height, width, marginRight);
        AppendIdealLine(sb, maxThreads, maxSpeedup, plotW, plotH, marginLeft, marginBottom, height);
        AppendDataSeries(sb, data, maxThreads, maxSpeedup, plotW, plotH, marginLeft, marginBottom, height);
        AppendAxisLabels(sb, data, maxThreads, plotW, marginLeft, marginBottom, width, height);

        sb.AppendLine("</svg>");
        File.WriteAllText(outputPath, sb.ToString());
    }

    private static void AppendTitle(StringBuilder sb, int width)
    {
        sb.AppendLine(Text(width / 2.0, 28, "Strong Scaling: Speed-up vs. Hilos", 16, "bold", "middle"));
    }

    private static void AppendAxes(StringBuilder sb, int ml, int mt, int mb, int w, int h, int mr)
    {
        sb.AppendLine(Line(ml, mt, ml, h - mb, "black", 2));
        sb.AppendLine(Line(ml, h - mb, w - mr, h - mb, "black", 2));
    }

    private static void AppendGridLines(
        StringBuilder sb, double maxSp, int plotH, int ml, int mb, int h, int w, int mr)
    {
        for (int tick = 0; tick <= (int)maxSp; tick++)
        {
            double py = h - mb - (tick / maxSp) * plotH;
            sb.AppendLine(Line(ml - 5, py, ml, py, "#666", 1));
            sb.AppendLine(Text(ml - 10, py + 4, $"{tick}×", 11, "normal", "end"));

            if (tick > 0)
                sb.AppendLine(Line(ml, py, w - mr, py, "#eee", 1));
        }
    }

    private static void AppendIdealLine(
        StringBuilder sb, int maxT, double maxSp, int plotW, int plotH, int ml, int mb, int h)
    {
        double x1 = ml + (1.0 / maxT) * plotW;
        double y1 = h - mb - (1.0 / maxSp) * plotH;
        double x2 = ml + plotW;
        double y2 = h - mb - ((double)maxT / maxSp) * plotH;

        sb.AppendLine($"<line x1=\"{F(x1)}\" y1=\"{F(y1)}\" x2=\"{F(x2)}\" y2=\"{F(y2)}\" " +
                       "stroke=\"#bbb\" stroke-width=\"1.5\" stroke-dasharray=\"6,4\"/>");
        sb.AppendLine(Text(x2 + 5, y2 + 4, "Ideal", 10, "normal", "start", "#999"));
    }

    private static void AppendDataSeries(
        StringBuilder sb, List<(int Threads, double Speedup)> data,
        int maxT, double maxSp, int plotW, int plotH, int ml, int mb, int h)
    {
        var points = data.Select(d => (
            px: ml + ((double)d.Threads / maxT) * plotW,
            py: h - mb - (d.Speedup / maxSp) * plotH,
            d.Speedup
        )).ToList();

        string pathD = string.Join(" ",
            points.Select((p, i) => $"{(i == 0 ? "M" : "L")}{F(p.px)},{F(p.py)}"));
        sb.AppendLine($"<path d=\"{pathD}\" fill=\"none\" stroke=\"#1976D2\" stroke-width=\"2.5\"/>");

        foreach (var p in points)
        {
            sb.AppendLine($"<circle cx=\"{F(p.px)}\" cy=\"{F(p.py)}\" r=\"5\" fill=\"#1976D2\"/>");
            sb.AppendLine(Text(p.px, p.py - 12, $"{F(p.Speedup, "F2")}×", 11, "bold", "middle", "#1976D2"));
        }
    }

    private static void AppendAxisLabels(
        StringBuilder sb, List<(int Threads, double Speedup)> data,
        int maxT, int plotW, int ml, int mb, int w, int h)
    {
        foreach (var d in data)
        {
            double px = ml + ((double)d.Threads / maxT) * plotW;
            sb.AppendLine(Text(px, h - mb + 20, d.Threads.ToString(), 12, "normal", "middle"));
        }

        sb.AppendLine(Text(w / 2.0, h - 8, "Hilos", 13, "normal", "middle"));
        sb.AppendLine($"<text x=\"18\" y=\"{h / 2}\" text-anchor=\"middle\" " +
                       $"font-family=\"Arial,sans-serif\" font-size=\"13\" " +
                       $"transform=\"rotate(-90,18,{h / 2})\">Speed-up</text>");
    }

    private static string F(double v, string fmt = "F1") =>
        v.ToString(fmt, CultureInfo.InvariantCulture);

    private static string Line(double x1, double y1, double x2, double y2, string color, double sw) =>
        $"<line x1=\"{F(x1)}\" y1=\"{F(y1)}\" x2=\"{F(x2)}\" y2=\"{F(y2)}\" " +
        $"stroke=\"{color}\" stroke-width=\"{F(sw)}\"/>";

    private static string Text(
        double x, double y, string text, int size, string weight, string anchor, string fill = "black") =>
        $"<text x=\"{F(x)}\" y=\"{F(y)}\" text-anchor=\"{anchor}\" " +
        $"font-family=\"Arial,sans-serif\" font-size=\"{size}\" font-weight=\"{weight}\" " +
        $"fill=\"{fill}\">{text}</text>";
}
