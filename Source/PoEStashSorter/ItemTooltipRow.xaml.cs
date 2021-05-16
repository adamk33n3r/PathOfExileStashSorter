﻿using System;
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
    /// Interaction logic for ItemTooltipRow.xaml
    /// </summary>
    public partial class ItemTooltipRow : UserControl
    {
        //public static readonly DependencyProperty ShowSeparatorProperty = DependencyProperty.Register("ShowProperty", typeof(bool), typeof(ItemTooltipRow), new PropertyMetadata(false, new PropertyChangedCallback((d, e) => {
        //    var ele = (ItemTooltipRow)d;
        //    Console.WriteLine("on prop changed: " + e.NewValue);
        //    ele.Separator.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        //})), new ValidateValueCallback((o) => { bool value = (bool)o; return value == true || value == false; }));
        //public bool ShowSeparator
        //{
        //    get { return (bool)GetValue(ShowSeparatorProperty); }
        //    set { Console.WriteLine("show sep set to: " + value.ToString()); SetValue(ShowSeparatorProperty, value); }
        //}

        public bool ShowSeparator
        {
            get { return Separator.Visibility == Visibility.Visible; }
            set { Separator.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        private TextCollection text;
        public TextCollection Text
        {
            get => text;
            set
            {
                this.text = value;
                Visibility = this.text.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public ItemTooltipRow()
        {
            InitializeComponent();
            DataContext = this;
            this.text = new TextCollection(Body.Inlines);
            this.text.ContentChanged += () => {
                Visibility = this.text.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            };
            //Body.Inlines.Clear();
            //Body.Inlines.Add(new Run("Requires Level "));
            //Body.Inlines.Add(new Run("69") { FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x66, 0xcc)) });
            //Body.Inlines.Add(new Run(", "));
            //Body.Inlines.Add(new Run("420 ") { FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0xff, 0xff, 0xff)) });
            //Body.Inlines.Add(new Run("Str"));
        }
    }
}
