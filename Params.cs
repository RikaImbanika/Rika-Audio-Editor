using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Controls;

namespace Rika_Audio
{
    public static class Params
    {
        public static int _windowSize = 2048;
        public static string _windowType = "Hanning";
        public static float _overlap = 0.5f;
        public static string _pf;
        public static int _resolution = 500;
        public static int _selectedModelId;
        public static string _selectedModelPath;
        public static string _selectedModelName;

        public static void Init()
        {
            _pf = Environment.CurrentDirectory + "\\ProgramFiles\\";
            _selectedModelName = File.ReadAllText($"{_pf}Params\\SelectedModelName.txt");
            MASTERER.LoadModelsPaths();
            _selectedModelId = Array.IndexOf(MASTERER._modelsPaths, $"{_pf}Models\\{_selectedModelName}.bin");
            _selectedModelPath = MASTERER._modelsPaths[_selectedModelId];
        }

        public static void LateInit()
        {
            string path = $"{_pf}Pics\\FLUTTERSHY TESO.jpg";
            Uri uri = new Uri(path);
            var gg = new System.Windows.Media.Imaging.BitmapImage(uri);
            WindowManager._mainWindow.PIC.Source = gg;
        }
    }
}