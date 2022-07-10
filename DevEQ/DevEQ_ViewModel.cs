using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        public void Message(string message)
        {
            if (null == message)
            {
                throw new ArgumentNullException("message");
            }
            string data = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}: {1}", DateTime.Now, message);
            Logs.Add(data);
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

        public DevEQ_Model MainModel { get; private set; }

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
                      }

                      Points.Clear();
                      for (int i = 0; i < MainModel.X.Count; i++)
                      {
                          Points.Add(new ObservablePoint(MainModel.X[i], MainModel.Y[i]));
                          Points[i].PropertyChanged += (s, e) => { MainModel.X[i] = Points[i].X; MainModel.Y[i] = Points[i].Y; };
                      }
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
