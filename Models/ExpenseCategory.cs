using SQLite;

namespace DailyExpenseTracker.Models;

public class ExpenseCategory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "🧾";

    public string Color { get; set; } = "#00897B";
    public int Sort { get; set; }
    public bool IsFreeText { get; set; }

    public string Hint { get; set; } = "";
}
