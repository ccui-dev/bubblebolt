using BubbleBolt.Gameplay.Arena;
using BubbleBolt.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BubbleBolt.UI
{
    /// <summary>
    /// Lightweight HUD bridge that reflects PrototypeSessionManager round/timer state.
    /// </summary>
    public class SessionStatusHud : MonoBehaviour
    {
        [SerializeField] private PrototypeSessionManager sessionManager;
        [SerializeField] private TextMeshProUGUI roundLabel;
        [SerializeField] private Slider roundTimerSlider;
        [SerializeField] private TextMeshProUGUI roundTimerLabel;
        [SerializeField] private TextMeshProUGUI sessionScoreLabel;

        private int _playerWins;
        private int _rivalWins;
        private bool _sessionFinished;

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
            TextMeshProUGUI roundText,
            Slider timerSlider,
            TextMeshProUGUI timerText,
            TextMeshProUGUI scoreText)
        {
            sessionManager = manager;
            roundLabel = roundText;
            roundTimerSlider = timerSlider;
            roundTimerLabel = timerText;
            sessionScoreLabel = scoreText;
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
                UpdateRoundLabel(0);
                UpdateTimer(0f);
                UpdateScoreLabel();
                return;
            }

            sessionManager.RoundStartedEvent += HandleRoundStarted;
            sessionManager.RoundTimerEvent += HandleRoundTimer;
            sessionManager.RoundCompletedEvent += HandleRoundCompleted;
            sessionManager.SessionCompletedEvent += HandleSessionCompleted;

            _playerWins = 0;
            _rivalWins = 0;
            _sessionFinished = false;
            UpdateRoundLabel(0);
            UpdateTimer(0f);
            UpdateScoreLabel();
        }

        private void Unbind()
        {
            if (sessionManager == null)
            {
                return;
            }

            sessionManager.RoundStartedEvent -= HandleRoundStarted;
            sessionManager.RoundTimerEvent -= HandleRoundTimer;
            sessionManager.RoundCompletedEvent -= HandleRoundCompleted;
            sessionManager.SessionCompletedEvent -= HandleSessionCompleted;
        }

        private void HandleRoundStarted(int roundIndex)
        {
            _sessionFinished = false;
            UpdateRoundLabel(roundIndex);
            UpdateTimer(0f);
        }

        private void HandleRoundTimer(int roundIndex, float normalized)
        {
            UpdateTimer(normalized);
        }

        private void HandleRoundCompleted(int roundIndex, float playerCoverage, float rivalCoverage, SectorOwner winner)
        {
            if (winner == SectorOwner.Player)
            {
                _playerWins++;
            }
            else if (winner == SectorOwner.Rival)
            {
                _rivalWins++;
            }

            UpdateScoreLabel();
        }

        private void HandleSessionCompleted(int playerWins, int rivalWins)
        {
            _playerWins = playerWins;
            _rivalWins = rivalWins;
            _sessionFinished = true;
            UpdateScoreLabel();
        }

        private void UpdateRoundLabel(int roundIndex)
        {
            if (roundLabel == null)
            {
                return;
            }

            if (sessionManager == null || roundIndex <= 0)
            {
                roundLabel.text = string.Empty;
                return;
            }

            roundLabel.text = $"Round {roundIndex}/{sessionManager.TotalRounds}";
        }

        private void UpdateTimer(float normalized)
        {
            if (roundTimerSlider != null)
            {
                roundTimerSlider.value = Mathf.Clamp01(normalized);
            }

            if (roundTimerLabel != null && sessionManager != null)
            {
                float remaining = Mathf.Max(0f, sessionManager.RoundDurationSeconds * (1f - Mathf.Clamp01(normalized)));
                int seconds = Mathf.CeilToInt(remaining);
                roundTimerLabel.text = $"{seconds:00}s";
            }
        }

        private void UpdateScoreLabel()
        {
            if (sessionScoreLabel == null)
            {
                return;
            }

            if (_sessionFinished)
            {
                if (_playerWins == _rivalWins)
                {
                    sessionScoreLabel.text = $"Tie { _playerWins }-{ _rivalWins }";
                }
                else if (_playerWins > _rivalWins)
                {
                    sessionScoreLabel.text = $"Player wins ({_playerWins}-{_rivalWins})";
                }
                else
                {
                    sessionScoreLabel.text = $"Rival wins ({_playerWins}-{_rivalWins})";
                }
            }
            else
            {
                sessionScoreLabel.text = $"Wins {_playerWins}-{_rivalWins}";
            }
        }
    }
}
