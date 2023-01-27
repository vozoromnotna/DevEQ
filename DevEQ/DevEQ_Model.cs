
using AO_Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AO_Lib.AO_Devices;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MathNet.Numerics.Interpolation;
using System.Timers;
using System.Xml.Linq;

namespace DevEQ
{
    public class DevEQ_Model : INotifyPropertyChanged
    {
        private DevInteraction DI;
        private AO_Filter filter;
        public AO_Filter Filter
        {
            get { return filter; }
            set 
            {
                filter = value;
                OnPropertyChanged();
                FilterChaged();
            }
        }

        private void FilterChaged()
        {
            OnPropertyChanged("minWL");
            OnPropertyChanged("maxWL");
            OnPropertyChanged("minHZ");
            OnPropertyChanged("maxHZ");
        }


        public double Current_HZ
        { 
            get
            {
                return (double)Filter.HZ_Current;
            }

            set
            {
                if (value < Filter.HZ_Min) return;
                if (value > Filter.HZ_Max) return;
                Filter.Set_Hz((float)value);
                OnPropertyChanged();
                OnPropertyChanged("Current_WL");
            }
        }

        public double Current_WL
        {
            get
            {
                return Filter.WL_Current;
            }

            set
            {
                Filter.Set_Wl((float)value);
                if (value < Filter.WL_Min) return;
                if (value > Filter.WL_Max) return;
                Filter.Set_Wl((float)value);
                OnPropertyChanged();
                OnPropertyChanged("Current_HZ");
            }
        }

        public DevEQ_Model()
        {

        }

        Timer AOF_Set_Timer;
        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (AOF_Set_Timer != null)
                if (AOF_Set_Timer.Enabled) AOF_Set_Timer.Stop();
            AOF_Set_Timer = new System.Timers.Timer(10);
            AOF_Set_Timer.AutoReset = false;
            AOF_Set_Timer.Elapsed += (s, p) =>
            {
                Interpolator = MathNet.Numerics.Interpolate.CubicSpline(X, Y);
                for (int i = 0; i < Hzs.Count; i++)
                {
                    Intens[i] = Interpolator.Interpolate(Hzs[i]) * maxIntense / 100;
                }
                var HzToAOF = Double2FloatArray(Hzs);
                var IntensToAOF = Double2FloatArray(Intens);
                var WlsToAOF = Double2FloatArray(Wls);
                Filter.EditAllData(WlsToAOF.Reverse().ToArray(), HzToAOF.Reverse().ToArray(), IntensToAOF.Reverse().ToArray());
                Filter.Set_Hz(Filter.HZ_Current);
                if (DI is OldDevInteraction)
                {
                    
                    if (Filter.FilterType != FilterTypes.Emulator)
                    {
                        Filter.PowerOff();
                        DI.Save(HzToAOF.Reverse().ToArray(), WlsToAOF.Reverse().ToArray(), IntensToAOF.Reverse().ToArray(), "temp.dev");
                        var Status = Filter.Read_dev_file("temp.dev");
                        Filter.PowerOn();
                    }
                }

            };
            AOF_Set_Timer.Enabled = true;

            
            
        }

        private float[] Double2FloatArray(IEnumerable<double> list)
        {
            float[] result = new float[list.Count()];
            for(int i = 0; i < list.Count(); i++)
            {
                result[i] = (float)list.ElementAt(i);
            }
            return result;
        }

        private ObservableCollection<double> Hzs; //хранит информацию о том, какие строчки записаны в исходном файле
        private ObservableCollection<double> Wls;
        private ObservableCollection<double> Intens;
        private IInterpolation Interpolator;

        private int points_count = 15;
        public int PointsCount
        {
            get { return points_count; }
            set { points_count = value; OnPropertyChanged(); }
        }

        private double min_x = 0;
        public double minX { get { return min_x; } private set { min_x = value; OnPropertyChanged(); } }

        private double max_x = 120;
        public double maxX { get { return max_x; } private set { max_x = value; OnPropertyChanged(); } }

        public double minY { get { return 0; } }
        public double maxY { get { return 100; } }

        private double max_intense = 2100;
        public double maxIntense
        {
            get { return max_intense; }
            set { max_intense = value; OnPropertyChanged(); if (DevPath != null) IntializateInterpolator(); }
        }


        public double minWL
        {
            get
            {
                try
                {
                    return Filter.WL_Min;
                }
                catch (Exception)
                {
                    return 0;
                }
            }

        }
        public double maxWL
        {
            get
            {
                try
                {
                    return Filter.WL_Max;
                }
                catch (Exception)
                {
                    return 100;
                }
            }
        }
        public double minHZ
        {
            get
            {
                try
                {
                    return Filter.HZ_Min;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        public double maxHZ
        {
            get
            {
                try
                {
                    return Filter.HZ_Max;
                }
                catch (Exception)
                {
                    return 100;
                }
            }
        }

        private bool save_init_hz = true;
        public bool SaveInitHz
        {
            get
            {
                return save_init_hz;
            }
            set
            {
                save_init_hz = value; OnPropertyChanged();
            }
        }

        private int points_count_to_save = 15;

        public int PointsCountToSave
        {
            get
            {
                return points_count_to_save;
            }
            set
            {
                points_count_to_save = value; OnPropertyChanged();
            }

        }

        public ObservableCollection<double> X;

        public ObservableCollection<double> Y;

        public void AddPoint(double x, double y)
        {
            X.CollectionChanged -= CollectionChanged;
            Y.CollectionChanged -= CollectionChanged;
            X.Add(x);
            Y.Add(y);
            X.CollectionChanged += CollectionChanged;
            Y.CollectionChanged += CollectionChanged;
        }

        public void InsertPoint(double x, double y, int index)
        {
            X.CollectionChanged -= CollectionChanged;
            Y.CollectionChanged -= CollectionChanged;
            X.Insert(index, x);
            Y.Insert(index, x);
            X.CollectionChanged += CollectionChanged;
            Y.CollectionChanged += CollectionChanged;
        }

        public void RemovePoint(int index)
        {
            X.CollectionChanged -= CollectionChanged;
            Y.CollectionChanged -= CollectionChanged;
            X.RemoveAt(index);
            Y.RemoveAt(index);
            X.CollectionChanged += CollectionChanged;
            Y.CollectionChanged += CollectionChanged;
        }

        private string dev_path;
        public string DevPath {
            get
            {
                return dev_path;
            }
            set
            {
                dev_path = value;
                try
                {
                    Filter.PowerOff();
                    var Status = Filter.Read_dev_file(dev_path);
                    FilterChaged();
                    if (Status != 0)
                        throw new Exception(Filter.Implement_Error(Status));
                    Filter.PowerOn();

                }
                catch (Exception exc)
                {
                    throw new Exception(exc.Message);
                }

                ReadDevStrings();

                IntializateInterpolator();

                OnPropertyChanged();

            } 
        }

        private void IntializateInterpolator()
        {
            Interpolator = MathNet.Numerics.Interpolate.CubicSpline(Hzs, Intens);
            minX = Hzs.Min();
            maxX = Hzs.Max();
            //minY = Intens.Min();
            //maxY = Intens.Max();
            X = new ObservableCollection<double>();
            Y = new ObservableCollection<double>();

            var stepX = (maxX - minX)/((double)PointsCount);
            for (double i = minX; i <= maxX; i+= stepX)
            {
                X.Add(i);
                Y.Add(Interpolator.Interpolate(i)/maxIntense * 100);
            }
            X.CollectionChanged += CollectionChanged;
            Y.CollectionChanged += CollectionChanged;
            CollectionChanged(null, null);
        }

        private void UpdateData()
        {
            Interpolator = MathNet.Numerics.Interpolate.CubicSpline(X, Y);
            for (int i = 0; i < Hzs.Count; i++)
            {

            }
        }

        private void ReadDevStrings()
        {
            
            var Data_from_dev = Helper.Files.Read_txt(dev_path);
            float[] Params = new float[3];
            try
            {
                Helper.Files.Get_WLData_fromDevString(Data_from_dev[0], 3, Params);
                DI = new CommonDevInteraction();
                (Hzs, Wls, Intens) = DI.ReadDevStrings(dev_path);
                maxIntense = Intens.Max();

            }
            catch
            {
                DI = new OldDevInteraction();
                (Hzs, Wls, Intens) = DI.ReadDevStrings(dev_path);
                maxIntense = Intens.Max();
            }

        }

        private (List<double> XToSave, List<double> YToSave) GetXYToSave()
        {
            List<double> XToSave = new List<double>();
            List<double> YToSave = new List<double>();
            var stepX = (maxX - minX) / ((double)PointsCountToSave);
            for (double i = minX; i <= maxX; i += stepX)
            {
                XToSave.Add(i);
                YToSave.Add(Interpolator.Interpolate(i) * maxIntense / 100);
            }
            return (XToSave, YToSave);

        }

        private List<double> GetWLsByHzs(List<double> HzList)
        {
            var WLInterpolator = MathNet.Numerics.Interpolate.CubicSpline(Hzs, Wls);

            var WLsList = new List<double>();

            foreach(var hz in HzList)
            {
                WLsList.Add(WLInterpolator.Interpolate(hz));
            }

            return WLsList;
        }

        public void SaveFileAsync(string Name)
        {
            IEnumerable<double> HzToSave;
            IEnumerable<double> IntensToSave;
            IEnumerable<double> WlToSave;
            if (SaveInitHz)
            {
                HzToSave = Hzs;
                IntensToSave = Intens;
                WlToSave = Wls;
            }
            else
            {
                (HzToSave, IntensToSave) = GetXYToSave();
                WlToSave = GetWLsByHzs(HzToSave as List<double>);
            }
            
            var HzToAOF = Double2FloatArray(HzToSave);
            var IntensToAOF = Double2FloatArray(IntensToSave);
            var WlsToAOF = Double2FloatArray(WlToSave);
            DI.Save(HzToAOF.Reverse().ToArray(), WlsToAOF.Reverse().ToArray(), IntensToAOF.Reverse().ToArray(), Name);

        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
