using Comfort.Common;
using EFT;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mass.Radar
{
    /// <summary>
    /// Displays registered players on a UI radar.
    /// </summary>
    public class RadarController : MonoBehaviour
    {
        [SerializeField]
        private float MaxRadarDistance = 60f;
        [SerializeField]
        private float ScanInterval = 0.25f;

        [SerializeField]
        private Image radarback = null;

        [SerializeField]
        private RectTransform markerContainer = null;

        [SerializeField]
        private Image markerTemplate = null;

        [SerializeField]
        [Min(1)]
        private int maxMarkerCount = 32;

        private GameWorld gameWorld;
        private readonly List<RectTransform> markerPool = new List<RectTransform>();
        private RectTransform radarRectTransform;
        private float markerHalfWidth;
        private float markerHalfHeight;
        private float nextScanTime;

        private void OnEnable()
        {
            gameWorld = Singleton<GameWorld>.Instantiated
                ? Singleton<GameWorld>.Instance
                : null;

            if (!TryCacheUiReferences())
            {
                enabled = false;
                return;
            }

            nextScanTime = 0f;
            HideMarkersFrom(0);
        }

        private void OnDisable()
        {
            gameWorld = null;
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScanTime)
            {
                return;
            }

            nextScanTime = Time.unscaledTime + ScanInterval;
            RefreshRadar();
        }

        private void RefreshRadar()
        {
            if (gameWorld == null)
            {
                HideMarkersFrom(0);
                return;
            }

            Player mainPlayer = gameWorld.MainPlayer;
            if (mainPlayer == null)
            {
                HideMarkersFrom(0);
                return;
            }

            Transform playerTransform = mainPlayer.Transform.Original;
            radarRectTransform.localEulerAngles = Vector3.zero;

            float halfWidth = radarRectTransform.rect.width * 0.5f;
            float height = radarRectTransform.rect.height;
            float horizontalRadius = Mathf.Max(0f, halfWidth - markerHalfWidth);
            float verticalRadius = Mathf.Max(0f, height - markerHalfHeight * 2f);
            float radarRadius = Mathf.Min(horizontalRadius, verticalRadius);
            float maxDistanceSquared = MaxRadarDistance * MaxRadarDistance;
            Vector3 radarForward = mainPlayer.LookDirection;
            radarForward.y = 0f;
            if (radarForward.sqrMagnitude <= Mathf.Epsilon)
            {
                HideMarkersFrom(0);
                return;
            }

            radarForward.Normalize();
            Vector3 radarRight = Vector3.Cross(Vector3.up, radarForward);
            int markerIndex = 0;
            var registeredPlayers = gameWorld.RegisteredPlayers;

            for (int i = 0; i < registeredPlayers.Count && markerIndex < maxMarkerCount; i++)
            {
                IPlayer currentPlayer = registeredPlayers[i];
                if (currentPlayer == null || ReferenceEquals(currentPlayer, mainPlayer))
                {
                    continue;
                }

                Vector3 offset = currentPlayer.Position - playerTransform.position;
                offset.y = 0f;
                float forwardDistance = Vector3.Dot(offset, radarForward);
                if (forwardDistance <= 0f || offset.sqrMagnitude > maxDistanceSquared)
                {
                    continue;
                }

                float rightDistance = Vector3.Dot(offset, radarRight);
                float markerX = rightDistance / MaxRadarDistance * radarRadius;
                float markerY = markerHalfHeight + forwardDistance / MaxRadarDistance * radarRadius;

                RectTransform markerRectTransform = GetOrCreateMarker(markerIndex);
                markerRectTransform.gameObject.SetActive(true);
                markerRectTransform.anchoredPosition = new Vector2(
                    Mathf.Clamp(markerX, -horizontalRadius, horizontalRadius),
                    Mathf.Clamp(markerY, markerHalfHeight, height - markerHalfHeight));
                markerIndex++;
            }

            HideMarkersFrom(markerIndex);
        }

        private bool TryCacheUiReferences()
        {
            if (radarback == null)
            {
                Debug.LogError("[Mass.Radar] Radar background is not assigned.", this);
                return false;
            }

            if (markerContainer == null)
            {
                Debug.LogError("[Mass.Radar] Marker container is not assigned.", this);
                return false;
            }

            if (markerTemplate == null)
            {
                Debug.LogError("[Mass.Radar] Marker template is not assigned.", this);
                return false;
            }

            if (maxMarkerCount < 1)
            {
                Debug.LogError("[Mass.Radar] Max marker count must be at least 1.", this);
                return false;
            }

            radarRectTransform = radarback.rectTransform;
            Rect markerRect = markerTemplate.rectTransform.rect;
            markerHalfWidth = Mathf.Abs(markerRect.width) * 0.5f;
            markerHalfHeight = Mathf.Abs(markerRect.height) * 0.5f;
            markerTemplate.gameObject.SetActive(false);
            return true;
        }

        private RectTransform GetOrCreateMarker(int index)
        {
            while (markerPool.Count <= index)
            {
                Image marker = Instantiate(markerTemplate, markerContainer, false);
                marker.name = $"RadarMarker_{markerPool.Count + 1:D2}";
                marker.gameObject.SetActive(false);
                markerPool.Add(marker.rectTransform);
            }

            return markerPool[index];
        }

        private void HideMarkersFrom(int firstUnusedIndex)
        {
            for (int i = firstUnusedIndex; i < markerPool.Count; i++)
            {
                markerPool[i].gameObject.SetActive(false);
            }
        }
    }
}
