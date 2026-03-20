using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    public class Board : MonoBehaviour
    {
        public const int Size = 8;
        private const string BestScoreKey = "BestScore";

        [Header("References")]
        [SerializeField] private Cell cellPrefab;
        [SerializeField] private Transform cellsTransform;

        [Header("UI")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text bestScoreText;

        private readonly Cell[,] cells = new Cell[Size, Size];
        private readonly int[,] data = new int[Size, Size]; // 0 empty, 1 hover, 2 placed

        private readonly List<Vector2Int> hoverPoints = new();
        private readonly List<int> fullLineColumns = new();
        private readonly List<int> fullLineRows = new();

        private readonly List<int> highlightColumns = new();
        private readonly List<int> highlightRows = new();

        private int score;
        private int bestScore;

        // =========================
        // INIT
        // =========================
        private void Start()
        {
            GenerateGrid();
            InitScore();
            SetupCamera();
        }

        private void GenerateGrid()
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    var cell = Instantiate(cellPrefab, cellsTransform);
                    cell.transform.position = new Vector3(c + 0.5f, r + 0.5f, 0);
                    cell.Hide();

                    cells[r, c] = cell;
                }
            }
        }

        private void InitScore()
        {
            score = 0;
            bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);

            UpdateScoreUI();
        }

        private void SetupCamera()
        {
            var cam = Camera.main?.GetComponent<GameCamera>();

            if (cam == null)
            {
                Debug.LogError("GameCamera not found!");
                return;
            }

            cam.View(Size);
        }

        // =========================
        // HOVER
        // =========================
        public void Hover(Vector2Int origin, int polyIndex)
        {
            ClearHover();
            ClearHighlight();

            var shape = Polyominos.Get(polyIndex);

            if (!TryGetHoverPoints(origin, shape, out var points))
                return;

            hoverPoints.AddRange(points);

            foreach (var p in hoverPoints)
            {
                data[p.y, p.x] = 1;
                cells[p.y, p.x].Hover();
            }

            PredictHighlight(origin, shape);
        }

        private bool TryGetHoverPoints(Vector2Int origin, int[,] shape, out List<Vector2Int> result)
        {
            result = new List<Vector2Int>();

            int rows = shape.GetLength(0);
            int cols = shape.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (shape[r, c] == 0) continue;

                    var point = origin + new Vector2Int(c, r);

                    if (!IsValid(point))
                        return false;

                    result.Add(point);
                }
            }

            return true;
        }

        private bool IsValid(Vector2Int p)
        {
            return p.x >= 0 && p.x < Size &&
                   p.y >= 0 && p.y < Size &&
                   data[p.y, p.x] == 0;
        }

        private void ClearHover()
        {
            foreach (var p in hoverPoints)
            {
                data[p.y, p.x] = 0;
                cells[p.y, p.x].Hide();
            }

            hoverPoints.Clear();
        }

        // =========================
        // PLACE
        // =========================
        public bool Place(Vector2Int origin, int polyIndex)
        {
            ClearHover();

            var shape = Polyominos.Get(polyIndex);

            if (!TryGetHoverPoints(origin, shape, out var points))
                return false;

            foreach (var p in points)
            {
                data[p.y, p.x] = 2;
                cells[p.y, p.x].Normal();
            }

            ClearLines(points);

            return true;
        }

        // =========================
        // CLEAR LINE
        // =========================
        private void ClearLines(List<Vector2Int> placedPoints)
        {
            fullLineColumns.Clear();
            fullLineRows.Clear();

            foreach (var p in placedPoints)
            {
                CheckColumn(p.x);
                CheckRow(p.y);
            }

            int cleared = fullLineColumns.Count + fullLineRows.Count;
            if (cleared > 0)
                AddScore(cleared * Size);

            foreach (var c in fullLineColumns)
            {
                for (int r = 0; r < Size; r++)
                {
                    data[r, c] = 0;
                    cells[r, c].Hide();
                }
            }

            foreach (var r in fullLineRows)
            {
                for (int c = 0; c < Size; c++)
                {
                    data[r, c] = 0;
                    cells[r, c].Hide();
                }
            }
        }

        private void CheckColumn(int c)
        {
            if (fullLineColumns.Contains(c)) return;

            for (int r = 0; r < Size; r++)
            {
                if (data[r, c] != 2)
                    return;
            }

            fullLineColumns.Add(c);
        }

        private void CheckRow(int r)
        {
            if (fullLineRows.Contains(r)) return;

            for (int c = 0; c < Size; c++)
            {
                if (data[r, c] != 2)
                    return;
            }

            fullLineRows.Add(r);
        }

        // =========================
        // HIGHLIGHT (PREVIEW)
        // =========================
        private void PredictHighlight(Vector2Int origin, int[,] shape)
        {
            highlightColumns.Clear();
            highlightRows.Clear();

            int rows = shape.GetLength(0);
            int cols = shape.GetLength(1);

            for (int c = 0; c < cols; c++)
            {
                if (IsColumnFullPreview(origin.x + c))
                {
                    highlightColumns.Add(c);
                    HighlightColumn(origin.x + c);
                }
            }

            for (int r = 0; r < rows; r++)
            {
                if (IsRowFullPreview(origin.y + r))
                {
                    highlightRows.Add(r);
                    HighlightRow(origin.y + r);
                }
            }
        }

        private bool IsColumnFullPreview(int c)
        {
            for (int r = 0; r < Size; r++)
            {
                if (data[r, c] == 0)
                    return false;
            }
            return true;
        }

        private bool IsRowFullPreview(int r)
        {
            for (int c = 0; c < Size; c++)
            {
                if (data[r, c] == 0)
                    return false;
            }
            return true;
        }

        private void HighlightColumn(int c)
        {
            for (int r = 0; r < Size; r++)
                cells[r, c].Highlight();
        }

        private void HighlightRow(int r)
        {
            for (int c = 0; c < Size; c++)
                cells[r, c].Highlight();
        }

        private void ClearHighlight()
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (data[r, c] == 2)
                        cells[r, c].Normal();
                }
            }
        }

        // =========================
        // CHECK PLACE (GAME OVER)
        // =========================
        public bool CheckPlace(int polyIndex)
        {
            var shape = Polyominos.Get(polyIndex);

            int rows = shape.GetLength(0);
            int cols = shape.GetLength(1);

            for (int r = 0; r <= Size - rows; r++)
            {
                for (int c = 0; c <= Size - cols; c++)
                {
                    if (TryGetHoverPoints(new Vector2Int(c, r), shape, out _))
                        return true;
                }
            }

            return false;
        }

        // =========================
        // SCORE
        // =========================
        private void AddScore(int amount)
        {
            score += amount;

            if (score > bestScore)
            {
                bestScore = score;
                PlayerPrefs.SetInt(BestScoreKey, bestScore);
            }

            UpdateScoreUI();
        }

        private void UpdateScoreUI()
        {
            scoreText.text = score.ToString();
            bestScoreText.text = bestScore.ToString();
        }

        // =========================
        // GETTERS
        // =========================
        public List<int> HighlightPolyominoColumns => highlightColumns;
        public List<int> HighlightPolyominoRows => highlightRows;
    }


}