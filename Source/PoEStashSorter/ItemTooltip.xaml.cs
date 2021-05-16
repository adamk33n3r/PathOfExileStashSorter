using PoEStashSorterModels;
using PoEStashSorterModels.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                Visibility = Visibility.Hidden;
                //Margin = new Thickness(-500, 0, 0, 0);
                return;
            }
            Console.WriteLine("Opening tooltip for: {0}", item.FullItemName);
            bool isDouble = item.Name != "";
            HeaderGrid.Height = isDouble ? 54 : 34;
            Name2.Visibility = isDouble ? Visibility.Visible : Visibility.Hidden;

            // Images
            HeaderLeft.Source = item.TooltipImages.Left;
            HeaderMiddle.Source = item.TooltipImages.Middle;
            HeaderRight.Source = item.TooltipImages.Right;
            foreach (ItemTooltipRow row in Rows.Children)
            {
                row.Separator.Source = item.TooltipImages.Separator;
            }
            //Separator1.Source = item.TooltipImages.Separator;
            //Separator2.Source = item.TooltipImages.Separator;
            //Separator3.Source = item.TooltipImages.Separator;
            //Separator4.Source = item.TooltipImages.Separator;
            //Separator5.Source = item.TooltipImages.Separator;

            if (isDouble)
            {
                Name1.Content = item.Name;
                Name2.Content = item.TypeLine;
            } else
            {
                Name1.Content = item.TypeLine;
            }

            // Content


            // Properties
            PropRow.Visibility = item.Properties.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            PropRow.ShowSeparator = false;
            PropRow.Text.Clear();
            var propList = new List<List<Inline>>();
            foreach (var prop in item.Properties)
            {
                var segment = new List<Inline>();
                propList.Add(segment);
                string sep = " ";
                switch (prop.DisplayMode)
                {
                    case DisplayMode.ColonSep:
                        //sep = ": ";
                        //goto case DisplayMode.SpaceSep;
                    {
                        segment.Add(new Run(prop.Name + (prop.Values.Count > 0 ? ": " : "")));
                        if (prop.Values.Count > 0)
                        {
                            var color = Colors[Convert.ToInt32(prop.Values[0][1])];
                            segment.Add(new Run(prop.Values[0][0] as string) { Foreground = new SolidColorBrush(color) });
                        }
                        break;
                    }
                    case DisplayMode.SpaceSep:
                        //string s = prop.Name;
                        //if (prop.Values.Count > 0)
                        //{
                        //    s += sep + string.Join(", ", prop.Values.Select(v => v[0]));
                        //}
                    {
                        var color = Colors[Convert.ToInt32(prop.Values[0][1])];
                        segment.Add(new Run(prop.Values[0][0] as string) { Foreground = new SolidColorBrush(color) });
                        segment.Add(new Run(" " + prop.Name));
                        break;
                    }
                    case DisplayMode.Formatted:
                        // Currently ignores display style
                        segment.Add(new Run(string.Format(prop.Name, prop.Values.Select(v => v[0]).ToArray())));
                        break;
                    case DisplayMode.Progress:
                        segment.Add(new Run(string.Format("{0}: {1}", prop.Name, prop.Values[0][0])));
                        break;
                }
            }
            //PropRow.Text += string.Join("\n", propList);
            PropRow.Text.AddRange(propList.Intersperse(() => new List<Inline>{new LineBreak()}).SelectMany(x => x));
            //PropRow.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //PropRow.Arrange(new Rect(0, 0, PropRow.DesiredSize.Width, PropRow.DesiredSize.Height));

            // Utility Mods (no sep)
            UtilityMods.ShowSeparator = false;
            UtilityMods.Text.Clear();
            //UtilityMods.Visibility = item.UtilityMods.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            foreach (var mod in item.UtilityMods)
            {
                if (UtilityMods.Text.Count > 0)
                {
                    UtilityMods.Text += new LineBreak();
                }
                UtilityMods.Text += new Run(mod) { Foreground = new SolidColorBrush(Augmented) };
            }

            // Requirements
            //ReqRow.Visibility = item.Requirements.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            ReqRow.ShowSeparator = PropRow.Visibility == Visibility.Visible || UtilityMods.Visibility == Visibility.Visible;
            ReqRow.Text.Clear();
            //ReqRow.Text += new List<Inline>() {
            //    new Run("Requires Level "),
            //    new Run("69") { FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x66, 0xcc)) },
            //    new Run(", "),
            //    new Run("420 ") { FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0xff, 0xff, 0xff)) },
            //    new Run("Str"),
            //};

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
                        var color = Colors[Convert.ToInt32(req.Values[0][1])];
                        segment.Add(new Run(req.Values[0][0] as string) { Foreground = new SolidColorBrush(color) });
                        break;
                    }
                    case DisplayMode.SpaceSep:
                    {
                        var color = Colors[Convert.ToInt32(req.Values[0][1])];
                        segment.Add(new Run(req.Values[0][0] as string) { Foreground = new SolidColorBrush(color) });
                        segment.Add(new Run(" " + req.Name));
                        break;
                    }
                    case DisplayMode.Formatted:
                        // Currently ignores display style
                        segment.Add(new Run(string.Format(req.Name, req.Values.Select(v => v[0]).ToArray())));
                        break;
                    case DisplayMode.Progress:
                        segment.Add(new Run(string.Format("{0}: {1}", req.Name, req.Values[0][0])));
                        break;
                }
            }
            ReqRow.Text.Add(new Run("Requires ")).AddRange(reqList.Intersperse(() => new List<Inline>{new Run(", ")}).SelectMany(x => x));

            SecondDescription.Text.Clear();

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

            //// Experience
            Experience.Text.Clear();
            if (item.AdditionalProperties.Count > 0)
            {
                Experience.Text += string.Format("{0} ({1})", item.AdditionalProperties[0].Values[0][0], item.AdditionalProperties[0].Progress);
            }

            //// Description
            Description.Text.Clear();
            if (item.DescrText != null)
            {
                Description.Text += new Run(item.DescrText) { FontStyle = FontStyles.Italic };
            }



            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));

            Visibility = Visibility.Visible;
        }
    }
}
