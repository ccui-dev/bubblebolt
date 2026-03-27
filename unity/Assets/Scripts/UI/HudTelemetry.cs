using BubbleBolt.Gameplay.Arena;
using BubbleBolt.Gameplay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BubbleBolt.UI
{
    public class HudTelemetry : MonoBehaviour
    {
        [SerializeField] private ArenaPainter arenaPainter;
        [SerializeField] private PlayerPainter playerPainter;
        [SerializeField] private TextMeshProUGUI comboLabel;
        [SerializeField] private TextMeshProUGUI scoreLabel;
        [SerializeField] private TextMeshProUGUI livesLabel;
        [SerializeField] private Slider playerCoverageBar;
        [SerializeField] private Slider rivalCoverageBar;

        private void Update()
        {
            if (playerPainter != null)
            {
                if (comboLabel != null)
                {
                    comboLabel.text = $"Combo x{playerPainter.Combo}";
                }

                if (scoreLabel != null)
                {
                    scoreLabel.text = Mathf.RoundToInt(playerPainter.Score).ToString();
                }

                if (livesLabel != null)
                {
                    livesLabel.text = new string('\u25CF', Mathf.Max(0, playerPainter.Lives));
                }
            }

            if (arenaPainter != null)
            {
                float playerCoverage = arenaPainter.GetCoverage(SectorOwner.Player);
                float rivalCoverage = arenaPainter.GetCoverage(SectorOwner.Rival);

                if (playerCoverageBar != null)
                {
                    playerCoverageBar.value = playerCoverage;
                }

                if (rivalCoverageBar != null)
                {
                    rivalCoverageBar.value = rivalCoverage;
                }
            }
        }

        public void Configure(
            ArenaPainter arena,
            PlayerPainter player,
            TextMeshProUGUI combo,
            TextMeshProUGUI score,
            TextMeshProUGUI lives,
            Slider playerBar,
            Slider rivalBar)
        {
            arenaPainter = arena;
            playerPainter = player;
            comboLabel = combo;
            scoreLabel = score;
            livesLabel = lives;
            playerCoverageBar = playerBar;
            rivalCoverageBar = rivalBar;
        }
    }
}
