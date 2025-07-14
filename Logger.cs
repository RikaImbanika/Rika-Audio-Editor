using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RIKA_IMBANIKA_AUDIO
{
    public static class Logger
    {
        public static string _log;
        public static bool _updated;
        public static bool _minimised;

        public static void Log(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"[{timestamp}] {message}";

            _log = string.IsNullOrEmpty(_log)
                ? logEntry
                : $"{logEntry}\n{_log}";

            _updated = true;
        }

        public static void StartLogShower()
        {
            Thread mt = new Thread(MT);
            mt.Name = "Log Shower";
            mt.Start();

            void MT()
            {
                while (true)
                {
                    if (!_minimised)
                        Thread.Sleep(200);
                    else
                        Thread.Sleep(1000);

                    if (_updated)
                    {
                        if (WindowsManager._logsWindow == null)
                        {
                            WindowsManager.Open(typeof(LogsWindow));
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (WindowsManager._logsWindow.WindowState != WindowState.Minimized)
                            {
                                try
                                {
                                    WindowsManager._logsWindow.Logs.Text = _log;
                                    _updated = false;
                                    if (_minimised)
                                        _minimised = false;
                                }
                                catch (Exception ex)
                                {
                                    string wtfomfg = ex.Message;
                                    WindowsManager.Close(typeof(LogsWindow));
                                    Thread.Sleep(666);
                                    WindowsManager.Open(typeof(LogsWindow));

                                    if (_minimised)
                                        _minimised = false;
                                }
                            }
                            else
                            {
                                _minimised = true;
                            }
                        });
                    }
                }
            }
        }
    }
}