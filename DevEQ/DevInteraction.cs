using AO_Lib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shell;

namespace DevEQ
{
    public abstract class DevInteraction
    {
        virtual public (ObservableCollection<double>, ObservableCollection<double>, ObservableCollection<double>) ReadDevStrings(string dev_path)
        {

            var Data_from_dev = Helper.Files.Read_txt(dev_path);
            string[] AllStrings = Data_from_dev.ToArray();
            float[] Params = new float[3];
            ObservableCollection<double>  Hzs = new ObservableCollection<double>();
            ObservableCollection<double>  Wls = new ObservableCollection<double>();
            ObservableCollection<double>  Intens = new ObservableCollection<double>();
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
            return (Hzs, Wls, Intens);
        }

        virtual public void Save(float[] HZ, float[] WL, float[] Intensity, string Path)
        {
            var CurCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            try
            {
                using (StreamWriter SW = new StreamWriter(Path, false, Encoding.UTF8))
                    for (int i = HZ.Length - 1; i >= 0; i--)
                    {
                        SW.Write(HZ[i].ToString("0.000") + "    " + WL[i].ToString("0.000") + "    " + Intensity[i].ToString("0.000") + Environment.NewLine);
                    }
            }
            catch
            {
                throw new Exception("Saver: Save file error");
            }
            System.Threading.Thread.CurrentThread.CurrentCulture = CurCulture;
        }
    }

    public class CommonDevInteraction : DevInteraction
    {

    }

    public class OldDevInteraction : DevInteraction
    {
        private List<string> AdditionalStrings = new List<string>();
        
        public override (ObservableCollection<double>, ObservableCollection<double>, ObservableCollection<double>) ReadDevStrings(string dev_path)
        {
            var Data_from_dev = Helper.Files.Read_txt(dev_path);
            string[] AllStrings = Data_from_dev.ToArray();
            float[] Params = new float[3];
            ObservableCollection<double> Hzs = new ObservableCollection<double>();
            ObservableCollection<double> Wls = new ObservableCollection<double>();
            ObservableCollection<double> Intens = new ObservableCollection<double>();
            var RelatInd = Data_from_dev.IndexOf("[Relation]");
            var countOfRelat = int.Parse(Data_from_dev[RelatInd + 1]);
            for (int i = RelatInd + 2; i < RelatInd + 2 + countOfRelat; i++)
            {
                Helper.Files.Get_WLData_fromDevString(AllStrings[i], 3, Params);
                Hzs.Add(Params[0]); Wls.Add(Params[1]); Intens.Add(Params[2]);
            }
            
            AdditionalStrings.AddRange(AllStrings.Skip(RelatInd + 2 + countOfRelat));

            Hzs.Reverse();
            Wls.Reverse();
            Intens.Reverse();
            return (Hzs, Wls, Intens);
        }

        override public void Save(float[] HZ, float[] WL, float[] Intensity, string Path)
        {
            var CurCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            try
            {
                using (StreamWriter SW = new StreamWriter(Path, false, Encoding.UTF8))
                {
                    SW.WriteLine("[Relation]");
                    SW.WriteLine(HZ.Length.ToString());
                    for (int i = HZ.Length - 1; i >= 0; i--)
                    {
                        SW.Write(HZ[i].ToString("0.000") + "    " + WL[i].ToString("0.000") + "    " + Intensity[i].ToString("0.000000") + Environment.NewLine);
                    }
                    foreach (string str in AdditionalStrings)
                    {
                        SW.WriteLine(str);
                    }
                }

            }
            catch
            {
                throw new Exception("Saver: Save file error");
            }
            System.Threading.Thread.CurrentThread.CurrentCulture = CurCulture;
        }
    }
}
