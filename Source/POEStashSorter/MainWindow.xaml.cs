﻿using System.Runtime.InteropServices;
using System.Windows.Interop;
using POEStashSorterModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using PoeStashSorterModels;
using static PoeStashSorterModels.WinApi;
using Rectangle=System.Drawing.Rectangle;

namespace POEStashSorter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        InterruptEvent interruptEvent=new InterruptEvent();
         IntPtr handle=new IntPtr();

        const int ESCAPE = 27;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                PoeSorter.Initialize(stashPanel, Dispatcher, ddlSortMode, ddlSortOption, chkIsInFolder);
                txtSearch.Visibility = System.Windows.Visibility.Hidden;
                StashTabs.DisplayMemberPath = "Name";
                ddlSortMode.DisplayMemberPath = "Name";
                PopulateLeagueDDL();
                PopulateSpeedSlider();
                PopulateSortingDDL();
                PopulateStashSize();

                if (ddlLeague.Items.Count == 0)
                {
                    ddlLeague.IsEnabled =
                        StartSorting.IsEnabled = ddlSortMode.IsEnabled = ddlSortOption.IsEnabled = false;
                }

                this.Activated += OnFocus;

                this.Loaded += delegate
                {
                    handle = new WindowInteropHelper(this).Handle;
                };
                this.Closed += delegate
                {
                    Unregistered();
                };
            }
            catch
            {
                Close();
                throw;
            }

        }

        private void Unregistered()
        {
            UnregisterHotKey(handle, 9999);
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY_MSG_ID = 0x0312;
            if (msg == WM_HOTKEY_MSG_ID)
            {
                var keyCode = lParam.ToInt32() >> 16;
                if (keyCode == ESCAPE)
                {
                    interruptEvent.Isinterrupted =  true;
                }
            }
            return IntPtr.Zero;
        }

      

        private void OnFocus(object sender, EventArgs e)
        {
            PoeSorter.ReloadAlgorithms();
        }
        #region PopulateControls

        private void PopulateSortingDDL()
        {
            ddlSortMode.ItemsSource = PoeSorter.SortingAlgorithms;
        }

        private void PopulateSpeedSlider()
        {
            sliderSpeed.Value = Settings.Instance.Speed;
        }

        private void PopulateStashSize()
        {
            List<StashPosSize> list = new List<StashPosSize>
            {
                new StashPosSize("Auto"),
                new StashPosSize("Auto(IR)", "Auto (Image Recognition)"),
                new StashPosSize(1600, 900, new Rectangle(12, 132, 540, 661)),
                new StashPosSize(1499, 900, new Rectangle(11, 132, 506, 661)),
                new StashPosSize(1366, 768, new Rectangle(10, 113, 461, 564)),
                new StashPosSize(1360, 768, new Rectangle(10, 113, 459, 564)),
                new StashPosSize(1280, 960, new Rectangle(13, 142, 578, 707)),
                new StashPosSize(1280, 800, new Rectangle(10, 118, 480, 588)),
                new StashPosSize(1280, 768, new Rectangle(10, 113, 461, 564)),
                new StashPosSize(1280, 720, new Rectangle(9, 107, 434, 529)),
                new StashPosSize(1024, 768, new Rectangle(10, 113, 461, 564)),
                new StashPosSize(800, 600, new Rectangle(8, 88, 360, 441))
              };
            cbStashSize.ItemsSource = list;
            cbStashSize.SelectedIndex = Settings.Instance.StashSizeID;
        }
        
        private void PopulateLeagueDDL()
        {
            ddlLeague.ItemsSource = PoeSorter.Leagues;
            ddlLeague.DisplayMemberPath = "Name";
            ddlLeague.SelectedItem = PoeSorter.SelectedLeague;
        }
        #endregion

        private void ddlLeague_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PoeSorter.ChangeLeague((League)ddlLeague.SelectedItem);
            StashTabs.ItemsSource = PoeSorter.SelectedLeague.Tabs;
            StashTabs.SelectedItem = PoeSorter.SelectedLeague.Tabs.FirstOrDefault();
        }

        private void ListViewScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta);
            e.Handled = true;
        }

        private void StashTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PoeSorter.SetSelectedTab((Tab)StashTabs.SelectedItem);
        }

        private async void StartSorting_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            interruptEvent.Isinterrupted =  false;
            RegisterHotKey(handle, 9999, 0, ESCAPE);
            await Task.Delay(300);
            var stashSize = (StashPosSize) cbStashSize.SelectedValue;
            await Task.Run(() => PoeSorter.StartSorting(interruptEvent, stashSize));
            Unregistered();
            BackToFront();
        }

        private void BackToFront()
        {
            SetForegroundWindow(handle);
            WindowState = WindowState.Normal;
        }

        private void ddlSortMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PoeSorter.SelectSortingAlgorithm((SortingAlgorithm)ddlSortMode.SelectedItem);
            if (PoeSorter.SelectedSortingAlgorithm != null)
            {
                ddlSortOption.ItemsSource = PoeSorter.SelectedSortingAlgorithm.SortOption.Options;
                ddlSortOption.SelectedItem = PoeSorter.SelectedSortingAlgorithm.SortOption.SelectedOption;
            }

        }

        private void ddlSortOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PoeSorter.SelectSortOption((string)(ddlSortOption.SelectedItem));
        }

        private void chkIsInFolder_Checked(object sender, RoutedEventArgs e)
        {
            PoeSorter.SetIsInFolder((bool)chkIsInFolder.IsChecked);
        }

        private void ReloadAlgorithms(object sender, RoutedEventArgs e)
        {
            PoeSorter.ReloadAlgorithms();
        }

        private void sliderSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PoeSorter.Initialized)
            {
                Settings.Instance.Speed = sliderSpeed.Value;
                Settings.Instance.SaveChanges();
            }

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
            {
                txtSearch.Visibility = (txtSearch.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
                if (txtSearch.Visibility == System.Windows.Visibility.Visible)
                    txtSearch.Focus();

            }
            if (e.Key == Key.F5)
            {
                if (PoeSorter.SelectedTab != null && PoeSorter.SelectedTab.Items != null)
                {
                    PoeSorter.SelectedTab.Items.ForEach(c =>
                    {
                        if (PoeSorter.ItemCanvas.Children.Contains(c.Image))
                            PoeSorter.ItemCanvas.Children.Remove(c.Image);
                    });
                    PoeSorter.SelectedTab.Items = null;
                    PoeSorter.SetSelectedTab(PoeSorter.SelectedTab); // trigger download
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PoeSorter.Initialized && PoeSorter.SelectedLeague != null)
            {
                foreach (var tab in PoeSorter.SelectedLeague.Tabs)
                    tab.IsVisible = tab.Name.ToLower().Contains(txtSearch.Text.ToLower());

                StashTabs.ItemsSource = null;
                StashTabs.ItemsSource = PoeSorter.SelectedLeague.Tabs;
            }

        }

        private void cbStashSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PoeSorter.Initialized)
            {
                Settings.Instance.StashSizeID = cbStashSize.SelectedIndex;
                Settings.Instance.SaveChanges();
            }
        }
    }
}
