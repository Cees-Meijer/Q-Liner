using System.Collections.ObjectModel;

namespace MauiApp1
{
    public class LineChartDrawable : IDrawable
    {
        private readonly ObservableCollection<(float x, float y)> dataPoints;

        public LineChartDrawable(ObservableCollection<(float x, float y)> dataPoints)
        {
            this.dataPoints = dataPoints;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.White;
            canvas.FillRectangle(dirtyRect);

            if (dataPoints.Count == 0)
                return;

            float width = dirtyRect.Width;
            float height = dirtyRect.Height;
            float leftPadding = 70;
            float rightPadding = 50;
            float topPadding = 20;
            float bottomPadding = 50;

            // Draw axes
            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 2;
            canvas.DrawLine(leftPadding, height - bottomPadding, width - rightPadding, height - bottomPadding);
            canvas.DrawLine(leftPadding, topPadding, leftPadding, height - bottomPadding);

            // Calculate scaling
            float maxX = dataPoints.Max(p => p.x);
            float maxY = dataPoints.Max(p => p.y);
            float scaleX = (width - leftPadding - rightPadding) / maxX;
            float scaleY = (height - topPadding - bottomPadding) / maxY;

            // Draw grid lines and Y-axis labels
            canvas.StrokeColor = Colors.LightGray;
            canvas.StrokeSize = 1;
            canvas.FontColor = Colors.Black;
            canvas.FontSize = 12;

            for (int i = 0; i <= 100; i += 20)
            {
                float y = height - bottomPadding - (i / maxY) * (height - topPadding - bottomPadding);
                canvas.DrawLine(leftPadding - 5, y, width - rightPadding, y);
                canvas.DrawString(i.ToString(), leftPadding - 15, y - 6, HorizontalAlignment.Right);
            }

            // Draw grid lines and X-axis labels
            for (int i = 0; i <= 20; i += 5)
            {
                float x = leftPadding + (i / maxX) * (width - leftPadding - rightPadding);
                canvas.DrawLine(x, height - bottomPadding, x, topPadding);
                canvas.DrawString(i.ToString(), x, height - bottomPadding + 15, HorizontalAlignment.Center);
            }

            // Draw line connecting points
            canvas.StrokeColor = Colors.Blue;
            canvas.StrokeSize = 2;

            for (int i = 0; i < dataPoints.Count - 1; i++)
            {
                float x1 = leftPadding + dataPoints[i].x * scaleX;
                float y1 = height - bottomPadding - dataPoints[i].y * scaleY;
                float x2 = leftPadding + dataPoints[i + 1].x * scaleX;
                float y2 = height - bottomPadding - dataPoints[i + 1].y * scaleY;
                canvas.DrawLine(x1, y1, x2, y2);
            }

            // Draw points
            canvas.FillColor = Colors.Red;
            foreach (var point in dataPoints)
            {
                float x = leftPadding + point.x * scaleX;
                float y = height - bottomPadding - point.y * scaleY;
                canvas.FillCircle(x, y, 4);
            }
        }
    }
}