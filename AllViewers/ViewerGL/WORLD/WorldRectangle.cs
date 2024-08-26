
namespace ViewerGL
{
    using System.Linq;
    public struct WorldRectangle
    {
        public float WorldLeft;
        public float WorldRight;
        public float WorldBottom;
        public float WorldTop;

        public float X => WorldLeft;
        public float Y => WorldBottom;

        
        /// <summary>
        /// Ширина региона
        /// </summary>
        public float CenterX
        {
            get => (this.WorldRight + this.WorldLeft)*0.5f;
        }
        public float CenterY
        {
            get => (this.WorldTop + this.WorldBottom) * 0.5f;
        }
        /// <summary>
        /// Ширина региона
        /// </summary>
        public float Width
        {
            get => this.WorldRight - this.WorldLeft;
        }
        /// <summary>
        /// Высота региона
        /// </summary>
        public float Height
        {
            get => this.WorldTop - this.WorldBottom;
        }
        public WorldRectangle(float worldLeft, float worldBottom, float worldRight,  float worldTop)
        {
            WorldLeft = worldLeft;
            WorldRight = worldRight;
            WorldBottom = worldBottom;
            WorldTop = worldTop;
        }
        public WorldRectangle(double[] x, double[] y)
        {
            WorldLeft =(float)x.Min();
            WorldRight = (float)x.Max();
            WorldBottom = (float)y.Min();
            WorldTop = (float)y.Max();
        }
        public WorldRectangle(float[] x, float[] y)
        {
            WorldLeft = x.Min();
            WorldRight = x.Max();
            WorldBottom = y.Min();
            WorldTop = y.Max();
        }
        public bool ViewportContains(float x, float y)
        {
            return (x > WorldLeft && x < WorldRight
                 && y > WorldBottom && y < WorldTop);
        }
        public void ToContains(ref float x,ref float y)
        {
            if (x < WorldLeft)
            {
                x = WorldLeft;
            }
            else if (x > WorldRight)
            {
                x = WorldRight;
            }
            if (y < WorldBottom)
            {
                y = WorldBottom;
            }
            else if (y > WorldTop)
            {
                y = WorldTop;
            }
        }
        public float RectangleScale()
        {
            return (Width < Height) ? Height: Width;
        }
    }
}
