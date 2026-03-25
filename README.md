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

## Inspecting the Unity content with Unity 6.4

Use these steps if you want to open/inspect the project in the bleeding-edge Unity 6.4 editor:

1. **Clone the repo**
   ```bash
   git clone https://github.com/ccui-dev/bubblebolt.git
   cd bubblebolt
   ```
2. **Create a safety copy (optional but recommended)** – Unity 6.4 will upgrade project metadata. Duplicate the `unity/` folder so you can keep the original 2022.3 version untouched:
   ```bash
   cp -R unity unity-6.4-preview
   ```
3. **Install Unity 6.4** – Via Unity Hub, install the 6.4 editor (or newer) with iOS + Android modules if you plan to build.
4. **Add the project to Hub** – In Unity Hub, click **Open → Add project from disk** and select the duplicated folder (`unity-6.4-preview` or the original `unity` directory if you skipped step 2). Unity will generate `Library/`, `ProjectSettings/`, and `Packages/` automatically.
5. **Accept the upgrade prompts** – When Unity asks to update the project version, confirm. Let the editor re-import assets; this can take a couple of minutes on the first launch.
6. **Install required packages** – Open `Window → Package Manager`:
   - Ensure **Input System**, **TextMeshPro**, and **UGUI** are installed/enabled.
   - Click **Project Settings → Player → Active Input Handling** and choose `Input System Package` or `Both`.
7. **Wire the scene** – Create a new scene (or reuse one) and follow `docs/prototype-scene-checklist.md` to place `ArenaPainter`, `PlayerOrbitMover`, hazards, and the HUD scripts.
8. **Verify play mode** – Enter Play Mode, drag with the mouse (or use Device Simulator touch input) to confirm orbiting, painting, hazards, and UI telemetry behave as expected.

> **Tip:** Keep the duplicated 6.4 folder out of `git` (it lives alongside `unity/` but is ignored) so upgrades don’t interfere with the main branch.

## Current Workstream

- Control + movement math implemented per `docs/bubble-bolt-control-spec.md`.
- Arena painting data model with combo + perfect-arc tracking and Overcharge gauge hooks.
- Rival ghost loop that reacts to player coverage deltas and attempts to steal sectors.
- ScriptableObject for exposing all balancing constants in one place.

Next milestones:
- Wire actual Unity scene/prefabs and drop temp art.
- Add hazard spawners + Overcharge VFX triggers.
- Connect analytics events according to the GDD.
