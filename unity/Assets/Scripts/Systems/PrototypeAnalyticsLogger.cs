using System;
using System.Collections;
using System.Text;
using BubbleBolt.Gameplay.Arena;
using BubbleBolt.Gameplay.Player;
using UnityEngine;
using UnityEngine.Networking;

namespace BubbleBolt.Systems
{
    /// <summary>
    /// Analytics bridge that can log to the console and optionally POST payloads to a backend endpoint.
    /// </summary>
    public class PrototypeAnalyticsLogger : MonoBehaviour
    {
        [SerializeField] private PrototypeSessionManager sessionManager;
        [SerializeField] private PlayerPainter playerPainter;
        [SerializeField] private bool logToConsole = true;
        [SerializeField] private bool sendToEndpoint = false;
        [SerializeField] private string endpointUrl = "https://analytics.bubblebolt.dev/v1/events";
        [SerializeField] private string apiKey = string.Empty;
        [SerializeField] private bool logEndpointFailures = true;

        private string _sessionId;

        public void Configure(PrototypeSessionManager manager, PlayerPainter painter)
        {
            if (sessionManager != manager || playerPainter != painter)
            {
                Unbind();
            }

            sessionManager = manager;
            playerPainter = painter;
            Bind();
        }

        public void ConfigureEndpoint(bool enableUpload, string endpoint, string newApiKey)
        {
            sendToEndpoint = enableUpload;
            endpointUrl = endpoint;
            apiKey = newApiKey;
        }

        private void Awake()
        {
            _sessionId = Guid.NewGuid().ToString("N");
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
            if (logToConsole)
            {
                Debug.Log($"[Analytics] round_result round={round} winner={winner} player={playerCoverage:P0} rival={rivalCoverage:P0}");
            }

            var payload = new RoundResultPayload
            {
                round = round,
                winner = winner.ToString(),
                playerCoverage = playerCoverage,
                rivalCoverage = rivalCoverage
            };

            Dispatch("round_result", payload);
        }

        private void HandleSessionCompleted(int playerWins, int rivalWins)
        {
            if (logToConsole)
            {
                Debug.Log($"[Analytics] session_result playerWins={playerWins} rivalWins={rivalWins} score={Mathf.RoundToInt(playerPainter?.Score ?? 0f)}");
            }

            var payload = new SessionResultPayload
            {
                playerWins = playerWins,
                rivalWins = rivalWins,
                score = Mathf.RoundToInt(playerPainter?.Score ?? 0f)
            };

            Dispatch("session_result", payload);
        }

        private void HandleOverchargeTriggered()
        {
            if (logToConsole)
            {
                Debug.Log("[Analytics] overcharge_use");
            }

            var payload = new OverchargeUsePayload
            {
                scoreAtFire = Mathf.RoundToInt(playerPainter?.Score ?? 0f),
                combo = playerPainter?.Combo ?? 0
            };

            Dispatch("overcharge_use", payload);
        }

        private void Dispatch<T>(string eventName, T payload)
        {
            if (!sendToEndpoint || string.IsNullOrEmpty(endpointUrl) || !isActiveAndEnabled)
            {
                return;
            }

            var envelope = new AnalyticsEnvelope<T>
            {
                eventName = eventName,
                sessionId = _sessionId,
                clientTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0,
                payload = payload
            };

            string json = JsonUtility.ToJson(envelope);
            StartCoroutine(PostJson(json));
        }

        private IEnumerator PostJson(string json)
        {
            using UnityWebRequest request = new UnityWebRequest(endpointUrl, UnityWebRequest.kHttpVerbPOST);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.SetRequestHeader("X-API-Key", apiKey);
            }

#if UNITY_2020_1_OR_NEWER
            yield return request.SendWebRequest();
            bool success = request.result == UnityWebRequest.Result.Success;
#else
            yield return request.SendWebRequest();
            bool success = !request.isNetworkError && !request.isHttpError;
#endif

            if (!success && logEndpointFailures)
            {
                Debug.LogWarning($"[Analytics] Endpoint error {request.responseCode}: {request.error}");
            }
        }

        [Serializable]
        private struct RoundResultPayload
        {
            public int round;
            public string winner;
            public float playerCoverage;
            public float rivalCoverage;
        }

        [Serializable]
        private struct SessionResultPayload
        {
            public int playerWins;
            public int rivalWins;
            public int score;
        }

        [Serializable]
        private struct OverchargeUsePayload
        {
            public int scoreAtFire;
            public int combo;
        }

        [Serializable]
        private struct AnalyticsEnvelope<T>
        {
            public string eventName;
            public string sessionId;
            public double clientTimestamp;
            public T payload;
        }
    }
}
