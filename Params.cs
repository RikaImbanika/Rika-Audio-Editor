using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Controls;

namespace RIKA_IMBANIKA_AUDIO
{
    public static class Params
    {
        public static string _MMStatPath;

        public static int _windowSize = 2048;
        public static string _windowType = "Hanning";
        public static float _overlap = 0.5f;
        public static string _pf;
        public static int _resolution = 500;
        public static int _selectedModelId;
        public static string _selectedModelPath;
        public static string _selectedModelName;

        public static ulong _musicMakerTracksMakedCount;
        public static ulong _musicMakerGlobalSavesCounter;
        public static UInt32 _appVersion;

        public static void Init()
        {
            ProgramFiles.Init();

            _selectedModelName = File.ReadAllText($"{PF}Params\\SelectedModelName.txt");
            MASTERER.LoadModelsPaths();
            _selectedModelId = Array.IndexOf(MASTERER._modelsPaths, $"{PF}Models\\{_selectedModelName}.bin");
            _selectedModelPath = MASTERER._modelsPaths[_selectedModelId];
            _appVersion = 1;
            _MMStatPath = $"{PF}Params\\MMStat";
        }

        public static void LateInit()
        {
            string path = $"{PF}Pics\\FLUTTERSHY TESO.jpg";
            Uri uri = new Uri(path);
            var gg = new System.Windows.Media.Imaging.BitmapImage(uri);
            WindowsManager._mainWindow.PIC.Source = gg;

            Calculator.Calculate();
        }

        public static string PF
        {
            get
            {
                return _pf;
            }
            set
            {
                _pf = value;
            }
        }
    }
}