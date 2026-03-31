using System;
using System.Collections;
using BubbleBolt.AI;
using BubbleBolt.Gameplay.Arena;
using BubbleBolt.Gameplay.Player;
using UnityEngine;
using UnityEngine.Events;

namespace BubbleBolt.Systems
{
    /// <summary>
    /// Orchestrates a lightweight best-of-N round flow for the Bubble Bolt prototype.
    /// Resets coverage, player state, and rival drift between rounds and raises events for UI.
    /// </summary>
    public class PrototypeSessionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ArenaPainter arenaPainter;
        [SerializeField] private PlayerPainter playerPainter;
        [SerializeField] private PlayerOrbitMover playerOrbitMover;
        [SerializeField] private RivalSpiritController rivalController;

        [Header("Round Flow")]
        [SerializeField, Range(1, 5)] private int totalRounds = 3;
        [SerializeField, Min(5f)] private float roundDurationSeconds = 20f;
        [SerializeField, Range(0.55f, 0.95f)] private float coverageToWin = 0.7f;
        [SerializeField, Range(0f, 0.2f)] private float coverageDeadband = 0.03f;
        [SerializeField, Min(0f)] private float interRoundDelaySeconds = 2f;
        [SerializeField] private bool autoStart = true;

        [Header("Events")]
        [SerializeField] private UnityEvent<int> onRoundStarted;
        [SerializeField] private UnityEvent<int, float> onRoundTimeNormalized;
        [SerializeField] private UnityEvent<int, float, float, SectorOwner> onRoundCompleted;
        [SerializeField] private UnityEvent<int, int> onSessionCompleted;

        public event Action<int> RoundStartedEvent;
        public event Action<int, float> RoundTimerEvent;
        public event Action<int, float, float, SectorOwner> RoundCompletedEvent;
        public event Action<int, int> SessionCompletedEvent;

        public int TotalRounds => totalRounds;
        public float RoundDurationSeconds => roundDurationSeconds;
        public bool IsRunning => _sessionRoutine != null;

        private Coroutine _sessionRoutine;
        private int _playerRoundsWon;
        private int _rivalRoundsWon;

        private void OnEnable()
        {
            if (autoStart && HasRequiredReferences())
            {
                BeginSession();
            }
        }

        private void OnDisable()
        {
            StopSession();
        }

        public void Configure(
            ArenaPainter arena,
            PlayerPainter player,
            PlayerOrbitMover orbitMover,
            RivalSpiritController rival)
        {
            arenaPainter = arena;
            playerPainter = player;
            playerOrbitMover = orbitMover;
            rivalController = rival;
        }

        public void ApplyRoundSettings(int rounds, float durationSeconds, float coverageThreshold, float deadband, float interRoundDelay)
        {
            totalRounds = Mathf.Clamp(rounds, 1, 5);
            roundDurationSeconds = Mathf.Max(5f, durationSeconds);
            coverageToWin = Mathf.Clamp(coverageThreshold, 0.55f, 0.95f);
            coverageDeadband = Mathf.Clamp(deadband, 0f, 0.2f);
            interRoundDelaySeconds = Mathf.Max(0f, interRoundDelay);
        }

        public void BeginSession()
        {
            if (!HasRequiredReferences())
            {
                Debug.LogWarning("PrototypeSessionManager requires ArenaPainter + PlayerPainter before starting.", this);
                return;
            }

            if (_sessionRoutine != null)
            {
                StopCoroutine(_sessionRoutine);
            }

            _sessionRoutine = StartCoroutine(SessionLoop());
        }

        public void StopSession()
        {
            if (_sessionRoutine != null)
            {
                StopCoroutine(_sessionRoutine);
                _sessionRoutine = null;
            }
        }

        private bool HasRequiredReferences()
        {
            return arenaPainter != null && playerPainter != null;
        }

        private IEnumerator SessionLoop()
        {
            _playerRoundsWon = 0;
            _rivalRoundsWon = 0;
            int roundsToWin = Mathf.CeilToInt(totalRounds * 0.5f);

            for (int roundIndex = 1; roundIndex <= totalRounds; roundIndex++)
            {
                PrepareRound();
                onRoundStarted?.Invoke(roundIndex);
                RoundStartedEvent?.Invoke(roundIndex);

                float timer = roundDurationSeconds;
                while (timer > 0f)
                {
                    yield return null;
                    timer -= Time.deltaTime;
                    float normalized = roundDurationSeconds <= 0.01f
                        ? 1f
                        : 1f - Mathf.Clamp01(timer / roundDurationSeconds);
                    onRoundTimeNormalized?.Invoke(roundIndex, normalized);
                    RoundTimerEvent?.Invoke(roundIndex, normalized);

                    if (TryResolveImmediateWinner(out _))
                    {
                        break;
                    }
                }

                SectorOwner winner = DetermineWinner();
                float playerCoverage = arenaPainter.GetCoverage(SectorOwner.Player);
                float rivalCoverage = arenaPainter.GetCoverage(SectorOwner.Rival);

                if (winner == SectorOwner.Player)
                {
                    _playerRoundsWon++;
                }
                else if (winner == SectorOwner.Rival)
                {
                    _rivalRoundsWon++;
                }

                onRoundCompleted?.Invoke(roundIndex, playerCoverage, rivalCoverage, winner);
                RoundCompletedEvent?.Invoke(roundIndex, playerCoverage, rivalCoverage, winner);

                if (_playerRoundsWon >= roundsToWin || _rivalRoundsWon >= roundsToWin)
                {
                    break;
                }

                if (interRoundDelaySeconds > 0f)
                {
                    yield return new WaitForSeconds(interRoundDelaySeconds);
                }
            }

            onSessionCompleted?.Invoke(_playerRoundsWon, _rivalRoundsWon);
            SessionCompletedEvent?.Invoke(_playerRoundsWon, _rivalRoundsWon);
            _sessionRoutine = null;
        }

        private void PrepareRound()
        {
            arenaPainter?.ResetCoverage();
            playerPainter?.ResetForNewRound(true);
            playerOrbitMover?.ResetOrbit();
            rivalController?.ResetForRound();
        }

        private bool TryResolveImmediateWinner(out SectorOwner winner)
        {
            winner = SectorOwner.Neutral;
            if (arenaPainter == null)
            {
                return false;
            }

            float playerCoverage = arenaPainter.GetCoverage(SectorOwner.Player);
            if (playerCoverage >= coverageToWin)
            {
                winner = SectorOwner.Player;
                return true;
            }

            float rivalCoverage = arenaPainter.GetCoverage(SectorOwner.Rival);
            if (rivalCoverage >= coverageToWin)
            {
                winner = SectorOwner.Rival;
                return true;
            }

            return false;
        }

        private SectorOwner DetermineWinner()
        {
            if (TryResolveImmediateWinner(out SectorOwner winner))
            {
                return winner;
            }

            float playerCoverage = arenaPainter.GetCoverage(SectorOwner.Player);
            float rivalCoverage = arenaPainter.GetCoverage(SectorOwner.Rival);
            float delta = playerCoverage - rivalCoverage;

            if (Mathf.Abs(delta) <= coverageDeadband)
            {
                return SectorOwner.Neutral;
            }

            return delta > 0f ? SectorOwner.Player : SectorOwner.Rival;
        }
    }
}
