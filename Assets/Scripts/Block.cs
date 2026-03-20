using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Game
{
    public class Block : MonoBehaviour
    {
        public const int Size = 5;

        [Header("References")]
        [SerializeField] private Board board;
        [SerializeField] private Blocks blocks;
        [SerializeField] private Cell cellPrefab;

        private SortingGroup sortingGroup;
        private Camera cam;

        private readonly Cell[,] cells = new Cell[Size, Size];

        private int polyIndex;

        private Vector3 startPosition;
        private Vector3 startScale;

        private Vector3 dragStartWorld;
        private Vector2 shapeCenter;

        private Vector2Int currentGridPos;
        private Vector2Int lastGridPos;

        private bool isDragging;

        // =========================
        // INIT
        // =========================
        private void Awake()
        {
            sortingGroup = GetComponent<SortingGroup>();
            cam = Camera.main;
        }

        public void Initialize()
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    cells[r, c] = Instantiate(cellPrefab, transform);
                }
            }

            startPosition = transform.localPosition;
            startScale = transform.localScale;
        }

        // =========================
        // SHOW SHAPE
        // =========================
        public void Show(int index)
        {
            polyIndex = index;
            HideAll();

            var shape = Polyominos.Get(index);

            int rows = shape.GetLength(0);
            int cols = shape.GetLength(1);

            shapeCenter = new Vector2(cols * 0.5f, rows * 0.5f);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (shape[r, c] == 0) continue;

                    cells[r, c].transform.localPosition =
                        new Vector3(c - shapeCenter.x + 0.5f, r - shapeCenter.y + 0.5f, 0);

                    cells[r, c].Normal();
                }
            }
        }

        private void HideAll()
        {
            for (int r = 0; r < Size; r++)
                for (int c = 0; c < Size; c++)
                    cells[r, c].Hide();
        }

        // =========================
        // INPUT
        // =========================
        private void OnMouseDown()
        {
            isDragging = true;

            dragStartWorld = GetMouseWorld();

            transform.localPosition = startPosition + Vector3.up * 2f;
            transform.localScale = Vector3.one;

            blocks.ResetBlocksSortingOrders();
            sortingGroup.sortingOrder = 1;

            UpdateGridPosition();
            board.Hover(currentGridPos, polyIndex);
            HighlightPreview();

            lastGridPos = currentGridPos;
        }

        private void OnMouseDrag()
        {
            if (!isDragging) return;

            Vector3 currentWorld = GetMouseWorld();
            Vector3 delta = currentWorld - dragStartWorld;

            transform.localPosition = startPosition + Vector3.up * 2f + delta * 1.4f;

            UpdateGridPosition();

            if (currentGridPos != lastGridPos)
            {
                lastGridPos = currentGridPos;

                board.Hover(currentGridPos, polyIndex);
                HighlightPreview();
            }
        }

        private void OnMouseUp()
        {
            if (!isDragging) return;

            isDragging = false;

            UpdateGridPosition();

            if (board.Place(currentGridPos, polyIndex))
            {
                gameObject.SetActive(false);
                blocks.Remove();
            }

            ResetTransform();
        }

        // =========================
        // GRID CALCULATION
        // =========================
        private void UpdateGridPosition()
        {
            Vector2 pos = transform.position;

            currentGridPos = new Vector2Int(
                Mathf.RoundToInt(pos.x - shapeCenter.x),
                Mathf.RoundToInt(pos.y - shapeCenter.y)
            );
        }

        private Vector3 GetMouseWorld()
        {
            Vector3 mouse = Input.mousePosition;
            mouse.z = Mathf.Abs(cam.transform.position.z);
            return cam.ScreenToWorldPoint(mouse);
        }

        // =========================
        // VISUAL
        // =========================
        private void HighlightPreview()
        {
            var shape = Polyominos.Get(polyIndex);

            int rows = shape.GetLength(0);
            int cols = shape.GetLength(1);

            ResetHighlight(shape, rows, cols);

            ApplyHighlight(shape, rows, cols, board.HighlightPolyominoColumns, true);
            ApplyHighlight(shape, rows, cols, board.HighlightPolyominoRows, false);
        }

        private void ResetHighlight(int[,] shape, int rows, int cols)
        {
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (shape[r, c] > 0)
                        cells[r, c].Normal();
        }

        private void ApplyHighlight(int[,] shape, int rows, int cols, List<int> list, bool isColumn)
        {
            foreach (var index in list)
            {
                // ❗ bỏ qua nếu vượt shape
                if (isColumn && (index < 0 || index >= cols)) continue;
                if (!isColumn && (index < 0 || index >= rows)) continue;

                for (int i = 0; i < (isColumn ? rows : cols); i++)
                {
                    int r = isColumn ? i : index;
                    int c = isColumn ? index : i;

                    // ❗ double check an toàn
                    if (r < 0 || r >= rows || c < 0 || c >= cols) continue;

                    if (shape[r, c] > 0)
                    {
                        cells[r, c].Highlight();
                    }
                }
            }
        }

        private void ResetTransform()
        {
            transform.localPosition = startPosition;
            transform.localScale = startScale;
        }

        public void SetSortingOrder(int order)
        {
            sortingGroup.sortingOrder = order;
        }
    }
}