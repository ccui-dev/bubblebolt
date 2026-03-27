using BubbleBolt.Config;
using UnityEngine;

namespace BubbleBolt.Gameplay.Player
{
    [RequireComponent(typeof(Transform))]
    public class PlayerOrbitMover : MonoBehaviour
    {
        [SerializeField] private PrototypeTuning tuning;
        [SerializeField] private Transform arenaCenter;

        private float _theta; // radians
        private float _angularVelocity; // radians per second
        private float _currentRadius;
        private float _targetRadius;

        public float AngleRadians => _theta;
        public float AngularVelocity => _angularVelocity;
        public float CurrentRadius => _currentRadius;
        public float NormalizedSpeed => Mathf.Approximately(tuning.omegaMax, 0f)
            ? 0f
            : Mathf.Clamp01(Mathf.Abs(_angularVelocity) / tuning.omegaMax);

        private void Reset()
        {
            arenaCenter = transform.parent;
        }

        private void Awake()
        {
            if (arenaCenter == null)
            {
                arenaCenter = new GameObject("ArenaCenter").transform;
                arenaCenter.position = Vector3.zero;
            }

            InitializeState();
        }

        public void Configure(PrototypeTuning tuningAsset, Transform arenaCenterTransform)
        {
            tuning = tuningAsset;
            arenaCenter = arenaCenterTransform;
            InitializeState();
        }

        private void InitializeState()
        {
            if (tuning == null)
            {
                return;
            }

            _theta = 0f;
            _targetRadius = tuning.targetRadius;
            _currentRadius = _targetRadius;
        }

        public void ApplyInput(Vector2 smoothedDelta, float deltaTime)
        {
            if (tuning == null)
            {
                return;
            }

            float sqrDeadZone = tuning.inputDeadZone * tuning.inputDeadZone;
            if (smoothedDelta.sqrMagnitude < sqrDeadZone)
            {
                smoothedDelta = Vector2.zero;
            }

            _angularVelocity = Mathf.Clamp(
                _angularVelocity + smoothedDelta.x * tuning.angularSensitivity,
                -tuning.omegaMax,
                tuning.omegaMax);

            float desired = tuning.targetRadius + smoothedDelta.y * tuning.radialRange;
            float minRadius = tuning.targetRadius - tuning.radialRange;
            float maxRadius = tuning.targetRadius + tuning.radialRange;
            _targetRadius = Mathf.Clamp(desired, minRadius, maxRadius);
        }

        private void Update()
        {
            if (tuning == null || arenaCenter == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            _theta += _angularVelocity * dt;
            _currentRadius = Mathf.Lerp(_currentRadius, _targetRadius, dt * tuning.radiusLerpSpeed);

            Vector3 polar = PolarToCartesian(_currentRadius, _theta);
            float drift = Mathf.Sin(Time.time * tuning.driftFrequency) * tuning.driftAmplitude;
            Vector3 driftOffset = Vector3.forward * 0f + Vector3.up * drift;

            transform.position = arenaCenter.position + polar + driftOffset;
            transform.up = polar.normalized;
        }

        private static Vector3 PolarToCartesian(float radius, float theta)
        {
            return new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0f) * radius;
        }
    }
}
