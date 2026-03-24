using System;
using System.Collections.Generic;
using BubbleBolt.Config;
using UnityEngine;

namespace BubbleBolt.Gameplay.Arena
{
    public enum SectorOwner
    {
        Neutral = 0,
        Player = 1,
        Rival = 2
    }

    [Serializable]
    public sealed class SectorState
    {
        public int index;
        public SectorOwner owner;
        [Range(0f, 1f)] public float playerFill;
        [Range(0f, 1f)] public float rivalFill;
    }

    public struct SectorPaintResult
    {
        public int sectorIndex;
        public SectorOwner owner;
        public bool ownerChanged;
        public float playerFill;
        public float rivalFill;
        public SectorOwner painter;
    }

    public class ArenaPainter : MonoBehaviour
    {
        [SerializeField] private PrototypeTuning tuning;
        [SerializeField] private List<SectorState> sectors = new();

        public IReadOnlyList<SectorState> Sectors => sectors;
        public int SectorCount => sectors.Count;

        private void Awake()
        {
            if (tuning == null)
            {
                Debug.LogError("PrototypeTuning reference missing on ArenaPainter.", this);
            }

            if (sectors.Count != tuning.sectorCount)
            {
                BuildSectors();
            }
        }

        private void BuildSectors()
        {
            sectors.Clear();
            for (int i = 0; i < Mathf.Max(1, tuning.sectorCount); i++)
            {
                sectors.Add(new SectorState
                {
                    index = i,
                    owner = SectorOwner.Neutral,
                    playerFill = 0f,
                    rivalFill = 0f
                });
            }
        }

        public SectorPaintResult Paint(SectorOwner painter, float angleRadians, float speedNormalized, float deltaTime, bool overcharged)
        {
            if (tuning == null || sectors.Count == 0)
            {
                return default;
            }

            int sectorIndex = GetSectorIndex(angleRadians);
            SectorState state = sectors[sectorIndex];

            float multiplier = tuning.speedToPaintMultiplier.Evaluate(Mathf.Clamp01(speedNormalized));
            float amount = tuning.baseFillRate * multiplier * deltaTime;
            if (overcharged)
            {
                amount *= tuning.overchargePaintMultiplier;
            }

            bool ownerChanged = false;

            switch (painter)
            {
                case SectorOwner.Player:
                    state.playerFill = Mathf.Clamp01(state.playerFill + amount);
                    state.rivalFill = Mathf.Max(0f, state.rivalFill - amount * 0.75f);
                    if (state.playerFill >= 1f)
                    {
                        ownerChanged = state.owner != SectorOwner.Player;
                        state.owner = SectorOwner.Player;
                        state.playerFill = 1f;
                        state.rivalFill = 0f;
                    }
                    break;

                case SectorOwner.Rival:
                    state.rivalFill = Mathf.Clamp01(state.rivalFill + amount);
                    state.playerFill = Mathf.Max(0f, state.playerFill - amount * 0.75f);
                    if (state.rivalFill >= 1f)
                    {
                        ownerChanged = state.owner != SectorOwner.Rival;
                        state.owner = SectorOwner.Rival;
                        state.rivalFill = 1f;
                        state.playerFill = 0f;
                    }
                    break;
            }

            sectors[sectorIndex] = state;

            return new SectorPaintResult
            {
                sectorIndex = sectorIndex,
                owner = state.owner,
                ownerChanged = ownerChanged,
                playerFill = state.playerFill,
                rivalFill = state.rivalFill,
                painter = painter
            };
        }

        public float GetCoverage(SectorOwner owner)
        {
            if (sectors.Count == 0)
            {
                return 0f;
            }

            int owned = 0;
            foreach (SectorState sector in sectors)
            {
                if (sector.owner == owner)
                {
                    owned++;
                }
            }

            return owned / (float)sectors.Count;
        }

        private int GetSectorIndex(float angleRadians)
        {
            float normalized = Mathf.Repeat(angleRadians, Mathf.PI * 2f) / (Mathf.PI * 2f);
            int index = Mathf.FloorToInt(normalized * sectors.Count);
            if (index >= sectors.Count)
            {
                index = sectors.Count - 1;
            }

            return index;
        }
    }
}
