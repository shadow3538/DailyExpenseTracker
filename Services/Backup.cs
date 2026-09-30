using System.Text.Json.Nodes;
using DailyExpenseTracker.Data;
using DailyExpenseTracker.Models;

namespace DailyExpenseTracker;

// Backup / restore
public static class Backup
{
    const int FormatVersion = 1;
    const string AppId = "DailyExpenseTracker";
    static readonly string DateFmt = "yyyy-MM-dd'T'HH:mm:ss";

    // prefs

    public const string Ask = "ask", Folder = "folder", OneFile = "file";
    const string FixedName = "PocketNama-Backup.json";

    static string P(string k) => Preferences.Default.Get(k, "");
    static void P(string k, string v) => Preferences.Default.Set(k, v);

    // ask / folder
    public static string Mode { get => Preferences.Default.Get("bk_mode", Ask); set => P("bk_mode", value); }
    public static string TargetUri { get => P("bk_uri"); set => P("bk_uri", value); }
    public static string TargetLabel { get => P("bk_label"); set => P("bk_label", value); }
    // off / daily
    public static string Freq { get => Preferences.Default.Get("bk_freq", "off"); set => P("bk_freq", value); }
    public static string LastWhere { get => P("bk_where"); private set => P("bk_where", value); }
    public static string LastError { get => P("bk_error"); private set => P("bk_error", value); }
    public static bool LastVerified { get => Preferences.Default.Get("bk_verified", false); private set => Preferences.Default.Set("bk_verified", value); }

    public static bool HasLinked => Mode != Ask && TargetUri.Length > 0;

    public static DateTime? LastBackup
    {
        get
        {
            var t = Preferences.Default.Get("last_backup_ticks", 0L);
            return t > 0 ? new DateTime(t) : null;
        }
        private set => Preferences.Default.Set("last_backup_ticks", value?.Ticks ?? 0L);
    }

    static int LastBackupCount
    {
        get => Preferences.Default.Get("last_backup_count", 0);
        set => Preferences.Default.Set("last_backup_count", value);
    }

    public static TimeSpan? Interval => Freq switch
    {
        "daily" => TimeSpan.FromDays(1),
        "weekly" => TimeSpan.FromDays(7),
        "monthly" => TimeSpan.FromDays(30),
        _ => null
    };

    public static bool IsDue
    {
        get
        {
            if (Interval is not TimeSpan i) return false;
            var l = LastBackup;
            return l == null || DateTime.Now - l.Value >= i;
        }
    }

    public static DateTime? NextDue => Interval is TimeSpan i ? (LastBackup ?? DateTime.Now) + i : null;

    // Entries added since
    public static async Task<int> NewSinceLastAsync()
    {
        var st = await Store.GetStatsAsync();
        return Math.Max(0, st.Count - LastBackupCount);
    }

    public static string FreqName(string f) => f switch
    {
        "daily" => L.T("প্রতিদিন", "Daily"),
        "weekly" => L.T("প্রতি সপ্তাহে", "Weekly"),
        "monthly" => L.T("প্রতি মাসে", "Monthly"),
        _ => L.T("যখন চাইব", "Manual")
    };

    // One-line place summary
    public static string TargetSummary()
    {
        if (!HasLinked) return L.T("প্রতিবার জিজ্ঞেস করবে", "Ask every time");
        var icon = Mode == Folder ? "📁 " : "☁️ ";
        return icon + (TargetLabel.Length > 0 ? TargetLabel : L.T("লিংক করা আছে", "Linked"));
    }

    // build json

    static async Task<(string Json, int Count)> BuildJsonAsync()
    {
        var expenses = await Store.GetAllExpensesAsync();
        var cats = await Store.GetCategoriesAsync();
        var choices = await Store.GetChoicesAsync();

        var root = new JsonObject
        {
            ["app"] = AppId,
            ["version"] = FormatVersion,
            ["exportedAt"] = DateTime.Now.ToString(DateFmt, Fmt.Inv),
            ["settings"] = new JsonObject
            {
                ["dailyLimit"] = (double)AppSettings.DailyLimit,
                ["monthlyLimit"] = (double)AppSettings.MonthlyLimit,
                ["rangeOn"] = Period.IsCustom,
                ["rangeStart"] = Period.CustomStart.ToString("yyyy-MM-dd", Fmt.Inv),
                ["rangeEnd"] = Period.CustomEnd.ToString("yyyy-MM-dd", Fmt.Inv),
                ["rangeLimit"] = (double)Period.RangeLimit
            },
            ["profile"] = BuildProfile()
        };

        var catArr = new JsonArray();
        foreach (var c in cats)
            catArr.Add(new JsonObject
            {
                ["id"] = c.Id,
                ["name"] = c.Name,
                ["icon"] = c.Icon,
                ["color"] = c.Color,
                ["sort"] = c.Sort,
                ["freeText"] = c.IsFreeText,
                ["hint"] = c.Hint
            });
        root["categories"] = catArr;

        var chArr = new JsonArray();
        foreach (var c in choices)
            chArr.Add(new JsonObject
            {
                ["catId"] = c.CategoryId,
                ["name"] = c.Name,
                ["sort"] = c.Sort
            });
        root["choices"] = chArr;

        var exArr = new JsonArray();
        foreach (var e in expenses)
            exArr.Add(new JsonObject
            {
                ["date"] = e.Date.ToString(DateFmt, Fmt.Inv),
                ["item"] = e.Item,
                ["category"] = e.Cat,
                ["amount"] = (double)e.Amount,
                ["note"] = e.Note ?? ""
            });
        root["expenses"] = exArr;

        var loanArr = new JsonArray();
        foreach (var l in await Store.GetAllLoansAsync())
            loanArr.Add(new JsonObject
            {
                ["iOwe"] = l.IOwe,
                ["person"] = l.Person,
                ["amount"] = (double)l.Amount,
                ["paid"] = (double)l.Paid,
                ["hasDue"] = l.HasDue,
                ["due"] = l.DueDate.ToString(DateFmt, Fmt.Inv),
                ["note"] = l.Note ?? "",
                ["created"] = l.Created.ToString(DateFmt, Fmt.Inv)
            });
        root["loans"] = loanArr;

        var salArr = new JsonArray();
        foreach (var s in await Store.GetAllSalariesAsync())
            salArr.Add(new JsonObject
            {
                ["date"] = s.Date.ToString(DateFmt, Fmt.Inv),
                ["amount"] = (double)s.Amount,
                ["note"] = s.Note ?? ""
            });
        root["salaries"] = salArr;

        return (root.ToJsonString(), expenses.Count);
    }

    // run backup

    static bool _busy;

    // Back up to
    public static async Task<(bool Ok, string Message)> RunAsync(bool interactive)
    {
        if (_busy) return (false, L.T("ব্যাকআপ চলছে...", "Backup in progress..."));
        _busy = true;
        try
        {
            if (!HasLinked && !interactive)
                return (false, L.T("ব্যাকআপের জায়গা বাছা নেই", "No backup place chosen"));

            var (json, count) = await BuildJsonAsync();
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            (bool Ok, bool Verified) res;
            string where;

            if (HasLinked && Mode == Folder)
            {
                res = await Saf.WriteInFolderAsync(TargetUri, FixedName, bytes);
                where = TargetLabel;
            }
            else if (HasLinked)
            {
                res = await Saf.WriteFileAsync(TargetUri, bytes);
                where = TargetLabel;
            }
            else
            {
                var name = "pocketnama-backup-" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm", Fmt.Inv) + ".json";
                var uri = await Saf.PickNewFileAsync(name);
                if (uri == null) return (false, L.T("বাতিল করা হয়েছে", "Cancelled"));
                res = await Saf.WriteFileAsync(uri, bytes);
                var prov = Saf.ProviderOf(uri);
                var fn = Saf.LabelOf(uri, false);
                where = (prov.Length > 0 ? prov : "") + (fn.Length > 0 ? " · " + fn : "");
            }

            if (!res.Ok)
            {
                LastError = L.T("ফাইলে লেখা যায়নি", "Could not write the file");
                return (false, LastError);
            }

            LastBackup = DateTime.Now;
            LastBackupCount = count;
            LastWhere = where.Trim(' ', '·');
            LastVerified = res.Verified;
            LastError = "";
            var msg = L.T("✔ ব্যাকআপ সেভ হয়েছে", "✔ Backup saved") + (LastWhere.Length > 0 ? " — " + LastWhere : "");
            return (true, msg);
        }
        catch (Exception ex)
        {
            AppLog.Error("Backup.Run", ex);
            LastError = ex.Message;
            return (false, L.T("ব্যাকআপ হয়নি: ", "Backup failed: ") + ex.Message);
        }
        finally { _busy = false; }
    }

    // App start: run
    public static async Task<(bool Ran, bool Ok, string Message)> AutoIfDueAsync()
    {
        if (!IsDue || !HasLinked) return (false, true, "");
        var r = await RunAsync(false);
        return (true, r.Ok, r.Message);
    }

    // Share a copy
    public static async Task ShareCopyAsync()
    {
        var (json, _) = await BuildJsonAsync();
        var path = Path.Combine(FileSystem.CacheDirectory,
            "hisab-backup-" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm", Fmt.Inv) + ".json");
        await System.IO.File.WriteAllTextAsync(path, json);
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = L.T("পকেটনামা — ব্যাকআপ", "PocketNama — Backup"),
            File = new ShareFile(path, "application/json")
        });
    }

    static JsonObject BuildProfile()
    {
        var o = new JsonObject
        {
            ["name"] = Profile.Name,
            ["phone"] = Profile.Phone,
            ["email"] = Profile.Email,
            ["job"] = Profile.Occupation,
            ["address"] = Profile.Address,
            ["bio"] = Profile.Bio,
            ["gender"] = Profile.Gender,
            ["blood"] = Profile.BloodGroup,
            ["dob"] = Profile.Dob?.ToString("yyyy-MM-dd", Fmt.Inv) ?? ""
        };
        try
        {
            var p = Profile.PhotoPath;
            if (p != null) o["photo"] = Convert.ToBase64String(System.IO.File.ReadAllBytes(p));
        }
        catch { }
        return o;
    }

    // restore

    // Read file, replace
    public static async Task<(bool Ok, string Message)> RestoreAsync(FileResult file)
    {
        try
        {
            string text;
            using (var s = await file.OpenReadAsync())
            using (var r = new StreamReader(s))
                text = await r.ReadToEndAsync();

            if (JsonNode.Parse(text) is not JsonObject root || (string?)root["app"] != AppId)
                return (false, L.T("এটা এই অ্যাপের ব্যাকআপ ফাইল নয়"));
            if (((int?)root["version"] ?? 0) > FormatVersion)
                return (false, L.T("ফাইলটা নতুন ভার্সনের অ্যাপে বানানো; অ্যাপ আপডেট করে চেষ্টা করুন"));

            // expenses
            var expenses = new List<Expense>();
            foreach (var n in root["expenses"] as JsonArray ?? new JsonArray())
            {
                if (n is not JsonObject o) continue;
                if (!DateTime.TryParse((string?)o["date"], Fmt.Inv, System.Globalization.DateTimeStyles.None, out var d)) continue;
                var amount = (decimal)((double?)o["amount"] ?? 0);
                if (amount <= 0) continue;
                expenses.Add(new Expense
                {
                    Date = d,
                    Item = (string?)o["item"] ?? "",
                    Category = (string?)o["category"] ?? "",
                    Amount = amount,
                    Note = (string?)o["note"] ?? ""
                });
            }

            // categories & options
            var cats = new List<(int OldId, ExpenseCategory Cat)>();
            foreach (var n in root["categories"] as JsonArray ?? new JsonArray())
            {
                if (n is not JsonObject o) continue;
                var name = ((string?)o["name"] ?? "").Trim();
                if (name.Length == 0) continue;
                cats.Add(((int?)o["id"] ?? 0, new ExpenseCategory
                {
                    Name = name,
                    Icon = (string?)o["icon"] ?? "🧾",
                    Color = (string?)o["color"] ?? "#00897B",
                    Sort = (int?)o["sort"] ?? 0,
                    IsFreeText = (bool?)o["freeText"] ?? false,
                    Hint = (string?)o["hint"] ?? ""
                }));
            }

            var choices = new List<(int OldCatId, Choice Choice)>();
            foreach (var n in root["choices"] as JsonArray ?? new JsonArray())
            {
                if (n is not JsonObject o) continue;
                var name = ((string?)o["name"] ?? "").Trim();
                if (name.Length == 0) continue;
                choices.Add(((int?)o["catId"] ?? 0, new Choice { Name = name, Sort = (int?)o["sort"] ?? 0 }));
            }

            if (cats.Count == 0 && expenses.Count == 0)
                return (false, L.T("ফাইলে কোনো ডেটা পাওয়া যায়নি"));

            // loans & salary
            List<Loan>? loans = null;
            if (root["loans"] is JsonArray la)
            {
                loans = new List<Loan>();
                foreach (var n in la)
                {
                    if (n is not JsonObject o) continue;
                    var amt = (decimal)((double?)o["amount"] ?? 0);
                    if (amt <= 0) continue;
                    DateTime.TryParse((string?)o["due"], Fmt.Inv, System.Globalization.DateTimeStyles.None, out var due);
                    DateTime.TryParse((string?)o["created"], Fmt.Inv, System.Globalization.DateTimeStyles.None, out var cr);
                    loans.Add(new Loan
                    {
                        IOwe = (bool?)o["iOwe"] ?? false,
                        Person = (string?)o["person"] ?? "",
                        Amount = amt,
                        Paid = (decimal)((double?)o["paid"] ?? 0),
                        HasDue = (bool?)o["hasDue"] ?? false,
                        DueDate = due == default ? DateTime.Today : due,
                        Note = (string?)o["note"] ?? "",
                        Created = cr == default ? DateTime.Now : cr
                    });
                }
            }
            List<Salary>? salaries = null;
            if (root["salaries"] is JsonArray sa)
            {
                salaries = new List<Salary>();
                foreach (var n in sa)
                {
                    if (n is not JsonObject o) continue;
                    if (!DateTime.TryParse((string?)o["date"], Fmt.Inv, System.Globalization.DateTimeStyles.None, out var sd)) continue;
                    var amt = (decimal)((double?)o["amount"] ?? 0);
                    if (amt <= 0) continue;
                    salaries.Add(new Salary { Date = sd, Amount = amt, Note = (string?)o["note"] ?? "" });
                }
            }

            await Store.ReplaceAllAsync(expenses, cats, choices, loans, salaries);

            // limits
            if (root["settings"] is JsonObject st)
            {
                AppSettings.DailyLimit = (decimal)((double?)st["dailyLimit"] ?? 0);
                AppSettings.MonthlyLimit = (decimal)((double?)st["monthlyLimit"] ?? 0);
                try
                {
                    if (DateTime.TryParse((string?)st["rangeStart"], Fmt.Inv, System.Globalization.DateTimeStyles.None, out var rs))
                        Period.CustomStart = rs;
                    if (DateTime.TryParse((string?)st["rangeEnd"], Fmt.Inv, System.Globalization.DateTimeStyles.None, out var re))
                        Period.CustomEnd = re;
                    Period.RangeLimit = (decimal)((double?)st["rangeLimit"] ?? 0);
                    Period.IsCustom = (bool?)st["rangeOn"] ?? false;
                }
                catch { }
            }

            // profile
            if (root["profile"] is JsonObject pr) RestoreProfile(pr);

            LastBackup = DateTime.Now;
            LastBackupCount = expenses.Count;
            return (true, expenses.Count.ToString(Fmt.Inv) + L.T("টি এন্ট্রি ফিরে এসেছে"));
        }
        catch (System.Text.Json.JsonException)
        {
            return (false, L.T("ফাইলটা পড়া যায়নি (নষ্ট বা ভুল ফাইল)"));
        }
        catch (Exception ex)
        {
            return (false, L.T("ফিরিয়ে আনা যায়নি: ") + ex.Message);
        }
    }

    static void RestoreProfile(JsonObject pr)
    {
        Profile.Name = (string?)pr["name"] ?? "";
        Profile.Phone = (string?)pr["phone"] ?? "";
        Profile.Email = (string?)pr["email"] ?? "";
        Profile.Occupation = (string?)pr["job"] ?? "";
        Profile.Address = (string?)pr["address"] ?? "";
        Profile.Bio = (string?)pr["bio"] ?? "";
        Profile.Gender = (string?)pr["gender"] ?? "";
        Profile.BloodGroup = (string?)pr["blood"] ?? "";
        Profile.Dob = DateTime.TryParse((string?)pr["dob"], Fmt.Inv, System.Globalization.DateTimeStyles.None, out var dob) ? dob : null;

        var b64 = (string?)pr["photo"];
        if (!string.IsNullOrEmpty(b64))
        {
            try
            {
                var tmp = Path.Combine(FileSystem.CacheDirectory, "restore_" + Guid.NewGuid().ToString("N") + ".jpg");
                System.IO.File.WriteAllBytes(tmp, Convert.FromBase64String(b64));
                Profile.CommitPhoto(tmp, false);
            }
            catch { }
        }
        Profile.RaiseChanged();
    }
}
