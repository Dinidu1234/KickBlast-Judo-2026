namespace KickBlastStudentUI.Helpers;

public static class CurrencyHelper
{
    public static string ToLkr(double amount)
    {
        return $"LKR {amount:N2}";
    }
}
