using System.Drawing;

namespace PoEStashSorterModels
{
    public struct StashPosSize
    {
        private readonly string toolTip;

        public StashPosSize(int width, int height, Rectangle rect)
        {
            Width = width;
            Height = height;
            Rect = rect;
            Text = toolTip = null;
        }

        public StashPosSize(string text, string tooltip = null)
        {
            Width = Height = 0;
            Rect = new Rectangle();
            Text = text;
            toolTip = tooltip;
        }

        public Rectangle Rect { get; }
        public int Height { get; }
        public int Width { get; }
        public string Text { get; }
        public string Tooltip => toolTip ?? ToString();

        public override string ToString()
        {
            if (Text != null)
                return Text;
            return $"{Width}x{Height}";
        }

        public override int GetHashCode()
        {
            return Height*1000000 + Width;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || typeof (StashPosSize) != obj.GetType())
            {
                return false;
            }
            return GetHashCode() == obj.GetHashCode();
        }
    }
}