using LiveCharts.Wpf;
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

namespace DevEQ
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ControlLsitView CLV;
        public MainWindow()
        {
            InitializeComponent();
            Chart.DataTooltip = null;

        }

        int EditablePoint;
        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed) return;
            var point = Chart.ConvertToChartValues(e.GetPosition(Chart));
            ViewModel.Points[EditablePoint].X = point.X;
            ViewModel.Points[EditablePoint].Y = point.Y;
            if (ChB_MouseTrack.IsChecked == true)
                ViewModel.CurrentHZ = ViewModel.Points[EditablePoint].X;
        }

        private void Chart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Chart.MouseUp -= Chart_MouseUp;
            Chart.MouseMove -= Chart_MouseMove;
        }

        private void Chart_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            if(Mouse.RightButton == MouseButtonState.Pressed)
            {
                EditablePoint = ViewModel.GetIndexByPoint(chartPoint);
                if (EditablePoint == -1) return;
                ViewModel.RemovePoint(EditablePoint);
                return;
            }
            EditablePoint = ViewModel.GetIndexByPoint(chartPoint);
            if (EditablePoint == -1) return;
            Chart.MouseUp += Chart_MouseUp;
            Chart.MouseMove += Chart_MouseMove;
            if (ChB_MouseTrack.IsChecked == true)
                ViewModel.CurrentHZ = ViewModel.Points[EditablePoint].X;
        }

        private void Chart_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var point = Chart.ConvertToChartValues(e.GetPosition(Chart));
            ViewModel.AddPoint(point);
        }

        private void Chart_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ChB_Precise_Checked(object sender, RoutedEventArgs e)
        {
            if (ChB_Precise.IsChecked == false) G_Main.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Star);
            if (ChB_Precise.IsChecked == true) G_Main.ColumnDefinitions[1].Width = new GridLength(30, GridUnitType.Star);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.MainModel.Filter.PowerOff();
            ViewModel.MainModel.Filter.Dispose();
        }
    }
}
