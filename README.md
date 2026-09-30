# PocketNama

**PocketNama (পকেটনামা)** is an offline-first personal expense tracker for Android, built with **.NET MAUI** and **.NET 9**.

## First Release — v5.9

This is the first public release of PocketNama.

### Features

- Daily, monthly and custom-range expense tracking
- Quick expense entry by category and option
- Edit and delete expense records
- Daily and monthly spending limits
- Reports and day-by-day history
- Custom categories and options
- Salary and loan tracking
- Expense reminders and loan due reminders
- Built-in calculator
- Backup and restore using JSON files
- Home-screen widget
- Bangla and English language support
- Light, dark and system themes
- Offline data storage
- Profile information and photo

## Update Check

PocketNama checks GitHub for a newer release **once per calendar day when the app is opened**.

- No update check is repeated on the same day.
- A notification is shown when a newer release is available.
- The notification opens the GitHub release page.
- The same release is not notified repeatedly.
- The app does not upload expense data during the update check.

### Configure GitHub Updates

Before publishing, set the repository URL in:

```text
Services/AppInfo.cs
```

Set:

```csharp
public const string GitHubUrl = "https://github.com/YOUR_USERNAME/YOUR_REPOSITORY";
```

The update checker uses this repository's latest GitHub Release.

Also fill in `DownloadUrl` if you want the app's About/Share pages to show a download link.

## Requirements

- Visual Studio 2022
- .NET 9 SDK
- Visual Studio workload: **.NET Multi-platform App UI development**
- Android SDK / emulator or a physical Android device

## Build

1. Open `DailyExpenseTracker.csproj` in Visual Studio.
2. Restore NuGet packages.
3. Select **Release** configuration.
4. Select an Android device or emulator.
5. Build and publish the Android APK.

The current Release configuration targets **Android ARM64** and produces an APK.

## Install

Install the generated APK on a supported Android device.

Existing app data is stored locally on the device. Use the built-in backup feature before uninstalling the app or changing devices.

## Release Checklist

Before creating a GitHub Release:

- [ ] Set `GitHubUrl` in `Services/AppInfo.cs`
- [ ] Set `DownloadUrl` in `Services/AppInfo.cs`
- [ ] Set `ReportEmail` if email feedback is required
- [ ] Increase `ApplicationVersion` in `DailyExpenseTracker.csproj` for the next Android build
- [ ] Update `ApplicationDisplayVersion` and `AppInfo.Version`
- [ ] Build a Release APK
- [ ] Test installation over the previous version
- [ ] Test the update notification with a higher GitHub release version
- [ ] Create the GitHub Release and upload the APK

## Data & Privacy

PocketNama is designed to keep expense data on the device. The app does not send expense records to the update-check service.

The only network request added for release updates is a GitHub Releases API request to check the latest app version.

## Developer

**Abdul Malek**

## Version

**v5.9**
