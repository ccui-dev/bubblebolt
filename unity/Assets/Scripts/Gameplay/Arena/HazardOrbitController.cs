using UnityEngine;

namespace BubbleBolt.Gameplay.Arena
{
    /// <summary>
    /// Simple runtime controller that keeps hazards orbiting around the arena so the ring never feels static.
    /// </summary>
    public sealed class HazardOrbitController : MonoBehaviour
    {
        [SerializeField, Tooltip("Degrees per second applied once targets are reached.")]
        private float baseSpeed = 18f;

        [SerializeField, Tooltip("Additional speed that can be added/subtracted when retargeting.")]
        private float variance = 12f;

        [SerializeField, Tooltip("Range for how frequently we pick a new speed/direction (seconds).")]
        private Vector2 retargetInterval = new Vector2(3.5f, 6f);

        [SerializeField, Tooltip("How quickly the current speed lerps toward the requested speed (deg/sec^2).")]
        private float acceleration = 40f;

        [SerializeField] private bool randomizeStartDirection = true;

        private float _currentSpeed;
        private float _targetSpeed;
        private float _timer;
        private bool _initialized;

        public void Configure(float newBaseSpeed, float newVariance, Vector2 interval, float accel, bool randomizeDirection)
        {
            baseSpeed = Mathf.Max(0f, newBaseSpeed);
            variance = Mathf.Max(0f, newVariance);
            retargetInterval = new Vector2(Mathf.Max(0.1f, Mathf.Min(interval.x, interval.y)), Mathf.Max(0.11f, Mathf.Max(interval.x, interval.y)));
            acceleration = Mathf.Max(0.1f, accel);
            randomizeStartDirection = randomizeDirection;
            _initialized = true;
            ResetState(force: true);
        }

        private void OnEnable()
        {
            ResetState(force: false);
        }

        private void ResetState(bool force)
        {
            if (!_initialized && !force)
            {
                return;
            }

            float direction = randomizeStartDirection && Random.value > 0.5f ? -1f : 1f;
            _currentSpeed = direction * baseSpeed;
            _targetSpeed = _currentSpeed;
            ScheduleNextRetarget();
        }

        private void ScheduleNextRetarget()
        {
            _timer = Random.Range(retargetInterval.x, retargetInterval.y);
            float direction = Random.value > 0.5f ? 1f : -1f;
            float offset = Random.Range(-variance, variance);
            _targetSpeed = direction * Mathf.Max(0f, baseSpeed + offset);
        }

        private void Update()
        {
            if (!_initialized)
            {
                return;
            }

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                ScheduleNextRetarget();
            }

            _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, acceleration * Time.deltaTime);
            transform.Rotate(0f, 0f, _currentSpeed * Time.deltaTime, Space.Self);
        }
    }
}
