using SQLite;

namespace DailyExpenseTracker.Models;

// Debt direction
public class Loan
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public bool IOwe { get; set; }
    public string Person { get; set; } = "";
    public decimal Amount { get; set; }
    // Paid amount
    public decimal Paid { get; set; }
    public bool HasDue { get; set; }
    public DateTime DueDate { get; set; }
    public string Note { get; set; } = "";
    public DateTime Created { get; set; }

    [Ignore] public decimal Remaining => Amount > Paid ? Amount - Paid : 0;
    [Ignore] public bool Settled => Paid >= Amount;
}
