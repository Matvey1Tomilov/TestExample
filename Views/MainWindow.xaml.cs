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
using TestExample.ViewModels;
using TestExample.Models;
using System.Runtime.Remoting.Channels;

namespace TestExample.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point _dragStartPoint;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new TabViewModel();
            this.Closed += (s, e) => (DataContext as TabViewModel)?.CancelAllPendingDeletions();
        }
        private void TabHeader_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is TabModel tab)
            {
                _dragStartPoint = e.GetPosition(this);
                element.CaptureMouse();
                e.Handled = false;
            }
        }
        private void TabHeader_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement element && element.IsMouseCaptured)
            {
                Point currentPoint = e.GetPosition(this);
                Vector diff = currentPoint - _dragStartPoint;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (element.DataContext is TabModel tab && DataContext is TabViewModel vm)
                    {
                        vm.StartDragCommand.Execute(tab);
                        element.ReleaseMouseCapture();
                        DragDrop.DoDragDrop(element, tab, DragDropEffects.Move);
                    }
                }
            }
        }

        private void TabHeader_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.IsMouseCaptured)
            {
                element.ReleaseMouseCapture();
                e.Handled = false; 
            }
        }
        private void TabHeader_DragOver(object sender, DragEventArgs e)
        {
            if (sender is Border border &&
                border.DataContext is TabModel targetTab &&
                DataContext is TabViewModel vm &&
                vm.MoveTabCommand.CanExecute(targetTab))
            {
                border.Background = new SolidColorBrush(Color.FromArgb(50, 0, 120, 215));
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void TabHeader_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = Brushes.Transparent;
            }
        }

        private void TabHeader_Drop(object sender, DragEventArgs e)
        {
            if (sender is Border border &&
                border.DataContext is TabModel targetTab &&
                DataContext is TabViewModel vm)
            {
                border.Background = Brushes.Transparent;
                vm.MoveTabCommand.Execute(targetTab);
                e.Handled = true;
            }
        }

        private void TabControl_DragLeave(object sender, DragEventArgs e)
        {
            if (DataContext is TabViewModel vm)
            {
                vm.CancelDragCommand.Execute(null);
            }
        }
    }
}
