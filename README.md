# SYMVOLTA

SYMVOLTA is a portrait-only Android precision drawing game built with Unity 6 LTS, C#, URP 2D, and Firebase.

## Core Flow

- `BootScene` initializes Firebase, managers, networking, Remote Config, notifications, local profile, offline queue, update checks, maintenance checks, and announcements.
- `MainMenu` shows profile access, rank title, settings, global score, world rank, and entry points.
- `ShapeSelect` offers Circle, Triangle, Square, Star, Heart, and Infinity modes.
- `Gameplay` runs a 2 minute drawing session with live accuracy, timer warnings, optimized touch drawing, score validation, and results.
- `Leaderboard` shows overall and per-shape global boards.
- `Profile` stores UID, username, icon, rank, shape highscores, and weighted global score.
- `OfflineMode` supports local play, local profile/settings/scores, and queued score sync.

## Firebase

Required Firebase products:

- Authentication: anonymous sign-in.
- Firestore: profiles, reserved usernames, and leaderboard score documents.
- Analytics: gameplay and security events.
- Cloud Messaging: push token and notification topic support.
- Remote Config: version gates, maintenance, timer tuning, announcements.

Remote Config keys:

- `min_version`
- `latest_version`
- `update_url`
- `update_notes`
- `tutorial_url`
- `support_url`
- `maintenance_mode`
- `game_timer_seconds`
- `announcement_active`
- `announcement_id`
- `announcement_title`
- `announcement_body`
- `accuracy_threshold`

## Regenerate Scenes

Open Unity and run:

`Tools/SYMVOLTA/Build ALL Core Scenes`

Batch mode:

`Unity.exe -batchmode -quit -projectPath . -executeMethod MasterBuilder.BuildAllScenes`

## Android Build

The builder configures:

- Android only
- Portrait orientation
- IL2CPP
- ARM64
- Minimum SDK 26
- Target frame rate 60
- Bundle id `com.symvolta.game`

Firebase Android config is expected at `Assets/google-services.json`.
