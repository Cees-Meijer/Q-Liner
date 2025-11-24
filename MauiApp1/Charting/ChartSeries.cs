namespace MauiApp1
{
    // Base class for all series
    public abstract class ChartSeries
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public bool IsVisible { get; set; } = true;

        protected ChartSeries(string name, Color color)
        {
            Name = name;
            Color = color;
        }

        public abstract void Draw(ICanvas canvas, RectF dirtyRect, float leftPadding, float rightPadding,
                                  float topPadding, float bottomPadding, float scaleX, float scaleY);

        public abstract (float minX, float maxX, float minY, float maxY) GetBounds();
    }
}
