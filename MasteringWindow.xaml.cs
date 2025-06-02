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

namespace Rika_Audio
{
    /// <summary>
    /// Interaction logic for MaseringWindow.xaml
    /// </summary>
    public partial class MasteringWindow : Window
    {
        string _wavPath;

        public MasteringWindow()
        {
            InitializeComponent();
        }

        private void SelHowToMaster(object sender, RoutedEventArgs e)
        {
            WindowManager.Open(typeof(HowToMasterWindow));
        }

        private void SelWhatToMaster(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "WAV files (*.wav)|*.wav",
                Title = "Select WAV File PLEASE"
            };

            _wavPath = dialog.ShowDialog() == true && File.Exists(dialog.FileName)
                ? dialog.FileName
                : string.Empty;

            if (!String.IsNullOrEmpty(_wavPath))
                WhatButton.Content = GetName(_wavPath);
        }

        string GetName(string path) => System.IO.Path.GetFileNameWithoutExtension(path);
    }
}
