using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RIKA_AUDIO
{
    public static class MUSICMAKER
    {
        public static IMBA _imba;

        public static void Init()
        {
            LoadMMStat();

            _imba = new IMBA();
            _imba._name = "NEW TRACK";
            _imba._lastPath = $"UNSAVED";
            _imba._firstPath = "UNSAVED";
            _imba._creation = DateTime.UtcNow;
            _imba._lastSave = DateTime.UtcNow;
            _imba._bpm = 120f;
            _imba._lastLoad = DateTime.MaxValue;
            _imba._trackSavesCounter = 0;
            _imba._musicMakerTrackNumber = Params._musicMakerTracksMakedCount + 1;
            _imba._musicMakerSavesCounter = Params._musicMakerGlobalSavesCounter;

            WindowsManager._editorWindow.Title = $"Rika Audio Music Maker, unsaved";

            Logger.Log($"Initialized MUSICMAKER.");
        }

        public static void AddAudio()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "WAV files (*.wav)|*.wav",
                Title = "Select WAV File PLEASE"
            };

            string wavPath = dialog.ShowDialog() == true && File.Exists(dialog.FileName)
                ? dialog.FileName
                : string.Empty;

            byte[] bytes = File.ReadAllBytes(wavPath);
            Wav wav = AudioParser.Parse(bytes);
        }

        public static void New()
        {
            string text = $"So, u want to create new project.\r\nSo current one will be not saved. Are u sure u really want it teso?";
            var wtf = MessageBox.Show(text, "ARE U GAY?", MessageBoxButton.YesNo);

            if (wtf == MessageBoxResult.Yes)
            {
                Logger.Log($"Okay okay. Creating new project teso!");

                Init();
            }
        }

        public static void Open()
        {
            string text = $"So,u want to open new project.\r\nSo current one will be not saved. Are u sure u really want it teso?";
            var wtf = MessageBox.Show(text, "ARE U GAY?", MessageBoxButton.YesNo);
            
            if (wtf == MessageBoxResult.Yes)
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "IMBA files (*.imba)|*.imba",
                    Title = "Select IMBA File PLEASE"
                };

                bool? opening = dialog.ShowDialog();

                if (opening == true && File.Exists(dialog.FileName))
                {
                    _imba = IMBA.Load(dialog.FileName);
                    WindowsManager._editorWindow.Title = $"Rika Audio Music Maker, {_imba._name}";
                    Logger.Log($"Opened {_imba._name} from {dialog.FileName} teso!");
                    Logger.Log(_imba.ToString());
                }
            }
        }

        public static void Save()
        {
            if (_imba._lastPath.Equals("UNSAVED"))
                SaveAss();
            else
            {
                string text = $"Are you sure?\r\nAre u really want to save it?";
                var wtf = MessageBox.Show(text, "ARE U GAY?", MessageBoxButton.YesNo);

                if (wtf == MessageBoxResult.Yes)
                {
                    _imba._lastSave = DateTime.UtcNow;
                    _imba._trackSavesCounter++;
                    Params._musicMakerGlobalSavesCounter++;
                    _imba._musicMakerSavesCounter = Params._musicMakerGlobalSavesCounter;
                    IMBA.Save(_imba, _imba._lastPath);

                    SaveMMStat();

                    Logger.Log($"Saved {_imba._name} to {_imba._lastPath} teso!");
                }
            }
        }

        public static void SaveAss()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "So we are saving now teso",
                Filter = "IMBA files (*.imba)|*.imba"
            };

            bool? so = dialog.ShowDialog();

            if (so == true)
            {
                _imba._lastPath = dialog.FileName;

                _imba._name = dialog.SafeFileName;
                _imba._name = _imba._name.Remove(_imba._name.Length - 5);

                _imba._lastSave = DateTime.Now;

                if (_imba._firstPath == "UNSAVED")
                {
                    _imba._firstPath = _imba._lastPath;
                    Params._musicMakerTracksMakedCount++;
                    _imba._musicMakerTrackNumber = Params._musicMakerTracksMakedCount;
                }

                Params._musicMakerGlobalSavesCounter++;

                _imba._trackSavesCounter++;
                _imba._musicMakerSavesCounter = Params._musicMakerGlobalSavesCounter;

                Logger.Log($"Trying to save .imba now. Path = {_imba._lastPath}. Name = {_imba._name}.");

                IMBA.Save(_imba, _imba._lastPath);

                WindowsManager._editorWindow.Title = $"Rika Audio Music Maker, {_imba._name}";

                SaveMMStat();

                Logger.Log($"Saved.");
            }
        }

        private static void LoadMMStat()
        {
            Stream sr = File.OpenRead(Params._MMStatPath);
            BinaryReader br = new BinaryReader(sr);

            Params._musicMakerTracksMakedCount = br.ReadUInt64();
            Params._musicMakerGlobalSavesCounter = br.ReadUInt64();

            br.Dispose();
            sr.DisposeAsync();

            Logger.Log($"Tracks created {Params._musicMakerTracksMakedCount}.");
            Logger.Log($"Tracks saves count {Params._musicMakerGlobalSavesCounter}.");
        }

        private static void SaveMMStat()
        {
            Stream ws = File.OpenWrite(Params._MMStatPath);
            BinaryWriter bw = new BinaryWriter(ws);

            bw.Write(Params._musicMakerTracksMakedCount);
            bw.Write(Params._musicMakerGlobalSavesCounter);

            ws.DisposeAsync();
            bw.DisposeAsync();

            Logger.Log($"Tracks created {Params._musicMakerTracksMakedCount}.");
            Logger.Log($"Tracks saves count {Params._musicMakerGlobalSavesCounter}.");
            Logger.Log($"{_imba._name} saves count {_imba._trackSavesCounter}.");
        }
    }
}
