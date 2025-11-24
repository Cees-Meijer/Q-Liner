using System.Collections.ObjectModel;

namespace MauiApp1
{
    // Vector series with arrows showing direction and magnitude
    public class VectorSeries : ChartSeries
    {
        public ObservableCollection<(float x, float y, float dx, float dy)> Vectors { get; set; }
        public float ArrowWidth { get; set; } = 2;
        public float ArrowHeadSize { get; set; } = 8;
        public bool NormalizeVectors { get; set; } = false;

        public VectorSeries(string name, Color color) : base(name, color)
        {
            Vectors = new ObservableCollection<(float x, float y, float dx, float dy)>();
        }

        public override void Draw(ICanvas canvas, RectF dirtyRect, float leftPadding, float rightPadding,
                                  float topPadding, float bottomPadding, float scaleX, float scaleY)
        {
            if (!IsVisible || Vectors.Count == 0)
                return;

            float height = dirtyRect.Height;
            canvas.StrokeColor = Color;
            canvas.StrokeSize = ArrowWidth;
            canvas.FillColor = Color;

            float maxMagnitude = 1f;
            if (NormalizeVectors)
            {
                maxMagnitude = Vectors.Max(v => (float)Math.Sqrt(v.dx * v.dx + v.dy * v.dy));
                if (maxMagnitude == 0) maxMagnitude = 1f;
            }

            foreach (var vector in Vectors)
            {
                float startX = leftPadding + vector.x * scaleX;
                float startY = height - bottomPadding - vector.y * scaleY;

                float dx = vector.dx;
                float dy = vector.dy;

                if (NormalizeVectors)
                {
                    float magnitude = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (magnitude > 0)
                    {
                        dx = dx / magnitude * (maxMagnitude / 2);
                        dy = dy / magnitude * (maxMagnitude / 2);
                    }
                }

                float endX = startX + dx * scaleX;
                float endY = startY - dy * scaleY;

                // Draw arrow line
                canvas.DrawLine(startX, startY, endX, endY);

                // Draw arrowhead
                float angle = (float)Math.Atan2(-(endY - startY), endX - startX);
                float arrowAngle1 = angle + (float)Math.PI * 0.85f;
                float arrowAngle2 = angle - (float)Math.PI * 0.85f;

                float arrowX1 = endX + ArrowHeadSize * (float)Math.Cos(arrowAngle1);
                float arrowY1 = endY + ArrowHeadSize * (float)Math.Sin(arrowAngle1);
                float arrowX2 = endX + ArrowHeadSize * (float)Math.Cos(arrowAngle2);
                float arrowY2 = endY + ArrowHeadSize * (float)Math.Sin(arrowAngle2);

                PathF arrowPath = new PathF();
                arrowPath.MoveTo(endX, endY);
                arrowPath.LineTo(arrowX1, arrowY1);
                arrowPath.LineTo(arrowX2, arrowY2);
                arrowPath.Close();
                canvas.FillPath(arrowPath);
            }
        }

        public override (float minX, float maxX, float minY, float maxY) GetBounds()
        {
            if (Vectors.Count == 0)
                return (0, 0, 0, 0);

            float minX = Vectors.Min(v => Math.Min(v.x, v.x + v.dx));
            float maxX = Vectors.Max(v => Math.Max(v.x, v.x + v.dx));
            float minY = Vectors.Min(v => Math.Min(v.y, v.y + v.dy));
            float maxY = Vectors.Max(v => Math.Max(v.y, v.y + v.dy));

            return (minX, maxX, minY, maxY);
        }
    }
}
