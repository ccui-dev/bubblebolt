using BubbleBolt.AI;
using BubbleBolt.Config;
using BubbleBolt.Gameplay.Arena;
using BubbleBolt.Gameplay.Player;
using BubbleBolt.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BubbleBolt.Systems
{
    /// <summary>
    /// Runtime helper that spawns a minimal playable greybox so the prototype can run without manual scene wiring.
    /// Drop this on an empty GameObject inside a blank scene and press Play.
    /// </summary>
    public class PrototypeSceneBootstrap : MonoBehaviour
    {
        [Header("Tuning & Visuals")]
        [SerializeField] private PrototypeTuning tuningAsset;
        [SerializeField] private Material playerMaterial;
        [SerializeField] private Material rivalMaterial;

        [Header("Layout")]
        [SerializeField, Range(0.4f, 1.2f)] private float arenaRadius = 0.8f;
        [SerializeField, Range(1, 8)] private int hazardCount = 3;
        [SerializeField, Range(5f, 90f)] private float hazardAngularSpanDegrees = 30f;
        [SerializeField, Range(0.01f, 0.2f)] private float hazardRadiusTolerance = 0.07f;

        [Header("Hazards")]
        [SerializeField, Range(2f, 60f)] private float hazardOrbitBaseSpeed = 24f;
        [SerializeField, Range(0f, 30f)] private float hazardOrbitVariance = 12f;
        [SerializeField] private Vector2 hazardOrbitRetargetInterval = new Vector2(3.5f, 6.5f);
        [SerializeField, Range(1f, 120f)] private float hazardOrbitAcceleration = 45f;
        [SerializeField] private Color hazardBaseColor = new Color(1f, 0.45f, 0.45f, 1f);
        [SerializeField] private Color hazardPulseColor = new Color(1f, 0.9f, 0.35f, 1f);
        [SerializeField, Range(0.1f, 6f)] private float hazardPulseSpeed = 2.4f;
        [SerializeField, Range(0f, 0.6f)] private float hazardPulseScale = 0.25f;

        [Header("Runtime HUD")]
        [SerializeField] private bool spawnRuntimeHud = true;

        [Header("Round Flow")]
        [SerializeField] private bool spawnSessionManager = true;
        [SerializeField, Range(1, 5)] private int sessionRounds = 3;
        [SerializeField, Min(5f)] private float sessionRoundDuration = 20f;
        [SerializeField, Range(0.55f, 0.95f)] private float sessionCoverageToWin = 0.7f;
        [SerializeField, Range(0f, 0.2f)] private float sessionCoverageDeadband = 0.03f;
        [SerializeField, Min(0f)] private float sessionInterRoundDelay = 2f;

        private PrototypeCameraShake cameraShake;

        private void Awake()
        {
            EnsureCamera();

            if (FindObjectOfType<PlayerPainter>() != null)
            {
                // Scene already assembled.
                return;
            }

            BuildSceneGraph();
        }

        private void BuildSceneGraph()
        {
            PrototypeTuning liveTuning = tuningAsset != null
                ? Instantiate(tuningAsset)
                : ScriptableObject.CreateInstance<PrototypeTuning>();
            liveTuning.name = tuningAsset != null ? tuningAsset.name + " (Runtime)" : "PrototypeTuning_Runtime";

            Transform root = new GameObject("PrototypeSceneRoot").transform;
            root.SetParent(transform, false);

            Transform arenaRoot = new GameObject("ArenaRoot").transform;
            arenaRoot.SetParent(root, false);
            Transform arenaCenter = new GameObject("Center").transform;
            arenaCenter.SetParent(arenaRoot, false);

            var arenaPainter = arenaRoot.gameObject.AddComponent<ArenaPainter>();
            arenaPainter.Configure(liveTuning);

            CreateTrackVisual(arenaRoot);

            GameObject playerBubble = CreateBubble("PlayerBubble", playerMaterial, new Color(0.8f, 0.95f, 1f));
            playerBubble.transform.SetParent(root, false);
            playerBubble.transform.localPosition = new Vector3(arenaRadius, 0f, 0f);

            var orbitMover = playerBubble.AddComponent<PlayerOrbitMover>();
            orbitMover.Configure(liveTuning, arenaCenter);

            var swipeController = playerBubble.AddComponent<SwipeOrbitController>();
            swipeController.Configure(liveTuning);

            var playerPainter = playerBubble.AddComponent<PlayerPainter>();
            playerPainter.Configure(liveTuning, arenaPainter);

            var glow = playerBubble.AddComponent<PlayerOverchargeGlow>();
            glow.Configure(playerPainter, new Color(0.8f, 0.95f, 1f), new Color(1f, 0.65f, 0.2f));
            var feedback = playerBubble.AddComponent<PlayerFeedbackController>();

            Transform hazardsRoot = new GameObject("Hazards").transform;
            hazardsRoot.SetParent(root, false);
            var hazardOrbit = hazardsRoot.gameObject.AddComponent<HazardOrbitController>();
            hazardOrbit.Configure(hazardOrbitBaseSpeed, hazardOrbitVariance, hazardOrbitRetargetInterval, hazardOrbitAcceleration, true);
            SpawnHazards(hazardsRoot, orbitMover, playerPainter);

            GameObject rivalGhost = CreateBubble("RivalGhost", rivalMaterial, new Color(1f, 0.65f, 0.85f));
            rivalGhost.transform.SetParent(root, false);
            rivalGhost.transform.localScale = Vector3.one * 0.12f;

            var rivalController = rivalGhost.AddComponent<RivalSpiritController>();
            rivalController.Configure(liveTuning, arenaPainter, arenaCenter);

            Transform systemsRoot = new GameObject("Systems").transform;
            systemsRoot.SetParent(root, false);
            var matchState = systemsRoot.gameObject.AddComponent<MatchStateController>();
            matchState.Configure(arenaPainter, playerPainter);

            PrototypeSessionManager sessionManager = null;
            if (spawnSessionManager)
            {
                sessionManager = systemsRoot.gameObject.AddComponent<PrototypeSessionManager>();
                sessionManager.Configure(arenaPainter, playerPainter, orbitMover, rivalController);
                sessionManager.ApplyRoundSettings(sessionRounds, sessionRoundDuration, sessionCoverageToWin, sessionCoverageDeadband, sessionInterRoundDelay);
                sessionManager.BeginSession();
            }

            var analytics = systemsRoot.gameObject.AddComponent<PrototypeAnalyticsLogger>();
            analytics.Configure(sessionManager, playerPainter);

            var tonePlayer = systemsRoot.gameObject.AddComponent<PrototypeTonePlayer>();
            feedback.Configure(playerPainter, tonePlayer, cameraShake);

            if (spawnRuntimeHud)
            {
                BuildHud(root, arenaPainter, playerPainter, sessionManager, swipeController);
            }
        }

        private void EnsureCamera()
        {
            Camera activeCamera = Camera.main;

            if (activeCamera == null)
            {
                GameObject cameraObject = new GameObject("PrototypeCamera");
                cameraObject.tag = "MainCamera";
                activeCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            if (activeCamera.GetComponent<AudioListener>() == null)
            {
                activeCamera.gameObject.AddComponent<AudioListener>();
            }

            activeCamera.orthographic = true;
            activeCamera.orthographicSize = Mathf.Max(1.2f, arenaRadius * 1.8f);
            activeCamera.transform.position = new Vector3(0f, 0f, -5f);
            activeCamera.transform.rotation = Quaternion.identity;
            activeCamera.clearFlags = CameraClearFlags.SolidColor;
            activeCamera.backgroundColor = new Color(0.02f, 0.02f, 0.02f, 1f);
            activeCamera.nearClipPlane = 0.1f;
            activeCamera.farClipPlane = 10f;

            var shaker = activeCamera.GetComponent<PrototypeCameraShake>();
            if (shaker == null)
            {
                shaker = activeCamera.gameObject.AddComponent<PrototypeCameraShake>();
            }
            cameraShake = shaker;
        }

        private GameObject CreateBubble(string name, Material overrideMaterial, Color fallbackColor)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = name;
            Destroy(sphere.GetComponent<Collider>());
            sphere.transform.localScale = Vector3.one * 0.15f;

            Renderer renderer = sphere.GetComponent<Renderer>();
            if (overrideMaterial != null)
            {
                renderer.sharedMaterial = overrideMaterial;
            }
            else
            {
                renderer.material.color = fallbackColor;
            }

            return sphere;
        }

        private void SpawnHazards(Transform parent, PlayerOrbitMover mover, PlayerPainter painter)
        {
            for (int i = 0; i < hazardCount; i++)
            {
                float angleDeg = (360f / Mathf.Max(1, hazardCount)) * i;
                float angleRad = angleDeg * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f);

                GameObject hazardObject = new GameObject($"Hazard_{i:00}");
                hazardObject.transform.SetParent(parent, false);
                hazardObject.transform.localPosition = direction * arenaRadius;
                hazardObject.transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg);

                var hazard = hazardObject.AddComponent<ArenaHazard>();
                hazard.Configure(
                    mover,
                    painter,
                    arenaRadius,
                    Mathf.Deg2Rad * (hazardAngularSpanDegrees * 0.5f),
                    hazardRadiusTolerance);

                CreateHazardMarker(hazardObject.transform);
            }
        }

        private void CreateHazardMarker(Transform parent)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(marker.GetComponent<Collider>());
            marker.transform.SetParent(parent, false);
            marker.transform.localScale = new Vector3(0.08f, 0.2f, 0.02f);
            marker.transform.localPosition = Vector3.zero;

            var renderer = marker.GetComponent<Renderer>();
            var pulse = marker.AddComponent<HazardMarkerPulse>();
            pulse.Initialize(renderer, hazardBaseColor, hazardPulseColor, hazardPulseSpeed, hazardPulseScale);
        }

        private void CreateTrackVisual(Transform parent)
        {
            GameObject track = new GameObject("OrbitTrack");
            track.transform.SetParent(parent, false);
            var line = track.AddComponent<LineRenderer>();
            line.loop = true;
            line.useWorldSpace = false;
            line.widthMultiplier = 0.02f;
            line.positionCount = 64;
            for (int i = 0; i < line.positionCount; i++)
            {
                float t = i / (float)line.positionCount;
                float angle = t * Mathf.PI * 2f;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * arenaRadius);
            }
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.material.color = new Color(0.2f, 0.6f, 1f, 0.4f);
        }

        private void BuildHud(Transform parent, ArenaPainter arenaPainter, PlayerPainter playerPainter, PrototypeSessionManager sessionManager, SwipeOrbitController swipeController)
        {
            GameObject canvasGO = new GameObject("HUD_Canvas");
            canvasGO.transform.SetParent(parent, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            canvasGO.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();

            TextMeshProUGUI comboLabel = CreateLabel(canvasGO.transform, "ComboLabel", new Vector2(40f, -80f), TextAlignmentOptions.Left, new Vector2(0f, 1f));
            comboLabel.text = "Combo x1";

            TextMeshProUGUI scoreLabel = CreateLabel(canvasGO.transform, "ScoreLabel", new Vector2(-40f, -80f), TextAlignmentOptions.Right, new Vector2(1f, 1f));
            scoreLabel.text = "0";

            TextMeshProUGUI livesLabel = CreateLabel(canvasGO.transform, "LivesLabel", new Vector2(40f, -140f), TextAlignmentOptions.Left, new Vector2(0f, 1f));
            livesLabel.text = "●●";

            TextMeshProUGUI roundLabel = CreateLabel(canvasGO.transform, "RoundLabel", new Vector2(0f, -40f), TextAlignmentOptions.Center, new Vector2(0.5f, 1f));
            roundLabel.fontSize = 42f;

            Slider roundTimer = CreateSlider(canvasGO.transform, "RoundTimer", new Vector2(0f, -120f), new Color(0.95f, 0.75f, 0.25f));

            TextMeshProUGUI roundTimerText = CreateLabel(canvasGO.transform, "RoundTimerText", new Vector2(0f, -170f), TextAlignmentOptions.Center, new Vector2(0.5f, 1f));
            roundTimerText.fontSize = 40f;

            TextMeshProUGUI sessionScoreLabel = CreateLabel(canvasGO.transform, "SessionScoreLabel", new Vector2(0f, -210f), TextAlignmentOptions.Center, new Vector2(0.5f, 1f));
            sessionScoreLabel.fontSize = 40f;

            Slider playerSlider = CreateSlider(canvasGO.transform, "PlayerCoverage", new Vector2(0f, -260f), new Color(0.2f, 0.75f, 1f));
            Slider rivalSlider = CreateSlider(canvasGO.transform, "RivalCoverage", new Vector2(0f, -320f), new Color(1f, 0.45f, 0.7f));

            var telemetry = canvasGO.AddComponent<HudTelemetry>();
            telemetry.Configure(arenaPainter, playerPainter, comboLabel, scoreLabel, livesLabel, playerSlider, rivalSlider);

            var sessionHud = canvasGO.AddComponent<SessionStatusHud>();
            sessionHud.Configure(sessionManager, roundLabel, roundTimer, roundTimerText, sessionScoreLabel);

            GameObject tutorialPanel = new GameObject("TutorialOverlay");
            tutorialPanel.transform.SetParent(canvasGO.transform, false);
            var tutorialRect = tutorialPanel.AddComponent<RectTransform>();
            tutorialRect.anchorMin = tutorialRect.anchorMax = new Vector2(0.5f, 0.5f);
            tutorialRect.pivot = new Vector2(0.5f, 0.5f);
            tutorialRect.sizeDelta = new Vector2(560f, 520f);
            var tutorialBg = tutorialPanel.AddComponent<Image>();
            tutorialBg.color = new Color(0f, 0f, 0f, 0.7f);
            tutorialPanel.SetActive(false);

            TextMeshProUGUI tutorialText = CreateLabel(tutorialRect, "TutorialText", new Vector2(0f, 60f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f));
            tutorialText.fontSize = 44f;
            tutorialText.text = "Swipe to orbit around the arena.\nTap Overcharge when the gauge is full.";

            Button tutorialButton = CreateButton(tutorialRect, "TutorialButton", new Vector2(0f, -150f), new Vector2(360f, 110f), "Let's Go");

            var tutorialOverlay = canvasGO.AddComponent<PrototypeTutorialOverlay>();
            tutorialOverlay.Configure(sessionManager, swipeController, tutorialPanel, tutorialText, tutorialButton);

            GameObject summaryPanel = new GameObject("SessionSummary");
            summaryPanel.transform.SetParent(canvasGO.transform, false);
            var summaryRect = summaryPanel.AddComponent<RectTransform>();
            summaryRect.anchorMin = summaryRect.anchorMax = new Vector2(0.5f, 0.5f);
            summaryRect.pivot = new Vector2(0.5f, 0.5f);
            summaryRect.sizeDelta = new Vector2(620f, 620f);
            var summaryBg = summaryPanel.AddComponent<Image>();
            summaryBg.color = new Color(0f, 0f, 0f, 0.75f);
            summaryPanel.SetActive(false);

            TextMeshProUGUI summaryHeadline = CreateLabel(summaryRect, "SummaryHeadline", new Vector2(0f, 180f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f));
            summaryHeadline.fontSize = 64f;

            TextMeshProUGUI summaryDetails = CreateLabel(summaryRect, "SummaryDetails", new Vector2(0f, 40f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f));
            summaryDetails.fontSize = 40f;

            Button replayButton = CreateButton(summaryRect, "ReplayButton", new Vector2(0f, -170f), new Vector2(360f, 110f), "Replay");

            var summaryPanelController = canvasGO.AddComponent<SessionSummaryPanel>();
            summaryPanelController.Configure(sessionManager, playerPainter, summaryPanel, summaryHeadline, summaryDetails, replayButton);

            CreateOverchargeButton(canvasGO.transform, playerPainter);
        }

        private TextMeshProUGUI CreateLabel(Transform parent, string name, Vector2 anchoredPosition, TextAlignmentOptions alignment, Vector2 anchor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }
            if (font != null)
            {
                text.font = font;
            }

            text.alignment = alignment;
            text.fontSize = 48f;
            text.text = "";

            RectTransform rect = text.rectTransform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(500f, 80f);

            return text;
        }

        private Button CreateButton(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, string labelText)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image background = go.AddComponent<Image>();
            background.color = new Color(0.95f, 0.75f, 0.25f, 0.9f);

            Button button = go.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;

            TextMeshProUGUI label = CreateLabel(rect, "Label", Vector2.zero, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f));
            label.fontSize = 48f;
            label.text = labelText;

            return button;
        }

        private Slider CreateSlider(Transform parent, string name, Vector2 anchoredPosition, Color fillColor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(400f, 28f);

            Image background = go.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.3f);

            GameObject fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(rect, false);
            RectTransform fillRect = fillGO.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = new Vector2(4f, 4f);
            fillRect.offsetMax = new Vector2(-4f, -4f);

            Image fillImage = fillGO.AddComponent<Image>();
            fillImage.color = fillColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

            Slider slider = go.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.targetGraphic = fillImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.transition = Selectable.Transition.None;

            return slider;
        }

        private void CreateOverchargeButton(Transform parent, PlayerPainter playerPainter)
        {
            GameObject buttonGO = new GameObject("OverchargeButton");
            buttonGO.transform.SetParent(parent, false);

            RectTransform rect = buttonGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 200f);
            rect.sizeDelta = new Vector2(320f, 140f);

            Image background = buttonGO.AddComponent<Image>();
            background.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);

            Button button = buttonGO.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;

            GameObject fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(rect, false);
            RectTransform fillRect = fillGO.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0.08f, 0.15f);
            fillRect.anchorMax = new Vector2(0.92f, 0.85f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImage = fillGO.AddComponent<Image>();
            fillImage.color = new Color(0.9f, 0.8f, 0.2f, 1f);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 0f;

            TextMeshProUGUI label = CreateLabel(rect, "Label", Vector2.zero, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f));
            label.text = "OVERCHARGE";
            label.fontSize = 40f;

            var overchargeButton = buttonGO.AddComponent<OverchargeButton>();
            overchargeButton.Configure(playerPainter, fillImage);
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }
}
