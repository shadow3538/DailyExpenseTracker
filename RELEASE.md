# Release checklist

## Release 1 — v5.9

- [x] Initial public release

## Release 2 — v6.0

### Version

- [ ] Set `AppInfo.Version` to `6.0`.
- [ ] Set `ApplicationDisplayVersion` to `6.0`.
- [ ] Set `ApplicationVersion` to `15`.

### Update safety

- [ ] Keep `ApplicationId` as `com.abdulmalek.dailyexpense`.
- [ ] Keep the same Android signing key used for v5.9.
- [ ] Do not change the SQLite database path/name.
- [ ] Install v6.0 over v5.9 and verify existing expenses, categories, loans, salary and profile data.

### UI

- [ ] Verify Home appears immediately and the other four tabs warm in the background after startup.
- [ ] Switch and swipe between tabs repeatedly and verify there is no Shell page navigation/render cycle.
- [ ] Verify the short fade + slide tab transition.
- [ ] Verify Add, Report, Calculator and Settings still work after switching tabs.
- [ ] Change Language and Theme and verify the chooser stays open; Back returns to Settings without a full Shell rebuild.
- [ ] Verify the Restore button on the Settings backup card opens the file picker directly without entering Backup & Restore.
- [ ] Verify Report history shows 10 rows at a time and Previous/Next works correctly.

### GitHub release

- [ ] Push the v6.0 code to GitHub.
- [ ] Create tag `v6.0`.
- [ ] Create the second GitHub Release from `v6.0`.
- [ ] Attach the final signed ARM64 APK.
- [ ] Keep the first release `v5.9` available.
- [ ] Verify v5.9 detects the v6.0 release through the update checker.

### Notes

- Report history is paginated in groups of 10 rows to reduce UI load; the report summary still uses the selected date range.

`v6.0` is a normal Android update over `v5.9` when the package ID and signing key are unchanged. User data is kept in the existing SQLite database.
