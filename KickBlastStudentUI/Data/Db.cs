using System.IO;
using KickBlastStudentUI.Helpers;
using KickBlastStudentUI.Models;
using Microsoft.Data.Sqlite;

namespace KickBlastStudentUI.Data;

public static class Db
{
    public static PricingSettings CurrentPricing { get; private set; } = new()
    {
        BeginnerWeeklyFee = 2000,
        IntermediateWeeklyFee = 3000,
        EliteWeeklyFee = 4500,
        CompetitionFee = 1500,
        CoachingHourlyRate = 1200
    };

    private static readonly string DataDir = Path.Combine(AppContext.BaseDirectory, "Data");
    private static readonly string DbPath = Path.Combine(DataDir, "kickblast_student.db");
    private static string ConnectionString => $"Data Source={DbPath}";

    public static void CreateTablesAndSeed()
    {
        Directory.CreateDirectory(DataDir);

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var sql = @"
CREATE TABLE IF NOT EXISTS Users(Id INTEGER PRIMARY KEY AUTOINCREMENT, Username TEXT UNIQUE, Password TEXT);
CREATE TABLE IF NOT EXISTS TrainingPlans(Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT UNIQUE, WeeklyFee REAL);
CREATE TABLE IF NOT EXISTS Athletes(Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Plan TEXT, CurrentWeight REAL, CategoryWeight REAL);
CREATE TABLE IF NOT EXISTS MonthlyCalculations(Id INTEGER PRIMARY KEY AUTOINCREMENT, Date TEXT, AthleteName TEXT, Plan TEXT,
Competitions INTEGER, CoachingHours REAL, TrainingCost REAL, CoachingCost REAL, CompetitionCost REAL, TotalCost REAL,
WeightMessage TEXT, SecondSaturday TEXT);";

        using var command = new SqliteCommand(sql, connection);
        command.ExecuteNonQuery();

        SeedUsers(connection);
        SeedPlans(connection);
        SeedAthletes(connection);
    }

    public static bool ValidateLogin(string username, string password)
    {
        try
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var command = new SqliteCommand("SELECT COUNT(*) FROM Users WHERE Username=@u AND Password=@p", connection);
            command.Parameters.AddWithValue("@u", username);
            command.Parameters.AddWithValue("@p", password);
            var count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }
        catch
        {
            return false;
        }
    }

    public static List<Athlete> GetAthletes(string search = "", string plan = "All")
    {
        var list = new List<Athlete>();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var query = "SELECT Id, Name, Plan, CurrentWeight, CategoryWeight FROM Athletes WHERE 1=1";
        if (!string.IsNullOrWhiteSpace(search)) query += " AND Name LIKE @search";
        if (!string.IsNullOrWhiteSpace(plan) && plan != "All") query += " AND Plan=@plan";
        query += " ORDER BY Name";

        using var command = new SqliteCommand(query, connection);
        if (!string.IsNullOrWhiteSpace(search)) command.Parameters.AddWithValue("@search", $"%{search}%");
        if (!string.IsNullOrWhiteSpace(plan) && plan != "All") command.Parameters.AddWithValue("@plan", plan);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Athlete
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Plan = reader.GetString(2),
                CurrentWeight = reader.GetDouble(3),
                CategoryWeight = reader.GetDouble(4)
            });
        }

        return list;
    }

    public static void SaveAthlete(Athlete athlete)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        SqliteCommand command;
        if (athlete.Id == 0)
        {
            command = new SqliteCommand("INSERT INTO Athletes(Name, Plan, CurrentWeight, CategoryWeight) VALUES(@n,@p,@cw,@cat)", connection);
        }
        else
        {
            command = new SqliteCommand("UPDATE Athletes SET Name=@n, Plan=@p, CurrentWeight=@cw, CategoryWeight=@cat WHERE Id=@id", connection);
            command.Parameters.AddWithValue("@id", athlete.Id);
        }

        command.Parameters.AddWithValue("@n", athlete.Name);
        command.Parameters.AddWithValue("@p", athlete.Plan);
        command.Parameters.AddWithValue("@cw", athlete.CurrentWeight);
        command.Parameters.AddWithValue("@cat", athlete.CategoryWeight);
        command.ExecuteNonQuery();
    }

    public static void DeleteAthlete(int id)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand("DELETE FROM Athletes WHERE Id=@id", connection);
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }

    public static MonthlyCalculation CalculateFee(Athlete athlete, int competitions, double coachingHours)
    {
        if (athlete.Plan == "Beginner") competitions = 0;

        var calc = new MonthlyCalculation
        {
            Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            AthleteName = athlete.Name,
            Plan = athlete.Plan,
            Competitions = competitions,
            CoachingHours = coachingHours,
            TrainingCost = GetPlanWeeklyFee(athlete.Plan) * 4,
            CoachingCost = coachingHours * 4 * CurrentPricing.CoachingHourlyRate,
            CompetitionCost = (athlete.Plan == "Intermediate" || athlete.Plan == "Elite") ? competitions * CurrentPricing.CompetitionFee : 0,
            SecondSaturday = DateHelper.GetSecondSaturday(DateTime.Now.Year, DateTime.Now.Month).ToString("yyyy-MM-dd")
        };

        calc.TotalCost = calc.TrainingCost + calc.CoachingCost + calc.CompetitionCost;
        if (athlete.CurrentWeight > athlete.CategoryWeight) calc.WeightMessage = "Over target weight";
        else if (athlete.CurrentWeight < athlete.CategoryWeight) calc.WeightMessage = "Under target weight";
        else calc.WeightMessage = "On target weight";

        return calc;
    }

    public static void SaveCalculation(MonthlyCalculation calc)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = new SqliteCommand(@"INSERT INTO MonthlyCalculations(Date, AthleteName, Plan, Competitions, CoachingHours, TrainingCost, CoachingCost, CompetitionCost, TotalCost, WeightMessage, SecondSaturday)
VALUES(@d,@a,@p,@c,@h,@t,@co,@cc,@tot,@w,@s)", connection);
        command.Parameters.AddWithValue("@d", calc.Date);
        command.Parameters.AddWithValue("@a", calc.AthleteName);
        command.Parameters.AddWithValue("@p", calc.Plan);
        command.Parameters.AddWithValue("@c", calc.Competitions);
        command.Parameters.AddWithValue("@h", calc.CoachingHours);
        command.Parameters.AddWithValue("@t", calc.TrainingCost);
        command.Parameters.AddWithValue("@co", calc.CoachingCost);
        command.Parameters.AddWithValue("@cc", calc.CompetitionCost);
        command.Parameters.AddWithValue("@tot", calc.TotalCost);
        command.Parameters.AddWithValue("@w", calc.WeightMessage);
        command.Parameters.AddWithValue("@s", calc.SecondSaturday);
        command.ExecuteNonQuery();
    }

    public static List<MonthlyCalculation> GetHistory(string athlete = "All", int month = 0, int year = 0)
    {
        var list = new List<MonthlyCalculation>();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var query = "SELECT Id, Date, AthleteName, Plan, Competitions, CoachingHours, TrainingCost, CoachingCost, CompetitionCost, TotalCost, WeightMessage, SecondSaturday FROM MonthlyCalculations WHERE 1=1";
        if (athlete != "All") query += " AND AthleteName=@a";
        if (month > 0) query += " AND strftime('%m', Date)=@m";
        if (year > 0) query += " AND strftime('%Y', Date)=@y";
        query += " ORDER BY Id DESC";

        using var command = new SqliteCommand(query, connection);
        if (athlete != "All") command.Parameters.AddWithValue("@a", athlete);
        if (month > 0) command.Parameters.AddWithValue("@m", month.ToString("D2"));
        if (year > 0) command.Parameters.AddWithValue("@y", year.ToString());

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new MonthlyCalculation
            {
                Id = reader.GetInt32(0),
                Date = reader.GetString(1),
                AthleteName = reader.GetString(2),
                Plan = reader.GetString(3),
                Competitions = reader.GetInt32(4),
                CoachingHours = reader.GetDouble(5),
                TrainingCost = reader.GetDouble(6),
                CoachingCost = reader.GetDouble(7),
                CompetitionCost = reader.GetDouble(8),
                TotalCost = reader.GetDouble(9),
                WeightMessage = reader.GetString(10),
                SecondSaturday = reader.GetString(11)
            });
        }

        return list;
    }

    public static (int athletes, int calculations, int plans) GetDashboardStats()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        var athletes = ExecuteCount(connection, "SELECT COUNT(*) FROM Athletes");
        var calculations = ExecuteCount(connection, "SELECT COUNT(*) FROM MonthlyCalculations");
        var plans = ExecuteCount(connection, "SELECT COUNT(*) FROM TrainingPlans");
        return (athletes, calculations, plans);
    }

    public static void SavePricing(PricingSettings pricing)
    {
        CurrentPricing = pricing;

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        UpdatePlan(connection, "Beginner", pricing.BeginnerWeeklyFee);
        UpdatePlan(connection, "Intermediate", pricing.IntermediateWeeklyFee);
        UpdatePlan(connection, "Elite", pricing.EliteWeeklyFee);
    }

    private static double GetPlanWeeklyFee(string plan)
    {
        return plan switch
        {
            "Beginner" => CurrentPricing.BeginnerWeeklyFee,
            "Intermediate" => CurrentPricing.IntermediateWeeklyFee,
            "Elite" => CurrentPricing.EliteWeeklyFee,
            _ => 0
        };
    }

    private static void UpdatePlan(SqliteConnection connection, string name, double fee)
    {
        using var command = new SqliteCommand("UPDATE TrainingPlans SET WeeklyFee=@f WHERE Name=@n", connection);
        command.Parameters.AddWithValue("@f", fee);
        command.Parameters.AddWithValue("@n", name);
        command.ExecuteNonQuery();
    }

    private static int ExecuteCount(SqliteConnection connection, string sql)
    {
        using var command = new SqliteCommand(sql, connection);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static void SeedUsers(SqliteConnection connection)
    {
        if (ExecuteCount(connection, "SELECT COUNT(*) FROM Users") == 0)
        {
            using var command = new SqliteCommand("INSERT INTO Users(Username, Password) VALUES('rashiii','123456')", connection);
            command.ExecuteNonQuery();
        }
    }

    private static void SeedPlans(SqliteConnection connection)
    {
        if (ExecuteCount(connection, "SELECT COUNT(*) FROM TrainingPlans") == 0)
        {
            InsertPlan(connection, "Beginner", CurrentPricing.BeginnerWeeklyFee);
            InsertPlan(connection, "Intermediate", CurrentPricing.IntermediateWeeklyFee);
            InsertPlan(connection, "Elite", CurrentPricing.EliteWeeklyFee);
        }
    }

    private static void InsertPlan(SqliteConnection connection, string name, double fee)
    {
        using var command = new SqliteCommand("INSERT INTO TrainingPlans(Name, WeeklyFee) VALUES(@n,@f)", connection);
        command.Parameters.AddWithValue("@n", name);
        command.Parameters.AddWithValue("@f", fee);
        command.ExecuteNonQuery();
    }

    private static void SeedAthletes(SqliteConnection connection)
    {
        if (ExecuteCount(connection, "SELECT COUNT(*) FROM Athletes") == 0)
        {
            AddSample(connection, "Nadeesha Silva", "Beginner", 58.2, 57.0);
            AddSample(connection, "Ravindu Perera", "Intermediate", 73.5, 73.0);
            AddSample(connection, "Mihiri Jayasuriya", "Elite", 51.0, 52.0);
            AddSample(connection, "Kasun Fernando", "Beginner", 66.4, 66.0);
            AddSample(connection, "Piumi Senanayake", "Intermediate", 60.0, 60.0);
            AddSample(connection, "Dineth Wickrama", "Elite", 81.2, 81.0);
        }
    }

    private static void AddSample(SqliteConnection connection, string name, string plan, double currentWeight, double categoryWeight)
    {
        using var command = new SqliteCommand("INSERT INTO Athletes(Name, Plan, CurrentWeight, CategoryWeight) VALUES(@n,@p,@cw,@cat)", connection);
        command.Parameters.AddWithValue("@n", name);
        command.Parameters.AddWithValue("@p", plan);
        command.Parameters.AddWithValue("@cw", currentWeight);
        command.Parameters.AddWithValue("@cat", categoryWeight);
        command.ExecuteNonQuery();
    }
}

public class PricingSettings
{
    public double BeginnerWeeklyFee { get; set; }
    public double IntermediateWeeklyFee { get; set; }
    public double EliteWeeklyFee { get; set; }
    public double CompetitionFee { get; set; }
    public double CoachingHourlyRate { get; set; }
}
