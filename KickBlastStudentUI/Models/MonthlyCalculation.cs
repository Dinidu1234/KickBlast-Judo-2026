namespace KickBlastStudentUI.Models;

public class MonthlyCalculation
{
    public int Id { get; set; }
    public string Date { get; set; } = "";
    public string AthleteName { get; set; } = "";
    public string Plan { get; set; } = "";
    public int Competitions { get; set; }
    public double CoachingHours { get; set; }
    public double TrainingCost { get; set; }
    public double CoachingCost { get; set; }
    public double CompetitionCost { get; set; }
    public double TotalCost { get; set; }
    public string WeightMessage { get; set; } = "";
    public string SecondSaturday { get; set; } = "";
}
