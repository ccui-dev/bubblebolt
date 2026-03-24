using BubbleBolt.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace BubbleBolt.UI
{
    [RequireComponent(typeof(Button))]
    public class OverchargeButton : MonoBehaviour
    {
        [SerializeField] private PlayerPainter playerPainter;
        [SerializeField] private Image fillImage;
        [SerializeField] private Color chargingColor = new(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color readyColor = new(0.9f, 0.8f, 0.2f, 1f);

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(TriggerOvercharge);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(TriggerOvercharge);
        }

        private void Update()
        {
            if (playerPainter == null)
            {
                _button.interactable = false;
                return;
            }

            bool ready = playerPainter.OverchargeGauge >= 1f && !playerPainter.OverchargeActive;
            _button.interactable = ready;

            if (fillImage != null)
            {
                fillImage.fillAmount = Mathf.Clamp01(playerPainter.OverchargeGauge);
                fillImage.color = ready ? readyColor : chargingColor;
            }
        }

        private void TriggerOvercharge()
        {
            playerPainter?.TriggerOvercharge();
        }
    }
}
