
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
        public AO_Lib.AO_Devices.AO_Filter Filter;

        public DevEQ_Model()
        {
            X.CollectionChanged += CollectionChanged;
            Y.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Interpolator = MathNet.Numerics.Interpolate.CubicSpline(X, Y);
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

        private double minX;
        private double maxX;
        private double minY;
        private double maxY;

        
        public ObservableCollection<double> X;

        public ObservableCollection<double> Y;
        

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
                    if (Status != 0)
                        throw new Exception(Filter.Implement_Error(Status)); 

                }
                catch (Exception exc)
                {
                    throw new Exception(exc.Message);
                }

                ReadDevStrings();

                IntializateInterpolator();

            } 
        }

        private void IntializateInterpolator()
        {
            Interpolator = MathNet.Numerics.Interpolate.CubicSpline(Hzs, Intens);
            minX = Hzs.Min();
            maxX = Hzs.Max();
            minY = Intens.Min();
            maxY = Intens.Max();
            X = new ObservableCollection<double>();
            Y = new ObservableCollection<double>();
            var stepX = (maxX - minX)/((double)PointsCount);
            for (double i = minX; i <= maxX; i+= stepX)
            {
                X.Add(i);
                Y.Add(Interpolator.Interpolate(i));
            }
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


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
