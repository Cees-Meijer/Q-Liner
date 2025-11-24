using System.Collections.ObjectModel;

namespace MauiApp1
{
    // Bar series for vertical bars
    public class BarSeries : ChartSeries
    {
        public ObservableCollection<(float x, float y)> DataPoints { get; set; }
        public float BarWidth { get; set; } = 0.8f; // As fraction of available space
        public bool ShowOutline { get; set; } = true;
        public Color OutlineColor { get; set; } = Colors.Black;

        public BarSeries(string name, Color color) : base(name, color)
        {
            DataPoints = new ObservableCollection<(float x, float y)>();
        }

        public override void Draw(ICanvas canvas, RectF dirtyRect, float leftPadding, float rightPadding,
                                  float topPadding, float bottomPadding, float scaleX, float scaleY)
        {
            if (!IsVisible || DataPoints.Count == 0)
                return;

            float height = dirtyRect.Height;
            canvas.FillColor = Color;

            float barWidthPixels = scaleX * BarWidth;

            foreach (var point in DataPoints)
            {
                float x = leftPadding + point.x * scaleX;
                float y = height - bottomPadding - point.y * scaleY;
                float barHeight = point.y * scaleY;

                float barLeft = x - barWidthPixels / 2;
                float barTop = y;

                canvas.FillRectangle(barLeft, barTop, barWidthPixels, barHeight);

                if (ShowOutline)
                {
                    canvas.StrokeColor = OutlineColor;
                    canvas.StrokeSize = 1;
                    canvas.DrawRectangle(barLeft, barTop, barWidthPixels, barHeight);
                }
            }
        }

        public override (float minX, float maxX, float minY, float maxY) GetBounds()
        {
            if (DataPoints.Count == 0)
                return (0, 0, 0, 0);

            return (
                DataPoints.Min(p => p.x),
                DataPoints.Max(p => p.x),
                0, // Bars typically start from zero
                DataPoints.Max(p => p.y)
            );
        }
    }
}
