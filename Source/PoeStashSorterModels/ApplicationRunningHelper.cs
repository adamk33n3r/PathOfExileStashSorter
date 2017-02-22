using System;
using System.Diagnostics;
using PoeStashSorterModels;
using static PoeStashSorterModels.WinApi;

namespace POEStashSorterModels
{
    public static class ApplicationHelper
    {
        private static Process currentProcess;

        public static Rect PathOfExileDimentions
        {
            get
            {
                Rect rect;
                WinApi.Point point;
                var handle = currentProcess.MainWindowHandle;
                GetClientRect(handle, out rect);
                ClientToScreen(handle, out point);
                return rect.ToRectangle(point);
            }
        }

        public static bool OpenPathOfExile()
        {
            const int swRestore = 9;

/*OLD VERSION THAT NO LONGER WORKS (as of: 2016-12-26)
            var arrProcesses = Process.GetProcessesByName("PathOfExile");
            if (arrProcesses.Length > 0)
            {
                currentProcess = arrProcesses[0];
                var hWnd = arrProcesses[0].MainWindowHandle;
                if (IsIconic(hWnd))
                    ShowWindowAsync(hWnd, swRestore);
                SetForegroundWindow(hWnd);
                return true;
            }
            arrProcesses = Process.GetProcessesByName("PathOfExileSteam");
            if (arrProcesses.Length > 0)
            {
                currentProcess = arrProcesses[0];

                var hWnd = arrProcesses[0].MainWindowHandle;
                if (IsIconic(hWnd))
                    ShowWindowAsync(hWnd, swRestore);
                SetForegroundWindow(hWnd);
                return true;
            }
*/

            /*
             *  New Version of routine for finding the PoE process no matter what form the name takes
             */
            Process[] arrProcesses;

            //Create an array of possible process names
            string[] processNameArray = 
                {
                    "PathOfExile"
                    ,"PathOfExile.exe"
                    ,"PathOfExile_x64"
                    ,"PathOfExile_x64.exe"
                    ,"PathOfExileSteam"
                    ,"PathOfExileSteam.exe"
                    ,"PathOfExile_x64Steam"
                    ,"PathOfExile_x64Steam.exe"
                };

            //Loop through the array of process names
            foreach (string processName in processNameArray)
            {
                arrProcesses = Process.GetProcessesByName(processName);
                if (arrProcesses.Length > 0)
                {
                    currentProcess = arrProcesses[0];

                    var hWnd = arrProcesses[0].MainWindowHandle;
                    if (IsIconic(hWnd))
                        ShowWindowAsync(hWnd, swRestore);
                    SetForegroundWindow(hWnd);
                    return true;
                }
            }

            throw new Exception("Path Of Exile isn't running");
        }

        public static void SetWindowSize(int width, int height, ref Rect sRect)
        {
            Rect wRect;
            var handle = currentProcess.MainWindowHandle;
            GetWindowRect(handle, out wRect);
            var borderTop = sRect.Top - wRect.Top;
            var borderLeft = sRect.Left - wRect.Left;
            var borderBottom = wRect.Bottom - wRect.Top - sRect.Bottom - borderTop;
            var windowWidth = width + 2*borderLeft;
            var windowHeight = height + borderTop + borderBottom;
            MoveWindow(handle, 0, 0, windowWidth, windowHeight, true);
            sRect = PathOfExileDimentions;
        }
    }
}