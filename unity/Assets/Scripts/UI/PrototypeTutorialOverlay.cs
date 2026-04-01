using BubbleBolt.Gameplay.Player;
using BubbleBolt.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BubbleBolt.UI
{
    /// <summary>
    /// Shows a simple onboarding overlay on Round 1 and gates player input until dismissed.
    /// </summary>
    public class PrototypeTutorialOverlay : MonoBehaviour
    {
        [SerializeField] private PrototypeSessionManager sessionManager;
        [SerializeField] private SwipeOrbitController swipeController;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI bodyLabel;
        [SerializeField] private Button continueButton;

        private bool _tutorialShown;

        private void Awake()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
        }

        public void Configure(
            PrototypeSessionManager manager,
            SwipeOrbitController controller,
            GameObject panel,
            TextMeshProUGUI label,
            Button advanceButton)
        {
            sessionManager = manager;
            swipeController = controller;
            panelRoot = panel;
            bodyLabel = label;
            continueButton = advanceButton;

            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(DismissTutorial);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            Bind();
        }

        private void Bind()
        {
            Unbind();

            if (sessionManager == null)
            {
                sessionManager = FindObjectOfType<PrototypeSessionManager>();
            }

            if (sessionManager == null)
            {
                return;
            }

            sessionManager.RoundStartedEvent += HandleRoundStarted;
        }

        private void Unbind()
        {
            if (sessionManager == null)
            {
                return;
            }

            sessionManager.RoundStartedEvent -= HandleRoundStarted;
        }

        private void HandleRoundStarted(int roundIndex)
        {
            if (_tutorialShown || roundIndex != 1)
            {
                return;
            }

            _tutorialShown = true;
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            swipeController?.SetInputEnabled(false);
        }

        private void DismissTutorial()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            swipeController?.SetInputEnabled(true);
        }
    }
}
