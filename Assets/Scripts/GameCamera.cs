using UnityEngine;
using UnityEngine.Assertions;

namespace Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class GameCamera : MonoBehaviour
    {
        [SerializeField] private Transform backgroundTransform;
        [SerializeField] private RectTransform scoresRectTransform;

        private Camera mainCamera;

        private Rect viewFrameRect;
        private Rect viewRect;

        private Vector2Int boardSize;

        private void Awake()
        {
            Assert.IsNotNull(backgroundTransform);
            Assert.IsNotNull(scoresRectTransform);

            mainCamera = gameObject.GetComponent<Camera>();
        }

        // =========================
        // PUBLIC API
        // =========================
        public void ViewFrame(Rect rect)
        {
            viewFrameRect = rect;
            Apply();
        }

        public void View(Rect rect, Vector2Int boardSize)
        {
            viewRect = rect;
            this.boardSize = boardSize;

            Apply();
        }

        // =========================
        // APPLY CAMERA
        // =========================
        public void Apply()
        {
            var height = viewRect.height;
            var orthographicSize = height * 0.5f;

            mainCamera.orthographicSize = orthographicSize;

            var center = viewFrameRect.center;

            transform.position = new Vector3(
            viewRect.center.x,
            viewRect.center.y,
            transform.position.z
        );

            // Background follow camera
            backgroundTransform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                0.0f
            );

            // Scale background
            var scaleFactor = Mathf.Max(
                height * mainCamera.aspect / 1080.0f,
                height / 1920.0f
            ) * 100.0f;

            backgroundTransform.localScale = new Vector3(
                scaleFactor,
                scaleFactor,
                scaleFactor
            );

            // =========================
            // UI SCORE POSITION
            // =========================
            var screenPoint = mainCamera.WorldToScreenPoint(
                new Vector3(boardSize.x * 0.5f, boardSize.y + 0.25f, 0.0f)
            );

            if (
                !float.IsNaN(screenPoint.x) &&
                !float.IsNaN(screenPoint.y) &&
                !float.IsNaN(screenPoint.z) &&
                !float.IsInfinity(screenPoint.x) &&
                !float.IsInfinity(screenPoint.y) &&
                !float.IsInfinity(screenPoint.z)
            )
            {
                RectTransform parentRect = scoresRectTransform.parent.GetComponent<RectTransform>();

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    screenPoint,
                    null,
                    out Vector2 localPoint))
                {
                    scoresRectTransform.localPosition = localPoint;
                }
            }
        }
    }
}