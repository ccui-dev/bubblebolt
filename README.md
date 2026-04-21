# Bubble Bolt

Early design + prototype work for the Bubble Bolt hybrid-casual mobile title. The current focus is standing up a playable greybox that exercises the swipe-to-orbit loop, arena painting, combo/Overcharge hooks, and a lightweight rival ghost.

## Repository Layout

```
README.md                ← project overview + setup steps
docs/                    ← design documentation (GDD, control spec, UA brief, etc.)
unity/                   ← full Unity project (Assets + ProjectSettings + Packages)
```

> **Note:** `unity/` already contains `Assets`, `ProjectSettings`, and `Packages`. Open that folder directly from Unity Hub (targeting 2022.3 LTS), load `Assets/Scenes/PrototypeBootstrapScene.unity`, and hit Play to let `PrototypeSceneBootstrap` assemble the greybox automatically.

## Getting Started

1. **Editor Version** – Target Unity **2022.3 LTS**. The repo ships with matching `ProjectSettings` + `Packages`, so use that exact stream in Unity Hub to avoid upgrade prompts.
2. **Open the packaged project** – In Unity Hub choose **Open → Add project from disk** and select the `unity/` folder. No asset copying is required.
3. **Run the bootstrap scene** – Open `Assets/Scenes/PrototypeBootstrapScene.unity`. The only GameObject in the scene hosts `PrototypeSceneBootstrap`, which spawns the arena, player/rival orbits, hazards, HUD, tutorial overlay, analytics logger, and best-of-three session logic automatically when you press Play. Use the inspector on this component to tweak hazard counts, arena radius, session durations, etc.
4. **Input System** – The new Input System package is already listed in `manifest.json` and `Active Input Handling` is set to Input System only. Flip it to "Both" if you need legacy UI events.
5. **Tuning** – (Optional) Create a `PrototypeTuning` asset via **Create → BubbleBolt → Prototype Tuning** inside `Assets/ScriptableObjects`. Assign it to the bootstrapper if you want deterministic parameter sets; otherwise the script generates a runtime clone.
6. **Build Targets** – Plan to validate iOS + Android portrait builds at 60 FPS (`FixedDeltaTime = 0.01666`). `PrototypeSessionManager` is enabled by default to loop rounds, but you can disable it from the inspector if you want a single freeplay session.

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
4. **Add the project to Hub** – In Unity Hub, click **Open → Add project from disk** and select the duplicated folder (`unity-6.4-preview` or the original `unity` directory if you skipped step 2). Unity will regenerate the `Library/` folder automatically (the repo already includes `ProjectSettings/` + `Packages/`).
5. **Accept the upgrade prompts** – When Unity asks to update the project version, confirm. Let the editor re-import assets; this can take a couple of minutes on the first launch.
6. **Install required packages** – Open `Window → Package Manager`:
   - Ensure **Input System**, **TextMeshPro**, and **UGUI** are installed/enabled.
   - Click **Project Settings → Player → Active Input Handling** and choose `Input System Package` or `Both`.
7. **Open the bootstrap scene** – Load `Assets/Scenes/PrototypeBootstrapScene.unity` (or follow `docs/prototype-scene-checklist.md` if you want to assemble a custom scene).
8. **Verify play mode** – Enter Play Mode, drag with the mouse (or use Device Simulator touch input) to confirm orbiting, painting, hazards, and UI telemetry behave as expected.

> **Tip:** Keep the duplicated 6.4 folder out of `git` (it lives alongside `unity/` but is ignored) so upgrades don’t interfere with the main branch.

## Current Workstream

- Control + movement math implemented per `docs/bubble-bolt-control-spec.md`.
- Arena painting data model with combo + perfect-arc tracking and Overcharge gauge hooks.
- Rival ghost loop that reacts to player coverage deltas and attempts to steal sectors.
- ScriptableObject for exposing all balancing constants in one place.
- Orbiting hazard ring with pulsing telegraphs so damage windows stay readable.
- Procedural tone bed + camera shake/damage flash hooks for Overcharge activations and hazard hits.

Next milestones:
- Replace the primitive bubbles/track with lightweight prefabs + temp art and juice passes.
- Layer Overcharge VFX/SFX (plus a clearer on-hit reaction) now that the loop is playable.
- Pipe analytics events to the real schema/backend once it lands.
