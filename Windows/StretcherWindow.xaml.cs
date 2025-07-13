using System;
using System.Collections.Generic;
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
    public partial class StretcherWindow : Window
    {
        public string _wavPath1;
        public string _wavPath2;

        public StretcherWindow()
        {
            InitializeComponent();
        }

        private void SelFirstTrack(object sender, RoutedEventArgs e)
        {
            Logger.Log($"Selecting track 1...");

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "WAV files (*.wav)|*.wav",
                Title = "Select WAV File PLEASE"
            };

            _wavPath1 = dialog.ShowDialog() == true && File.Exists(dialog.FileName)
                ? dialog.FileName
                : string.Empty;

            if (!String.IsNullOrEmpty(_wavPath1))
            {
                Logger.Log($"Selected track 1 as {_wavPath1}.");
                STRETCHER.RenderTrack(_wavPath1);
            }
        }

        private void SelSecondTrack(object sender, RoutedEventArgs e)
        {
            Logger.Log($"Selecting track 2...");

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "WAV files (*.wav)|*.wav",
                Title = "Select WAV File PLEASE"
            };

            _wavPath2 = dialog.ShowDialog() == true && File.Exists(dialog.FileName)
                ? dialog.FileName
                : string.Empty;

            if (!String.IsNullOrEmpty(_wavPath2))
            {
                Logger.Log($"Selected track 2 as {_wavPath2}.");
                STRETCHER.RenderTrack(_wavPath2);
            }
        }
    }
}