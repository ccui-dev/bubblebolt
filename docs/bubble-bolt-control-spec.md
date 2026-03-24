# Bubble Bolt – Control & Systems Spec (Draft)
_Date: 2026-03-23_

## 1. Input Handling
- **Sampling:** Unity Input System (Touchscreen) – capture drag delta each frame, normalized by screen width.
- **Smoothing:** `smoothedDelta = Lerp(prevDelta, rawDelta, clamp01(dt * 12f))` to avoid jitter.
- **Dead zone:** Ignore deltas < 0.01 to prevent accidental drift when players hold steady.

## 2. Movement Model
Let:
- `theta` = angular position
- `omega` = angular velocity
- `r` = radius offset from arena center
- `targetR` = lane radius (default 0.8)
- `drift` = small vertical wiggle for juice

Equations:
```
omega = clamp(omega + smoothedDelta.x * sensitivity, -omegaMax, omegaMax)
theta += omega * dt
r = Mathf.Lerp(r, targetR + smoothedDelta.y * radialRange, dt * 6f)
position = center + PolarToCartesian(r, theta) + drift
```

## 3. Painting & Scoring
- Arena split into `N` sectors (default 24). Each sector stores owner enum + fill amount.
- When player passes through sector, increase fill by `(baseRate * speedMultiplier * dt)`.
- Sector flips to player when fill >= 1.0; add combo meter +1.
- Score per sector = `coverageWeight * comboMultiplier`.
- Perfect arc bonus: completing ≥4 adjacent sectors without collision grants +10%.

## 4. Collision & Damage
- Hazards defined by radius + angular span; collision check each frame.
- On collision w/out Overcharge:
  - Lose 1 life (2 lives total per session).
  - Combo reset to 0.
  - Temporary invulnerability 0.75s.

## 5. Overcharge System
- Consumable item; triggered via tap button when gauge ≥1.
- Effects (5 seconds):
  - Invulnerability
  - Painting rate ×2
  - Trail color swap + lens flare
- Gauge filled by painting sectors (0.05 per sector) or rewarded ads.

## 6. Rival AI Outline
- Rivals follow bezier path hugging target radius.
- Behavior tree:
  1. Maintain preferred speed (0.75× player max).
  2. If player leads by >20% coverage, increase aggression: target same sector as player to "steal".
  3. If stunned (player Overcharge), move outward to avoid.

## 7. Camera & FX Hooks
- Cinemachine virtual cam anchored to arena center, FOV 35.
- Post-processing profile: subtle bloom + chromatic aberration pulses when combo ≥5.
- Screen shake amplitude tied to sector completion events.

## 8. Data Hooks
- Serialize tuning constants (sensitivity, omegaMax, fill rates) via ScriptableObjects for live balancing.
- Debug UI toggles: slow motion, no hazards, rival speed slider.
