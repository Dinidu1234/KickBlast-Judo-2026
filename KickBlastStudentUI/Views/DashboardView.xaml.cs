using System.Windows.Controls;
using KickBlastStudentUI.Data;

namespace KickBlastStudentUI.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        RefreshStats();
    }

    public void RefreshStats()
    {
        try
        {
            var stats = Db.GetDashboardStats();
            AthletesCountText.Text = stats.athletes.ToString();
            CalculationsCountText.Text = stats.calculations.ToString();
            PlansCountText.Text = stats.plans.ToString();
            UpdatedText.Text = $"Updated: {DateTime.Now:yyyy-MM-dd HH:mm}";
        }
        catch
        {
            UpdatedText.Text = "Updated: failed to load";
        }
    }
}
