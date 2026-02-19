using System.Windows;
using System.Windows.Controls;
using KickBlastStudentUI.Views;

namespace KickBlastStudentUI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        OpenDashboard();
    }

    private void ShowView(UserControl view, string pageTitle)
    {
        MainContent.Content = view;
        PageTitleText.Text = pageTitle;

        if (view is IStatusAware aware)
        {
            aware.SetStatusAction(SetStatus);
        }
    }

    public void SetStatus(string text)
    {
        StatusText.Text = text;
    }

    public void RefreshDashboardIfVisible()
    {
        if (MainContent.Content is DashboardView dashboard)
        {
            dashboard.RefreshStats();
        }
    }

    public void RefreshHistoryIfVisible()
    {
        if (MainContent.Content is HistoryView history)
        {
            history.LoadHistory();
        }
    }

    private void OpenDashboard()
    {
        var dashboard = new DashboardView();
        ShowView(dashboard, "Dashboard");
    }

    private void DashboardButton_Click(object sender, RoutedEventArgs e) => OpenDashboard();

    private void AthletesButton_Click(object sender, RoutedEventArgs e)
    {
        ShowView(new AthletesView(this), "Athletes");
    }

    private void CalculatorButton_Click(object sender, RoutedEventArgs e)
    {
        ShowView(new CalculatorView(this), "Monthly Fee Calculator");
    }

    private void HistoryButton_Click(object sender, RoutedEventArgs e)
    {
        ShowView(new HistoryView(), "History");
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        ShowView(new SettingsView(this), "Settings");
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        var login = new LoginWindow();
        login.Show();
        Close();
    }
}
