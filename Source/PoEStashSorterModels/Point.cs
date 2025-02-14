﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoEStashSorterModels
{
    public class Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }

    }
}
