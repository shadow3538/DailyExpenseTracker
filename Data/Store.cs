using SQLite;
using DailyExpenseTracker.Models;

namespace DailyExpenseTracker.Data;

public static class Store
{
    static SQLiteAsyncConnection? _db;
    static Task? _init;
    static int _notifyQueued;

    public static event Action? Changed;

    public static volatile bool Quiet;

    public static void ResetListeners()
    {
        Changed = null;
        Interlocked.Exchange(ref _notifyQueued, 0);
    }
    static void Notify()
    {
        if (Quiet) return;

        if (Interlocked.Exchange(ref _notifyQueued, 1) != 0) return;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                if (!Quiet) Changed?.Invoke();
            }
            finally
            {
                Interlocked.Exchange(ref _notifyQueued, 0);
            }
        });
    }
    public static void RaiseChanged() => Notify();

    static readonly string[] DefaultChoices =
    {
        "সকালের নাস্তা", "দুপুরের খাবার", "নাস্তা", "রাতের খাবার", "Extra"
    };

    // Data migration
    public static async Task InitializeAsync() => await Db();

    static async Task<SQLiteAsyncConnection> Db()
    {
        _init ??= Setup();
        await _init;
        return _db!;
    }

    static async Task Setup()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "hisab_simple.db3");
        var db = new SQLiteAsyncConnection(path);

        await db.ExecuteScalarAsync<string>("PRAGMA journal_mode=WAL;");
        await db.ExecuteAsync("PRAGMA synchronous=NORMAL;");
        await db.CreateTableAsync<Expense>();
        await db.CreateTableAsync<Choice>();
        await db.CreateTableAsync<ExpenseCategory>();
        await db.CreateTableAsync<Loan>();
        await db.CreateTableAsync<Salary>();

        await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Expense_Category_Date ON Expense(Category, Date);");
        await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Choice_CategoryId_Sort ON Choice(CategoryId, Sort);");
        await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Loan_DueDate ON Loan(DueDate);");
        _db = db;

        if (!Preferences.Default.Get("imported_v3", false))
        {
            try { await ImportOld(db); }
            catch (Exception ex) { AppLog.Error("ImportOld", ex); }
            Preferences.Default.Set("imported_v3", true);
        }

        if (!Preferences.Default.Get("categories_v53", false))
        {
            try { await MigrateCategories(db); }
            catch (Exception ex) { AppLog.Error("MigrateCategories", ex); }
            Preferences.Default.Set("categories_v53", true);
        }
        else if (await db.Table<ExpenseCategory>().CountAsync() == 0)
        {
            await SeedCategories(db);
        }

        await RefreshStyles(db);
    }

    static readonly string[] TransportWords = { "যাতায়াত", "ভাড়া", "বাস", "গাড়ি", "সিএনজি", "রিক্সা", "রিকশা", "ট্রেন", "মেট্রো", "লঞ্চ", "উবার", "পাঠাও" };

    static async Task SeedCategories(SQLiteAsyncConnection db)
    {
        var food = new ExpenseCategory { Name = "খাওয়া", Icon = "🍽️", Color = "#F59E0B", Sort = 1, IsFreeText = false };
        var trans = new ExpenseCategory { Name = "যাতায়াত", Icon = "🚌", Color = "#3B82F6", Sort = 2, IsFreeText = false };
        var shop = new ExpenseCategory { Name = "কেনাকাটা", Icon = "🛍️", Color = "#8B5CF6", Sort = 3, IsFreeText = true, Hint = "কী কিনলেন? (যেমন: জুতা, বই)" };
        var extra = new ExpenseCategory { Name = "Extra", Icon = "✨", Color = "#14B8A6", Sort = 4, IsFreeText = true, Hint = "কী বাবদ খরচ? (যেমন: ঔষধ)" };
        await db.InsertAsync(food);
        await db.InsertAsync(trans);
        await db.InsertAsync(shop);
        await db.InsertAsync(extra);

        int i = 1;
        foreach (var n in new[] { "বাস", "সিএনজি", "রিক্সা", "ট্রেন" })
            await db.InsertAsync(new Choice { Name = n, Sort = i++, CategoryId = trans.Id });
    }

    static async Task MigrateCategories(SQLiteAsyncConnection db)
    {
        if (await db.Table<ExpenseCategory>().CountAsync() == 0) await SeedCategories(db);
        var cats = await db.Table<ExpenseCategory>().ToListAsync();
        var food = cats.Where(c => !c.IsFreeText).OrderBy(c => c.Sort).First();
        var trans = cats.FirstOrDefault(c => c.Name == "যাতায়াত") ?? food;

        var choices = await db.Table<Choice>().ToListAsync();
        var foodChoices = choices.Where(c => c.CategoryId == food.Id).ToList();

        if (choices.All(c => c.CategoryId != 0) && foodChoices.Count == 0)
        {
            int i = 1;
            foreach (var n in new[] { "সকালের নাস্তা", "দুপুরের খাবার", "নাস্তা", "রাতের খাবার" })
                await db.InsertAsync(new Choice { Name = n, Sort = i++, CategoryId = food.Id });
        }

        foreach (var c in choices.Where(c => c.CategoryId == 0))
        {
            if (c.Name.Equals("Extra", StringComparison.OrdinalIgnoreCase))
            {
                await db.DeleteAsync(c);
                continue;
            }
            c.CategoryId = TransportWords.Any(w => c.Name.Contains(w)) ? trans.Id : food.Id;
            await db.UpdateAsync(c);
        }

        choices = await db.Table<Choice>().ToListAsync();
        foreach (var c in choices)
        {
            var cat = cats.FirstOrDefault(x => x.Id == c.CategoryId);
            if (cat == null) continue;
            await db.ExecuteAsync(
                "UPDATE Expense SET Category = ? WHERE Item = ? AND (Category IS NULL OR Category = '')",
                cat.Name, c.Name);
        }

        await db.ExecuteAsync("UPDATE Expense SET Category = 'Extra' WHERE Category IS NULL OR Category = ''");
    }

    static async Task RefreshStyles(SQLiteAsyncConnection db)
    {
        try { Ui.SetCategoryStyles(await db.Table<ExpenseCategory>().ToListAsync()); }
        catch (Exception ex) { AppLog.Error("RefreshStyles", ex); }
    }

    public static async Task<List<Expense>> GetRangeAsync(DateTime start, DateTime endExclusive)
    {
        var db = await Db();
        var list = await db.Table<Expense>().Where(e => e.Date >= start && e.Date < endExclusive).ToListAsync();
        return list.OrderByDescending(e => e.Date).ThenByDescending(e => e.Id).ToList();
    }

    public static async Task<(int Count, decimal Total, DateTime? First)> GetStatsAsync()
    {
        var db = await Db();
        int n = await db.Table<Expense>().CountAsync();
        if (n == 0) return (0, 0, null);
        var total = await db.ExecuteScalarAsync<double>("select ifnull(sum(Amount),0) from Expense");
        var first = await db.Table<Expense>().OrderBy(e => e.Date).FirstOrDefaultAsync();
        return (n, (decimal)total, first?.Date);
    }

    public static async Task AddAsync(Expense e)
    {
        var db = await Db();
        await db.InsertAsync(e);
        Notify();
    }

    public static async Task UpdateAsync(Expense e)
    {
        var db = await Db();
        await db.UpdateAsync(e);
        Notify();
    }

    public static async Task DeleteAsync(Expense e)
    {
        var db = await Db();
        await db.DeleteAsync(e);
        Notify();
    }

    static readonly string[] CatPalette =
    {
        "#F59E0B", "#3B82F6", "#8B5CF6", "#14B8A6", "#EC4899", "#EF4444", "#84CC16", "#0EA5E9"
    };

    static readonly string[] CatIcons = { "🏠", "📱", "🎁", "🎓", "💡", "🏥", "🎮", "👕", "🧾" };

    public static async Task<List<ExpenseCategory>> GetCategoriesAsync()
    {
        var db = await Db();
        var list = await db.Table<ExpenseCategory>().ToListAsync();
        return list.OrderBy(c => c.Sort).ThenBy(c => c.Id).ToList();
    }

    public static async Task<bool> AddCategoryAsync(string name, bool freeText)
    {
        name = name.Trim();
        if (name.Length == 0) return false;
        var list = await GetCategoriesAsync();
        if (list.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) return false;
        var db = await Db();
        int n = list.Count;
        await db.InsertAsync(new ExpenseCategory
        {
            Name = name,
            Icon = CatIcons[n % CatIcons.Length],
            Color = CatPalette[n % CatPalette.Length],
            Sort = (list.Count == 0 ? 0 : list.Max(c => c.Sort)) + 1,
            IsFreeText = freeText,
            Hint = freeText ? L.T("কী বাবদ খরচ? (লিখুন)", "What is it for? (type)") : ""
        });
        await RefreshStyles(db);
        Notify();
        return true;
    }

    public static async Task<bool> RenameCategoryAsync(ExpenseCategory c, string newName)
    {
        newName = newName.Trim();
        if (newName.Length == 0) return false;
        var list = await GetCategoriesAsync();
        if (list.Any(x => x.Id != c.Id && x.Name.Equals(newName, StringComparison.OrdinalIgnoreCase))) return false;
        var db = await Db();
        var old = c.Name;
        c.Name = newName;
        await db.UpdateAsync(c);

        await db.ExecuteAsync("UPDATE Expense SET Item = ?, Category = ? WHERE Category = ? AND Item = ?", newName, newName, old, old);
        await db.ExecuteAsync("UPDATE Expense SET Category = ? WHERE Category = ?", newName, old);
        await RefreshStyles(db);
        Notify();
        return true;
    }

    public static async Task DeleteCategoryAsync(ExpenseCategory c)
    {
        var db = await Db();
        await db.ExecuteAsync("DELETE FROM Choice WHERE CategoryId = ?", c.Id);
        await db.DeleteAsync(c);
        await RefreshStyles(db);
        Notify();
    }

    public static async Task<List<Choice>> GetChoicesAsync()
    {
        var db = await Db();
        var list = await db.Table<Choice>().ToListAsync();
        return list.OrderBy(c => c.Sort).ThenBy(c => c.Id).ToList();
    }

    public static async Task<bool> AddChoiceAsync(string name, int categoryId)
    {
        name = name.Trim();
        if (name.Length == 0) return false;
        var list = (await GetChoicesAsync()).Where(c => c.CategoryId == categoryId).ToList();
        if (list.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) return false;
        var db = await Db();
        int max = list.Count == 0 ? 0 : list.Max(c => c.Sort);
        await db.InsertAsync(new Choice { Name = name, Sort = max + 1, CategoryId = categoryId });
        Notify();
        return true;
    }

    public static async Task<bool> RenameChoiceAsync(Choice c, string newName)
    {
        newName = newName.Trim();
        if (newName.Length == 0) return false;
        var list = (await GetChoicesAsync()).Where(x => x.CategoryId == c.CategoryId).ToList();
        if (list.Any(x => x.Id != c.Id && x.Name.Equals(newName, StringComparison.OrdinalIgnoreCase))) return false;
        var db = await Db();
        var cat = (await db.Table<ExpenseCategory>().ToListAsync()).FirstOrDefault(x => x.Id == c.CategoryId);
        var old = c.Name;
        c.Name = newName;
        await db.UpdateAsync(c);
        if (cat != null)
            await db.ExecuteAsync("UPDATE Expense SET Item = ? WHERE Item = ? AND Category = ?", newName, old, cat.Name);
        Notify();
        return true;
    }

    public static async Task DeleteChoiceAsync(Choice c)
    {
        var db = await Db();
        await db.DeleteAsync(c);
        Notify();
    }

    static readonly (string Bn, string En)[] DefaultCats =
    {
        ("খাওয়া", "Food"), ("যাতায়াত", "Transport"), ("কেনাকাটা", "Shopping")
    };

    static readonly (string Bn, string En)[] DefaultChoiceNames =
    {
        ("সকালের নাস্তা", "Breakfast"), ("দুপুরের খাবার", "Lunch"), ("নাস্তা", "Snacks"), ("রাতের খাবার", "Dinner"),
        ("বাস", "Bus"), ("সিএনজি", "CNG"), ("রিক্সা", "Rickshaw"), ("ট্রেন", "Train")
    };

    static readonly (string Bn, string En)[] DefaultHints =
    {
        ("কী কিনলেন? (যেমন: জুতা, বই)", "What did you buy? (e.g. shoes, book)"),
        ("কী বাবদ খরচ? (যেমন: ঔষধ)", "What is it for? (e.g. medicine)"),
        ("কী বাবদ খরচ? (লিখুন)", "What is it for? (type)")
    };

    public static async Task LocalizeDefaultsAsync()
    {
        try
        {
            bool en = L.IsEn;
            var db = await Db();

            foreach (var c in await GetCategoriesAsync())
            {
                foreach (var (bn, e) in DefaultCats)
                {
                    var from = en ? bn : e;
                    var to = en ? e : bn;
                    if (c.Name == from) { await RenameCategoryAsync(c, to); break; }
                }
            }

            foreach (var c in await GetChoicesAsync())
            {
                foreach (var (bn, e) in DefaultChoiceNames)
                {
                    var from = en ? bn : e;
                    var to = en ? e : bn;
                    if (c.Name == from) { await RenameChoiceAsync(c, to); break; }
                }
            }

            foreach (var c in await GetCategoriesAsync())
            {
                foreach (var (bn, e) in DefaultHints)
                {
                    var from = en ? bn : e;
                    var to = en ? e : bn;
                    if (c.Hint == from)
                    {
                        c.Hint = to;
                        await db.UpdateAsync(c);
                        break;
                    }
                }
            }
            await RefreshStyles(db);
            Notify();
        }
        catch { }
    }

    public static async Task<List<Loan>> GetLoansAsync(bool iOwe)
    {
        var db = await Db();
        var list = await db.Table<Loan>().Where(x => x.IOwe == iOwe).ToListAsync();

        return list
            .OrderBy(x => x.Settled)
            .ThenBy(x => x.HasDue ? 0 : 1)
            .ThenBy(x => x.HasDue ? x.DueDate : DateTime.MaxValue)
            .ThenByDescending(x => x.Id)
            .ToList();
    }

    public static async Task<List<Loan>> GetAllLoansAsync()
    {
        var db = await Db();
        return await db.Table<Loan>().ToListAsync();
    }

    public static async Task<(decimal Total, int Count)> LoanTotalAsync(bool iOwe)
    {
        var list = await GetLoansAsync(iOwe);
        var open = list.Where(x => !x.Settled).ToList();
        return (open.Sum(x => x.Remaining), open.Count);
    }

    public static async Task SaveLoanAsync(Loan l)
    {
        var db = await Db();
        if (l.Id == 0) await db.InsertAsync(l); else await db.UpdateAsync(l);
        Notify();
        _ = Reminders.RescheduleAsync();
    }

    public static async Task DeleteLoanAsync(Loan l)
    {
        var db = await Db();
        await db.DeleteAsync(l);
        Notify();
        _ = Reminders.RescheduleAsync();
    }

    public static async Task<List<Salary>> GetSalariesAsync(DateTime start, DateTime endExclusive)
    {
        var db = await Db();
        var list = await db.Table<Salary>().Where(x => x.Date >= start && x.Date < endExclusive).ToListAsync();
        return list.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id).ToList();
    }

    public static async Task<List<Salary>> GetAllSalariesAsync()
    {
        var db = await Db();
        return await db.Table<Salary>().ToListAsync();
    }

    public static async Task SaveSalaryAsync(Salary s)
    {
        var db = await Db();
        if (s.Id == 0) await db.InsertAsync(s); else await db.UpdateAsync(s);
        Notify();
    }

    public static async Task DeleteSalaryAsync(Salary s)
    {
        var db = await Db();
        await db.DeleteAsync(s);
        Notify();
    }

    public static async Task<List<Expense>> GetAllExpensesAsync()
    {
        var db = await Db();
        var list = await db.Table<Expense>().ToListAsync();
        return list.OrderBy(e => e.Date).ThenBy(e => e.Id).ToList();
    }

    public static async Task ReplaceAllAsync(
        List<Expense> expenses,
        List<(int OldId, ExpenseCategory Cat)> cats,
        List<(int OldCatId, Choice Choice)> choices,
        List<Loan>? loans = null,
        List<Salary>? salaries = null)
    {
        var db = await Db();
        await db.RunInTransactionAsync(conn =>
        {
            conn.DeleteAll<Expense>();
            conn.DeleteAll<Choice>();
            conn.DeleteAll<ExpenseCategory>();

            var map = new Dictionary<int, int>();
            foreach (var (oldId, cat) in cats)
            {
                cat.Id = 0;
                conn.Insert(cat);
                map[oldId] = cat.Id;
            }
            foreach (var (oldCatId, ch) in choices)
            {
                if (!map.TryGetValue(oldCatId, out var nid)) continue;
                ch.Id = 0;
                ch.CategoryId = nid;
                conn.Insert(ch);
            }
            foreach (var e in expenses) e.Id = 0;
            conn.InsertAll(expenses);

            if (loans != null)
            {
                conn.DeleteAll<Loan>();
                foreach (var l in loans) l.Id = 0;
                conn.InsertAll(loans);
            }
            if (salaries != null)
            {
                conn.DeleteAll<Salary>();
                foreach (var s in salaries) s.Id = 0;
                conn.InsertAll(salaries);
            }
        });
        await RefreshStyles(db);
        Notify();
        _ = Reminders.RescheduleAsync();
    }

    public class OldRow
    {
        public DateTime DateTime { get; set; }
        public string? Type { get; set; }
        public string? SubType { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }

    public class OldBudget
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Limit { get; set; }
    }

    static async Task ImportOld(SQLiteAsyncConnection db)
    {
        var oldPath = Path.Combine(FileSystem.AppDataDirectory, "daily_expense.db3");
        if (!File.Exists(oldPath)) return;
        if (await db.Table<Expense>().CountAsync() > 0) return;

        var old = new SQLiteAsyncConnection(oldPath);
        try
        {
            var rows = await old.QueryAsync<OldRow>(
                "SELECT DateTime, Type, SubType, Amount, Note FROM Expense WHERE IsIncome = 0");
            var list = new List<Expense>();
            foreach (var r in rows)
            {
                var item = MapItem(r.Type ?? "", r.SubType ?? "", out var extra);
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(extra)) parts.Add(extra);
                if (!string.IsNullOrWhiteSpace(r.Note)) parts.Add(r.Note!.Trim());
                list.Add(new Expense
                {
                    Date = r.DateTime,
                    Item = item,
                    Amount = r.Amount,
                    Note = string.Join(" · ", parts)
                });
            }
            if (list.Count > 0) await db.InsertAllAsync(list);

            if (AppSettings.MonthlyLimit <= 0)
            {
                try
                {
                    var budgets = await old.QueryAsync<OldBudget>("SELECT Year, Month, \"Limit\" AS \"Limit\" FROM MonthlyBudget");
                    var now = DateTime.Today;
                    var pick = budgets.FirstOrDefault(b => b.Year == now.Year && b.Month == now.Month)
                               ?? budgets.OrderByDescending(b => b.Year * 12 + b.Month).FirstOrDefault();
                    if (pick != null && pick.Limit > 0) AppSettings.MonthlyLimit = pick.Limit;
                }
                catch { }
            }
        }
        finally
        {
            try { await old.CloseAsync(); } catch { }
        }
    }

    static string MapItem(string type, string sub, out string extra)
    {
        extra = "";
        type = type.Trim();
        sub = sub.Trim();
        if (DefaultChoices.Contains(type)) return type;
        if (type == "খাবার")
        {
            return sub switch
            {
                "সকাল" => "সকালের নাস্তা",
                "রাত" => "রাতের খাবার",
                _ => "দুপুরের খাবার"
            };
        }
        if (type == "নাস্তা") return "নাস্তা";
        extra = sub.Length == 0 ? type : type + " · " + sub;
        return "Extra";
    }
}
