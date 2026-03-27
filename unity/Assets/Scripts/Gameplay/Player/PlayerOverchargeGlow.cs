using UnityEngine;

namespace BubbleBolt.Gameplay.Player
{
    [RequireComponent(typeof(Renderer))]
    public class PlayerOverchargeGlow : MonoBehaviour
    {
        [SerializeField] private PlayerPainter playerPainter;
        [SerializeField] private Color idleColor = new(0.8f, 0.95f, 1f, 1f);
        [SerializeField] private Color overchargeColor = new(1f, 0.65f, 0.2f, 1f);
        [SerializeField] private float pulseSpeed = 6f;

        private Renderer _renderer;
        private Material _materialInstance;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _materialInstance = _renderer.material;
                ApplyColor(idleColor);
            }
        }

        private void Update()
        {
            if (playerPainter == null || _materialInstance == null)
            {
                return;
            }

            float targetLerp = playerPainter.OverchargeActive
                ? (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f)
                : Mathf.Clamp01(playerPainter.OverchargeGauge);

            Color targetColor = Color.Lerp(idleColor, overchargeColor, targetLerp);
            ApplyColor(targetColor);
        }

        public void Configure(PlayerPainter painter, Color idle, Color overcharge)
        {
            playerPainter = painter;
            idleColor = idle;
            overchargeColor = overcharge;
            if (_materialInstance != null)
            {
                ApplyColor(idleColor);
            }
        }

        private void ApplyColor(Color value)
        {
            if (_materialInstance == null)
            {
                return;
            }

            _materialInstance.color = value;
        }
    }
}
