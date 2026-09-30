namespace DailyExpenseTracker;

// App metadata
public static class AppInfo
{
    public const string AppNameBn = "পকেটনামা";
    public const string AppNameEn = "PocketNama";
    public const string Version = "5.9";

    // Release links
    public const string DownloadUrl = "";
    public const string GitHubUrl = "";
    public const string ReportEmail = "";

    // Developer
    public static readonly (string Icon, string Name, string NoteBn, string NoteEn)[] Developers =
    {
        ("👤", "Abdul Malek", "", ""),
    };

    // Developer bio
    public const string DevBioBn = "";
    public const string DevBioEn = "";

    // Developer contacts
    public static readonly (string Icon, string LabelBn, string LabelEn, string Value, string Url)[] DevContacts =
    {
        ("✉️", "ইমেইল", "Email", "", ""),               // Email link
        ("📞", "ফোন", "Phone", "", ""),                  // Phone link
        ("🐙", "গিটহাব", "GitHub", "", ""),              // GitHub link
        ("📘", "ফেসবুক", "Facebook", "", ""),
        ("💼", "লিংকডইন", "LinkedIn", "", ""),
    };

    // Developer details
    public static readonly (string Icon, string TitleBn, string TitleEn, string TextBn, string TextEn)[] DevExtra =
    {
        ("🎓", "শিক্ষা", "Education", "", ""),
        ("🛠️", "দক্ষতা", "Skills", "", ""),
        ("📍", "ঠিকানা", "Location", "", ""),
    };

    // App features
    public static readonly (string Icon, string TitleBn, string TitleEn, string TextBn, string TextEn)[] Features =
    {
        ("🏠", "হোম", "Home",
            "আজকের খরচ, আজ বাকি, মাসে খরচ ও মাসে বাকি এক নজরে; সাথে আজকের এন্ট্রি।",
            "Today's spending, what's left today and this month, plus today's entries at a glance."),
        ("➕", "দ্রুত খরচ যোগ", "Quick add",
            "ক্যাটাগরি বেছে প্রতিটি অপশনের পাশে টাকা লিখে 'যোগ' চাপলেই হয়ে যায়। কীবোর্ড খোলা রেখেই একের পর এক যোগ করা যায়।",
            "Pick a category, type the amount next to an option and tap Add. Keep the keyboard open and add one after another."),
        ("✏️", "এডিট ও মোছা", "Edit & delete",
            "যেকোনো এন্ট্রিতে ট্যাপ করে অপশন, টাকা, নোট, তারিখ বদলানো বা মুছে ফেলা যায়।",
            "Tap any entry to change the option, amount, note or date, or delete it."),
        ("📊", "রিপোর্ট", "Reports",
            "দিন, মাস বা নিজের বেছে নেওয়া রেঞ্জ অনুযায়ী খরচের হিসাব ও বিশ্লেষণ।",
            "Spending totals and breakdowns by day, month or a custom range."),
        ("🎯", "দৈনিক ও মাসিক লিমিট", "Daily & monthly limits",
            "লিমিট দিলে কতটা খরচ হলো আর কতটা বাকি তা রঙ ও বারে দেখা যায়।",
            "Set limits and see how much is spent and left through colours and bars."),
        ("🔔", "রিমাইন্ডার", "Reminders",
            "নির্দিষ্ট সময়ে নোটিফিকেশন, যাতে খরচ লিখতে ভুলে না যান।",
            "Notifications at set times so you don't forget to log expenses."),
        ("⬆️", "আপডেট নোটিফিকেশন", "Update alerts",
            "নতুন ভার্সন এলে নোটিফিকেশন দিয়ে জানায়।",
            "Checks GitHub daily and notifies when a newer release is available."),
        ("🤝", "ঋণ ও বেতন", "Loans & salary",
            "কাকে দিলেন, কার কাছে পাবেন এবং মাসের বেতনের হিসাব রাখা যায়।",
            "Track money you owe, money to receive and your monthly salary."),
        ("🗂️", "নিজের ক্যাটাগরি", "Custom categories",
            "ক্যাটাগরি ও অপশন যোগ করা, নাম বদলানো ও মোছা যায়।",
            "Add, rename and delete categories and options."),
        ("🧮", "ক্যালকুলেটর", "Calculator",
            "অ্যাপের ভেতরেই বড় ফলাফলের সহজ ক্যালকুলেটর।",
            "A simple calculator with a large result area, built into the app."),
        ("💾", "ব্যাকআপ ও রিস্টোর", "Backup & restore",
            "ফোনের ফোল্ডার বা গুগল ড্রাইভে ব্যাকআপ; ফোন বদলালেও সব ফিরিয়ে আনা যায়।",
            "Back up to a phone folder or Google Drive and restore everything on a new phone."),
        ("📱", "উইজেট", "Home-screen widget",
            "হোম স্ক্রিনে উইজেটে আজকের হিসাব দেখা যায়।",
            "See today's numbers on a home-screen widget."),
        ("🎨", "বাংলা/ইংরেজি ও থিম", "Language & theme",
            "বাংলা বা ইংরেজি ভাষা; লাইট, ডার্ক বা ফোনের মতো থিম।",
            "Bangla or English, and light, dark or automatic theme."),
        ("🔒", "অফলাইন ও নিরাপদ", "Offline & private",
            "ইন্টারনেট লাগে না। সব ডেটা আপনার ফোনেই থাকে।",
            "No internet needed. All your data stays on your phone."),
    };
}
