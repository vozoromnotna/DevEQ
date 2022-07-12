using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Xceed.Wpf.Toolkit;

namespace DevEQ
{
    public class ControlLsitView
    {
        Grid grid;

        ChartValues<ObservablePoint> points;

        DevEQ_ViewModel vm;

        ObservableCollection<Grid> GridList;
        ObservableCollection<Slider> SliderList;
        ObservableCollection<DoubleUpDown> XNudList;
        ObservableCollection<DoubleUpDown> YNudList;
        

        public ControlLsitView(Grid grid, DevEQ_ViewModel vm)
        {
            this.grid = grid;
            this.points = vm.Points;
            this.vm = vm;

            MakeListOfGrid();
            MakeGrid();

            vm.PropertyChanged += Points_CollectionChanged;
        }

        private void Points_CollectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Points")
            {
                this.points = vm.Points;
                MakeListOfGrid();
                MakeGrid();
            }
        }

        private void MakeListOfGrid()
        {

            GridList = new ObservableCollection<Grid>();
            SliderList = new ObservableCollection<Slider>();
            XNudList = new ObservableCollection<DoubleUpDown>();
            YNudList = new ObservableCollection<DoubleUpDown>();
            for (int i = 0; i < points.Count; i++)
            {
                GridList.Add(new Grid());
                GridList[i].ColumnDefinitions.Add(new ColumnDefinition());
                GridList[i].ColumnDefinitions.Add(new ColumnDefinition());
                GridList[i].ColumnDefinitions.Add(new ColumnDefinition());
                GridList[i].RowDefinitions.Add(new RowDefinition());
                GridList[i].RowDefinitions.Add(new RowDefinition());

                SliderList.Add(new Slider());
                Grid.SetColumn(SliderList[i], 1);
                Grid.SetRow(SliderList[i], 1);

                XNudList.Add(new DoubleUpDown());
                Grid.SetColumn(XNudList[i], 2);
                Grid.SetRow(XNudList[i], 0);

                YNudList.Add(new DoubleUpDown());
                Grid.SetColumn(YNudList[i], 2);
                Grid.SetRow(YNudList[i], 1);

                GridList[i].Children.Add(SliderList[i]);
                GridList[i].Children.Add(XNudList[i]);
                GridList[i].Children.Add(YNudList[i]);

                Grid.SetRow(GridList[i], i);
                #region Привязки
                var XBinding = new Binding
                {
                    Path = new PropertyPath("X"),
                    Source = points[i],
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                var YBinding = new Binding
                {
                    Path = new PropertyPath("Y"),
                    Source = points[i],
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                var minXBinding = new Binding
                {
                    Path = new PropertyPath("minX"),
                    Source = vm.MainModel,
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                var minYBinding = new Binding
                {
                    Path = new PropertyPath("minY"),
                    Source = vm.MainModel,
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                var maxXBinding = new Binding
                {
                    Path = new PropertyPath("maxX"),
                    Source = vm.MainModel,
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                var maxYBinding = new Binding
                {
                    Path = new PropertyPath("maxY"),
                    Source = vm.MainModel,
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                SliderList[i].SetBinding(Slider.ValueProperty, YBinding);
                SliderList[i].SetBinding(Slider.MaximumProperty, maxYBinding);
                SliderList[i].SetBinding(Slider.MinimumProperty, minYBinding);

                YNudList[i].SetBinding(DoubleUpDown.ValueProperty, YBinding);
                YNudList[i].SetBinding(DoubleUpDown.MaximumProperty, maxYBinding);
                YNudList[i].SetBinding(DoubleUpDown.MinimumProperty, minYBinding);

                XNudList[i].SetBinding(DoubleUpDown.ValueProperty, XBinding);
                XNudList[i].SetBinding(DoubleUpDown.MaximumProperty, maxXBinding);
                XNudList[i].SetBinding(DoubleUpDown.MinimumProperty, minXBinding);
                #endregion

                
            }
        }


        private void MakeGrid()
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            for (int i = 0; i < points.Count; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                grid.Children.Add(GridList[i]);
            }
        }
    }
}
