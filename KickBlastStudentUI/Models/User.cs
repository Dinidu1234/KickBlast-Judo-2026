namespace KickBlastStudentUI.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordPlain { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}
