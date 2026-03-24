# Bubble Bolt

Early design + prototype work for the Bubble Bolt hybrid-casual mobile title. The current focus is standing up a playable greybox that exercises the swipe-to-orbit loop, arena painting, combo/Overcharge hooks, and a lightweight rival ghost.

## Repository Layout

```
README.md                ← project overview + setup steps
docs/                    ← design documentation (GDD, control spec, UA brief, etc.)
unity/                   ← prototype-ready Unity content (Assets-only for now)
```

> **Note:** The Unity folder only contains authored content (`Assets/…`). Create/open a Unity 2022.3 LTS project and drop these folders into it, or point a new project directly at `unity/` to let the editor generate `Library`, `Packages`, and `ProjectSettings` locally.

## Getting Started

1. **Editor Version** – Target Unity **2022.3 LTS**. (URP vs Built-in is undecided; the prototype scripts are renderer-agnostic.)
2. **Input System** – Enable the new Input System package (or Input System + legacy if you need UI). The scripts read from `UnityEngine.Input`. Switching to `InputAction`s can happen once the project is in editor.
3. **Scene Setup** – Create an empty scene with:
   - An `ArenaPainter` on the arena root (set `sectorCount = 24`).
   - A `PlayerOrbitMover` + `SwipeOrbitController` + `PlayerPainter` on the player bubble.
   - A `RivalSpiritController` on a separate ghost to test steal/back-pressure behavior.
4. **Tuning** – Instantiate `PrototypeTuning` via `Create → BubbleBolt → Prototype Tuning`. Assign the asset anywhere scripts expect it.
5. **Build Targets** – Plan to validate on iOS + Android portrait builds; keep `FixedDeltaTime` at 60 FPS for consistent feel.

## Current Workstream

- Control + movement math implemented per `docs/bubble-bolt-control-spec.md`.
- Arena painting data model with combo + perfect-arc tracking and Overcharge gauge hooks.
- Rival ghost loop that reacts to player coverage deltas and attempts to steal sectors.
- ScriptableObject for exposing all balancing constants in one place.

Next milestones:
- Wire actual Unity scene/prefabs and drop temp art.
- Add hazard spawners + Overcharge VFX triggers.
- Connect analytics events according to the GDD.
