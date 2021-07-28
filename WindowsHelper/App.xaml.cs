using System.Windows;
using Serilog;

namespace WindowsHelper
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Seq("http://localhost:5341")
                .WriteTo.Debug()
                .MinimumLevel.Debug()
                .CreateLogger();

            base.OnStartup(e);
        }
    }
}
