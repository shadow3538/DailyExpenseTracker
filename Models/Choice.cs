using SQLite;

namespace DailyExpenseTracker.Models;

// Category option
public class Choice
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Sort { get; set; }
    // Owner category id
    public int CategoryId { get; set; }
}
