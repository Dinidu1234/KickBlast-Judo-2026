using System.Windows;
using System.Windows.Controls;
using KickBlastStudentUI.Data;
using KickBlastStudentUI.Helpers;
using KickBlastStudentUI.Models;

namespace KickBlastStudentUI.Views;

public partial class HistoryView : UserControl, IStatusAware
{
    private Action<string>? _setStatus;

    public HistoryView()
    {
        InitializeComponent();
        LoadFilters();
        LoadHistory();
    }

    public void SetStatusAction(Action<string> statusAction)
    {
        _setStatus = statusAction;
        _setStatus?.Invoke("History loaded.");
    }

    private void LoadFilters()
    {
        var athletes = Db.GetAthletes().Select(a => a.Name).Distinct().ToList();
        athletes.Insert(0, "All");
        AthleteFilterCombo.ItemsSource = athletes;
        AthleteFilterCombo.SelectedIndex = 0;

        var months = new List<string> { "All" };
        months.AddRange(Enumerable.Range(1, 12).Select(m => m.ToString()));
        MonthFilterCombo.ItemsSource = months;
        MonthFilterCombo.SelectedIndex = 0;

        var years = new List<string> { "All" };
        years.AddRange(Enumerable.Range(DateTime.Now.Year - 3, 6).Select(y => y.ToString()));
        YearFilterCombo.ItemsSource = years;
        YearFilterCombo.SelectedItem = DateTime.Now.Year.ToString();
    }

    public void LoadHistory()
    {
        var athlete = AthleteFilterCombo.SelectedItem?.ToString() ?? "All";
        var month = MonthFilterCombo.SelectedItem?.ToString() ?? "All";
        var year = YearFilterCombo.SelectedItem?.ToString() ?? "All";

        var monthValue = month == "All" ? 0 : int.Parse(month);
        var yearValue = year == "All" ? 0 : int.Parse(year);

        var list = Db.GetHistory(athlete, monthValue, yearValue);
        HistoryGrid.ItemsSource = list;
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        LoadHistory();
        _setStatus?.Invoke("History filter applied.");
    }

    private void HistoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (HistoryGrid.SelectedItem is MonthlyCalculation item)
        {
            DetailsText.Text =
                $"Training: {CurrencyHelper.ToLkr(item.TrainingCost)}\n" +
                $"Coaching: {CurrencyHelper.ToLkr(item.CoachingCost)}\n" +
                $"Competition: {CurrencyHelper.ToLkr(item.CompetitionCost)}\n" +
                $"Total: {CurrencyHelper.ToLkr(item.TotalCost)}\n" +
                $"Weight: {item.WeightMessage}\nSecond Saturday: {item.SecondSaturday}";
        }
    }
}
