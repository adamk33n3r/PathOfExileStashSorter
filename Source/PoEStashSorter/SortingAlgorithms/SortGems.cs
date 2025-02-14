﻿using PoEStashSorterModels;
using System;
using System.Collections.Generic;
using System.Linq;


public class SortGems : SortingAlgorithm
{
    public override string Name { get { return "Sort Gems"; } }

    protected override void Sort(Tab tab, SortOption options)
    {
        IEnumerable<Item> gems = tab.Items;

        if (options["Sort by color then type"])
        {
            SortEm(
                tab,
                gems.Where(x => x.GemRequirement == GemRequirement.Str).OrderByDescending(x => x.GemType).ThenBy(x => x.FullItemName).ThenBy(x => x.Level),
                gems.Where(x => x.GemRequirement == GemRequirement.Dex).OrderByDescending(x => x.GemType).ThenBy(x => x.FullItemName).ThenBy(x => x.Level),
                gems.Where(x => x.GemRequirement == GemRequirement.Int).OrderByDescending(x => x.GemType).ThenBy(x => x.FullItemName).ThenBy(x => x.Level)
            );
        }
        else if (options["Sort by type"])
        {
            SortEm(
                tab,
                gems.Where(x => x.GemType == GemType.Normal).OrderBy(x => x.FullItemName).ThenBy(x => x.LevelRequirement),
                gems.Where(x => x.GemType == GemType.Support).OrderBy(x => x.FullItemName).ThenBy(x => x.LevelRequirement),
                gems.Where(x => x.GemType == GemType.Aura).OrderBy(x => x.FullItemName).ThenBy(x => x.LevelRequirement)
            );
        }
        else if (options["Sort by quality"])
        {
            SortEm(
                tab,
                gems.Where(x => x.GemRequirement == GemRequirement.Str).OrderBy(x => x.Quality).ThenBy(x => x.FullItemName),
                gems.Where(x => x.GemRequirement == GemRequirement.Dex).OrderBy(x => x.Quality).ThenBy(x => x.FullItemName),
                gems.Where(x => x.GemRequirement == GemRequirement.Int).OrderBy(x => x.Quality).ThenBy(x => x.FullItemName)
            );
        }
        else if (options["Find quality gems"])
        {
            SortEm(
                tab,
                gems.Where(x => x.Quality == 0).OrderByDescending(x => x.GemRequirement).ThenBy(x => x.GemType).ThenBy(x => x.FullItemName),
                gems.Where(x => x.LevelRequirement > 99999), // No middle
                gems.Where(x => x.Quality > 0).OrderByDescending(x => x.Quality).ThenBy(x => x.FullItemName)
            );
        }
        else if (options["Sort by color then level"])
        {
            SortEm(
                tab,
                gems.Where(x => x.GemRequirement == GemRequirement.Str).OrderBy(x => x.Level).ThenBy(x => x.Quality),
                gems.Where(x => x.GemRequirement == GemRequirement.Dex).OrderBy(x => x.Level).ThenBy(x => x.Quality),
                gems.Where(x => x.GemRequirement == GemRequirement.Int).OrderBy(x => x.Level).ThenBy(x => x.Quality)
            );
        }
        else if (options["Sort by level"])
        {
            SortFromBottom(tab, gems.OrderByDescending(x => x.LevelRequirement));
        }

    }

    private void SortFromBottom(Tab tab, IEnumerable<Item> gems)
    {
        int x = tab.Size - 1;
        int y = tab.Size - 1;
        foreach (var item in gems)
        {
            item.X = x;
            item.Y = y;

            if (x == 0)
            {
                x = tab.Size;
                y--;
            }
            x--;
        }
    }

    private void SortEm(Tab tab, IEnumerable<Item> g1, IEnumerable<Item> g2, IEnumerable<Item> g3)
    {
        List<Item> sortedSoFar = g1.Union(g3).ToList();
        int fromLeft = 0;
        int fromRight = tab.Size - 1;
        int yMatters = 0;

        int x = 0;
        int y = 0;
        foreach (var item in g1)
        {
            item.X = x;
            item.Y = y;
            y++;
            if (y == tab.Size)
            {
                x += 1;
                y = 0;
            }
        }
        yMatters = y;
        fromLeft = x;


        x = tab.Size - 1;
        y = 0;
        foreach (var item in g3)
        {
            item.X = x;
            item.Y = y;
            y++;
            if (y == tab.Size)
            {
                x -= 1;
                y = 0;
            }
        }

        fromRight = x;

        int freeRows = fromRight - fromLeft;

        int rowsToTake = (int)Math.Ceiling(g2.Count() / (float)tab.Size);

        if (rowsToTake >= freeRows)
        {
            x = fromLeft;
            y = yMatters;
        }
        else if (rowsToTake < freeRows)
        {
            x = fromLeft + (int)Math.Ceiling((freeRows - rowsToTake) / 2f);
            y = 0;
        }

        foreach (var item in g2)
        {
            item.X = x;
            item.Y = y;
            do
            {
                y++;
                if (y == tab.Size)
                {
                    x += 1;
                    y = 0;
                }
            } while ((sortedSoFar.Any(c => c.X == x && c.Y == y)));
        }

    }

}
