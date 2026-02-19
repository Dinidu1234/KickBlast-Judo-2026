using System.Windows;
using System.Windows.Controls;
using KickBlastStudentUI.Data;
using KickBlastStudentUI.Helpers;

namespace KickBlastStudentUI.Views;

public partial class SettingsView : UserControl, IStatusAware
{
    private readonly MainWindow _mainWindow;
    private Action<string>? _setStatus;

    public SettingsView(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        LoadPricing();
    }

    public void SetStatusAction(Action<string> statusAction) => _setStatus = statusAction;

    private void LoadPricing()
    {
        var pricing = Db.CurrentPricing;
        BeginnerTextBox.Text = pricing.BeginnerWeeklyFee.ToString();
        IntermediateTextBox.Text = pricing.IntermediateWeeklyFee.ToString();
        EliteTextBox.Text = pricing.EliteWeeklyFee.ToString();
        CompetitionTextBox.Text = pricing.CompetitionFee.ToString();
        CoachingTextBox.Text = pricing.CoachingHourlyRate.ToString();
    }

    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!ValidationHelper.IsDouble(BeginnerTextBox.Text, out var beginner) ||
                !ValidationHelper.IsDouble(IntermediateTextBox.Text, out var intermediate) ||
                !ValidationHelper.IsDouble(EliteTextBox.Text, out var elite) ||
                !ValidationHelper.IsDouble(CompetitionTextBox.Text, out var competition) ||
                !ValidationHelper.IsDouble(CoachingTextBox.Text, out var coaching))
            {
                MessageBox.Show("Please enter valid numeric settings.");
                return;
            }

            Db.SavePricing(new PricingSettings
            {
                BeginnerWeeklyFee = beginner,
                IntermediateWeeklyFee = intermediate,
                EliteWeeklyFee = elite,
                CompetitionFee = competition,
                CoachingHourlyRate = coaching
            });

            _mainWindow.RefreshDashboardIfVisible();
            _setStatus?.Invoke("Settings saved successfully.");
            MessageBox.Show("Pricing saved.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Save settings failed: {ex.Message}");
        }
    }
}
