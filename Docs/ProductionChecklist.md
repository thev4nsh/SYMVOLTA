# SYMVOLTA Production Checklist

- Add the production Firebase project and keep `Assets/google-services.json` in sync with bundle id `com.symvolta.game`.
- Publish Firestore rules from `Docs/FirebaseFirestoreRules.rules`.
- Configure Remote Config defaults before release.
- Set Play Store URL in `update_url`.
- Keep `min_version` equal to the oldest playable version; raising it blocks older clients.
- Test forced update, maintenance, offline play, reconnect sync, and notification permission on Android 13+.
- Build Android with IL2CPP and ARM64 only.
- Run `Tools/SYMVOLTA/Build ALL Core Scenes` after script/UI changes.
- Validate leaderboards with test accounts before public release.
