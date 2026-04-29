# Bubble Bolt – Prototype Scene Checklist

_Last updated: 2026-04-28_

Use this to assemble the first playable greybox inside Unity 2022.3 LTS.

> **Shortcut:** Open `Assets/Scenes/PrototypeBootstrapScene.unity` to see everything auto-instantiated by `PrototypeSceneBootstrap`. Use this checklist if you need to hand-wire a custom layout or replace pieces of the generated graph.

## 1. Project Prep
- Enable **Input System** (Edit → Project Settings → Player → Active Input Handling → "Input System Package").
- Install **TextMeshPro** essentials for HUD text.
- Create a `Resources/Tuning` folder and add a `PrototypeTuning` asset. Suggested first-pass values are already populated in the ScriptableObject.

## 2. Scene Hierarchy
```
SceneRoot
├─ ArenaRoot (ArenaPainter)
│  ├─ Center (empty transform – reference for orbit scripts)
│  ├─ OrbitTrack (LineRenderer + OrbitTrackPulse)
│  ├─ Hazard_00/01… (ArenaHazard + HazardTelegraphArc + HazardMarkerPulse)
├─ PlayerBubble (PlayerOrbitMover, SwipeOrbitController, PlayerPainter, PlayerOverchargeGlow, BubbleAuraPulse, TrailRenderer)
├─ RivalGhost (RivalSpiritController + BubbleAuraPulse/TrailRenderer)
├─ UI_Canvas
│  ├─ ComboLabel / ScoreLabel / LivesLabel (HudTelemetry refs)
│  ├─ CoverageBars (Slider x2 for player/rival)
│  ├─ Session UI (SessionStatusHud + SessionSummaryPanel)
│  └─ OverchargeButton (Button + Image + OverchargeButton script)
└─ Systems (MatchStateController, PrototypeSessionManager, PrototypeAnalyticsLogger, PrototypeTonePlayer, EventSystem, etc.)
```

## 3. Wiring Steps
1. **ArenaPainter**
   - Set `tuning` reference to the PrototypeTuning asset.
   - Leave `sectorCount = 24` unless testing variants.
2. **PlayerOrbitMover / SwipeOrbitController / PlayerPainter**
   - Reference the same `tuning` asset.
   - Point `arenaCenter` to `ArenaRoot/Center`.
   - `PlayerOverchargeGlow` + `BubbleAuraPulse` expect a Renderer/LineRenderer on the bubble (the bootstrapper adds them automatically). Override colors if you want unique shells.
3. **RivalSpiritController**
   - Assign `arenaCenter`, `arenaPainter`, and `tuning`.
   - Adjust `orbitRadius` so it visually hugs the player lane.
4. **ArenaHazard**
   - Position each hazard on the ring, rotating its Z-axis to match the angular span center.
   - Tune `radius`, `angularHalfExtent`, and optional `onHit` events for juice.
   - Parent all hazards under a `Hazards` root with `HazardOrbitController` so they keep circling the arena. Add `HazardMarkerPulse` + `HazardTelegraphArc` (line renderer) to every hazard for readable telegraphs and hook the `ArenaHazard.Hit` event to flash them.
5. **UI**
   - `HudTelemetry`: set `arenaPainter`, `playerPainter`, text labels, and coverage sliders.
   - `OverchargeButton`: hook `playerPainter`, assign the gauge fill `Image`, tweak colors.
   - `SessionStatusHud`: feed it the `PrototypeSessionManager` plus the timer/label references if you want the round timer + win tracker to show up.
   - `SessionSummaryPanel`: overlay that listens for `SessionCompleted` and shows the replay CTA / score.
   - `PrototypeTutorialOverlay`: display the Round 1 onboarding panel and disable swipe input until the player taps "Let's Go".
6. **MatchStateController & PrototypeSessionManager**
   - `MatchStateController` mirrors coverage into HUD sliders if you prefer events over polling.
   - `PrototypeSessionManager` resets coverage/lives and emits round/session events. Assign the ArenaPainter + PlayerPainter + PlayerOrbitMover + RivalSpiritController.
7. **Juice Layer**
   - Add `PrototypeTonePlayer` (plus an `AudioSource`) under Systems and hook it into `PlayerFeedbackController` so Overcharge/damage fire procedural tones + a sustained Overcharge loop.
   - Keep a `PrototypeCameraShake` on the Main Camera so hits add a quick screen shake.
   - Use the new serialized gradients on `PrototypeSceneBootstrap` (or your own prefabs) to tweak bubble trails, aura rings, and hazard ribbons for capture-ready contrast.
8. **Analytics**
   - Drop `PrototypeAnalyticsLogger` on the Systems object. Assign the session manager + player painter references.
   - Enable **Upload Events** on the component (or via the bootstrapper) if you want to POST JSON payloads to the analytics service. Provide the endpoint + API key once the backend is ready; until then it will just log to the console.

## 4. Debug Helpers
- Add a `Gizmos` toggle or `DebugOverlay` script to view player angle/radius during tuning.
- Use Unity’s Device Simulator set to iPhone 15 Pro / Pixel 8 for touch testing.
- Temporarily map keyboard input by modifying `SwipeOrbitController.useMouseInEditor` (already supported).

## 5. Definition of Done (Prototype)
- Player can orbit, paint, and trigger Overcharge successfully.
- Rival steals sectors when trailing.
- Colliding with at least one hazard removes a life + resets combo (with readable telegraphs + flash).
- HUD reflects combo, score, lives, coverage, and Overcharge state in real time.
- Analytics events (`round_result`, `session_result`, `overcharge_use`) log locally and optionally POST to the backend endpoint.
- Scene runs at 60 FPS in the simulator with no allocations per frame (watch profiler once assets exist).
