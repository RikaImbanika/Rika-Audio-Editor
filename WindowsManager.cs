using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace RIKA_AUDIO
{
    public static class WindowsManager
    {
        public static MainWindow _mainWindow;
        public static RepairWindow _repairWindow;
        public static LogsWindow _logsWindow;
        public static MasteringWindow _masteringWindow;
        public static HowToMasterWindow _howToMasterWindow;
        public static StretcherWindow _stretcherWindow;
        public static MusicMakerWindow _editorWindow;
        public static AudioPlayerWindow _audioPlayerWindow;

        public static void Open(Type windowT)
        {
            try
            {
                Window window = null;

                if (windowT == typeof(RepairWindow)) window = _repairWindow;
                else if (windowT == typeof(LogsWindow)) window = _logsWindow;
                else if (windowT == typeof(MasteringWindow)) window = _masteringWindow;
                else if (windowT == typeof(HowToMasterWindow)) window = _howToMasterWindow;
                else if (windowT == typeof(StretcherWindow)) window = _stretcherWindow;
                else if (windowT == typeof(MusicMakerWindow)) window = _editorWindow;
                else if (windowT == typeof(AudioPlayerWindow)) window = _audioPlayerWindow;

                if (window == null || PresentationSource.FromVisual(window) == null)
                {
                    Reopen(windowT);
                    return;
                }

                window.Dispatcher.Invoke(() =>
                {
                    if (window.WindowState == WindowState.Minimized)
                        window.WindowState = WindowState.Normal;

                    if (!window.IsVisible)
                        window.Show();

                    window.Activate();
                    window.Topmost = true;
                    window.Topmost = false;
                    window.Focus();
                });
            }
            catch (InvalidOperationException)
            {
                Reopen(windowT);
            }

            void Reopen(Type type)
            {
                if (type == typeof(RepairWindow))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _repairWindow?.Close();
                        _repairWindow = new RepairWindow();
                        _repairWindow.Closed += (s, e) => _repairWindow = null;
                        _repairWindow.Show();
                    });
                }
                else if (type == typeof(LogsWindow))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _logsWindow?.Close();
                        _logsWindow = new LogsWindow();
                        _logsWindow.Closed += (s, e) => _logsWindow = null;
                        _logsWindow.Show();
                    });
                }
                else if (type == typeof(MasteringWindow))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _masteringWindow?.Close();
                        _masteringWindow = new MasteringWindow();
                        _masteringWindow.Closed += (s, e) => _masteringWindow = null;
                        _masteringWindow.Show();
                    });
                }
                else if (type == typeof(HowToMasterWindow))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _howToMasterWindow?.Close();
                        _howToMasterWindow = new HowToMasterWindow();
                        _howToMasterWindow.Closed += (s, e) => _howToMasterWindow = null;
                        _howToMasterWindow.Show();
                    });
                }
                else if (type == typeof(StretcherWindow))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _stretcherWindow?.Close();
                        _stretcherWindow = new StretcherWindow();
                        _stretcherWindow.Closed += (s, e) => _stretcherWindow = null;
                        _stretcherWindow.Show();
                    });
                }
                else if (type == typeof(MusicMakerWindow))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _editorWindow?.Close();
                        _editorWindow = new MusicMakerWindow();
                        _editorWindow.Closed += (s, e) => _editorWindow = null;
                        _editorWindow.Show();
                    });
                }
                else if (type == typeof(AudioPlayerWindow))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _audioPlayerWindow?.Close();
                        _audioPlayerWindow = new AudioPlayerWindow();
                        _audioPlayerWindow.Closed += (s, e) => _audioPlayerWindow = null;
                        _audioPlayerWindow.Show();
                    });
                }
            }
        }

        public static void Close(Type WindowT)
        {
            Window window = null;

            if (WindowT == typeof(RepairWindow)) window = _repairWindow;
            else if (WindowT == typeof(LogsWindow)) window = _logsWindow;
            else if (WindowT == typeof(MasteringWindow)) window = _masteringWindow;
            else if (WindowT == typeof(HowToMasterWindow)) window = _howToMasterWindow;
            else if (WindowT == typeof(StretcherWindow)) window = _stretcherWindow;
            else if (WindowT == typeof(MusicMakerWindow)) window = _editorWindow;
            else if (WindowT == typeof(AudioPlayerWindow)) window = _audioPlayerWindow;

            if (window == null) return;

            try
            {
                window.Dispatcher.Invoke(() =>
                {
                    if (PresentationSource.FromVisual(window) != null)
                    {
                        window.Close();
                        if (WindowT == typeof(RepairWindow)) _repairWindow = null;
                        else if (WindowT == typeof(LogsWindow)) _logsWindow = null;
                        else if (WindowT == typeof(MasteringWindow)) _masteringWindow = null;
                        else if (WindowT == typeof(HowToMasterWindow)) _howToMasterWindow = null;
                        else if (WindowT == typeof(StretcherWindow)) _stretcherWindow = null;
                        else if (WindowT == typeof(MusicMakerWindow)) _stretcherWindow = null;
                        else if (WindowT == typeof(AudioPlayerWindow)) _audioPlayerWindow = null;
                    }
                });
            }
            catch (InvalidOperationException)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (WindowT == typeof(RepairWindow)) _repairWindow = null;
                    else if (WindowT == typeof(LogsWindow)) _logsWindow = null;
                    else if (WindowT == typeof(MasteringWindow)) _masteringWindow = null;
                    else if (WindowT == typeof(HowToMasterWindow)) _howToMasterWindow = null;
                    else if (WindowT == typeof(StretcherWindow)) _stretcherWindow = null;
                    else if (WindowT == typeof(MusicMakerWindow)) _stretcherWindow = null;
                    else if (WindowT == typeof(AudioPlayerWindow)) _audioPlayerWindow = null;
                });
            }
        }
    }
}