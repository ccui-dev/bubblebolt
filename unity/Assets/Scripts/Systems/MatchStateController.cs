using BubbleBolt.Gameplay.Arena;
using BubbleBolt.Gameplay.Player;
using UnityEngine;
using UnityEngine.Events;

namespace BubbleBolt.Systems
{
    public class MatchStateController : MonoBehaviour
    {
        [SerializeField] private ArenaPainter arenaPainter;
        [SerializeField] private PlayerPainter playerPainter;
        [SerializeField] private UnityEvent<float> onPlayerCoverage;
        [SerializeField] private UnityEvent<float> onRivalCoverage;

        private void Update()
        {
            if (arenaPainter == null)
            {
                return;
            }

            onPlayerCoverage?.Invoke(arenaPainter.GetCoverage(SectorOwner.Player));
            onRivalCoverage?.Invoke(arenaPainter.GetCoverage(SectorOwner.Rival));
        }

        public void NotifyHazardCollision()
        {
            playerPainter?.RegisterHit();
        }
    }
}
