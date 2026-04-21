using System.Collections.Generic;
using UnityEngine;

namespace BubbleBolt.Systems
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class PrototypeTonePlayer : MonoBehaviour
    {
        [SerializeField] private int sampleRate = 44100;
        [SerializeField] private AnimationCurve envelope = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

        private readonly Dictionary<long, AudioClip> _cache = new Dictionary<long, AudioClip>();
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = 0f;
        }

        public void PlayTone(float frequency, float duration, float volume = 1f)
        {
            if (frequency <= 0f || duration <= 0f || _audioSource == null)
            {
                return;
            }

            AudioClip clip = GetOrCreateClip(frequency, duration);
            if (clip != null)
            {
                _audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
            }
        }

        private AudioClip GetOrCreateClip(float frequency, float duration)
        {
            long key = GenerateKey(frequency, duration);
            if (_cache.TryGetValue(key, out AudioClip existing))
            {
                return existing;
            }

            AudioClip clip = BuildClip(frequency, duration);
            if (clip != null)
            {
                _cache[key] = clip;
            }
            return clip;
        }

        private AudioClip BuildClip(float frequency, float duration)
        {
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            if (sampleCount <= 0)
            {
                return null;
            }

            float[] data = new float[sampleCount];
            float increment = 2f * Mathf.PI * frequency / sampleRate;
            float phase = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                float normalizedTime = sampleCount == 1 ? 0f : i / (float)(sampleCount - 1);
                float env = Mathf.Clamp01(envelope.Evaluate(normalizedTime));
                data[i] = Mathf.Sin(phase) * env;
                phase += increment;
            }

            AudioClip clip = AudioClip.Create($"tone_{frequency:0}_{duration:0.000}", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private long GenerateKey(float frequency, float duration)
        {
            int freqKey = Mathf.RoundToInt(frequency * 10f);
            int durKey = Mathf.RoundToInt(duration * 1000f);
            return ((long)freqKey << 32) | (uint)durKey;
        }
    }
}
