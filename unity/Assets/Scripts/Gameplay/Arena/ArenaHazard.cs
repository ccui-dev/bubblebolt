using System;
using BubbleBolt.Gameplay.Player;
using UnityEngine;
using UnityEngine.Events;

namespace BubbleBolt.Gameplay.Arena
{
    public class ArenaHazard : MonoBehaviour
    {
        [SerializeField] private PlayerOrbitMover orbitMover;
        [SerializeField] private PlayerPainter playerPainter;
        [SerializeField, Tooltip("World-space radius where the hazard lives.")]
        private float radius = 0.8f;
        [SerializeField, Tooltip("Half-width of the hazard arc in radians.")]
        private float angularHalfExtent = 0.2f;
        [SerializeField] private float radiusTolerance = 0.07f;
        [SerializeField] private float damageCooldown = 0.75f;
        [SerializeField] private UnityEvent onHit;

        private float _cooldown;

        public event Action Hit;

        public float Radius => radius;
        public float AngularHalfExtent => angularHalfExtent;

        private void Reset()
        {
            orbitMover ??= FindObjectOfType<PlayerOrbitMover>();
            playerPainter ??= FindObjectOfType<PlayerPainter>();
        }

        private void Update()
        {
            if (orbitMover == null || playerPainter == null)
            {
                return;
            }

            if (_cooldown > 0f)
            {
                _cooldown -= Time.deltaTime;
            }

            if (_cooldown > 0f || playerPainter.OverchargeActive)
            {
                return;
            }

            if (!IsPlayerWithinArc())
            {
                return;
            }

            playerPainter.RegisterHit();
            onHit?.Invoke();
            Hit?.Invoke();
            _cooldown = damageCooldown;
        }

        private bool IsPlayerWithinArc()
        {
            float playerRadius = orbitMover.CurrentRadius;
            if (Mathf.Abs(playerRadius - radius) > radiusTolerance)
            {
                return false;
            }

            float hazardAngle = transform.eulerAngles.z * Mathf.Deg2Rad;
            float delta = Mathf.DeltaAngle(Mathf.Rad2Deg * hazardAngle, Mathf.Rad2Deg * orbitMover.AngleRadians);
            return Mathf.Abs(delta * Mathf.Deg2Rad) <= angularHalfExtent;
        }

        public void Configure(PlayerOrbitMover mover, PlayerPainter painter, float hazardRadius, float angularHalfExtentRadians, float toleranceOverride = -1f)
        {
            orbitMover = mover;
            playerPainter = painter;
            radius = hazardRadius;
            angularHalfExtent = angularHalfExtentRadians;
            if (toleranceOverride >= 0f)
            {
                radiusTolerance = toleranceOverride;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 center = transform.position;
            float startAngle = transform.eulerAngles.z - Mathf.Rad2Deg * angularHalfExtent;
            float endAngle = transform.eulerAngles.z + Mathf.Rad2Deg * angularHalfExtent;

            Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.4f);
            DrawArc(center, radius, startAngle, endAngle);
        }

        private static void DrawArc(Vector3 center, float rad, float startDeg, float endDeg, int segments = 24)
        {
            Vector3 prev = center + new Vector3(Mathf.Cos(startDeg * Mathf.Deg2Rad), Mathf.Sin(startDeg * Mathf.Deg2Rad), 0f) * rad;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = Mathf.Lerp(startDeg, endDeg, t) * Mathf.Deg2Rad;
                Vector3 next = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * rad;
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
#endif
    }
}
