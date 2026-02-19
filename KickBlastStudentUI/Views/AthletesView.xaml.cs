using System.Windows;
using System.Windows.Controls;
using KickBlastStudentUI.Data;
using KickBlastStudentUI.Helpers;
using KickBlastStudentUI.Models;

namespace KickBlastStudentUI.Views;

public partial class AthletesView : UserControl, IStatusAware
{
    private readonly MainWindow _mainWindow;
    private int _selectedId;
    private Action<string>? _setStatus;

    public AthletesView(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        LoadPlans();
        LoadGrid();
    }

    public void SetStatusAction(Action<string> statusAction) => _setStatus = statusAction;

    private void LoadPlans()
    {
        var plans = new List<string> { "Beginner", "Intermediate", "Elite" };
        PlanComboBox.ItemsSource = plans;
        PlanComboBox.SelectedIndex = 0;

        var searchPlans = new List<string> { "All", "Beginner", "Intermediate", "Elite" };
        SearchPlanComboBox.ItemsSource = searchPlans;
        SearchPlanComboBox.SelectedIndex = 0;
    }

    private void LoadGrid()
    {
        AthletesGrid.ItemsSource = Db.GetAthletes(SearchTextBox.Text.Trim(), SearchPlanComboBox.SelectedItem?.ToString() ?? "All");
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        LoadGrid();
        _setStatus?.Invoke("Athlete search done.");
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!ValidationHelper.IsRequired(NameTextBox.Text) || !ValidationHelper.IsDouble(CurrentWeightTextBox.Text, out var currentWeight) || !ValidationHelper.IsDouble(CategoryWeightTextBox.Text, out var categoryWeight))
            {
                MessageBox.Show("Please enter valid athlete details.");
                return;
            }

            Db.SaveAthlete(new Athlete
            {
                Id = _selectedId,
                Name = NameTextBox.Text.Trim(),
                Plan = PlanComboBox.SelectedItem?.ToString() ?? "Beginner",
                CurrentWeight = currentWeight,
                CategoryWeight = categoryWeight
            });

            LoadGrid();
            ClearForm();
            _mainWindow.RefreshDashboardIfVisible();
            _setStatus?.Invoke("Athlete saved.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Save failed: {ex.Message}");
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedId == 0)
        {
            MessageBox.Show("Please select an athlete to delete.");
            return;
        }

        if (MessageBox.Show("Delete selected athlete?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            Db.DeleteAthlete(_selectedId);
            LoadGrid();
            ClearForm();
            _mainWindow.RefreshDashboardIfVisible();
            _setStatus?.Invoke("Athlete deleted.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Delete failed: {ex.Message}");
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private void ClearForm()
    {
        _selectedId = 0;
        NameTextBox.Text = "";
        PlanComboBox.SelectedIndex = 0;
        CurrentWeightTextBox.Text = "";
        CategoryWeightTextBox.Text = "";
        AthletesGrid.SelectedItem = null;
    }

    private void AthletesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AthletesGrid.SelectedItem is Athlete athlete)
        {
            _selectedId = athlete.Id;
            NameTextBox.Text = athlete.Name;
            PlanComboBox.SelectedItem = athlete.Plan;
            CurrentWeightTextBox.Text = athlete.CurrentWeight.ToString();
            CategoryWeightTextBox.Text = athlete.CategoryWeight.ToString();
        }
    }
}
