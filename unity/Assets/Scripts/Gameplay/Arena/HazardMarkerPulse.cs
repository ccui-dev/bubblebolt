using UnityEngine;

namespace BubbleBolt.Gameplay.Arena
{
    /// <summary>
    /// Lightweight visual feedback for hazards – pulses scale/color so danger is readable.
    /// </summary>
    public sealed class HazardMarkerPulse : MonoBehaviour
    {
        [SerializeField] private Renderer markerRenderer;
        [SerializeField] private Color idleColor = new Color(1f, 0.45f, 0.45f, 1f);
        [SerializeField] private Color pulseColor = new Color(1f, 0.9f, 0.35f, 1f);
        [SerializeField, Range(0.1f, 10f)] private float pulseSpeed = 2.5f;
        [SerializeField, Range(0f, 0.6f)] private float scaleAmount = 0.2f;
        [SerializeField, Range(0f, 1f)] private float impactFlashBoost = 0.35f;
        [SerializeField, Range(0.05f, 0.6f)] private float impactFlashDuration = 0.2f;

        private Vector3 _baseScale;
        private float _phaseOffset;
        private bool _configured;
        private Material _runtimeMaterial;
        private float _impactTimer;

        private void Awake()
        {
            markerRenderer ??= GetComponent<Renderer>();
            CacheMaterial();
            _baseScale = transform.localScale;
            _phaseOffset = Random.value * Mathf.PI * 2f;
            _configured = true;
            Apply(0f);
        }

        private void OnEnable()
        {
            Apply(0f);
        }

        public void Initialize(Renderer renderer, Color baseColor, Color warningColor, float speed, float scale)
        {
            markerRenderer = renderer;
            CacheMaterial();
            idleColor = baseColor;
            pulseColor = warningColor;
            pulseSpeed = Mathf.Max(0.1f, speed);
            scaleAmount = Mathf.Clamp01(scale);
            _configured = true;
            _baseScale = transform.localScale;
            _phaseOffset = Random.value * Mathf.PI * 2f;
            Apply(0f);
        }

        public void TriggerImpactFlash()
        {
            _impactTimer = impactFlashDuration;
        }

        private void Update()
        {
            if (!_configured)
            {
                return;
            }

            float pulse = (Mathf.Sin(_phaseOffset + Time.time * Mathf.PI * 2f * pulseSpeed) + 1f) * 0.5f;
            Apply(pulse);
        }

        private void Apply(float t)
        {
            float colorT = t;
            if (_impactTimer > 0f)
            {
                _impactTimer = Mathf.Max(0f, _impactTimer - Time.deltaTime);
                float impact = Mathf.Clamp01(_impactTimer / impactFlashDuration);
                colorT = Mathf.Clamp01(colorT + impact * impactFlashBoost);
            }

            if (_runtimeMaterial != null)
            {
                _runtimeMaterial.color = Color.Lerp(idleColor, pulseColor, colorT);
            }

            float scale = 1f + (t - 0.5f) * 2f * scaleAmount;
            scale += Mathf.Clamp01(_impactTimer / Mathf.Max(0.0001f, impactFlashDuration)) * scaleAmount;
            transform.localScale = _baseScale * Mathf.Max(0.1f, scale);
        }

        private void CacheMaterial()
        {
            if (markerRenderer == null)
            {
                return;
            }

            _runtimeMaterial = markerRenderer.material;
        }
    }
}
