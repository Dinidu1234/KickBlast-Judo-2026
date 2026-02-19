namespace KickBlastStudentUI.Helpers;

public static class ValidationHelper
{
    public static bool IsNonNegativeInt(string text, out int value)
    {
        return int.TryParse(text, out value) && value >= 0;
    }

    public static bool IsDouble(string text, out double value)
    {
        return double.TryParse(text, out value);
    }

    public static bool IsRequired(string text)
    {
        return !string.IsNullOrWhiteSpace(text);
    }
}
