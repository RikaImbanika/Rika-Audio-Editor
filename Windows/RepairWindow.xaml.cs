using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using System.Windows.Shapes;

namespace RIKA_AUDIO
{
    /// <summary>
    /// Interaction logic for RepairWindow.xaml
    /// </summary>
    public partial class RepairWindow : Window
    {
        string _wavPath;
        public RepairWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "WAV files (*.wav)|*.wav",
                Title = "Select WAV File"
            };

            _wavPath = dialog.ShowDialog() == true && File.Exists(dialog.FileName)
                ? dialog.FileName
                : string.Empty;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_wavPath))
                Logger.Log("WAV PATH NOT SELECTED!");
            else
            {
                byte[] wavBytes = File.ReadAllBytes(_wavPath);
                var wav = AudioParser.Parse(wavBytes);

                string ct = CT.Text;
                string ps = PS.Text;
                string ht = HT.Text;
                string st = StartTime.Text;
                string et = EndTime.Text;
                string lfc = LFC.Text;
                //string th = TH.Text;
                string lt = LT.Text;
                string maxt = AmpThreshold.Text;
                string mt = MT.Text;

                if (string.IsNullOrEmpty(et))
                    et = "0";

                if (string.IsNullOrEmpty(st))
                    st = "0";


                Declicker.Declick(wav, ct, ps, ht, st, et, lfc, mt, lt, maxt);
            }
        }
    }
}
