using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using DependenciesTracking;
using DependenciesTracking.Interfaces;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Microsoft.Win32;


namespace DevEQ
{
    public class DevEQ_ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> Logs
        {
            get;
            set;
        }

        Timer AOF_Set_Timer;

        int AOF_Delay = 5;

        public double CurrentHZ
        {
            get
            {
                return MainModel.Current_HZ;
            }
            set
            {
                if (AOF_Set_Timer != null)
                    if (AOF_Set_Timer.Enabled) AOF_Set_Timer.Stop();
                AOF_Set_Timer = new System.Timers.Timer(AOF_Delay);
                AOF_Set_Timer.AutoReset = false;
                AOF_Set_Timer.Elapsed += (s, e) =>
                {
                    MainModel.Current_HZ = value;
                    System.Windows.Application.Current.Dispatcher.Invoke(
                    (Action)(() => { Message("Установлена частота " + value.ToString("0.000") + " МГц; Соответствующая длина волны " + CurrentWL.ToString("0.000") + " нм."); }));
                    OnPropertyChanged();
                    OnPropertyChanged("CurrentWL");
                };
                AOF_Set_Timer.Enabled = true;
            }
        }

        public double CurrentWL
        {
            get
            {
                return MainModel.Current_WL;
            }
            set
            {
                if (AOF_Set_Timer != null)
                    if (AOF_Set_Timer.Enabled) AOF_Set_Timer.Stop();
                AOF_Set_Timer = new System.Timers.Timer(AOF_Delay);
                AOF_Set_Timer.AutoReset = false;
                AOF_Set_Timer.Elapsed += (s, e) =>
                {
                    MainModel.Current_WL = value;
                    System.Windows.Application.Current.Dispatcher.Invoke(
                    (Action)(() => { Message("Установлена длина волны " + value.ToString("0.000") + " нм; Соответствующая частота " + CurrentHZ.ToString("0.000") + " МГц."); }));
                    OnPropertyChanged();
                    OnPropertyChanged("CurrentHZ");
                };
                AOF_Set_Timer.Enabled = true;
            }
        }

        public bool IsDevRead
        {
            get
            {
                if (MainModel.Filter == null) return false;
                if (MainModel.DevPath == null) return false;
                return true;
            }
        }
        public string DevName
        {
            get
            {
                if (!IsDevRead) return "";
                var index = MainModel.DevPath.LastIndexOf("\\");
                var str = MainModel.DevPath.Substring(index + 1);
                return str;
            }
        }
        public void Message(string message)
        {
            if (null == message)
            {
                throw new ArgumentNullException("message");
            }
            string data = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}: {1}", DateTime.Now, message);
            Logs.Insert(0,data);
        }
        private ChartValues<ObservablePoint> points;
        public ChartValues<ObservablePoint> Points
        {
            get { return points; }
            set { points = value; OnPropertyChanged(); }
        }

        public DevEQ_ViewModel()
        {
            Logs = new ObservableCollection<string>();
            Message(" - текущее время");
            this.MainModel = new DevEQ_Model();
            var Filters = AO_Lib.AO_Devices.AO_Filter.Find_all_filters();
            if (Filters.Count > 0)
            {
                if (Filters.Count > 1) Message("Обнаружено несколько фильтров. Будет подключен первый обнаруженный...");
                MainModel.Filter = Filters[0];
            }
            else if (Filters.Count == 0)
            {
                Message("Не обнаружены именованные фильтры. Поиск неименнованного фильтра...");
                MainModel.Filter = AO_Lib.AO_Devices.AO_Filter.Find_and_connect_AnyFilter();
            }

            if (MainModel.Filter.FilterType == AO_Lib.AO_Devices.FilterTypes.Emulator) { Message("ПРЕДУПРЕЖДЕНИЕ: Не обнаружены подключенные АО фильтры. Фильтр будет эмулирован."); }
            else { Message("Обнаружен подключенный АО фильтр. Тип фильтра: " + MainModel.Filter.FilterType.ToString()); }

            Points = new ChartValues<ObservablePoint>();

        }


        private void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {

                var point = e.NewItems[0] as ObservablePoint;
                point.PropertyChanged += (s, p) =>
                {
                    var index = Points.IndexOf(s);
                    MainModel.X[index] = Points[index].X;
                    MainModel.Y[index] = Points[index].Y;
                };
                MainModel.InsertPoint(point.X, point.Y, e.NewStartingIndex);
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                var index = e.OldStartingIndex;
                MainModel.RemovePoint(index);
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                var new_point = e.NewItems[0] as ObservablePoint;
                var old_point = e.OldItems[0] as ObservablePoint;

                var old_index = e.OldStartingIndex;
                var new_index = e.NewStartingIndex;

                MainModel.X[old_index] = old_point.X;
                MainModel.Y[old_index] = old_point.Y;

                MainModel.X[new_index] = new_point.X;
                MainModel.Y[new_index] = new_point.Y;
            }
            OnPropertyChanged("Points");
        }

        public void AddPoint(Point cp)
        {
            for(int i = 0; i < Points.Count; i++)
            {
                if (cp.X < Points[i].X)
                {
                    Points.Insert(i, new ObservablePoint(cp.X, cp.Y));
                    break;
                }
                    
            }
        }

        public void RemovePoint(int index)
        {
            Points.RemoveAt(index);
        }

        public DevEQ_Model MainModel { get; private set; }

        public int GetIndexByPoint(ChartPoint cp)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i].X == cp.X)
                    return i;
            }    
            return -1;
        }
        public int GetIndexByPoint(Point cp)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i].X == cp.X)
                    if (Points[i].Y == cp.Y)
                        return i;
            }
            return -1;
        }


        #region Комманды
        private RelayCommand open_dev;
        public RelayCommand OpenDev
        {
            get
            {
                return open_dev ??
                  (open_dev = new RelayCommand(obj =>
                  {
                      OpenFileDialog OPF = new OpenFileDialog();
                      OPF.Filter = "DEV config files (*.dev)|*.dev|All files (*.*)|*.*";
                      OPF.FilterIndex = 0;
                      OPF.RestoreDirectory = true;

                      if (OPF.ShowDialog() == true)
                      {
                          MainModel.DevPath = OPF.FileName;
                          Message(MainModel.DevPath + " - файл считан успешно!");
                          OnPropertyChanged("IsDevRead");
                          OnPropertyChanged("DevName");
                      }
                      else
                      {
                          return;
                      }

                      UpdatePoints();

                      MainModel.Current_HZ = MainModel.Filter.HZ_Min;
                      MainModel.Current_WL = MainModel.Filter.WL_Max;
                      OnPropertyChanged("CurrentHZ");
                      OnPropertyChanged("CurrentWL");
                      

                  }));
            }
        }

        private void UpdatePoints()
        {
            Points = new ChartValues<ObservablePoint>();
            for (int i = 0; i < MainModel.X.Count; i++)
            {
                Points.Add(new ObservablePoint(MainModel.X[i], MainModel.Y[i]));
                Points[i].PropertyChanged += (s, e) =>
                {
                    var index = Points.IndexOf(s);
                    MainModel.X[index] = Points[index].X;
                    MainModel.Y[index] = Points[index].Y;
                };
            }
            Points.CollectionChanged += Points_CollectionChanged;
            OnPropertyChanged("Points");
        }

        private RelayCommand save_dev;
        public RelayCommand SaveDev
        {
            get
            {
                return save_dev ??
                  (save_dev = new RelayCommand(obj =>
                  {
                      SaveFileDialog SAF = new SaveFileDialog();
                      SAF.Filter = "DEV config files (*.dev)|*.dev";
                      SAF.DefaultExt = ".dev";
                      SAF.FilterIndex = 0;
                      SAF.RestoreDirectory = true;


                      if (SAF.ShowDialog() == true)
                      {
                          string FileName = SAF.FileName;
                          try
                          {
                              MainModel.SaveFileAsync(FileName);
                          }
                          catch (Exception ex)
                          {
                              Message(ex.Message);
                          }
                      }
                  }));
            }
        }

        private RelayCommand open_settings;
        public RelayCommand OpenSettings
        {
            get
            {
                return open_settings ??
                  (open_settings = new RelayCommand(obj =>
                  {
                      var Settings = new Settings_Window(this);
                      Settings.ShowDialog();
                      if (Points.Count != 0)
                        UpdatePoints();
                  }));
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
