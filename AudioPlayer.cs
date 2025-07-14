using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace RIKA_IMBANIKA_AUDIO
{
    public static class AudioPlayer
    {
        static bool _inititalized;
        static string[] _wavPaths;
        static string[] _names;
        static int _index;
        static Wav _wav;
        static MediaElement _player;
        static DesktopTextWindow _dtw;
        static bool _paused;
        static string _title;
        static Thread _trackTheTrackThread;

        static AudioPlayer()
        {

        }

        public static void Init()
        {
            if (!_inititalized)
            {
                _wavPaths = new string[0];
                _names = new string[0];
                _player = new MediaElement();
                _player.LoadedBehavior = MediaState.Manual;
                _player.UnloadedBehavior = MediaState.Manual;
                _player.MediaEnded += (s, e) => Next();
                _paused = true;
                _title = "RIKA IMBANIKA AUDIO - SPECTRUM PLAYER";

                Logger.Log($"Audio Payer initialized teso.");
                _inititalized = true;
            }
        }

        public static void Open()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "WAV files (*.wav)|*.wav",
                Title = "Select WAV Files PLEASE",
                Multiselect = true
            };

            var newWavPaths = dialog.ShowDialog() == true
                ? dialog.FileNames.Where(File.Exists).ToArray()
                : Array.Empty<string>();

            if (newWavPaths.Length > 0)
            {
                int counter = 0;

                for (int i = 0; i < newWavPaths.Length; i++)
                {
                    if (!String.IsNullOrEmpty(newWavPaths[i]))
                    {
                        bool was = false;

                        for (int j = 0; j < _wavPaths.Length; j++)
                        {
                            if (_wavPaths[j].Equals(newWavPaths[i]))
                            {
                                _wavPaths[j] = "DELME";
                                was = true;
                            }
                        }

                        if (!was)
                            counter++;
                    }
                }

                var wavPaths2 = new string[_wavPaths.Length + counter];

                int id = 0;
                for (int i = 0; i < _wavPaths.Length; i++)
                {
                    if (!_wavPaths[i].Equals("DELME"))
                    {
                        wavPaths2[id] = _wavPaths[i];
                        id++;
                    }
                }

                _index = id;

                for (int i = 0; i < newWavPaths.Length; i++)
                {
                    wavPaths2[id] = newWavPaths[i];
                    id++;
                }

                _wavPaths = wavPaths2;
                _names = new string[_wavPaths.Length];

                string namesString = "";

                for (int i = 0; i < _wavPaths.Length; i++)
                {
                    if (!String.IsNullOrEmpty(_wavPaths[i]))
                    {
                        _names[i] = GetName(_wavPaths[i]);
                        namesString += $"{_names[i]}, ";
                    }
                }

                namesString = namesString.Remove(namesString.Length - 2);

                Logger.Log($"Selected {namesString} teso.");
                StartPlay();

                Thread mt = new Thread(MT);
                mt.Start();

                void MT()
                {
                    if (!String.IsNullOrEmpty(_wavPaths[0]))
                    {
                        _wav = new Wav();
                        var bytes = File.ReadAllBytes(_wavPaths[0]);
                        Logger.Log($"Readen {_names[0]} teso.");
                        _wav = AudioParser.Parse(bytes);
                        Logger.Log($"Parsed {_names[0]} teso.");
                    }
                }
            }
        }

        public static void Teleport(double progress)
        {
            Duration dura = _player.NaturalDuration;
            if (dura.HasTimeSpan)
            {
                double nowMs = dura.TimeSpan.TotalMilliseconds * progress;
                TimeSpan now = TimeSpan.FromMilliseconds(nowMs);
                _player.Position = now;
            }

            SetProgress(progress);
        }

        public static void StartPlay()
        {
            if (_wavPaths.Count() <= 0)
            {
                WindowsManager._audioPlayerWindow.Title = $"{_title} - CONNICHUA!";
                DTW($"💿 NOTHING TO PLAY, SORRE. SELECT SOMETHING, LMAO.");
                WindowsManager._audioPlayerWindow.Timeline.Visibility = Visibility.Hidden;
                SetProgress(0);
                _paused = true;
            }
            else if (File.Exists(_wavPaths[_index]))
            {
                _player.Stop();
                _player.Source = new Uri(_wavPaths[_index]);
                _player.Play();
                _paused = false;

                WindowsManager._audioPlayerWindow.Timeline.Visibility = Visibility.Visible;

                DTW($"💿 {_names[_index]}");

                /*              string wtf = _names[_index];

                                if (wtf.Contains("-") && wtf.Length > 3)
                                    wtf = wtf.Substring(wtf.LastIndexOf("-") + 1);
                                if (wtf.Contains("‒") && wtf.Length > 3)
                                    wtf = wtf.Substring(wtf.LastIndexOf("‒") + 1);
                                if (wtf.Contains("—") && wtf.Length > 3)
                                    wtf = wtf.Substring(wtf.LastIndexOf("—") + 1);*/

                WindowsManager._audioPlayerWindow.Title = $"{_title} - 💿 {_names[_index]}";
                WindowsManager._audioPlayerWindow.PlayPauseButton.Content = "Pause";
                Logger.Log($"So player started play 💿 {_names[_index]} teso.");
            }
            else
            {
                DTW($"💿 FILE NOT EXIST ({_names[_index]})");
                Pause();
                WindowsManager._audioPlayerWindow.Timeline.Visibility = Visibility.Hidden;
                SetProgress(0);
                WindowsManager._audioPlayerWindow.Title = $"{_title} - 💿 FILE NOT EXIST ({_names[_index]})";
                Thread.Sleep(1000);
                Next();
            }
        }

        public static void TrackTheTrack()
        {
            if (_trackTheTrackThread == null)
            {
                Do();
            }
            else if (_trackTheTrackThread.ThreadState != ThreadState.Running)
            {
                try
                {
                    _trackTheTrackThread.Interrupt();
                }
                catch
                {

                }

                Do();
            }

            void Do()
            {
                _trackTheTrackThread = new Thread(TrackTread);
                _trackTheTrackThread.Start();

                void TrackTread()
                {
                    while (true)
                    {
                        try
                        {
                            Thread.Sleep(200);

                            Duration dura;

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                dura = _player.NaturalDuration;
                            });

                            double progress = 0;

                            if (WindowsManager._audioPlayerWindow != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (WindowsManager._audioPlayerWindow.WindowState != WindowState.Minimized)
                                        if (WindowsManager._audioPlayerWindow.Timeline.Visibility == Visibility.Visible)
                                            if (dura.HasTimeSpan)
                                            {
                                                double total = dura.TimeSpan.TotalMilliseconds;
                                                progress = _player.Position.TotalMilliseconds / total;

                                                try
                                                {
                                                    SetProgress(progress);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Log($"Error on TrackTheTrack: {ex} | {ex.Message}");
                                                }
                                            }
                                });
                            }
                        }
                        catch (ThreadInterruptedException ex)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public static void DTW(string text)
        {
            if (_dtw != null)
            {
                if (_dtw.IsVisible)
                    _dtw.Close();
                _dtw = null; //okay bro
            }

            _dtw = new DesktopTextWindow(text);
            _dtw.Show();
        }

        public static void PlayPause()
        {
            if (!_player.HasAudio)
                StartPlay();
            else
            {
                if (_paused)
                    Unpause();
                else
                    Pause();
            }
        }

        public static void Pause()
        {
            _player.Pause();
            _paused = true;
            WindowsManager._audioPlayerWindow.PlayPauseButton.Content = "Play";
            WindowsManager._audioPlayerWindow.Title = $"{_title} - 💿 {_names[_index]} (PAUSED)";
            Logger.Log($"Player paused teso.");
        }

        public static void Unpause()
        {
            _player.Play();
            _paused = false;
            WindowsManager._audioPlayerWindow.PlayPauseButton.Content = "Pause";
            WindowsManager._audioPlayerWindow.Timeline.Visibility = Visibility.Visible;
            WindowsManager._audioPlayerWindow.Title = $"{_title} - 💿 {_names[_index]}";
            Logger.Log($"Player unpaused teso. PlayerPosition = {_player.Position} teso.");
        }

        public static void SetProgress(double progres)
        {
            WindowsManager._audioPlayerWindow.HereItIs.ColumnDefinitions[0].Width = new GridLength(progres, GridUnitType.Star);
            WindowsManager._audioPlayerWindow.HereItIs.ColumnDefinitions[1].Width = new GridLength(1 - progres, GridUnitType.Star);
        }

        public static void Next()
        {
            if (_wavPaths.Count() > 0)
            {
                _index++;
                if (_index >= _wavPaths.Count())
                    _index = 0;
                StartPlay();
            }
        }

        public static void Prev()
        {
            if (_wavPaths.Count() > 0)
            {
                _index--;
                if (_index < 0)
                    _index = _wavPaths.Count() - 1;
                StartPlay();
            }
        }

        public static void Stop()
        {
            _player.Stop();
            _paused = true;
            WindowsManager._audioPlayerWindow.Timeline.Visibility = Visibility.Hidden;
            SetProgress(0);

            if (_names.Length > 0)
                WindowsManager._audioPlayerWindow.Title = $"{_title} - 💿 {_names[_index]} (STOPPED)";
            else
                WindowsManager._audioPlayerWindow.Title = $"{_title} - {ProgramFiles.GetHello()}";

            WindowsManager._audioPlayerWindow.PlayPauseButton.Content = "Start";
        }

        static string GetName(string path) => System.IO.Path.GetFileNameWithoutExtension(path);
    }
}