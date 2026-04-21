using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChromeProfileLauncher.Models;
using ChromeProfileLauncher.ViewModels;

namespace ChromeProfileLauncher
{
    public partial class SettingsWindow : Window
    {
        private Point _startPoint;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is ViewModels.SettingsViewModel vm)
            {
                vm.SaveWindowSettings();
            }
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)

        {
            _startPoint = e.GetPosition(null);
        }

        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = _startPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    ListBox listBox = sender as ListBox;
                    if (listBox == null) return;

                    // Ensure dragging starts only from the handle
                    if (!(e.OriginalSource is TextBlock tb && tb.Text == "☰"))
                    {
                        return;
                    }

                    ListBoxItem listBoxItem = FindVisualParent<ListBoxItem>((DependencyObject)e.OriginalSource);
                    if (listBoxItem != null)
                    {
                        ProfileInfo profile = (ProfileInfo)listBox.ItemContainerGenerator.ItemFromContainer(listBoxItem);
                        if (profile == null) return;

                        DataObject dragData = new DataObject("ProfileInfo", profile);
                        DragDrop.DoDragDrop(listBoxItem, dragData, DragDropEffects.Move);
                    }
                }
            }
        }

        private ListBoxItem _lastTargetItem = null;

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            var targetItem = FindVisualParent<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (targetItem != _lastTargetItem)
            {
                if (_lastTargetItem != null)
                {
                    _lastTargetItem.Opacity = 1.0;
                }
                _lastTargetItem = targetItem;
                if (_lastTargetItem != null)
                {
                    _lastTargetItem.Opacity = 0.4; // Visual drop indicator
                }
            }
        }

        private void ListBox_DragLeave(object sender, DragEventArgs e)
        {
            var targetItem = FindVisualParent<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (targetItem != null && targetItem == _lastTargetItem)
            {
                _lastTargetItem.Opacity = 1.0;
                _lastTargetItem = null;
            }
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (_lastTargetItem != null)
            {
                _lastTargetItem.Opacity = 1.0;
                _lastTargetItem = null;
            }

            if (e.Data.GetDataPresent("ProfileInfo"))
            {
                ProfileInfo source = e.Data.GetData("ProfileInfo") as ProfileInfo;
                ListBox listBox = sender as ListBox;
                if (listBox == null || source == null) return;

                ListBoxItem listBoxItem = FindVisualParent<ListBoxItem>((DependencyObject)e.OriginalSource);

                if (listBoxItem != null)
                {
                    ProfileInfo target = (ProfileInfo)listBox.ItemContainerGenerator.ItemFromContainer(listBoxItem);
                    if (target == null || source == target) return;

                    int oldIndex = listBox.Items.IndexOf(source);
                    int newIndex = listBox.Items.IndexOf(target);

                    if (oldIndex != -1 && newIndex != -1)
                    {
                        var vm = listBox.DataContext as SettingsViewModel;
                        vm?.MoveProfile(oldIndex, newIndex);
                    }
                }
            }
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null) return parent;
            return FindVisualParent<T>(parentObject);
        }
    }
}
