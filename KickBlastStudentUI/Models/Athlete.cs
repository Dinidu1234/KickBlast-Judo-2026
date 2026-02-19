namespace KickBlastStudentUI.Models;

public class Athlete
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Plan { get; set; } = "";
    public double CurrentWeight { get; set; }
    public double CategoryWeight { get; set; }
}
