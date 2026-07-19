using Comfort.Common;
using EFT;
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
        private Image radarback;

        [SerializeField]
        private Image enemy01;

        [SerializeField]
        private Image enemy02;

        [SerializeField]
        private Image enemy03;

        [SerializeField]
        private Image enemy04;

        [SerializeField]
        private Image enemy05;

        [SerializeField]
        private Image enemy06;

        [SerializeField]
        private Image enemy07;

        [SerializeField]
        private Image enemy08;

        [SerializeField]
        private Image enemy09;

        [SerializeField]
        private Image enemy10;

        [SerializeField]
        private float maxScanDistance = 60f;

        private const float ScanInterval = 0.25f;

        private static GameWorld gameWorld;
        private Image[] enemyImages;
        private RectTransform[] enemyMarkers;
        private RectTransform radarBackRect;
        private float nextScanTime;

        private static bool Entermap()
        {
            return Singleton<GameWorld>.Instantiated;
        }

        private void OnEnable()
        {
            CacheUiReferences();
            if (!ValidateUiReferences())
            {
                enabled = false;
                return;
            }

            gameWorld = Entermap() ? Singleton<GameWorld>.Instance : null;
            nextScanTime = Time.unscaledTime;
            HideAllMarkers();
        }

        private void OnDisable()
        {
            HideAllMarkers();
            gameWorld = null;
        }

        private void Update()
        {
            if (gameWorld == null || Time.unscaledTime < nextScanTime)
            {
                return;
            }

            nextScanTime = Time.unscaledTime + ScanInterval;
            ScanRadar();
        }

        private void ScanRadar()
        {
            Player mainPlayer = gameWorld.MainPlayer;
            if (mainPlayer == null)
            {
                HideAllMarkers();
                return;
            }

            Transform playerTransform = mainPlayer.Transform.Original;
            radarBackRect.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                transform.eulerAngles.y,
                playerTransform.eulerAngles.y + 180f);

            HideAllMarkers();

            float radarRadius = Mathf.Min(radarBackRect.rect.width, radarBackRect.rect.height) * 0.5f;
            if (radarRadius <= 0f || maxScanDistance <= 0f)
            {
                return;
            }

            float maxDistanceSquared = maxScanDistance * maxScanDistance;
            int markerIndex = 0;
            var registeredPlayers = gameWorld.RegisteredPlayers;

            for (int i = 0; i < registeredPlayers.Count && markerIndex < enemyMarkers.Length; i++)
            {
                IPlayer currentPlayer = registeredPlayers[i];
                if (currentPlayer == null || ReferenceEquals(currentPlayer, mainPlayer))
                {
                    continue;
                }

                Vector3 offset = currentPlayer.Position - playerTransform.position;
                Vector2 planarOffset = new Vector2(offset.x, offset.z);
                if (planarOffset.sqrMagnitude > maxDistanceSquared)
                {
                    continue;
                }

                RectTransform marker = enemyMarkers[markerIndex];
                marker.anchoredPosition = -planarOffset * (radarRadius / maxScanDistance);
                marker.gameObject.SetActive(true);
                markerIndex++;
            }
        }

        private void CacheUiReferences()
        {
            enemyImages = new[]
            {
                enemy01,
                enemy02,
                enemy03,
                enemy04,
                enemy05,
                enemy06,
                enemy07,
                enemy08,
                enemy09,
                enemy10
            };

            radarBackRect = radarback != null ? radarback.rectTransform : null;
            enemyMarkers = new RectTransform[enemyImages.Length];
            for (int i = 0; i < enemyImages.Length; i++)
            {
                enemyMarkers[i] = enemyImages[i] != null ? enemyImages[i].rectTransform : null;
            }
        }

        private bool ValidateUiReferences()
        {
            if (radarBackRect == null)
            {
                Debug.LogError("[Mass.Radar] Radar background is not assigned.", this);
                return false;
            }

            for (int i = 0; i < enemyMarkers.Length; i++)
            {
                if (enemyMarkers[i] == null)
                {
                    Debug.LogError($"[Mass.Radar] Enemy marker {i + 1} is not assigned.", this);
                    return false;
                }
            }

            return true;
        }

        private void HideAllMarkers()
        {
            if (enemyMarkers == null)
            {
                return;
            }

            for (int i = 0; i < enemyMarkers.Length; i++)
            {
                if (enemyMarkers[i] != null)
                {
                    enemyMarkers[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
