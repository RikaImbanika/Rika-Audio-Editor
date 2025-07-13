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
    public partial class PlayerWindow : Window
    {
        public PlayerWindow()
        {
            InitializeComponent();
        }

        private void PlayClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("U click to play teso.");
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("U click to open teso.");
        }
    }
}
