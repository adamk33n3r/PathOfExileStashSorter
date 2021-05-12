using PoEStashSorterModels;
using System;
using System.Collections.Generic;
using System.Linq;


public class SortBelts : SortingAlgorithm
{
    public override string Name
    {
        get { return "Sort Belts"; }
    }

    protected override void Sort(Tab tab, SortOption options)
    {
        if (options["Default"])
        {
            sortItems(tab, tab.Size);
        }
        // if (options["Dynamic"])
        // {
        //     int numberOfColumns = (int)Math.Ceiling(tab.Items.Count() / 12f);
        //     sortItems(tab, numberOfColumns);
        // }
    }

    void sortItems(Tab tab, int numberOfColumns)
    {
        // Sort items of `tab` by accessing `tab.Items` and 
        // setting each item's `.x` and `.y` to the new location.

        int numberOfItems = tab.Items.Count();
        if (numberOfItems < 1)
        {
            return;
        }

        // starting position
        int x = tab.Size - 1;
        int y = x;

        var itemQueue = tab.Items
            .OrderByDescending(item => item.Category)
            .ThenBy(item => item.SubCategory)
            .ThenBy(item => item.H)
            // .ThenBy(item => item.Icon)
            .ThenBy(item => item.ItemLevel);

            // .ThenBy(item => item.ItemLevel);
            // .ThenBy(item => item.Icon);

            // .OrderByDescending(item => item.Category)
            // .ThenBy(item => item.H)
            // .ThenBy(item => item.ItemLevel)
            // .ThenBy(item => item.Icon);

            // .OrderByDescending(item => item.FrameType)
            // .ThenBy(item => item.ExplicitMods ? item.ExplicitMods.Count : 0)

        var lastItem = itemQueue.FirstOrDefault();
        bool maySkip = true;
        int itemsProcessed = 0;
        int rowHeight = 1;

        // FIXME: Doesn't really work well for variable width objects
        bool spaciousMode = false;

        foreach (var item in itemQueue)
        {
            bool newIconType = (lastItem.Icon != item.Icon);
            int remainingItems = numberOfItems - itemsProcessed;
            // bool spaciousMode = (remainingItems < numberOfColumns * y);

            if (maySkip && newIconType && !spaciousMode)
            {
                maySkip = false;
            }

            bool canSkip = (newIconType && spaciousMode);

            // we advance to the next row if we run out of space,
            // OR if new base item and enough space
            int emptyColumns = (tab.Size - numberOfColumns);
            int remainingXSpace = 1 + x - emptyColumns;

            if (remainingXSpace < item.W || canSkip && maySkip)
            {
                x = tab.Size - 1;
                y -= rowHeight;
                rowHeight = 1;
            }

            item.X = x - item.W + 1;
            item.Y = y - item.H + 1;

            itemsProcessed++;
            x -= item.W;

            // increase the current row height to be the height of the largest item.
            if (item.H > rowHeight) {
                rowHeight = item.H;
            }

            lastItem = item;
        }

        //Move all items up!
        // why?
        // int minY = tab.Items.Min(item => item.Y);
        // tab.Items.ForEach(c => c.Y -= minY);
    }
}
