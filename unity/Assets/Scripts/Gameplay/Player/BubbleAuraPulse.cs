using UnityEngine;

namespace BubbleBolt.Gameplay.Player
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class BubbleAuraPulse : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField, Range(0.05f, 0.5f)] private float radius = 0.15f;
        [SerializeField, Range(8, 128)] private int segments = 48;
        [SerializeField, Range(0f, 1f)] private float pulseAmount = 0.2f;
        [SerializeField, Range(0.1f, 10f)] private float pulseSpeed = 2.5f;
        [SerializeField, Range(0.005f, 0.1f)] private float baseWidth = 0.02f;
        [SerializeField] private Gradient gradient;

        private float _angleStep;
        private float _widthAtRest;
        private Material _material;

        private void Awake()
        {
            lineRenderer ??= GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                enabled = false;
                return;
            }

            _widthAtRest = baseWidth;
            SetupRenderer();
            BuildRing();
        }

        private void Update()
        {
            if (lineRenderer == null)
            {
                return;
            }

            float pulse = (Mathf.Sin(Time.time * Mathf.PI * 2f * pulseSpeed) + 1f) * 0.5f;
            float width = Mathf.Lerp(_widthAtRest * (1f - pulseAmount), _widthAtRest * (1f + pulseAmount), pulse);
            lineRenderer.widthMultiplier = width;
        }

        public void Configure(float targetRadius, Gradient colorGradient, float width = 0.02f)
        {
            radius = targetRadius;
            gradient = colorGradient;
            baseWidth = width;
            _widthAtRest = width;
            SetupRenderer();
            BuildRing();
        }

        private void SetupRenderer()
        {
            if (lineRenderer == null)
            {
                return;
            }

            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = false;
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.widthMultiplier = _widthAtRest;

            if (gradient != null)
            {
                lineRenderer.colorGradient = gradient;
            }

            if (_material == null)
            {
                _material = new Material(Shader.Find("Sprites/Default"));
            }
            lineRenderer.material = _material;
        }

        private void BuildRing()
        {
            if (lineRenderer == null)
            {
                return;
            }

            segments = Mathf.Max(8, segments);
            lineRenderer.positionCount = segments;
            _angleStep = Mathf.PI * 2f / segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * _angleStep;
                Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
                lineRenderer.SetPosition(i, pos);
            }
        }
    }
}
