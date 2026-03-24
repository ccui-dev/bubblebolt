# Bubble Bolt – Greybox Prototype Plan
_Date: 2026-03-23_

## Goals
- Validate swipe/drag controls for orbiting movement in under 1 week.
- Produce a capture-ready vertical build showing 3 escalating rounds (no final art).
- Instrument minimum analytics hooks (`round_result`, `overcharge_use`) for early feel.

## Work Breakdown
1. **Project Setup**
   - Unity 2022 LTS portrait template, URP minimal lighting.
   - Input system: leverage Unity Input System package for swipe detection.
2. **Player Controller**
   - Translate swipe delta → tangential velocity + radial offset constraints.
   - Implement Overcharge state (timer + invulnerability flag).
3. **Arena System**
   - ScriptableObject definition for ring radius, rotation speed, hazard spawn table.
   - Visual debug meshes showing sectors (color-coded states: neutral, player, rival).
4. **AI Rivals (MVP)**
   - Ghost bots follow sinusoidal paths with perturbations + simple avoidance.
   - Elastic difficulty: adjust bot speed based on player dominance.
5. **Scoring & UI**
   - Coverage percentage per round, streak multiplier, lightweight HUD with Overcharge button.
   - End-of-session summary card.
6. **Game Flow**
   - 3-round session manager, tutorial overlay on first boot, basic pause/state handling.
7. **Build & Capture Tools**
   - Vertical frame capture camera (Cinemachine) for marketing clips.
   - Debug toggles to force slow-mo / Overcharge for content team.

## Risks & Mitigations
- **Swipe precision:** If one-finger accuracy is sloppy, consider hybrid scheme (tap lanes) for MVP.
- **Motion sickness:** Excess rotation may cause discomfort; add counter-rotating background for reference.
- **Performance on low-end Android:** Keep shader graph simple; cap particle count.

## Deliverables
- Playable APK/IPA test build
- 30s capture reel demonstrating Round 1-3 flow
- Note doc summarizing tuning constants for designers/marketing
