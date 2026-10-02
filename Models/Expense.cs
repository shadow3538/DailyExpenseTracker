using SQLite;

namespace DailyExpenseTracker.Models;

public class Expense
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public DateTime Date { get; set; }

    public string Item { get; set; } = "";

    public string Category { get; set; } = "";
    public decimal Amount { get; set; }
    public string Note { get; set; } = "";

    [Ignore]
    public string Cat => string.IsNullOrWhiteSpace(Category) ? "Extra" : Category;
}
