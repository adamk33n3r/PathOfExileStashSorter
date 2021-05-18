using PoEStashSorterModels;
using PoEStashSorterModels.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace PoEStashSorter
{
    /// <summary>
    /// Interaction logic for ItemTooltip.xaml
    /// </summary>
    public partial class ItemTooltip : UserControl
    {
        private static readonly Color Augmented = Color.FromRgb(0x88, 0x88, 0xFF);
        private static readonly Color[] Colors = {
            Color.FromRgb(0xFF, 0xFF, 0xFF),
            Augmented,
            Color.FromRgb(0xD2, 0x00, 0x00),
            Color.FromRgb(0xFF, 0xFF, 0xFF),
            Color.FromRgb(0x96, 0x00, 0x00),
            Color.FromRgb(0x36, 0x64, 0x92),
            Color.FromRgb(0xFF, 0xD7, 0x00),
            Color.FromRgb(0xD0, 0x20, 0x90),
        };

        /*
         * 0 = Default #FFFFFF
         * 1 = Augmented #8888FF
         * 2 = Unmet #D20000
         * 3 = Physical Damage #FFFFFF
         * 4 = Fire Damage #960000
         * 5 = Cold Damage #366492
         * 6 = Lightning Damage #FFD700
         * 7 = Chaos Damage #D02090
         */

        public ItemTooltip()
        {
            InitializeComponent();
        }

        public void UpdateItemData(Item item)
        {
            if (item == null)
            {
                return;
            }
            Console.WriteLine("Opening tooltip for: {0}", item.FullItemName);

            /* SETUP */

            bool isDouble = item.Name != "";
            HeaderGrid.Height = isDouble ? 54 : 34;

            Name1.Foreground = Name2.Foreground = item.TooltipColor;
            Name2.Visibility = isDouble ? Visibility.Visible : Visibility.Hidden;
            Name1.Margin = new Thickness(0, isDouble ? -2 : 0, 0, 0);

            // Images
            HeaderLeft.Source = item.TooltipImages.Left;
            HeaderMiddle.Source = item.TooltipImages.Middle;
            HeaderRight.Source = item.TooltipImages.Right;
            foreach (ItemTooltipRow row in Rows.Children)
            {
                row.Separator.Source = item.TooltipImages.Separator;
            }

            /* CONTENT */

            if (isDouble)
            {
                Name1.Content = item.Name;
                Name2.Content = item.TypeLine;
            } else
            {
                Name1.Content = item.TypeLine;
            }

            // Properties
            PropRow.Visibility = item.Properties.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            PropRow.ShowSeparator = false;
            PropRow.Text.Clear();
            var propList = new List<List<Inline>>();
            foreach (var prop in item.Properties)
            {
                var segment = new List<Inline>();
                propList.Add(segment);
                switch (prop.DisplayMode)
                {
                    case DisplayMode.ColonSep:
                    {
                        segment.Add(new Run(prop.Name + (prop.Values.Count > 0 ? ": " : "")));
                        if (prop.Values.Count > 0)
                        {
                            var color = Colors[Convert.ToInt32(prop.Values[0].ValueType)];
                            segment.Add(new Run(prop.Values[0].Value) { Foreground = new SolidColorBrush(color) });
                        }
                        break;
                    }
                    case DisplayMode.SpaceSep:
                    {
                        var color = Colors[Convert.ToInt32(prop.Values[0].ValueType)];
                        segment.Add(new Run(prop.Values[0].Value) { Foreground = new SolidColorBrush(color) });
                        segment.Add(new Run(" " + prop.Name));
                        break;
                    }
                    case DisplayMode.Formatted:
                        segment.AddRange(
                            ParseFmt(new DisplayFormat() {
                                fmt = prop.Name,
                                values = prop.Values.Select(i => (i.Value, Colors[i.ValueType])).ToList(),
                            })
                        );
                        break;
                    case DisplayMode.Progress:
                        segment.Add(new Run(string.Format("{0}: {1}", prop.Name, prop.Values[0].Value)));
                        break;
                }
            }
            PropRow.Text.AddRange(propList.Intersperse(() => new List<Inline>{new LineBreak()}).SelectMany(x => x));

            // Utility Mods (no sep)
            UtilityMods.ShowSeparator = false;
            UtilityMods.Text.Clear();
            foreach (var mod in item.UtilityMods)
            {
                if (UtilityMods.Text.Count > 0)
                {
                    UtilityMods.Text += new LineBreak();
                }
                UtilityMods.Text += new Run(mod) { Foreground = new SolidColorBrush(Augmented) };
            }

            // Requirements
            ReqRow.ShowSeparator = PropRow.Visibility == Visibility.Visible || UtilityMods.Visibility == Visibility.Visible;
            ReqRow.Text.Clear();

            var reqList = new List<List<Inline>>();
            foreach (var req in item.Requirements)
            {
                var segment = new List<Inline>();
                reqList.Add(segment);
                switch (req.DisplayMode)
                {
                    case DisplayMode.ColonSep:
                    {
                        segment.Add(new Run(req.Name + " "));
                        var color = Colors[req.Values[0].ValueType];
                        segment.Add(new Run(req.Values[0].Value) { Foreground = new SolidColorBrush(color) });
                        break;
                    }
                    case DisplayMode.SpaceSep:
                    {
                        var color = Colors[Convert.ToInt32(req.Values[0].ValueType)];
                        segment.Add(new Run(req.Values[0].Value) { Foreground = new SolidColorBrush(color) });
                        segment.Add(new Run(" " + req.Name));
                        break;
                    }
                    case DisplayMode.Formatted:
                        segment.AddRange(
                            ParseFmt(new DisplayFormat() {
                                fmt = req.Name,
                                values = req.Values.Select(i => (i.Value, Colors[i.ValueType])).ToList(),
                                //text = req.Values.Select(i => (string)i[0]).ToList(),
                                //colors = req.Values.Select(i => Colors[(long)i[1]]).ToList(),
                            })
                        );
                        break;
                    case DisplayMode.Progress:
                        segment.Add(new Run(string.Format("{0}: {1}", req.Name, req.Values[0].Value)));
                        break;
                }
            }
            ReqRow.Text.Add(new Run("Requires ")).AddRange(reqList.Intersperse(() => new List<Inline>{new Run(", ")}).SelectMany(x => x));

            SecondDescription.Foreground = item.TooltipColor;
            SecondDescription.Text.Clear();
            if (!string.IsNullOrEmpty(item.SecDescrText))
            {
                SecondDescription.Text += item.SecDescrText;
            }

            // Implicit Mods
            ImplicitMods.Text.Clear();
            foreach (var mod in item.ImplicitMods)
            {
                if (ImplicitMods.Text.Count > 0)
                {
                    ImplicitMods.Text += new LineBreak();
                }
                ImplicitMods.Text += new Run(mod) { Foreground = new SolidColorBrush(Augmented) };
            }

            // Explicit Mods
            ExplicitMods.Text.Clear();
            foreach (var mod in item.ExplicitMods)
            {
                if (ExplicitMods.Text.Count > 0)
                {
                    ExplicitMods.Text += new LineBreak();
                }
                ExplicitMods.Text += new Run(mod) { Foreground = new SolidColorBrush(Augmented) };
            }

            // Experience
            Experience.Text.Clear();
            if (item.AdditionalProperties.Count > 0)
            {
                Experience.Text += string.Format("{0} ({1})", item.AdditionalProperties[0].Values[0].Value, item.AdditionalProperties[0].Progress);
            }

            // Description
            Description.Text.Clear();
            if (item.DescrText != null)
            {
                Description.Text += new Run(item.DescrText) { FontStyle = FontStyles.Italic };
            }

            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));
        }

        struct DisplayFormat
        {
            public string fmt;
            public List<(string, Color)> values;
        }

        struct FormatNode
        {
            public int idx;
            public int length;
            public int argIdx;
        }

        private static List<Inline> ParseFmt(DisplayFormat displayFormat)
        {
            var reg = new Regex(@"{(\d+)}");
            var matches = reg.Matches(displayFormat.fmt);
            var idxList = new List<FormatNode>();
            foreach (Match match in matches)
            {
                Group fmtS = match.Groups[0];
                Group fmtIdx = match.Groups[1];
                int argIdx = Int32.Parse(fmtIdx.Value);
                idxList.Add(new FormatNode { idx = fmtS.Index, length = fmtS.Length, argIdx = argIdx });
            }

            var inlines = new List<Inline>();
            int pos = 0;
            foreach (var (node, idx) in idxList.Select((i, idx) => (i, idx)))
            {
                // Text up until node
                inlines.Add(new Run(displayFormat.fmt.Substring(pos, node.idx - pos)));
                // Node text
                inlines.Add(new Run(displayFormat.values[idx].Item1) { Foreground = new SolidColorBrush(displayFormat.values[idx].Item2) });
                pos = node.idx + node.length;
            }
            inlines.Add(new Run(displayFormat.fmt.Substring(pos, displayFormat.fmt.Length - pos)));
            return inlines;
        }
    }
}
