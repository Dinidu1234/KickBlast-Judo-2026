using System.Windows;
using KickBlastStudentUI.Data;

namespace KickBlastStudentUI.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Db.ValidateLogin(UsernameTextBox.Text.Trim(), PasswordTextBox.Password))
            {
                var main = new MainWindow();
                main.Show();
                Close();
                return;
            }

            ErrorText.Text = "Invalid username or password.";
        }
        catch (Exception ex)
        {
            ErrorText.Text = $"Login failed: {ex.Message}";
        }
    }
}
