using BubbleBolt.Config;
using BubbleBolt.Gameplay.Arena;
using UnityEngine;

namespace BubbleBolt.AI
{
    public class RivalSpiritController : MonoBehaviour
    {
        [SerializeField] private PrototypeTuning tuning;
        [SerializeField] private ArenaPainter arenaPainter;
        [SerializeField] private Transform arenaCenter;
        [SerializeField] private float orbitRadius = 0.82f;

        private float _theta;
        private float _angularSpeed;

        private void Reset()
        {
            arenaCenter = transform.parent;
        }

        private void Update()
        {
            if (tuning == null || arenaPainter == null || arenaCenter == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            float coverageDelta = arenaPainter.GetCoverage(SectorOwner.Player) - arenaPainter.GetCoverage(SectorOwner.Rival);
            float targetSpeed01 = coverageDelta > tuning.rivalryThreshold ? tuning.rivalAggressiveSpeed : tuning.rivalBaseSpeed;
            targetSpeed01 = Mathf.Clamp01(targetSpeed01);

            _angularSpeed = Mathf.MoveTowards(_angularSpeed, targetSpeed01 * tuning.omegaMax * 0.9f, dt * 2f);
            _theta += _angularSpeed * dt;

            Vector3 offset = new(Mathf.Cos(_theta) * orbitRadius, Mathf.Sin(_theta) * orbitRadius, 0f);
            transform.position = arenaCenter.position + offset;
            transform.up = offset.normalized;

            arenaPainter.Paint(SectorOwner.Rival, _theta, targetSpeed01, dt, false);
        }

        public void Configure(PrototypeTuning tuningAsset, ArenaPainter painter, Transform center)
        {
            tuning = tuningAsset;
            arenaPainter = painter;
            arenaCenter = center;
        }
    }
}
