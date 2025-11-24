using System.Collections.ObjectModel;

namespace MauiApp1
{
    // Line series with connected points
    public class LineSeries : ChartSeries
    {
        public ObservableCollection<(float x, float y)> DataPoints { get; set; }
        public float LineWidth { get; set; } = 2;
        public bool ShowPoints { get; set; } = true;
        public float PointRadius { get; set; } = 4;

        public LineSeries(string name, Color color) : base(name, color)
        {
            DataPoints = new ObservableCollection<(float x, float y)>();
        }

        public LineSeries(string name, Color color, ObservableCollection<(float x, float y)> dataPoints)
            : base(name, color)
        {
            DataPoints = dataPoints;
        }

        public override void Draw(ICanvas canvas, RectF dirtyRect, float leftPadding, float rightPadding,
                                  float topPadding, float bottomPadding, float scaleX, float scaleY)
        {
            if (!IsVisible || DataPoints.Count == 0)
                return;

            float height = dirtyRect.Height;

            // Draw line connecting points
            canvas.StrokeColor = Color;
            canvas.StrokeSize = LineWidth;

            for (int i = 0; i < DataPoints.Count - 1; i++)
            {
                float x1 = leftPadding + DataPoints[i].x * scaleX;
                float y1 = height - bottomPadding - DataPoints[i].y * scaleY;
                float x2 = leftPadding + DataPoints[i + 1].x * scaleX;
                float y2 = height - bottomPadding - DataPoints[i + 1].y * scaleY;
                canvas.DrawLine(x1, y1, x2, y2);
            }

            // Draw points
            if (ShowPoints)
            {
                canvas.FillColor = Color;
                foreach (var point in DataPoints)
                {
                    float x = leftPadding + point.x * scaleX;
                    float y = height - bottomPadding - point.y * scaleY;
                    canvas.FillCircle(x, y, PointRadius);
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
                DataPoints.Min(p => p.y),
                DataPoints.Max(p => p.y)
            );
        }
    }
}
