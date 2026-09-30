# পকেটনামা (PocketNama)

**পকেটনামা** is an offline-first Android expense tracker built with **.NET MAUI, C#, and SQLite**. It is designed for quick daily expense entry, budgeting, reports, reminders, backup/restore, loans, salary tracking, and a home-screen widget.

## Release

- **Version:** `5.9`
- **Release tag:** `v5.9`
- **Package:** `com.abdulmalek.dailyexpense`
- **Minimum Android:** 5.0 (API 21)
- **Release architecture:** ARM64
- **Repository:** https://github.com/shadow3538/DailyExpenseTracker

This README describes the current codebase as the **initial release for this project state**. Older development-history notes are intentionally not included here.

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

### How it works

1. The app reads the repository from `Services/AppInfo.cs`.
2. Around **12:00 local time**, Android schedules an inexact background alarm.
3. The app queries GitHub's latest release API.
4. If no release exists yet, it falls back to `ApplicationDisplayVersion` in the repository's `DailyExpenseTracker.csproj`.
5. The remote version is compared with `AppInfo.Version`.
6. If the remote version is newer, a notification is shown once for that version.
7. Tapping the notification opens the release/download page.
8. Opening the app also performs a check when the previous successful check is older than about 20 hours.

### Important

The user must allow notifications for update notifications to appear. Internet access is required for the GitHub check. Expense data is **not** uploaded to GitHub by this feature.

The setting is available under **Settings → App update notifications** and is enabled by default.

## Developer configuration

Developer information is centralized in:

`Services/AppInfo.cs`

Current developer:

- **Name:** Abdul Malek
- **Email:** shuvroakash68@gmail.com
- **GitHub:** https://github.com/shadow3538/DailyExpenseTracker
- **LinkedIn:** https://www.linkedin.com/in/md-abdul-malek-a0826833a/

The app's report/feedback email is also configured to `shuvroakash68@gmail.com`.

## Project structure

```text
DailyExpenseTracker/
├── Data/                  # SQLite storage
├── Models/                # Data models
├── Services/              # Business logic and shared services
├── Views/                 # MAUI pages and UI
├── Platforms/Android/     # Android alarms, notifications and widget
├── Resources/             # Icons, splash and UI resources
├── App.xaml               # App resources
├── AppShell.xaml          # Main navigation shell
├── MauiProgram.cs         # MAUI startup
└── DailyExpenseTracker.csproj
```

## Requirements

- Visual Studio 2022 with the **.NET MAUI / Android** workload, or the equivalent .NET 9 SDK/workload setup.
- Android SDK.
- Android device or emulator.
- USB debugging enabled for a physical device.

## Build

### Visual Studio

1. Open `DailyExpenseTracker.csproj`.
2. Restore NuGet packages.
3. Select **Release** and an Android device/emulator.
4. Build or publish the project.

### Command line

```bash
dotnet workload install maui-android
dotnet restore
dotnet build -f net9.0-android -c Release
dotnet publish -f net9.0-android -c Release
```

The Release configuration targets `android-arm64` and produces an APK suitable for modern 64-bit Android devices.

## First GitHub release

For this initial public release, use the tag `v5.9`.

```bash
git init
git add .
git commit -m "Initial release v5.9"
git branch -M master
git remote add origin https://github.com/shadow3538/DailyExpenseTracker.git
git push -u origin master
git tag v5.9
git push origin v5.9
```

Then create a GitHub Release from tag **`v5.9`** and attach the final APK.

For later releases, increase both values together:

- `ApplicationDisplayVersion` / `AppInfo.Version`: user-facing version, e.g. `5.10`
- `ApplicationVersion`: Android version code, e.g. `15`

Create a matching tag such as `v5.10` and publish the APK in that GitHub Release. The installed app will then detect the newer release automatically.

## Permissions

The Android app uses permissions required for its features:

- `INTERNET` — GitHub update checks.
- `POST_NOTIFICATIONS` — reminders and update notifications on Android 13+.
- `RECEIVE_BOOT_COMPLETED` — restore scheduled alarms after reboot.
- `SCHEDULE_EXACT_ALARM` — reminder scheduling where Android permits exact alarms.
- `CAMERA` — optional profile-photo capture.

No permission is used to upload expense records to a remote server.

## Data and privacy

- Expense and profile data are stored locally on the device.
- The app does not require an online account for normal expense tracking.
- GitHub is contacted only for the version-update check when that feature is enabled.
- Backup files are created only when the user chooses to back up data.

## License

No open-source license is declared in this repository yet. Add a license before advertising the project as reusable open-source software.

## Author

**Abdul Malek**

- Email: shuvroakash68@gmail.com
- LinkedIn: https://www.linkedin.com/in/md-abdul-malek-a0826833a/
- GitHub: https://github.com/shadow3538
