﻿using System;
using System.Threading;
using PoEStashSorterModels;
using static PoEStashSorterModels.WinApi;

namespace PoEStashSorterModels
{
    internal static class MouseTools
    {
        public static Vector2 GetMousePosition()
        {
            WinApi.Point w32Mouse;
            GetCursorPos(out w32Mouse);
            return new Vector2(w32Mouse.X, w32Mouse.Y);
        }

        public static void MoveCursor(Vector2 p1, Vector2 p2, int step = 3)
        {
            var start = new Vector2(p1.X, p1.Y);
            var end = new Vector2(p2.X, p2.Y);

            var distance = Vector2.Distance(start, end);
            var angle = Vector2.Angle(start, end);

            for (float i = 0; i <= 200; i += step)
            {
                var factor = i / 200f;
                //factor = 0.000001f * (float)Math.Pow((100 - factor * 100) - 100, 4) / 100;

                var addDistance = distance * factor;
                var currentPos = start + new Vector2((float)Math.Cos(angle) * addDistance, (float)Math.Sin(angle) * addDistance);

                SetCursorPos((int)currentPos.X, (int)currentPos.Y);
                Thread.Sleep(4);
            }
        }

        private static WinApi.Point GetCursorPosition()
        {
            WinApi.Point currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint)
            {
                currentMousePoint = new WinApi.Point(0, 0);
            }
            return currentMousePoint;
        }

        public static void MouseClickEvent()
        {
            MouseEvent(MouseEventFlags.LeftDown);
            Thread.Sleep(70);
            MouseEvent(MouseEventFlags.LeftUp);
        }

        public static void MouseEvent(MouseEventFlags value)
        {
            var position = GetCursorPosition();

            mouse_event
                ((int)value,
                    position.X,
                    position.Y,
                    0,
                    0)
                ;
        }
    }
}