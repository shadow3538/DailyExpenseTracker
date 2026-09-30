using SQLite;

namespace DailyExpenseTracker.Models;

public class Salary
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }

    public string Note { get; set; } = "";
}
