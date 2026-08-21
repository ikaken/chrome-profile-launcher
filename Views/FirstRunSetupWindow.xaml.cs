using System.ComponentModel;
using System.Windows;
using ChromeProfileLauncher.ViewModels;

namespace ChromeProfileLauncher
{
    public partial class FirstRunSetupWindow : Window
    {
        private readonly FirstRunSetupViewModel _viewModel;
        private bool _completed;
        private bool _closing;

        public FirstRunSetupWindow(FirstRunSetupViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
            viewModel.Completed += OnCompleted;
            Closing += OnClosing;
        }

        private void OnCompleted(object? sender, System.EventArgs e)
        {
            _completed = true;
            if (!_closing) Close();
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (_completed) return;
            _closing = true;
            _viewModel.SaveCommand.Execute(null);
        }
    }
}
