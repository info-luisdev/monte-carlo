namespace Epidemia.Core;

public static class Grid
{
    public static byte[] Create(SimulationConfig config)
    {
        var buffer = new byte[config.BufferLength];
        PlaceInitialInfected(buffer, config);
        return buffer;
    }

    public static int CountInfectedNeighbors(byte[] grid, int index, int stride)
    {
        const byte infected = (byte)CellState.Infected;
        int count = 0;

        if (grid[index - stride - 1] == infected) count++;
        if (grid[index - stride] == infected) count++;
        if (grid[index - stride + 1] == infected) count++;
        if (grid[index - 1] == infected) count++;
        if (grid[index + 1] == infected) count++;
        if (grid[index + stride - 1] == infected) count++;
        if (grid[index + stride] == infected) count++;
        if (grid[index + stride + 1] == infected) count++;

        return count;
    }

    public static byte[] CaptureFrame(byte[] buffer, SimulationConfig config)
    {
        var frame = new byte[config.TotalCells];
        int stride = config.Stride;

        for (int y = 0; y < config.Height; y++)
            Buffer.BlockCopy(buffer, (y + 1) * stride + 1, frame, y * config.Width, config.Width);

        return frame;
    }

    private static void PlaceInitialInfected(byte[] buffer, SimulationConfig config)
    {
        int stride = config.Stride;
        int cx = config.Width / 2;
        int cy = config.Height / 2;
        int placed = 0;

        for (int radius = 0; placed < config.InitialInfectedCount; radius++)
        for (int dy = -radius; dy <= radius && placed < config.InitialInfectedCount; dy++)
        for (int dx = -radius; dx <= radius && placed < config.InitialInfectedCount; dx++)
        {
            if (radius > 0 && Math.Abs(dx) < radius && Math.Abs(dy) < radius)
                continue;

            int x = cx + dx;
            int y = cy + dy;

            if (x < 0 || x >= config.Width || y < 0 || y >= config.Height)
                continue;

            buffer[(y + 1) * stride + (x + 1)] = (byte)CellState.Infected;
            placed++;
        }
    }
}
