using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using PoeStashSorterModels;


namespace POEStashSorterModels
{
    public class StartSortingParams
    {
        public StartSortingParams(Tab unsortedTab, Tab sortedTab, InterruptEvent interruptEvent, StashPosSize stashPosSize)
        {
            UnsortedTab = unsortedTab;
            SortedTab = sortedTab;
            InterruptEvent = interruptEvent;
            StashPosSize = stashPosSize;
        }

        public Tab UnsortedTab { get; }
        public Tab SortedTab { get; }
        public InterruptEvent InterruptEvent { get; }
        public StashPosSize StashPosSize { get; }
    }

    public abstract class SortingAlgorithm
    {
        private static bool isSorting = false;
        private static Point startPos;
        private static float cellWidth;
        private static float cellHeight;
        private static bool interrupt;

        public SortOption SortOption { get; set; }

        public SortingAlgorithm()
        {
            SortOption = new SortOption();
            SortOption.ReadMode = true;
            Sort(new Tab() { Items = new List<Item>() }, SortOption);
            SortOption.ReadMode = false;
            SortOption.SelectedOption = SortOption.Options.FirstOrDefault();
        }

        public Tab SortTab(Tab tab)
        {
            Tab sortedTab = new Tab();
            sortedTab.Index = tab.Index;
            sortedTab.ID = tab.ID;
            sortedTab.League = tab.League;
            sortedTab.Name = tab.Name;
            sortedTab.srcC = tab.srcC;
            sortedTab.srcL = tab.srcL;
            sortedTab.srcR = tab.srcR;
            sortedTab.Items = tab.Items.Select(item => item.CloneItem()).ToList();


            Sort(sortedTab, SortOption);

            foreach (var item in sortedTab.Items)
            {
                double offsetX = 47.4 * 13;
                item.Image.Margin = new Thickness(item.X * 47.4f + 2.2f + offsetX, item.Y * 47.4f + 2.2f, 0, 0);
            }

            return sortedTab;
        }

        public abstract string Name { get; }

        protected abstract void Sort(Tab tab, SortOption options);

        public static SortingAlgorithm CreateFromType(Type type)
        {
            return (SortingAlgorithm)Activator.CreateInstance(type);
        }

        private void CalcStashDimensions(StartSortingParams sortingParams)
        {
            const int CELL_COUNT_X = 12;
            WinApi.Rect rect = ApplicationHelper.PathOfExileDimensions;

            float startX, startY;
            if (sortingParams.StashPosSize.Text == "Auto")
            {
                cellHeight = rect.Bottom * 0.0484f;
                startX = rect.Bottom * 0.033f - cellHeight / 2.0f;
                startY = rect.Bottom * 0.1783f - cellHeight / 2.0f - (Settings.Instance.GetSortingAlgorithmForTab(PoeSorter.SelectedTab).IsInFolder ? 0 : cellHeight / 2.0f);
            }
            else
            {
                // TODO: Fix the rectangle detection in non-folders
                var stashRectangle = sortingParams.StashPosSize.Text == "Auto(IR)"
                    ? GetStashRectangleViaImageRecognition(rect)
                    : SetScreenSize(sortingParams, ref rect);

                cellHeight = (float)stashRectangle.Width / CELL_COUNT_X;
                startX = stashRectangle.Left + cellHeight / 2.0f;
                startY = stashRectangle.Top + cellHeight / 2.0f;
            }

            cellWidth = cellHeight;
            startPos = new Point(rect.Left + (int)startX, rect.Top + (int)startY);
        }

        private static Rectangle SetScreenSize(StartSortingParams sortingParams, ref WinApi.Rect rect)
        {
            Func<WinApi.Rect, bool> checkScreenSize =
                (r) => r.Right != sortingParams.StashPosSize.Width && r.Bottom != sortingParams.StashPosSize.Height;
            if (checkScreenSize(rect))
            {
                ApplicationHelper.SetWindowSize(sortingParams.StashPosSize.Width, sortingParams.StashPosSize.Height, ref rect);
                Task.Delay(3000).Wait();
                if (checkScreenSize(rect))
                    throw new Exception("Failed to change screen resolution to " + sortingParams.StashPosSize);
            }
            var stashRectangle = sortingParams.StashPosSize.Rect;
            return stashRectangle;
        }

        private struct PointColor
        {
            public int Hue;
            public int X, Y;

            public PointColor(int x, int y, int hue)
            {
                X = x;
                Y = y;
                Hue = hue;
            }
        }

        private static Rectangle GetStashRectangleViaImageRecognition(WinApi.Rect rect)
        {
            using (Bitmap img = new Bitmap(rect.Right/2, rect.Bottom))
            {
                using (Graphics g = Graphics.FromImage(img))
                {
                    g.CopyFromScreen(rect.Left, rect.Top, 0, 0, img.Size, CopyPixelOperation.SourceCopy);
                }
                //img.Save(@"C:\Users\adamg\444.png", System.Drawing.Imaging.ImageFormat.Png);
                var colorFilter = new ColorFiltering();
                colorFilter.Red = new IntRange(0, 65);
                colorFilter.Green = new IntRange(0, 65);
                colorFilter.Blue = new IntRange(0, 65);
                colorFilter.FillOutsideRange = false;

                colorFilter.ApplyInPlace(img);
                
                var blobCounter = new BlobCounter();

                blobCounter.FilterBlobs = true;
                blobCounter.MinHeight = 50;
                blobCounter.MinWidth = 50;

                blobCounter.ProcessImage(img);
                Blob[] blobs = blobCounter.GetObjectsInformation();
                var shapeChecker = new SimpleShapeChecker();
                foreach (var blob in blobs)
                {
                    var edgePoints = blobCounter.GetBlobsEdgePoints(blob);
                    List<IntPoint> cornerPoints;

                    if (shapeChecker.IsQuadrilateral(edgePoints, out cornerPoints))
                    {
                        if (shapeChecker.CheckPolygonSubType(cornerPoints) == PolygonSubType.Square)
                        {
                            var rectangle = new Rectangle(cornerPoints[0].X, cornerPoints[1].Y, cornerPoints[1].X - cornerPoints[0].X, cornerPoints[2].Y - cornerPoints[1].Y);
                            if (rectangle.Width < 200 || rectangle.Height < 200)
                                continue;
                            //Graphics g = Graphics.FromImage(img);
                            //g.DrawRectangle(new Pen(Color.Red), rectangle);
                            //img.Save(@"C:\Users\adamg\result.png");
                            return rectangle;
                        }
                    }
                }

            }
            throw new Exception("Stash hasn't been found");
        }

        public bool ItemOverlap(Item A, Item B) {
            String foo = A.FullItemName;
            String bar = B.FullItemName;

            int AX2 = A.X + A.W;
            int AY2 = A.Y + A.H;
            int BX2 = B.X + B.W;
            int BY2 = B.Y + B.H;

            bool collision = (
                A.X < BX2 && // 
                AX2 > B.X && // 
                A.Y < BY2 && // 
                AY2 > B.Y    // 
            );

            // Debug.WriteLine(collision 
            //     + ", A: " + A.X + "-" + A.Y + ", " + A.W + "/" + A.H 
            //     + ", B: " + B.X + "-" + B.Y + ", " + B.W + "/" + B.H 
            //     + "(" + A.FullItemName + ", " + B.FullItemName + ")");

            // Debug.WriteLine( A.X + " < " + BX2 + " = " + ( A.X < BX2));
            // Debug.WriteLine( AX2 + " > " + B.X + " = " + ( AX2 > B.X));
            // Debug.WriteLine( A.Y + " < " + BY2 + " = " + ( A.Y < BY2));
            // Debug.WriteLine( AY2 + " > " + B.Y + " = " + ( AY2 > B.Y));

            return collision;
        }

        public Item FindMovableItem(List<Item> unsortedItems) {
            // FIXME: implement
            return unsortedItems.FirstOrDefault();
        }

        public void StartSorting(StartSortingParams sortingParams)
        {
            try
            {
                ApplicationHelper.OpenPathOfExile();
                List<Item> unsortedItems = sortingParams.UnsortedTab.Items.Where(
                    x => sortingParams.SortedTab.Items.Any(
                        c => c.Id == x.Id && c.X == x.X && x.Y == c.Y) == false
                    ).ToList();
                if (isSorting == false)
                {
                    CalcStashDimensions(sortingParams);
                    isSorting = true;

                    // Item unsortedItem = unsortedItems.FirstOrDefault();
                    Item unsortedItem = FindMovableItem(unsortedItems);

                    if (unsortedItem != null)
                    {
                        MouseTools.MoveCursor(MouseTools.GetMousePosition(), new Vector2(
                            startPos.X + unsortedItem.X * cellWidth,
                            startPos.Y + unsortedItem.Y * cellHeight
                        ), 20);
                        bool selectGem = true;

                        while (unsortedItem != null)
                        {
                            if (sortingParams.InterruptEvent.Isinterrupted)
                            {
                                throw new Exception("Interrupted");
                            }
                            Item sortedItem = sortingParams.SortedTab.Items.FirstOrDefault(x => x.Id == unsortedItem.Id);
                            Vector2 unsortedPos = new Vector2(
                                startPos.X + (unsortedItem.X + unsortedItem.W / 2f) * cellWidth,
                                startPos.Y + (unsortedItem.Y + unsortedItem.H / 2f) * cellHeight
                            );

                            Vector2 sortedPos = new Vector2(
                                startPos.X + (sortedItem.X + sortedItem.W / 2f) * cellWidth,
                                startPos.Y + (sortedItem.Y + sortedItem.H / 2f) * cellHeight
                            );
                            // Log.Message("Moving " + unsortedItem.Name + " from " + unsortedItem.X + "," + unsortedItem.Y + " to " + sortedItem.X + "," + sortedItem.Y);

                            List<Item> itemsInTargetSpace = unsortedItems.Where(
                                item => item != unsortedItem && ItemOverlap(item, sortedItem)
                            ).ToList();

                            if (itemsInTargetSpace.Count > 1)
                            {
                                // FIXME
                                throw new Exception("itemsInTargetSpace.Count > 1");
                                // FindMovableItem(unsortedItems);
                            }

                            if (selectGem)
                            {
                                //Move to item
                                MouseTools.MoveCursor(MouseTools.GetMousePosition(), unsortedPos, 10);

                                Thread.Sleep((int)(80f / Settings.Instance.Speed));
                                //select item
                                MouseTools.MouseClickEvent();
                                //wait a little (internet delay)
                                Thread.Sleep((int)(80f / Settings.Instance.Speed));
                            }

                            //move to correct position
                            MouseTools.MoveCursor(MouseTools.GetMousePosition(), sortedPos, 10);

                            Thread.Sleep((int)(80f / Settings.Instance.Speed));
                            //place item
                            MouseTools.MouseClickEvent();
                            //wait a little (internet delay)
                            Thread.Sleep((int)(80f / Settings.Instance.Speed));

                            //remove unsorted now that it is sorted
                            unsortedItems.Remove(unsortedItem);

                            //if there wassent a item where the item was placed
                            if (itemsInTargetSpace.Count == 1)
                            {
                                unsortedItem = itemsInTargetSpace[0];
                                selectGem = false;
                            }
                            else
                            {
                                //selected a new to sort
                                unsortedItem = FindMovableItem(unsortedItems);
                                selectGem = true;
                            }

                        }
                    }
                    //Log.Message("Sorting Complete");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                isSorting = false;
            }


        }

    }
}
