using System.Windows;
using System.Windows.Controls;
using KickBlastStudentUI.Data;
using KickBlastStudentUI.Helpers;
using KickBlastStudentUI.Models;

namespace KickBlastStudentUI.Views;

public partial class CalculatorView : UserControl, IStatusAware
{
    private readonly MainWindow _mainWindow;
    private MonthlyCalculation? _lastCalculation;
    private Action<string>? _setStatus;

    public CalculatorView(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        LoadAthletes();
    }

    public void SetStatusAction(Action<string> statusAction) => _setStatus = statusAction;

    private void LoadAthletes()
    {
        var athletes = Db.GetAthletes();
        AthleteComboBox.ItemsSource = athletes;
        AthleteComboBox.DisplayMemberPath = "Name";
        if (athletes.Count > 0)
        {
            AthleteComboBox.SelectedIndex = 0;
        }
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (AthleteComboBox.SelectedItem is not Athlete athlete)
            {
                MessageBox.Show("Please select an athlete.");
                return;
            }

            if (!ValidationHelper.IsNonNegativeInt(CompetitionsTextBox.Text, out var competitions) || !ValidationHelper.IsDouble(CoachingTextBox.Text, out var coachingHours))
            {
                MessageBox.Show("Please enter valid competition/coaching values.");
                return;
            }

            if (coachingHours < 0 || coachingHours > 5)
            {
                MessageBox.Show("Coaching hours per week must be between 0 and 5.");
                return;
            }

            _lastCalculation = Db.CalculateFee(athlete, competitions, coachingHours);
            var beginnerNote = athlete.Plan == "Beginner" ? "\nNote: Beginner competitions forced to 0." : "";

            OutputText.Text =
                $"Athlete: {_lastCalculation.AthleteName}\n" +
                $"Plan: {_lastCalculation.Plan}\n" +
                $"Training Cost: {CurrencyHelper.ToLkr(_lastCalculation.TrainingCost)}\n" +
                $"Coaching Cost: {CurrencyHelper.ToLkr(_lastCalculation.CoachingCost)}\n" +
                $"Competition Cost: {CurrencyHelper.ToLkr(_lastCalculation.CompetitionCost)}\n" +
                $"TOTAL: {CurrencyHelper.ToLkr(_lastCalculation.TotalCost)}\n\n" +
                $"Weight: {_lastCalculation.WeightMessage}\n" +
                $"Second Saturday: {_lastCalculation.SecondSaturday}{beginnerNote}";

            _setStatus?.Invoke("Calculation completed.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Calculation failed: {ex.Message}");
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_lastCalculation == null)
        {
            MessageBox.Show("Please calculate first.");
            return;
        }

        try
        {
            Db.SaveCalculation(_lastCalculation);
            _mainWindow.RefreshDashboardIfVisible();
            _mainWindow.RefreshHistoryIfVisible();
            _setStatus?.Invoke("Calculation saved.");
            MessageBox.Show("Saved to history.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Save failed: {ex.Message}");
        }
    }
}
