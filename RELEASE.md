# Release checklist

## Initial release

- [ ] Set `AppInfo.Version` and `ApplicationDisplayVersion` to the same value.
- [ ] Increase `ApplicationVersion` from the previous Android build.
- [ ] Build a Release APK for `android-arm64`.
- [ ] Install and smoke-test the APK on a real Android device.
- [ ] Verify update notifications are enabled.
- [ ] Verify the About/Developer links.
- [ ] Push the code to `master`.
- [ ] Create a Git tag such as `v5.9`.
- [ ] Create a GitHub Release from that tag.
- [ ] Attach the final APK to the GitHub Release.

## Later releases

For example, move from `5.9` to `5.10`:

```text
AppInfo.Version             = 5.10
ApplicationDisplayVersion  = 5.10
ApplicationVersion         = 15
Git tag                     = v5.10
```

Publish the APK in the GitHub Release. Installed versions will detect the newer release through the built-in daily update checker.
