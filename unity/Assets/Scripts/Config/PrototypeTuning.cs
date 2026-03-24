using UnityEngine;

namespace BubbleBolt.Config
{
    [CreateAssetMenu(menuName = "BubbleBolt/Prototype Tuning", fileName = "PrototypeTuning")]
    public sealed class PrototypeTuning : ScriptableObject
    {
        [Header("Input")]
        [Tooltip("Lerp factor applied to raw swipe delta each frame.")]
        public float inputSmoothing = 12f;

        [Tooltip("Minimum normalized swipe delta before movement reacts.")]
        public float inputDeadZone = 0.01f;

        [Tooltip("Multiplier that converts input.x into angular acceleration.")]
        public float angularSensitivity = 5f;

        [Tooltip("Maximum absolute angular velocity (radians per second).")]
        public float omegaMax = 6f;

        [Tooltip("Base radius (in world units) the player tries to orbit at.")]
        public float targetRadius = 0.8f;

        [Tooltip("How far up/down swipe input can offset the radius.")]
        public float radialRange = 0.25f;

        [Tooltip("Lerp speed when easing toward the target radius.")]
        public float radiusLerpSpeed = 6f;

        [Tooltip("Amplitude of the cosmetic vertical drift added to the player bubble.")]
        public float driftAmplitude = 0.02f;

        [Tooltip("Frequency of the cosmetic drift (Hz).")]
        public float driftFrequency = 1.75f;

        [Header("Painting")]
        public int sectorCount = 24;

        [Tooltip("Base sector fill rate per second before multipliers.")]
        public float baseFillRate = 0.35f;

        [Tooltip("Maps normalized angular speed (0-1) to an additional multiplier.")]
        public AnimationCurve speedToPaintMultiplier = AnimationCurve.Linear(0f, 0.8f, 1f, 1.35f);

        [Tooltip("Extra score multiplier applied per combo step.")]
        public float comboMultiplierStep = 0.1f;

        [Tooltip("How many adjacent sectors must flip to trigger the perfect arc bonus.")]
        public int perfectArcThreshold = 4;

        [Tooltip("Percentage bonus (0.1 = +10%) granted for a perfect arc streak.")]
        public float perfectArcBonus = 0.1f;

        [Header("Lives & Damage")]
        public int maxLives = 2;

        public float invulnerabilitySeconds = 0.75f;

        [Header("Overcharge")]
        public float overchargeGainPerSector = 0.05f;

        public float overchargeDuration = 5f;

        public float overchargePaintMultiplier = 2f;

        [Header("Rival")]
        [Tooltip("Normalized speed (0-1 of player max) used during calm state.")]
        public float rivalBaseSpeed = 0.75f;

        [Tooltip("Normalized speed (0-1) used when player leads by a large margin.")]
        public float rivalAggressiveSpeed = 1.15f;

        [Tooltip("Coverage delta that forces the rival into steal/aggressive mode.")]
        public float rivalryThreshold = 0.2f;

        private void OnValidate()
        {
            sectorCount = Mathf.Max(6, sectorCount);
            perfectArcThreshold = Mathf.Max(2, perfectArcThreshold);
            maxLives = Mathf.Clamp(maxLives, 1, 5);
            comboMultiplierStep = Mathf.Max(0f, comboMultiplierStep);
            overchargeGainPerSector = Mathf.Max(0.001f, overchargeGainPerSector);
        }
    }
}
