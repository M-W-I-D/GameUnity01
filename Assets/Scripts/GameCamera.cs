using UnityEngine;

namespace Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class GameCamera : MonoBehaviour
    {
        [SerializeField] private Transform backgroundTransform;

        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        public void View(int boardSize)
        {
            Apply(boardSize);
        }

        private void Apply(int size)
        {
            // 🔥 chia rõ không gian
            float bottomSpace = 6f; // chỗ để block
            float topSpace = 4f;    // chỗ cho UI

            float totalHeight = size + bottomSpace + topSpace;

            // ZOOM
            cam.orthographicSize = totalHeight * 0.5f;

            // 🔥 CENTER CHUẨN (fix lệch)
            float centerY = size * 0.5f + (bottomSpace - topSpace) * 0.5f;

            Vector3 center = new Vector3(size * 0.5f, centerY, -10f);
            transform.position = center;

            // BACKGROUND
            if (backgroundTransform != null)
            {
                backgroundTransform.position = new Vector3(center.x, center.y, 0f);

                float scale = Mathf.Max(totalHeight, totalHeight * cam.aspect);
                backgroundTransform.localScale = Vector3.one * scale;
            }
        }
    }
}