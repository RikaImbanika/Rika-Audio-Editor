using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Interaction logic for PlayerWindow.xaml
    /// </summary>
    public partial class AudioPlayerWindow : Window
    {
        public AudioPlayerWindow()
        {
            InitializeComponent();
            AudioPlayer.Init();
        }

        private void PlayPauseClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("U click to play / pause teso.");
            AudioPlayer.PlayPause();
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("U click to open teso.");
            AudioPlayer.Open();
        }

        private void NextClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("U click to play next teso.");
            AudioPlayer.Next();
        }

        private void PreviousClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("U click to play previous teso.");
            AudioPlayer.Prev();
        }

        private void StopClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("U click to stop teso.");
            AudioPlayer.Stop();
        }

        private void ButtClick(object sender, RoutedEventArgs e)
        {
            /*          double width = WindowManager._playerWindow.ActualWidth;
                        Matrix what = WindowManager._playerWindow.LayoutTransform.Value;
                        double left = what.OffsetX;
                        width *= 4.0 / 5.0;
                        left += width * 10.0 / 145.0;
                        width *= 125.0 / 145.0;
                        left += width * 1.0 / 177.0;
                        width *= 175.0 / 177.0;*/

            double width = Butt.ActualWidth;
            double pos = Mouse.GetPosition(Butt).X;

            double percent = 0;

            if (pos < 0)
                percent = 0;
            else if (pos > width)
                percent = 1;
            else
                percent = pos / width;

            AudioPlayer.Teleport(percent);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AudioPlayer.TrackTheTrack();
            WindowsManager._audioPlayerWindow.Timeline.Visibility = Visibility.Hidden;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
