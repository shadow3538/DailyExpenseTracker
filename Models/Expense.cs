using SQLite;

namespace DailyExpenseTracker.Models;

public class Expense
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    // Fast queries
    [Indexed]
    public DateTime Date { get; set; }
    // Option name
    public string Item { get; set; } = "";
    // Category name
    public string Category { get; set; } = "";
    public decimal Amount { get; set; }
    public string Note { get; set; } = "";

    // Legacy category
    [Ignore]
    public string Cat => string.IsNullOrWhiteSpace(Category) ? "Extra" : Category;
}
