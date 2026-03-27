using System;
using BubbleBolt.Config;
using BubbleBolt.Gameplay.Arena;
using UnityEngine;
using UnityEngine.Events;

namespace BubbleBolt.Gameplay.Player
{
    [RequireComponent(typeof(PlayerOrbitMover))]
    public class PlayerPainter : MonoBehaviour
    {
        [SerializeField] private PrototypeTuning tuning;
        [SerializeField] private ArenaPainter arenaPainter;
        [SerializeField] private UnityEvent<SectorPaintResult> onSectorPainted;
        [SerializeField] private UnityEvent<int> onComboChanged;
        [SerializeField] private UnityEvent<float> onOverchargeChanged;

        private PlayerOrbitMover _orbitMover;
        private float _overchargeGauge;
        private bool _overchargeActive;
        private float _overchargeTimer;
        private int _combo;
        private int _perfectArcRun;
        private int _lastClaimedSector = -1;
        private float _score;
        private int _lives;
        private float _invulnerabilityTimer;

        public float OverchargeGauge => _overchargeGauge;
        public bool OverchargeActive => _overchargeActive;
        public float Score => _score;
        public int Lives => _lives;
        public int Combo => _combo;

        private void Awake()
        {
            _orbitMover = GetComponent<PlayerOrbitMover>();
            if (tuning == null || arenaPainter == null)
            {
                return;
            }

            ResetState();
        }

        public void Configure(PrototypeTuning tuningAsset, ArenaPainter arenaPainterRef)
        {
            tuning = tuningAsset;
            arenaPainter = arenaPainterRef;
            _orbitMover ??= GetComponent<PlayerOrbitMover>();

            if (tuning != null)
            {
                ResetState();
            }
        }

        private void ResetState()
        {
            if (tuning == null)
            {
                return;
            }

            _lives = tuning.maxLives;
            _overchargeGauge = 0f;
            _overchargeActive = false;
            _overchargeTimer = 0f;
            _combo = 0;
            _perfectArcRun = 0;
            _lastClaimedSector = -1;
            _score = 0f;
            _invulnerabilityTimer = 0f;
        }

        private void Update()
        {
            if (tuning == null || arenaPainter == null || _orbitMover == null)
            {
                return;
            }

            float dt = Time.deltaTime;

            if (_overchargeActive)
            {
                _overchargeTimer -= dt;
                if (_overchargeTimer <= 0f)
                {
                    _overchargeActive = false;
                }
            }

            if (_invulnerabilityTimer > 0f)
            {
                _invulnerabilityTimer -= dt;
            }

            SectorPaintResult result = arenaPainter.Paint(
                SectorOwner.Player,
                _orbitMover.AngleRadians,
                _orbitMover.NormalizedSpeed,
                dt,
                _overchargeActive);

            HandlePaintResult(result);
        }

        private void HandlePaintResult(SectorPaintResult result)
        {
            if (result.painter != SectorOwner.Player)
            {
                return;
            }

            if (result.ownerChanged && result.owner == SectorOwner.Player)
            {
                _combo++;
                onComboChanged?.Invoke(_combo);

                bool adjacent = _lastClaimedSector >= 0 && IsAdjacent(_lastClaimedSector, result.sectorIndex, arenaPainter.SectorCount);
                _perfectArcRun = adjacent ? _perfectArcRun + 1 : 1;
                _lastClaimedSector = result.sectorIndex;

                float comboMultiplier = 1f + _combo * tuning.comboMultiplierStep;
                float perfectBonus = _perfectArcRun >= tuning.perfectArcThreshold ? 1f + tuning.perfectArcBonus : 1f;
                _score += comboMultiplier * perfectBonus;

                _overchargeGauge = Mathf.Clamp01(_overchargeGauge + tuning.overchargeGainPerSector);
                onOverchargeChanged?.Invoke(_overchargeGauge);
            }

            onSectorPainted?.Invoke(result);
        }

        public void TriggerOvercharge()
        {
            if (_overchargeActive || _overchargeGauge < 1f)
            {
                return;
            }

            _overchargeGauge -= 1f;
            _overchargeActive = true;
            _overchargeTimer = tuning.overchargeDuration;
            onOverchargeChanged?.Invoke(_overchargeGauge);
        }

        public void RegisterHit()
        {
            if (_invulnerabilityTimer > 0f)
            {
                return;
            }

            _lives = Mathf.Max(0, _lives - 1);
            _combo = 0;
            _perfectArcRun = 0;
            onComboChanged?.Invoke(_combo);
            _invulnerabilityTimer = tuning.invulnerabilitySeconds;
        }

        private static bool IsAdjacent(int lastIndex, int currentIndex, int sectorCount)
        {
            if (sectorCount <= 0)
            {
                return false;
            }

            int diff = Mathf.Abs(currentIndex - lastIndex);
            return diff == 1 || diff == sectorCount - 1;
        }
    }
}
