# Bubble Bolt – Game Design Doc (v0.1)
_Date: 2026-03-23_

## 1. Vision & KPI Targets
- **Premise:** Swipe a mischievous ghost bubble around rotating arenas, painting segments while dodging rival spirits. Each 15–60 second "track bite" is engineered for vertical video virality and frictionless entry from Instagram.
- **North-star KPI:** 30-day cumulative downloads ≥ 1M with blended CPI <$0.40 via Reels/TikTok creatives.
- **Retention:** D1 35%, D7 8% (leveraged through streak missions + limited-time arenas).
- **Monetization mix:** 55% ads / 45% IAP; ARPDAU target $0.22 by week 8.

## 2. Audience & Platforms
- **Audience:** 13–30 year-old casual players who bounce between puzzle/.io hits (Block Blast!, Hole.io) and viral TikTok challenges.
- **Platforms:** iOS & Android (portrait only) with optional desktop WebGL build for influencer capture.
- **Social entry:** Instagram Stories/Reels, TikTok, YouTube Shorts (CTA deep-links).

## 3. Core Loop
1. **Matchmaking:** Player spawns solo vs. lightweight AI rivals (PvP feel without server costs for MVP).
2. **Round play (15s each):**
   - Arena ring spins; players swipe to steer their bubble ghost.
   - Hold-to-boost drains charge; painting unclaimed sectors refills charge.
   - Colliding with hazards costs a life unless Overcharge is active.
3. **Round results:** Score = painted coverage × style multipliers (perfect arcs, bolt streaks).
4. **Meta rewards:** Earn sparks (soft currency) + aura shards (collectibles) → unlock trails, emotes, overworld routes.

## 4. Controls & Camera
- One-finger swipe/drag controls; horizontal movement mapped to tangential velocity, vertical to radial offset.
- Tap to trigger Overcharge (consumable) for temporary invulnerability + 2× paint.
- Camera locked to vertical portrait with slight shake when sectors complete.

## 5. Session Structure
- **Arcade run:** 3 rounds per session (≈60 seconds). Difficulty increases via rotation speed, rival aggressiveness, and hazard density.
- **Boss rings (Round 3):** Multi-layer arenas with shifting rotation directions for climax moments.
- **End of session:** Summary card optimized for screenshots (score, best streak, aura preview).

## 6. Progression & Meta Layers
- **Circuit Board map:** Node-based overworld; each completed arena unlocks branching paths and cosmetic drop chances.
- **Daily streaks:** 3 rotating objectives (e.g., “Perfect-paint 4 sectors”) granting Overcharge tokens + streak badges displayed on profile.
- **Collections:**
  - Aura Trails (4 rarity tiers)
  - Emotes (quick sticker bursts shown during idle moments)
  - Ghost Shells (premium skins planned for post-MVP)
- **Events:** Weekly "Voltage Rush" arena modifiers (double speed, inverted colors) to refresh UA creatives.

## 7. Monetization Design
- **Ads:**
  - Interstitial after each full session (frequency capping to avoid churn).
  - Rewarded video: revive, extra Overcharge, double sparks.
  - Playable ads for cross-promo slots.
- **IAP:**
  - Starter bundles (cosmetic + Overcharge tokens) at $2.99.
  - Specter Pass (28-day mini battle pass) with exclusive trails/emotes ($4.99, includes ad-free toggle for session interstitials only).
  - Booster packs (consumable Overcharge stacks) for ad-averse players.
- **Economy safeguards:** Soft currency used for path unlocks; premium currency (Plasma) only via IAP/events.

## 8. Content Roadmap (MVP → LiveOps)
- **MVP:** 6 arena themes, 12 aura trails, 4 emotes, 1 boss template, basic Circuit Board map.
- **Post-launch Phase 1:** Limited-time arenas tied to social challenges, leaderboard seasons, Specter Pass.
- **Phase 2:** Asynchronous PvP ghosts, user-generated challenge codes, brand integration skins.

## 9. Analytics & Telemetry
- Firebase + Appsflyer events:
  - `tutorial_complete`, `round_result`, `streak_progress`, `ad_view`, `iap_purchase`, `overcharge_use`.
- Funnels to monitor: Story/Reel clickthrough → install → tutorial completion, ad opt-in rate, Specter Pass conversion.

## 10. Tech & Pipeline
- **Engine:** Unity 2022 LTS (URP) for lightweight lighting + shader graph.
- **Systems:** ScriptableObject-driven arena configs, Addressables for live updates, Cinemachine for polish.
- **Tools:**
  - Art: Blender/Procreate for minimal assets.
  - Spark AR Studio for Instagram filter prototype.
  - Notion/Jira equivalent (TBD) for sprint tracking.

## 11. Marketing Alignment
- Capture-ready build (HUD-light) for influencers.
- Spark AR mini-game replicates Round 1 pacing to prime players.
- Weekly Reel brief: highlight perfect paint animation + Overcharge effect; CTA text pack included in build repo.

## 12. Open Questions
1. Difficulty curves for AI rivals (elastic vs fixed?).
2. Cross-progression with future multiplayer mode?
3. Need app/store localization at launch or post-KPIs?

---
_Next deliverable: visual mood/reference board + prototype task breakdown._
