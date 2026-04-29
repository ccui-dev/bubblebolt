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
        private readonly Dictionary<int, AudioClip> _loopCache = new Dictionary<int, AudioClip>();
        private AudioSource _oneShotSource;
        private AudioSource _loopSource;

        private void Awake()
        {
            _oneShotSource = GetComponent<AudioSource>();
            if (_oneShotSource == null)
            {
                _oneShotSource = gameObject.AddComponent<AudioSource>();
            }
            _oneShotSource.playOnAwake = false;
            _oneShotSource.loop = false;
            _oneShotSource.spatialBlend = 0f;

            _loopSource = gameObject.AddComponent<AudioSource>();
            _loopSource.playOnAwake = false;
            _loopSource.loop = true;
            _loopSource.spatialBlend = 0f;
            _loopSource.volume = 0f;
        }

        public void PlayTone(float frequency, float duration, float volume = 1f)
        {
            if (frequency <= 0f || duration <= 0f || _oneShotSource == null)
            {
                return;
            }

            AudioClip clip = GetOrCreateClip(frequency, duration);
            if (clip != null)
            {
                _oneShotSource.PlayOneShot(clip, Mathf.Clamp01(volume));
            }
        }

        public void StartLoop(float frequency, float volume = 0.3f)
        {
            if (frequency <= 0f || _loopSource == null)
            {
                return;
            }

            AudioClip clip = GetOrCreateLoopClip(frequency);
            if (clip == null)
            {
                return;
            }

            if (_loopSource.clip != clip)
            {
                _loopSource.clip = clip;
            }

            _loopSource.volume = Mathf.Clamp01(volume);
            if (!_loopSource.isPlaying)
            {
                _loopSource.Play();
            }
        }

        public void StopLoop()
        {
            if (_loopSource != null && _loopSource.isPlaying)
            {
                _loopSource.Stop();
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

        private AudioClip GetOrCreateLoopClip(float frequency)
        {
            int key = Mathf.RoundToInt(frequency * 10f);
            if (_loopCache.TryGetValue(key, out AudioClip existing))
            {
                return existing;
            }

            AudioClip clip = BuildLoopClip(frequency);
            if (clip != null)
            {
                _loopCache[key] = clip;
            }
            return clip;
        }

        private AudioClip BuildLoopClip(float frequency)
        {
            const float loopDuration = 0.35f;
            int sampleCount = Mathf.CeilToInt(sampleRate * loopDuration);
            if (sampleCount <= 0)
            {
                return null;
            }

            float[] data = new float[sampleCount];
            float increment = 2f * Mathf.PI * frequency / sampleRate;
            float phase = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                data[i] = Mathf.Sin(phase);
                phase += increment;
            }

            AudioClip clip = AudioClip.Create($"tone_loop_{frequency:0}", sampleCount, 1, sampleRate, false);
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
