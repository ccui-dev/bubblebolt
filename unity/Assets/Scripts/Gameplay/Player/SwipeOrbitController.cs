using BubbleBolt.Config;
using UnityEngine;

namespace BubbleBolt.Gameplay.Player
{
    [RequireComponent(typeof(PlayerOrbitMover))]
    public class SwipeOrbitController : MonoBehaviour
    {
        [SerializeField] private PrototypeTuning tuning;
        [SerializeField] private bool useMouseInEditor = true;

        private PlayerOrbitMover _mover;
        private Vector2 _smoothedDelta;
        private Vector2 _lastPointerPos;
        private bool _pointerActive;

        private void Awake()
        {
            _mover = GetComponent<PlayerOrbitMover>();
            if (tuning == null)
            {
                Debug.LogWarning("PrototypeTuning reference missing on SwipeOrbitController.", this);
            }
        }

        private void Update()
        {
            if (tuning == null || _mover == null)
            {
                return;
            }

            Vector2 rawDelta = SamplePointerDelta();
            float lerp = Mathf.Clamp01(Time.deltaTime * tuning.inputSmoothing);
            _smoothedDelta = Vector2.Lerp(_smoothedDelta, rawDelta, lerp);
            _mover.ApplyInput(_smoothedDelta, Time.deltaTime);
        }

        private Vector2 SamplePointerDelta()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    _pointerActive = true;
                    _lastPointerPos = touch.position;
                    return Vector2.zero;
                }

                if (!_pointerActive)
                {
                    _pointerActive = true;
                    _lastPointerPos = touch.position;
                    return Vector2.zero;
                }

                Vector2 delta = (touch.position - _lastPointerPos) / Screen.height;
                _lastPointerPos = touch.position;
                if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
                {
                    _pointerActive = false;
                }
                return delta;
            }

            if (useMouseInEditor && Input.GetMouseButton(0))
            {
                Vector2 pos = Input.mousePosition;
                if (!_pointerActive)
                {
                    _pointerActive = true;
                    _lastPointerPos = pos;
                    return Vector2.zero;
                }

                Vector2 delta = (pos - _lastPointerPos) / Screen.height;
                _lastPointerPos = pos;
                return delta;
            }

            _pointerActive = false;
            _lastPointerPos = Vector2.zero;
            return Vector2.zero;
        }
    }
}
