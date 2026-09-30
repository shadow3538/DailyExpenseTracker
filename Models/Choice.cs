using SQLite;

namespace DailyExpenseTracker.Models;

public class Choice
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Sort { get; set; }

    public int CategoryId { get; set; }
}
