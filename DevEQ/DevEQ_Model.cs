
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

namespace DevEQ
{
    public class DevEQ_Model : INotifyPropertyChanged
    {
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

        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Interpolator = MathNet.Numerics.Interpolate.CubicSpline(X, Y);
            for (int i = 0; i < Hzs.Count; i++)
            {
                Intens[i] = Interpolator.Interpolate(Hzs[i]);
            }
            var HzToAOF = Double2FloatArray(Hzs);
            var IntensToAOF = Double2FloatArray(Intens);
            var WlsToAof = Double2FloatArray(Wls);
            Filter.EditAllData(WlsToAof.Reverse().ToArray(), HzToAOF.Reverse().ToArray(), IntensToAOF.Reverse().ToArray());
            Filter.Set_Hz(Filter.HZ_Current);
            
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

        private int points_count = 10;
        public int PointsCount
        {
            get { return points_count; }
            set { points_count = value; }
        }

        private double min_x = 0;
        public double minX { get { return min_x; } private set { min_x = value; OnPropertyChanged(); } }

        private double max_x = 120;
        public double maxX { get { return max_x; } private set { max_x = value; OnPropertyChanged(); } }

        public double minY { get { return 0; } }
        public double maxY { get { return 2700; } }

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
                    var Status = Filter.Read_dev_file(dev_path);
                    FilterChaged();
                    if (Status != 0)
                        throw new Exception(Filter.Implement_Error(Status)); 

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
                Y.Add(Interpolator.Interpolate(i));
            }
            X.CollectionChanged += CollectionChanged;
            Y.CollectionChanged += CollectionChanged;
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
            string[] AllStrings = Data_from_dev.ToArray();
            float[] Params = new float[3];
            Hzs = new ObservableCollection<double>();
            Wls = new ObservableCollection<double>();
            Intens = new ObservableCollection<double>();
            for (int i = 0; i < Data_from_dev.ToArray().Length; i++)
            {
                try
                {
                    Helper.Files.Get_WLData_fromDevString(AllStrings[i], 3, Params);
                    Hzs.Add(Params[0]); Wls.Add(Params[1]); Intens.Add(Params[2]);
                }
                catch
                {
                    continue;
                }
            }
            Hzs.Reverse();
            Wls.Reverse();
            Intens.Reverse();
        }

        public async void SaveFileAsync(string Name)
        {
            await Task.Run(() =>
            {
                var HzToAOF = Double2FloatArray(Hzs);
                var IntensToAOF = Double2FloatArray(Intens);
                var WlsToAOF = Double2FloatArray(Wls);
                DevSaver NewSaver = new DevSaver(HzToAOF.Reverse().ToArray(), WlsToAOF.Reverse().ToArray(), IntensToAOF.Reverse().ToArray(), Name);
                NewSaver.Save();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
