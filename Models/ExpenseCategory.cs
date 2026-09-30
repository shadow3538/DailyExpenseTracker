using SQLite;

namespace DailyExpenseTracker.Models;

// Expense category
public class ExpenseCategory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "🧾";
    // Hex color
    public string Color { get; set; } = "#00897B";
    public int Sort { get; set; }
    public bool IsFreeText { get; set; }
    // Placeholder for free-text
    public string Hint { get; set; } = "";
}
