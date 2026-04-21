using BubbleBolt.Systems;
using UnityEngine;

namespace BubbleBolt.Gameplay.Player
{
    public sealed class PlayerFeedbackController : MonoBehaviour
    {
        [SerializeField] private PlayerPainter playerPainter;
        [SerializeField] private PrototypeTonePlayer tonePlayer;
        [SerializeField] private PrototypeCameraShake cameraShake;
        [SerializeField] private float overchargeToneFrequency = 880f;
        [SerializeField] private float overchargeToneDuration = 0.22f;
        [SerializeField] private float damageToneFrequency = 220f;
        [SerializeField] private float damageToneDuration = 0.28f;
        [SerializeField, Range(0f, 1f)] private float overchargeShake = 0.12f;
        [SerializeField, Range(0f, 1f)] private float damageShake = 0.35f;

        private void Awake()
        {
            if (playerPainter == null)
            {
                playerPainter = GetComponent<PlayerPainter>();
            }
        }

        private void OnEnable()
        {
            Attach();
        }

        private void OnDisable()
        {
            Detach();
        }

        public void Configure(PlayerPainter painter, PrototypeTonePlayer tone, PrototypeCameraShake shake)
        {
            if (playerPainter != painter)
            {
                Detach();
                playerPainter = painter;
                Attach();
            }

            tonePlayer = tone;
            cameraShake = shake;
        }

        private void Attach()
        {
            if (playerPainter != null)
            {
                playerPainter.OverchargeTriggered += HandleOvercharge;
                playerPainter.DamageTaken += HandleDamage;
            }
        }

        private void Detach()
        {
            if (playerPainter != null)
            {
                playerPainter.OverchargeTriggered -= HandleOvercharge;
                playerPainter.DamageTaken -= HandleDamage;
            }
        }

        private void HandleOvercharge()
        {
            tonePlayer?.PlayTone(overchargeToneFrequency, overchargeToneDuration, 0.9f);
            cameraShake?.AddShake(overchargeShake);
        }

        private void HandleDamage(int livesRemaining)
        {
            tonePlayer?.PlayTone(damageToneFrequency, damageToneDuration, 1f);
            cameraShake?.AddShake(damageShake);
        }
    }
}
