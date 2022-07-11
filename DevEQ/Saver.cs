using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevEQ
{
    class DevSaver
    {
        private float[] HZ;
        private float[] WL;
        private float[] Intensity;
        private string Path;

        public DevSaver(float[] HZIn, float[] WLIn, float[] IntensIn, string PathIn)
        {
            HZ = HZIn;
            WL = WLIn;
            Intensity = IntensIn;
            Path = PathIn;
        }

        public void Save()
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
}
