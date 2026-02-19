using System.Windows;
using KickBlastStudentUI.Data;
using KickBlastStudentUI.Views;

namespace KickBlastStudentUI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            Db.CreateTablesAndSeed();
            var login = new LoginWindow();
            login.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Startup failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}
