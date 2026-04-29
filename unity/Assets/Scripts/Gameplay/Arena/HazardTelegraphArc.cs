using UnityEngine;

namespace BubbleBolt.Gameplay.Arena
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class HazardTelegraphArc : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField, Range(0.02f, 0.2f)] private float baseWidth = 0.04f;
        [SerializeField, Range(0f, 1f)] private float pulseAmount = 0.25f;
        [SerializeField, Range(0.1f, 6f)] private float pulseSpeed = 1.6f;
        [SerializeField, Range(8, 128)] private int segments = 48;
        [SerializeField] private Gradient gradient;
        [SerializeField, Range(0.05f, 0.6f)] private float flashDuration = 0.25f;
        [SerializeField] private Color flashColor = Color.white;

        private float _radius = 0.8f;
        private float _arcRadians = 0.4f;
        private float _flashTimer;
        private Material _material;

        public void Configure(float radius, float arcRadians, Gradient colorGradient, float width, float pulse)
        {
            _radius = radius;
            _arcRadians = arcRadians;
            gradient = colorGradient;
            baseWidth = width;
            pulseSpeed = pulse;
            Setup();
        }

        public void TriggerFlash()
        {
            _flashTimer = flashDuration;
        }

        private void Awake()
        {
            Setup();
        }

        private void Update()
        {
            if (lineRenderer == null)
            {
                return;
            }

            float t = (Mathf.Sin(Time.time * Mathf.PI * 2f * pulseSpeed) + 1f) * 0.5f;
            float width = Mathf.Lerp(baseWidth * (1f - pulseAmount), baseWidth * (1f + pulseAmount), t);
            lineRenderer.widthMultiplier = width;

            if (_flashTimer > 0f && _material != null)
            {
                _flashTimer = Mathf.Max(0f, _flashTimer - Time.deltaTime);
                float flashT = Mathf.Clamp01(_flashTimer / flashDuration);
                _material.color = Color.Lerp(Color.white, flashColor, flashT);
            }
            else if (_material != null)
            {
                _material.color = Color.white;
            }
        }

        private void Setup()
        {
            lineRenderer ??= GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                return;
            }

            if (_material == null)
            {
                _material = new Material(Shader.Find("Sprites/Default"));
            }

            lineRenderer.material = _material;
            lineRenderer.positionCount = Mathf.Max(8, segments + 1);
            lineRenderer.useWorldSpace = false;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.loop = false;

            if (gradient != null)
            {
                lineRenderer.colorGradient = gradient;
            }

            BuildArc();
        }

        private void BuildArc()
        {
            if (lineRenderer == null)
            {
                return;
            }

            int points = Mathf.Max(8, segments);
            lineRenderer.positionCount = points;
            float start = -_arcRadians;
            float end = _arcRadians;
            for (int i = 0; i < points; i++)
            {
                float t = i / (float)(points - 1);
                float angle = Mathf.Lerp(start, end, t);
                Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * _radius;
                lineRenderer.SetPosition(i, pos);
            }
        }
    }
}
