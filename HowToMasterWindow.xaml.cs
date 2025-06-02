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

namespace Rika_Audio
{
    /// <summary>
    /// Interaction logic for HowToMasterWindow.xaml
    /// </summary>
    public partial class HowToMasterWindow : Window
    {
        string[] _wavPaths;
        float[] _weights;
        bool _ignoreMe;

        public HowToMasterWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "WAV files (*.wav)|*.wav",
                Title = "Select WAV Files PLEASE",
                Multiselect = true
            };

            _wavPaths = dialog.ShowDialog() == true
                ? dialog.FileNames.Where(File.Exists).ToArray()
                : Array.Empty<string>();

            if (_wavPaths.Length > 0)
            {
                _weights = new float[_wavPaths.Length];
                TracksBox.Items.Clear();

                for (int i = 0; i < _wavPaths.Length; i++)
                {
                    _weights[i] = 100;
                    string name = GetName(_wavPaths[i]);
                    TracksBox.Items.Add($"({_weights[i]}) {name}");
                }

                Weight.Text = "100";
            }
        }

        private void MakeModel(object sender, RoutedEventArgs e)
        {
            MASTERER.MakeModel(_wavPaths, _weights);
        }

        string GetName(string path) => System.IO.Path.GetFileNameWithoutExtension(path);

        private void Weight_TextChanged(object sender, TextChangedEventArgs e)
        {
            float result;
            var text = ((TextBox)sender).Text;

            bool isValid = float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result);

            float weightValue = (isValid && result >= 0) ? result : 0f;

            if (isValid)
            {
                int id = TracksBox.SelectedIndex;
                if (id > -1)
                {
                    _weights[id] = weightValue;
                    TracksBox.Items[id] = $"({_weights[id]}) {GetName(_wavPaths[id])}";
                    _ignoreMe = true;
                    TracksBox.SelectedIndex = id;
                }
                Logger.Log($"Weight changed to {weightValue}.");
            }
            else
            {
                Logger.Log($"Invalid weight value: {text}.");
            }
        }

        private void TracksBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int id = TracksBox.SelectedIndex;
            if (id > -1 && !_ignoreMe)
                Weight.Text = _weights[id].ToString("0.0###", CultureInfo.InvariantCulture);
            else if (_ignoreMe)
                _ignoreMe = false;
        }
    }
}
