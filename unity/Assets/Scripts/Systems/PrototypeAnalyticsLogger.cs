using BubbleBolt.Gameplay.Arena;
using BubbleBolt.Gameplay.Player;
using UnityEngine;

namespace BubbleBolt.Systems
{
    /// <summary>
    /// Minimal analytics bridge that logs round/session results and Overcharge usage.
    /// Swap the internals later to send real analytics events.
    /// </summary>
    public class PrototypeAnalyticsLogger : MonoBehaviour
    {
        [SerializeField] private PrototypeSessionManager sessionManager;
        [SerializeField] private PlayerPainter playerPainter;
        [SerializeField] private bool logToConsole = true;

        public void Configure(PrototypeSessionManager manager, PlayerPainter painter)
        {
            sessionManager = manager;
            playerPainter = painter;
            Bind();
        }

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void Bind()
        {
            Unbind();

            if (sessionManager == null)
            {
                sessionManager = FindObjectOfType<PrototypeSessionManager>();
            }

            if (playerPainter == null)
            {
                playerPainter = FindObjectOfType<PlayerPainter>();
            }

            if (sessionManager != null)
            {
                sessionManager.RoundCompletedEvent += HandleRoundCompleted;
                sessionManager.SessionCompletedEvent += HandleSessionCompleted;
            }

            if (playerPainter != null)
            {
                playerPainter.OverchargeTriggered += HandleOverchargeTriggered;
            }
        }

        private void Unbind()
        {
            if (sessionManager != null)
            {
                sessionManager.RoundCompletedEvent -= HandleRoundCompleted;
                sessionManager.SessionCompletedEvent -= HandleSessionCompleted;
            }

            if (playerPainter != null)
            {
                playerPainter.OverchargeTriggered -= HandleOverchargeTriggered;
            }
        }

        private void HandleRoundCompleted(int round, float playerCoverage, float rivalCoverage, SectorOwner winner)
        {
            if (!logToConsole)
            {
                return;
            }

            Debug.Log($"[Analytics] round_result round={round} winner={winner} player={playerCoverage:P0} rival={rivalCoverage:P0}");
        }

        private void HandleSessionCompleted(int playerWins, int rivalWins)
        {
            if (!logToConsole)
            {
                return;
            }

            Debug.Log($"[Analytics] session_result playerWins={playerWins} rivalWins={rivalWins} score={Mathf.RoundToInt(playerPainter?.Score ?? 0f)}");
        }

        private void HandleOverchargeTriggered()
        {
            if (!logToConsole)
            {
                return;
            }

            Debug.Log("[Analytics] overcharge_use");
        }
    }
}
