using PoEStashSorterModels;
using PoEStashSorterModels.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;


public class Compact : SortingAlgorithm
{
    public override string Name
    {
        get { return "Compact"; }
    }

    protected override void Sort(Tab tab, SortOption options)
    {
        if (options["Default"])
        {
            SortItems(tab, tab.Size);
        }
        if (options["Drop"])
        {
            Drop(tab);
        }
        //if (options["Tight"])
        //{
        //    CompactItems(tab);
        //}
        // if (options["Dynamic"])
        // {
        //     int numberOfColumns = (int)Math.Ceiling(tab.Items.Count() / 12f);
        //     sortItems(tab, numberOfColumns);
        // }
    }

    void Drop(Tab tab)
    {
        Console.WriteLine("\n\n");
        Console.WriteLine("------------");
        Console.WriteLine("Drop Sorting");
        Console.WriteLine(tab.Size);
        Console.WriteLine("------------");
        var itemQueue = tab.Items
            // Important to compacting
            .OrderByDescending(item => item.H * item.W)
            .ThenByDescending(item => item.H)

            // Cosmetic
            .ThenBy(item => item.Category)
            .ThenBy(item => item.FlaskType)
            .ThenBy(item => item.BaseType)
            .ThenByDescending(item => item.ItemLevel)
            .ThenByDescending(item => item.LevelRequirement)
        ;

        var heightMap = new Dictionary<int, int>(tab.Size);
        for (int h = 0; h < tab.Size; h++)
        {
            heightMap[h] = 0;
        }
        int j = 0;
        int prevCol = 0;
        Item prevItem = null;
        var used = new List<Item>();
        foreach (var item in itemQueue)
        {
            // If we used this item to fill in gaps, skip it
            if (used.Contains(item))
            {
                continue;
            }

            Console.WriteLine(string.Join(", ", heightMap));
            var ordered = heightMap.OrderBy(yx => yx.Value).ThenBy(yx => yx.Key).Where(yx => yx.Key + item.H <= tab.Size);
            var bestPos = ordered.First();
            Console.WriteLine(string.Join(", ", ordered));
            //x = heightMap.GetValueOrDefault(y, 0);
            item.X = bestPos.Value;
            item.Y = bestPos.Key;

            if (item.X > prevCol)
            {
                Console.WriteLine("moved to next col. checking if we left space");
                // Check if we left space in previous column
                Console.WriteLine("{0}, {1}", prevItem.Y, prevItem.H);
                int spaceLeft = tab.Size - (prevItem.Y + prevItem.H);
                Console.WriteLine("space left: " + spaceLeft);
                if (heightMap.GetValueOrDefault(prevItem.Y + prevItem.H, tab.Size) <= prevItem.X && spaceLeft > 0)
                {
                    var itemsThatFitInSpace = itemQueue.Skip(j + 1).Where(other => !used.Any(oo => oo == other)).Where(other => other.H <= spaceLeft).Where(other => other.W <= prevItem.W);
                    int accH = 0;
                    int accW = 0;
                    foreach (var otherItem in itemsThatFitInSpace)
                    {
                        if (otherItem.W > prevItem.W - accW || otherItem.H > spaceLeft - accH)
                            continue;
                        otherItem.X = prevItem.X;
                        otherItem.Y = prevItem.Y + prevItem.H;
                        accH += otherItem.H;
                        accW += otherItem.W;
                        used.Add(otherItem);

                        if (accH == spaceLeft && accW == prevItem.W)
                            break;
                    }
                    for (int i = 0; i < spaceLeft; i++)
                    {
                        Console.WriteLine(prevItem.Y + prevItem.H + i);
                        //if (prevItem.Y + prevItem.H + i >= tab.Size)
                        //    break;
                        heightMap[prevItem.Y + prevItem.H + i] += prevItem.W;
                    }
                }
            }

            for (int i = item.Y; i < item.Y + item.H; i++)
            {
                if (i > tab.Size)
                {
                    // Hit edge
                    //Console.WriteLine("There are {0} spaces left", tab.Size - i);
                    break;
                }
                // Might be wrong if there is overhang but there shouldn't be any overhang...?
                heightMap[i] += item.W;
            }
            Console.WriteLine(string.Join(", ", heightMap));
            Console.WriteLine("({0}) {1}: {2}, {3}", j, item.FullItemName, item.X, item.Y);
            prevCol = item.X;
            prevItem = item;
            j++;
            //if (j == 2)
            //    break;
        }
        Console.WriteLine("Used {0} items to fill in gaps", used.Count);
        Console.WriteLine(string.Join(", ", used.Select(u => u.FullItemName)));
    }

    void CompactItems(Tab tab)
    {
        var itemQueue = tab.Items
            .OrderByDescending(item => item.H)
        ;
    }

    void Place(IEnumerable<Item> items, int height)
    {
        int x = 0, y = 0;
        foreach (var item in items)
        {
            item.X = x;
            item.Y = y;

            x += item.W;
        }
    }

    void SortItems(Tab tab, int numberOfColumns)
    {
        // Sort items of `tab` by accessing `tab.Items` and 
        // setting each item's `.x` and `.y` to the new location.

        int numberOfItems = tab.Items.Count();
        if (numberOfItems < 1)
        {
            return;
        }

        // starting position
        int x = 0;
        int y = x;

        var itemQueue = tab.Items
            .OrderBy(item => item.W * item.H)
            .ThenBy(item => item.Category)
            .ThenBy(item => item.SubCategory)
            //.ThenBy(item => item.H)
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
            int remainingXSpace = tab.Size - x - emptyColumns;

            if (remainingXSpace < item.W || canSkip && maySkip)
            {
                x = 0;
                y += rowHeight;
                rowHeight = 1;
            }

            item.X = x;// + item.W - 1;
            item.Y = y;// - item.H + 1;

            itemsProcessed++;
            x += item.W;

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
