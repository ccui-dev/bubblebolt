using UnityEngine;

namespace BubbleBolt.Gameplay.Player
{
    [RequireComponent(typeof(Renderer))]
    public class PlayerOverchargeGlow : MonoBehaviour
    {
        [SerializeField] private PlayerPainter playerPainter;
        [SerializeField] private Color idleColor = new Color(0.8f, 0.95f, 1f, 1f);
        [SerializeField] private Color overchargeColor = new Color(1f, 0.65f, 0.2f, 1f);
        [SerializeField] private float pulseSpeed = 6f;
        [SerializeField] private Color damageFlashColor = Color.white;
        [SerializeField, Range(0.05f, 0.5f)] private float damageFlashDuration = 0.25f;

        private Renderer _renderer;
        private Material _materialInstance;
        private float _damageFlashTimer;
        private bool _listening;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _materialInstance = _renderer.material;
                ApplyColor(idleColor);
            }
        }

        private void OnEnable()
        {
            AttachListeners();
        }

        private void OnDisable()
        {
            DetachListeners();
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

            if (_damageFlashTimer > 0f)
            {
                _damageFlashTimer = Mathf.Max(0f, _damageFlashTimer - Time.deltaTime);
                float flashT = Mathf.Clamp01(_damageFlashTimer / damageFlashDuration);
                targetColor = Color.Lerp(targetColor, damageFlashColor, flashT);
            }

            ApplyColor(targetColor);
        }

        public void Configure(PlayerPainter painter, Color idle, Color overcharge)
        {
            if (playerPainter != painter)
            {
                DetachListeners();
                playerPainter = painter;
                AttachListeners();
            }
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


        private void AttachListeners()
        {
            if (playerPainter != null && !_listening)
            {
                playerPainter.DamageTaken += HandleDamageTaken;
                _listening = true;
            }
        }

        private void DetachListeners()
        {
            if (playerPainter != null && _listening)
            {
                playerPainter.DamageTaken -= HandleDamageTaken;
                _listening = false;
            }
        }

        private void HandleDamageTaken(int livesRemaining)
        {
            _damageFlashTimer = damageFlashDuration;
        }

    }
}
