using System.Collections.ObjectModel;

namespace MauiApp1
{

    // Updated chart drawable to support multiple series
    public class MauiChartDrawable : IDrawable
    {
        public ObservableCollection<ChartSeries> Series { get; set; }
        public string XAxisLabel { get; set; } = "X";
        public string YAxisLabel { get; set; } = "Y";
        public bool AutoScale { get; set; } = true;
        public float MinX { get; set; } = 0;
        public float MaxX { get; set; } = 20;
        public float MinY { get; set; } = 0;
        public float MaxY { get; set; } = 100;

        public MauiChartDrawable()
        {
            Series = new ObservableCollection<ChartSeries>();
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.White;
            canvas.FillRectangle(dirtyRect);

            float width = dirtyRect.Width;
            float height = dirtyRect.Height;
            float leftPadding = 70;
            float rightPadding = 50;
            float topPadding = 40;
            float bottomPadding = 60;

            // Calculate bounds from all visible series
            if (AutoScale && Series.Any(s => s.IsVisible))
            {
                var visibleSeries = Series.Where(s => s.IsVisible).ToList();
                if (visibleSeries.Count > 0)
                {
                    var bounds = visibleSeries.Select(s => s.GetBounds()).ToList();
                    MinX = bounds.Min(b => b.minX);
                    MaxX = bounds.Max(b => b.maxX);
                    MinY = bounds.Min(b => b.minY);
                    MaxY = bounds.Max(b => b.maxY);

                    // Add some padding
                    float xRange = MaxX - MinX;
                    float yRange = MaxY - MinY;
                    if (xRange > 0)
                    {
                        MinX -= xRange * 0.05f;
                        MaxX += xRange * 0.05f;
                    }
                    if (yRange > 0)
                    {
                        MinY -= yRange * 0.05f;
                        MaxY += yRange * 0.05f;
                    }
                }
            }

            // Ensure we have valid ranges
            if (MaxX <= MinX) MaxX = MinX + 1;
            if (MaxY <= MinY) MaxY = MinY + 1;

            // Draw axes
            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 2;
            canvas.DrawLine(leftPadding, height - bottomPadding, width - rightPadding, height - bottomPadding);
            canvas.DrawLine(leftPadding, topPadding, leftPadding, height - bottomPadding);

            // Calculate scaling
            float scaleX = (width - leftPadding - rightPadding) / (MaxX - MinX);
            float scaleY = (height - topPadding - bottomPadding) / (MaxY - MinY);

            // Draw grid lines and Y-axis labels
            canvas.StrokeColor = Colors.LightGray;
            canvas.StrokeSize = 1;
            canvas.FontColor = Colors.Black;
            canvas.FontSize = 12;

            int ySteps = 5;
            for (int i = 0; i <= ySteps; i++)
            {
                float yValue = MinY + (MaxY - MinY) * i / ySteps;
                float y = height - bottomPadding - (yValue - MinY) * scaleY;
                canvas.DrawLine(leftPadding - 5, y, width - rightPadding, y);
                canvas.DrawString(yValue.ToString("F1"), leftPadding - 15, y - 6, HorizontalAlignment.Right);
            }

            // Draw grid lines and X-axis labels
            int xSteps = 5;
            for (int i = 0; i <= xSteps; i++)
            {
                float xValue = MinX + (MaxX - MinX) * i / xSteps;
                float x = leftPadding + (xValue - MinX) * scaleX;
                canvas.DrawLine(x, height - bottomPadding, x, topPadding);
                canvas.DrawString(xValue.ToString("F1"), x, height - bottomPadding + 15, HorizontalAlignment.Center);
            }

            // Draw axis labels
            canvas.FontSize = 14;
            canvas.DrawString(XAxisLabel, width / 2, height - 10, HorizontalAlignment.Center);

            canvas.SaveState();
            canvas.Rotate(-90, leftPadding - 50, height / 2);
            canvas.DrawString(YAxisLabel, leftPadding - 50, height / 2, HorizontalAlignment.Center);
            canvas.RestoreState();

            // Draw all series (adjusting for MinX/MinY offset)
            float adjustedScaleX = scaleX;
            float adjustedScaleY = scaleY;
            float xOffset = MinX * scaleX;
            float yOffset = MinY * scaleY;

            foreach (var series in Series.Where(s => s.IsVisible))
            {
                canvas.SaveState();
                series.Draw(canvas, dirtyRect, leftPadding - xOffset, rightPadding,
                           topPadding, bottomPadding - yOffset, adjustedScaleX, adjustedScaleY);
                canvas.RestoreState();
            }

            // Draw legend
            DrawLegend(canvas, width, topPadding);
        }

        private void DrawLegend(ICanvas canvas, float width, float topPadding)
        {
            float legendX = width - 160;
            float legendY = topPadding + 10;
            float lineLength = 20;
            float spacing = 25;

            canvas.FontSize = 12;

            int index = 0;
            foreach (var series in Series.Where(s => s.IsVisible))
            {
                float y = legendY + index * spacing;

                canvas.StrokeColor = series.Color;
                canvas.StrokeSize = 2;
                canvas.DrawLine(legendX, y, legendX + lineLength, y);

                canvas.FontColor = Colors.Black;
                canvas.DrawString(series.Name, legendX + lineLength + 5, y - 6, HorizontalAlignment.Left);

                index++;
            }
        }
    }
}
