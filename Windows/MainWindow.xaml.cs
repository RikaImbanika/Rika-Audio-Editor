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

namespace RIKA_IMBANIKA_AUDIO;

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
        WindowsManager._mainWindow = this;
        Title = $"RIKA IMBANIKA AUDIO - {ProgramFiles.GetHello()}";
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Logger.Log("U SELECTED DECLICKER TESO!");
        WindowsManager.Open(typeof(RepairWindow));
        WindowsManager.Open(typeof(LogsWindow));
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        WindowsManager.Open(typeof(MasteringWindow));
        WindowsManager.Open(typeof(LogsWindow));
        Logger.Log("U SELECTED MASTERING TESO!");
        Logger.Log("Превет, как дела, что делаешь?");
    }

    private void StretcherClick(object sender, RoutedEventArgs e)
    {
        WindowsManager.Open(typeof(StretcherWindow));
        WindowsManager.Open(typeof(LogsWindow));
        Logger.Log("U SELECTED STRETCER TESO!");
        Logger.Log("Превет, как дела, что делаешь тесо?");
        Logger.Log("Now select 2 (TWO) tracks, please.");
        Logger.Log("**It doesn't working now.");
    }

    private void OkayBro(object sender, RoutedEventArgs e)
    {
        WindowsManager.Open(typeof(LogsWindow));
        WindowsManager.Open(typeof(MusicMakerWindow));
        Logger.Log("U SELECTED MUSIC MAKER TESO!");
        Logger.Log("Окей, заюш, щас ми будем хуйнячить мьюзику тесо.");
        MUSICMAKER.Init();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Params.LateInit();
    }

    private void PlayerClick(object sender, RoutedEventArgs e)
    {
        bool can = true;

        if (WindowsManager._audioPlayerWindow == null || PresentationSource.FromVisual(WindowsManager._audioPlayerWindow) == null)
        {
            MessageBoxResult mbr = MessageBox.Show("Are u sure?\r\nSometimes it is better to create music,\r\nnot to listen it.\r\n", "Hello", MessageBoxButton.YesNo);
            can = mbr == MessageBoxResult.Yes;
        }

        if (can)
        {
            WindowsManager.Open(typeof(LogsWindow));
            WindowsManager.Open(typeof(AudioPlayerWindow));
            AudioPlayer.Init();
            Logger.Log("U SELECTED SPECTROGRAM PLAYER TESO!");
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        var gg = MessageBox.Show("Why did U close me???\r\nSo now I will be closed teso.", "RIKA IMBANIKA AUDIO - CHAO", MessageBoxButton.OK);

        Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        Application.Current.Shutdown();
    }
}