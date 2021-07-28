using System.Windows;
using System.Windows.Input;
using WPFTaskbarNotifier;

namespace WindowsHelper
{
    public partial class MainWindow : TaskbarNotifier
    {
        public MainWindow() => InitializeComponent();

        private void OpenMenuItem_OnClick(object sender, RoutedEventArgs e) => Notify();

        private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e) => Close();

        private void ConfigureMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
