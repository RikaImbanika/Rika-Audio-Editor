using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Rika_Audio
{
    public static class Logger
    {
        public static string _log;
        public static bool _updated;

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
                    Thread.Sleep(200);

                    if (_updated)
                    {
                        if (WindowManager._logsWindow == null)
                        {
                            WindowManager.Open(typeof(LogsWindow));
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                WindowManager._logsWindow.Logs.Text = _log;
                                _updated = false;
                            }
                            catch (Exception ex)
                            {
                                string wtfomfg = ex.Message;
                                WindowManager.Close(typeof(LogsWindow));
                                Thread.Sleep(666);
                                WindowManager.Open(typeof(LogsWindow));
                            }
                        });
                    }
                }
            }
        }
    }
}