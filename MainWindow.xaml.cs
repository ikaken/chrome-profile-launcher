using System.Windows;

namespace ChromeProfileLauncher;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel vm)
        {
            vm.SaveWindowSettings();
        }
    }
}
