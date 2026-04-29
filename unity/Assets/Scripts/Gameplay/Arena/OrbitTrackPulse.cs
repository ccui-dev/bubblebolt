using UnityEngine;

namespace BubbleBolt.Gameplay.Arena
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class OrbitTrackPulse : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Gradient gradient;
        [SerializeField, Range(0.005f, 0.1f)] private float baseWidth = 0.02f;
        [SerializeField, Range(0f, 1f)] private float pulseAmount = 0.25f;
        [SerializeField, Range(0.1f, 6f)] private float pulseSpeed = 1.5f;

        private Material _material;

        public void Configure(LineRenderer renderer, Gradient colorGradient, float width, float pulse)
        {
            lineRenderer = renderer;
            gradient = colorGradient;
            baseWidth = width;
            pulseAmount = pulse;
            Setup();
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
            lineRenderer.useWorldSpace = false;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.widthMultiplier = baseWidth;
            if (gradient != null)
            {
                lineRenderer.colorGradient = gradient;
            }
        }
    }
}
