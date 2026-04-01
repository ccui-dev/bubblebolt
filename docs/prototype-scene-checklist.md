# Bubble Bolt – Prototype Scene Checklist

_Last updated: 2026-03-24_

Use this to assemble the first playable greybox inside Unity 2022.3 LTS.

## 1. Project Prep
- Enable **Input System** (Edit → Project Settings → Player → Active Input Handling → "Input System Package").
- Install **TextMeshPro** essentials for HUD text.
- Create a `Resources/Tuning` folder and add a `PrototypeTuning` asset. Suggested first-pass values are already populated in the ScriptableObject.

## 2. Scene Hierarchy
```
SceneRoot
├─ ArenaRoot (ArenaPainter)
│  ├─ Center (empty transform – reference for orbit scripts)
│  ├─ TrackMesh (temp sprite/mesh for the loop)
│  ├─ Hazard_A/B/... (ArenaHazard)
├─ PlayerBubble (PlayerOrbitMover, SwipeOrbitController, PlayerPainter)
├─ RivalGhost (RivalSpiritController)
├─ UI_Canvas
│  ├─ ComboLabel / ScoreLabel / LivesLabel (HudTelemetry refs)
│  ├─ CoverageBars (Slider x2 for player/rival)
│  └─ OverchargeButton (Button + Image + OverchargeButton script)
└─ Systems (MatchStateController, PrototypeSessionManager, EventSystem, Volume, etc.)
```

## 3. Wiring Steps
1. **ArenaPainter**
   - Set `tuning` reference to the PrototypeTuning asset.
   - Leave `sectorCount = 24` unless testing variants.
2. **PlayerOrbitMover / SwipeOrbitController / PlayerPainter**
   - Reference the same `tuning` asset.
   - Point `arenaCenter` to `ArenaRoot/Center`.
   - Hook `PlayerPainter.onSectorPainted` / `onComboChanged` to temp VFX/audio if needed.
3. **RivalSpiritController**
   - Assign `arenaCenter`, `arenaPainter`, and `tuning`.
   - Adjust `orbitRadius` so it visually hugs the player lane.
4. **ArenaHazard**
   - Position each hazard on the ring, rotating its Z-axis to match the angular span center.
   - Tune `radius`, `angularHalfExtent`, and optional `onHit` events for juice.
5. **UI**
   - `HudTelemetry`: set `arenaPainter`, `playerPainter`, text labels, and coverage sliders.
   - `OverchargeButton`: hook `playerPainter`, assign the gauge fill `Image`, tweak colors.
   - `SessionStatusHud`: feed it the `PrototypeSessionManager` plus the timer/label references if you want the round timer + win tracker to show up.
   - `SessionSummaryPanel`: optional overlay that listens for `SessionCompleted` and shows the replay CTA / score.
   - `PrototypeTutorialOverlay`: display the Round 1 onboarding panel and disable swipe input until the player taps "Let's Go".
6. **MatchStateController**
   - Wire `onPlayerCoverage` / `onRivalCoverage` into slider `SetValueWithoutNotify` if you prefer events over polling.
7. **PrototypeSessionManager (optional but recommended)**
   - Drop it under Systems, assign `ArenaPainter`, `PlayerPainter`, `PlayerOrbitMover`, and `RivalSpiritController`.
   - Defaults to a best-of-three loop (20s rounds, 70% coverage to win) and resets coverage/lives between rounds.

## 4. Debug Helpers
- Add a `Gizmos` toggle or `DebugOverlay` script to view player angle/radius during tuning.
- Use Unity’s Device Simulator set to iPhone 15 Pro / Pixel 8 for touch testing.
- Temporarily map keyboard input by modifying `SwipeOrbitController.useMouseInEditor` (already supported).

## 5. Definition of Done (Prototype)
- Player can orbit, paint, and trigger Overcharge successfully.
- Rival steals sectors when trailing.
- Colliding with at least one hazard removes a life + resets combo.
- HUD reflects combo, score, lives, coverage, and Overcharge state in real time.
- Scene runs at 60 FPS in the simulator with no allocations per frame (watch profiler once assets exist).
