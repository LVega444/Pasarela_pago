using System.Windows;

namespace PasarelaPagoWPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string monto = "0.00";
            if (e.Args.Length > 0)
            {
                monto = e.Args[0];
            }

            MainWindow mainWindow = new MainWindow(monto);
            mainWindow.Show();
        }
    }
}