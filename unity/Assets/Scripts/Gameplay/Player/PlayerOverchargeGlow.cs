using UnityEngine;

namespace BubbleBolt.Gameplay.Player
{
    [RequireComponent(typeof(Renderer))]
    public class PlayerOverchargeGlow : MonoBehaviour
    {
        [SerializeField] private PlayerPainter playerPainter;
        [SerializeField] private Transform pulseTarget;
        [SerializeField] private Color idleColor = new Color(0.8f, 0.95f, 1f, 1f);
        [SerializeField] private Color overchargeColor = new Color(1f, 0.65f, 0.2f, 1f);
        [SerializeField] private float pulseSpeed = 6f;
        [SerializeField] private Color damageFlashColor = Color.white;
        [SerializeField, Range(0.05f, 0.5f)] private float damageFlashDuration = 0.25f;
        [SerializeField, Range(1f, 2f)] private float overchargeScaleMultiplier = 1.25f;
        [SerializeField, Range(0f, 1.5f)] private float idleEmission = 0.18f;
        [SerializeField, Range(0f, 2f)] private float overchargeEmission = 1f;
        [SerializeField, Range(0.05f, 0.6f)] private float overchargeBurstDuration = 0.25f;
        [SerializeField, Range(0.05f, 0.6f)] private float damageScaleKick = 0.2f;
        [SerializeField, Range(1f, 20f)] private float scaleLerpSpeed = 10f;

        private Renderer _renderer;
        private Material _materialInstance;
        private float _damageFlashTimer;
        private float _overchargeBurstTimer;
        private bool _listening;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _materialInstance = _renderer.material;
                EnableEmission();
                ApplyColor(idleColor, idleEmission);
            }

            if (pulseTarget == null)
            {
                pulseTarget = transform;
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

            float overchargeT = playerPainter.OverchargeActive
                ? (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f)
                : Mathf.Clamp01(playerPainter.OverchargeGauge);

            Color targetColor = Color.Lerp(idleColor, overchargeColor, overchargeT);
            float emission = Mathf.Lerp(idleEmission, overchargeEmission, playerPainter.OverchargeActive ? 1f : playerPainter.OverchargeGauge);

            if (_damageFlashTimer > 0f)
            {
                _damageFlashTimer = Mathf.Max(0f, _damageFlashTimer - Time.deltaTime);
                float flashT = Mathf.Clamp01(_damageFlashTimer / damageFlashDuration);
                targetColor = Color.Lerp(targetColor, damageFlashColor, flashT);
            }

            if (_overchargeBurstTimer > 0f)
            {
                _overchargeBurstTimer = Mathf.Max(0f, _overchargeBurstTimer - Time.deltaTime);
            }

            float burstT = overchargeBurstDuration <= 0f ? 0f : Mathf.Clamp01(_overchargeBurstTimer / overchargeBurstDuration);
            emission = Mathf.Lerp(emission, overchargeEmission * 1.3f, burstT);

            ApplyColor(targetColor, emission);
            UpdateScale(burstT);
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
                ApplyColor(idleColor, idleEmission);
            }
        }

        private void ApplyColor(Color value, float emissionMultiplier)
        {
            if (_materialInstance == null)
            {
                return;
            }

            _materialInstance.color = value;
            _materialInstance.SetColor("_EmissionColor", value * emissionMultiplier);
        }

        private void UpdateScale(float burstT)
        {
            if (pulseTarget == null)
            {
                return;
            }

            float targetScale = playerPainter != null && playerPainter.OverchargeActive ? overchargeScaleMultiplier : 1f;
            if (_damageFlashTimer > 0f)
            {
                float flashT = Mathf.Clamp01(_damageFlashTimer / damageFlashDuration);
                targetScale += damageScaleKick * flashT;
            }

            targetScale += burstT * 0.35f;
            float current = Mathf.Lerp(pulseTarget.localScale.x, targetScale, Time.deltaTime * scaleLerpSpeed);
            pulseTarget.localScale = Vector3.one * Mathf.Max(0.01f, current);
        }

        private void EnableEmission()
        {
            if (_materialInstance == null)
            {
                return;
            }

            _materialInstance.EnableKeyword("_EMISSION");
        }

        private void AttachListeners()
        {
            if (playerPainter != null && !_listening)
            {
                playerPainter.DamageTaken += HandleDamageTaken;
                playerPainter.OverchargeTriggered += HandleOverchargeTriggered;
                _listening = true;
            }
        }

        private void DetachListeners()
        {
            if (playerPainter != null && _listening)
            {
                playerPainter.DamageTaken -= HandleDamageTaken;
                playerPainter.OverchargeTriggered -= HandleOverchargeTriggered;
                _listening = false;
            }
        }

        private void HandleDamageTaken(int livesRemaining)
        {
            _damageFlashTimer = damageFlashDuration;
        }

        private void HandleOverchargeTriggered()
        {
            _overchargeBurstTimer = overchargeBurstDuration;
        }
    }
}
