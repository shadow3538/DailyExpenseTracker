# পকেটনামা (PocketNama)

**পকেটনামা** is an offline-first Android expense tracker built with **.NET MAUI, C#, and SQLite**. It is designed for quick daily expense entry, budgeting, reports, reminders, backup/restore, loans, salary tracking, and a home-screen widget.

## Release

- **Version:** `5.9`
- **Release tag:** `v5.9`
- **Package:** `com.abdulmalek.dailyexpense`
- **Minimum Android:** 5.0 (API 21)
- **Release architecture:** ARM64
- **Repository:** https://github.com/shadow3538/DailyExpenseTracker


## Features

### Expense management

- Add expenses quickly by category and option.
- Add free-text expenses for shopping and custom items.
- Edit or delete any entry.
- Add expenses to previous dates; future dates are blocked.
- Keep daily and monthly spending limits.
- View the last 7 days of spending.

### Reports and analysis

- Daily, monthly, and custom-range reports.
- Spending versus the selected limit.
- Category percentages and detailed breakdowns.
- Day-by-day history with detailed entry cards.

### Finance tools

- Loan tracking: money to pay and money to receive.
- Partial repayment/collection tracking.
- Monthly salary entries.
- Remaining salary after expenses.
- Built-in calculator with direct expense entry.

### Reminders and updates

- Daily expense reminder at a user-selected time.
- Loan due-date reminders.
- **GitHub update checker:** when enabled, the app checks the GitHub repository once per day and can notify the user when a newer version is available.
- The update checker also checks after app startup when the previous check is older than about 20 hours, so delayed Android alarms do not permanently prevent checks.
- Android boot/package-replacement events re-arm the update alarm.

### Backup and restore

- Backup expenses, categories, options, limits, profile, loans, and salary data.
- Save backups to a phone/SD folder or Google Drive through Android's file picker/share flow.
- Restore from a JSON backup.
- Existing data remains local to the device.

### Profile and customization

- Local profile with photo and personal details.
- Bangla and English UI.
- Light, dark, and system theme modes.
- Custom categories and options.
- Home-screen widget for today's numbers and quick actions.

## GitHub update notification

The update checker is already part of the application.

## Data and privacy

- Expense and profile data are stored locally on the device.
- The app does not require an online account for normal expense tracking.
- GitHub is contacted only for the version-update check when that feature is enabled.
- Backup files are created only when the user chooses to back up data.

## Author

**Abdul Malek**

- Email: shuvroakash68@gmail.com
- LinkedIn: https://www.linkedin.com/in/md-abdul-malek-a0826833a/
- GitHub: https://github.com/shadow3538
