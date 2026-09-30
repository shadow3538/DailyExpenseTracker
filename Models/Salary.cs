using SQLite;

namespace DailyExpenseTracker.Models;

// Income entry
public class Salary
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    // Source / note
    public string Note { get; set; } = "";
}
