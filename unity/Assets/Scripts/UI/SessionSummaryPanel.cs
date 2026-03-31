using BubbleBolt.Gameplay.Arena;
using BubbleBolt.Gameplay.Player;
using BubbleBolt.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BubbleBolt.UI
{
    /// <summary>
    /// Shows an end-of-session overlay with score/coverage summary and lets players restart.
    /// </summary>
    public class SessionSummaryPanel : MonoBehaviour
    {
        [SerializeField] private PrototypeSessionManager sessionManager;
        [SerializeField] private PlayerPainter playerPainter;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI headlineLabel;
        [SerializeField] private TextMeshProUGUI detailLabel;
        [SerializeField] private Button replayButton;

        private float _lastPlayerCoverage;
        private float _lastRivalCoverage;

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
            PlayerPainter painter,
            GameObject panel,
            TextMeshProUGUI headline,
            TextMeshProUGUI detail,
            Button replay)
        {
            sessionManager = manager;
            playerPainter = painter;
            panelRoot = panel;
            headlineLabel = headline;
            detailLabel = detail;
            replayButton = replay;

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (replayButton != null)
            {
                replayButton.onClick.RemoveAllListeners();
                replayButton.onClick.AddListener(RestartSession);
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
            sessionManager.RoundCompletedEvent += HandleRoundCompleted;
            sessionManager.SessionCompletedEvent += HandleSessionCompleted;
        }

        private void Unbind()
        {
            if (sessionManager == null)
            {
                return;
            }

            sessionManager.RoundStartedEvent -= HandleRoundStarted;
            sessionManager.RoundCompletedEvent -= HandleRoundCompleted;
            sessionManager.SessionCompletedEvent -= HandleSessionCompleted;
        }

        private void HandleRoundStarted(int roundIndex)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void HandleRoundCompleted(int roundIndex, float playerCoverage, float rivalCoverage, SectorOwner winner)
        {
            _lastPlayerCoverage = playerCoverage;
            _lastRivalCoverage = rivalCoverage;
        }

        private void HandleSessionCompleted(int playerWins, int rivalWins)
        {
            if (panelRoot == null)
            {
                return;
            }

            UpdateSummaryText(playerWins, rivalWins);
            panelRoot.SetActive(true);
        }

        private void UpdateSummaryText(int playerWins, int rivalWins)
        {
            if (headlineLabel != null)
            {
                if (playerWins > rivalWins)
                {
                    headlineLabel.text = "Victory";
                }
                else if (playerWins < rivalWins)
                {
                    headlineLabel.text = "Defeat";
                }
                else
                {
                    headlineLabel.text = "Tie Game";
                }
            }

            if (detailLabel != null)
            {
                float score = playerPainter != null ? playerPainter.Score : 0f;
                detailLabel.text = string.Format(
                    "Score {0}\nCoverage {1:0}% vs {2:0}%\nWins {3}-{4}",
                    Mathf.RoundToInt(score),
                    _lastPlayerCoverage * 100f,
                    _lastRivalCoverage * 100f,
                    playerWins,
                    rivalWins);
            }
        }

        private void RestartSession()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            sessionManager?.BeginSession();
        }
    }
}
