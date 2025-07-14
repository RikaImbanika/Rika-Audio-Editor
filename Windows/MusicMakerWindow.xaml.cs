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

namespace RIKA_IMBANIKA_AUDIO
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class MusicMakerWindow : Window
    {
        public MusicMakerWindow()
        {
            InitializeComponent();
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            MUSICMAKER.Open();
        }

        private void NewClick(object sender, RoutedEventArgs e)
        {
            MUSICMAKER.New();
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            MUSICMAKER.Save();
        }

        private void SaveAssClick(object sender, RoutedEventArgs e)
        {
            MUSICMAKER.SaveAss();
        }

        private void AddAudio(object sender, RoutedEventArgs e)
        {
            MUSICMAKER.AddAudio();
        }
    }
}
