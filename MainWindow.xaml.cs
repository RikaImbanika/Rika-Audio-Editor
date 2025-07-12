using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rika_Audio;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Logger.StartLogShower();
        Params.Init();
        WindowManager._mainWindow = this;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        WindowManager.Open(typeof(RepairWindow));
        WindowManager.Open(typeof(LogsWindow));
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        WindowManager.Open(typeof(MasteringWindow));
        WindowManager.Open(typeof(LogsWindow));
        Logger.Log("Превет, как дела, что делаешь?");
    }

    private void StretcherClick(object sender, RoutedEventArgs e)
    {
        WindowManager.Open(typeof(StretcherWindow));
        WindowManager.Open(typeof(LogsWindow));
        Logger.Log("Превет, как дела, что делаешь тесо?");
        Logger.Log("Now select 2 (TWO) tracks, please.");
        Logger.Log("**It doesn't working now.");
    }

    private void OkayBro(object sender, RoutedEventArgs e)
    {
        WindowManager.Open(typeof(LogsWindow));
        WindowManager.Open(typeof(EditorWindow));
        Logger.Log("Окей, бро, щас ми будем хуячить музыку тесо.");
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Params.LateInit();
    }
}