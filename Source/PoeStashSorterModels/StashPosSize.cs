using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POEStashSorterModels;

namespace PoeStashSorterModels
{
    public struct StashPosSize
    {
        public Rectangle Rect { get; private set; }
        public int Height { get; set; }
        public int Widht { get; private set; }

        public StashPosSize(int widht, int height, Rectangle rect)
        {
            Widht = widht;
            Height = height;
            Rect = rect;
        }
        public override string ToString()
        {
            if (Height == Widht && Widht == 0)
                return "Auto";
            return string.Format("{0}x{1}", Widht, Height);
        }

        public override int GetHashCode()
        {
            return Height * 1000000 + Widht;
        }


        public override bool Equals(object obj)
        {
            if (obj == null || typeof(StashPosSize) != obj.GetType())
            {
                return false;
            }
            return GetHashCode() == obj.GetHashCode();
        }
    }
}
