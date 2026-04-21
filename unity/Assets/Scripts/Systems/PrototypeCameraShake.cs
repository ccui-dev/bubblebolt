using UnityEngine;

namespace BubbleBolt.Systems
{
    public sealed class PrototypeCameraShake : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 2f)] private float traumaDecay = 1.5f;
        [SerializeField, Range(0f, 0.5f)] private float maxTranslation = 0.08f;
        [SerializeField, Range(0f, 10f)] private float maxRotation = 3f;

        private float _trauma;
        private Vector3 _defaultLocalPosition;
        private Quaternion _defaultLocalRotation;

        private void Awake()
        {
            _defaultLocalPosition = transform.localPosition;
            _defaultLocalRotation = transform.localRotation;
        }

        public void AddShake(float intensity)
        {
            _trauma = Mathf.Clamp01(_trauma + Mathf.Abs(intensity));
        }

        private void Update()
        {
            if (_trauma <= 0f)
            {
                transform.localPosition = _defaultLocalPosition;
                transform.localRotation = _defaultLocalRotation;
                return;
            }

            float shake = _trauma * _trauma;
            Vector2 offset = Random.insideUnitCircle * maxTranslation * shake;
            float angle = Random.Range(-maxRotation, maxRotation) * shake;

            transform.localPosition = _defaultLocalPosition + new Vector3(offset.x, offset.y, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            _trauma = Mathf.Clamp01(_trauma - traumaDecay * Time.deltaTime);
        }
    }
}
