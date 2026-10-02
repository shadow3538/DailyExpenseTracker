# পকেটনামা (PocketNama)

**পকেটনামা** is an offline-first Android expense tracker built with **.NET MAUI, C#, and SQLite**. It is designed for quick daily expense entry, budgeting, reports, reminders, backup/restore, loans, salary tracking, and a home-screen widget.

## Release

- **Version:** `6.0`
- **Release tag:** `v6.0`
- **Release number:** `2`
- **Previous release:** `v5.9`
- **Package:** `com.abdulmalek.dailyexpense`
- **Minimum Android:** 5.0 (API 21)
- **Release architecture:** ARM64
- **Repository:** https://github.com/shadow3538/DailyExpenseTracker

This is the **second release** of the project. The first release was `v5.9`.

## What changed from v5.9

### Navigation and smoothness

- Main tabs are created at startup and warmed in the background after Home is ready, so the first visit to another tab does not need to perform its initial data/UI load.
- Replaced Shell TabBar switching with a single main host that keeps the five tab views alive; changing tabs no longer performs Shell page navigation.
- Added a short 140–150 ms fade + slide transition for tab changes.
- Swipe navigation now switches the cached tab view directly, avoiding the previous Shell render/navigation cycle.
- Report history now shows only 10 history rows at a time with Previous/Next controls, reducing the number of UI elements created at once.
- Database initialization starts at app launch so later tab changes do not wait for the first database setup.
- Language and theme changes now stay inside their current settings dialog; the dialog refreshes in place and Back returns directly to the refreshed Settings tab instead of rebuilding the whole app shell.
- The Restore button on the Settings backup card works directly without opening the Backup & Restore details page first.
- Existing page-level features and data logic are preserved.

### Update safety

- `ApplicationId` remains `com.abdulmalek.dailyexpense`.
- SQLite database remains `hisab_simple.db3` in the app data directory.
- Android version code increased from `14` to `15`.
- Display version changed from `5.9` to `6.0`.
- The app can therefore be installed as an update over v5.9 when the APK is signed with the same signing key.

### Code cleanup

- Removed unnecessary Bangla and explanatory comments.
- Kept only short English logic hints where they help locate important code.

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
├── AppShell.xaml          # Minimal root shell
├── MainTabPage.cs         # Cached single-host tab navigation
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

For each later release, increase the user-facing version and Android version code, create a matching Git tag, and publish the APK in that GitHub Release. The installed app will then detect the newer release automatically.

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
