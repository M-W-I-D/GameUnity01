using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    public class Board : MonoBehaviour
    {
        public const int Size = 8;

        private const string BestScoreKey = "BestScore";

        [SerializeField] private Cell cellPrefab;
        [SerializeField] private Transform cellsTransform;

        [Space(0.8f)]
        [SerializeField] private TMP_Text scoreText;

        [SerializeField] private TMP_Text bestScoreText;

        private readonly Cell[,] cells = new Cell[Size, Size];

        // 0 empty
        // 1 hover
        // 2 placed
        private readonly int[,] data = new int[Size, Size];

        private readonly List<Vector2Int> hoverPoints = new();
        private readonly List<int> highlightPolyominoColumns = new();
        private readonly List<int> highlightPolyominoRows = new();
        private readonly List<int> fullLineColumns = new();
        private readonly List<int> fullLineRows = new();

        private Vector2Int previousHoverPoint;
        private bool hasPreviousHoverPoint;
        private readonly List<Vector2Int> previousHoverPoints = new();

        private int score;

        private int bestScore;

        private void Start()
        {
            for (var r = 0; r < Size; r++)
            {
                for (var c = 0; c < Size; c++)
                {
                    cells[r, c] = Instantiate(cellPrefab, cellsTransform);
                    cells[r, c].transform.position = new Vector3(c + 0.5f, r + 0.5f, 0);
                    cells[r, c].Hide();
                }
            }

            score = 0;
            bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
            scoreText.text = score.ToString();
            bestScoreText.text = bestScore.ToString();

            var blockCellWidth = (float)Size / (Block.Size*3 + 3 + 1);
            var offset = new Vector2(0.25f + 0.5f, 0.25f + blockCellWidth*8);
            var gameCamera = Camera.main.GetComponent<GameCamera>();
            gameCamera.View(new Rect(-offset.x, -offset.y, Size + offset.x*2.0f, Size + offset.y + 0.25f), new(Size, Size));
        }

        // =========================
        // HOVER
        // =========================
        public void Hover(Vector2Int point, int polyominoIndex)
        {
            var polyomino = Polyominos.Get(polyominoIndex);
            int rows = polyomino.GetLength(0);
            int cols = polyomino.GetLength(1);

            Unhover();
            Unhighlight();

            highlightPolyominoColumns.Clear();
            highlightPolyominoRows.Clear();

            HoverPoints(point, rows, cols, polyomino);

            if (hoverPoints.Count > 0)
            {
                previousHoverPoint = point;
                hasPreviousHoverPoint = true;
                previousHoverPoints.Clear();
                previousHoverPoints.AddRange(hoverPoints);

                Hover();
                Highlight(point, cols, rows);
            }
            else if (hasPreviousHoverPoint && Mathf.Abs(point.x - previousHoverPoint.x) < 2 && Mathf.Abs(point.y - previousHoverPoint.y) < 2)
            {
                point = previousHoverPoint;
                hoverPoints.Clear();
                hoverPoints.AddRange(previousHoverPoints);

                Hover();
                Highlight(point, cols, rows);
            }
            else
            {
                hasPreviousHoverPoint = false;
                previousHoverPoints.Clear();
            }
        }

        private void HoverPoints(Vector2Int point, int rows, int cols, int[,] polyomino)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (polyomino[r, c] > 0)
                    {
                        Vector2Int hoverPoint = point + new Vector2Int(c, r);
                        if (!IsValidPoint(hoverPoint))
                        {
                            hoverPoints.Clear();
                            return;
                        }
                        hoverPoints.Add(hoverPoint);
                    }
                }
            }
        }

        private bool IsValidPoint(Vector2Int point)
        {
            if (point.x < 0 || point.x >= Size) return false;
            if (point.y < 0 || point.y >= Size) return false;
            if (data[point.y, point.x] > 0) return false;
            return true;
        }

        private void Hover()
        {
            foreach (var p in hoverPoints)
            {
                data[p.y, p.x] = 1;
                cells[p.y, p.x].Hover();
            }
        }

        private void Unhover()
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
        public bool Place(Vector2Int point, int polyominoIndex)
        {
            var polyomino = Polyominos.Get(polyominoIndex);
            int rows = polyomino.GetLength(0);
            int cols = polyomino.GetLength(1);

            Unhover();
            HoverPoints(point, rows, cols, polyomino);

            if (hoverPoints.Count > 0)
            {
                Place(point, cols, rows);

                hasPreviousHoverPoint = false;
                previousHoverPoints.Clear();
                return true;
            }
            else if (hasPreviousHoverPoint && Mathf.Abs(point.x - previousHoverPoint.x) < 2 && Mathf.Abs(point.y - previousHoverPoint.y) < 2)
            {
                point = previousHoverPoint;
                hoverPoints.Clear();
                hoverPoints.AddRange(previousHoverPoints);

                Place(point, cols, rows);
                hasPreviousHoverPoint = false;
                previousHoverPoints.Clear();
                return true;
            }

            hasPreviousHoverPoint = false;
            previousHoverPoints.Clear();
            return false;
        }

        private void Place(Vector2Int point, int cols, int rows)
        {
            foreach (var p in hoverPoints)
            {
                data[p.y, p.x] = 2;
                cells[p.y, p.x].Normal();
            }

            ClearFullLines(point, cols, rows);
            hoverPoints.Clear();
        }

        // =========================
        // CLEAR LINE
        // =========================
        private void ClearFullLines(Vector2Int point, int cols, int rows)
        {
            FullLineColumns(point.x, point.x + cols);
            FullLineRows(point.y, point.y + rows);

            AddScore(fullLineColumns.Count * Size + fullLineRows.Count * Size);

            ClearFullLineColumns();
            ClearFullLineRows();
        }

        private void FullLineColumns(int fromColumn, int toColumnExclusive)
        {
            fullLineColumns.Clear();
            for (int c = fromColumn; c < toColumnExclusive; c++)
            {
                bool isFullLine = true;
                for (int r = 0; r < Size; r++)
                {
                    if (data[r, c] != 2)
                    {
                        isFullLine = false;
                        break;
                    }
                }
                if (isFullLine) fullLineColumns.Add(c);
            }
        }

        private void FullLineRows(int fromRow, int toRowExclusive)
        {
            fullLineRows.Clear();
            for (int r = fromRow; r < toRowExclusive; r++)
            {
                bool isFullLine = true;
                for (int c = 0; c < Size; c++)
                {
                    if (data[r, c] != 2)
                    {
                        isFullLine = false;
                        break;
                    }
                }
                if (isFullLine) fullLineRows.Add(r);
            }
        }

        private void ClearFullLineColumns()
        {
            foreach (var c in fullLineColumns)
            {
                for (int r = 0; r < Size; r++)
                {
                    data[r, c] = 0;
                    cells[r, c].Hide();
                }
            }
        }

        private void ClearFullLineRows()
        {
            foreach (var r in fullLineRows)
            {
                for (int c = 0; c < Size; c++)
                {
                    data[r, c] = 0;
                    cells[r, c].Hide();
                }
            }
        }

        // =========================
        // HIGHLIGHT
        // =========================
        private void Highlight(Vector2Int point, int cols, int rows)
        {
            PredictFullLineColumns(point.x, point.x + cols);
            PredictFullLineRows(point.y, point.y + rows);

            HighlightFullLineColumns();
            HighlightFullLineRows();

            foreach (var fullLineColumn in fullLineColumns)
            {
                highlightPolyominoColumns.Add(fullLineColumn - point.x);
            }

            foreach (var fullLineRow in fullLineRows)
            {
                highlightPolyominoRows.Add(fullLineRow - point.y);
            }
        }

        private void PredictFullLineColumns(int fromColumn, int toColumnExclusive)
        {
            fullLineColumns.Clear();
            for (int c = fromColumn; c < toColumnExclusive; c++)
            {
                bool isFullLine = true;
                for (int r = 0; r < Size; r++)
                {
                    if (data[r, c] != 1 && data[r, c] != 2)
                    {
                        isFullLine = false;
                        break;
                    }
                }
                if (isFullLine) fullLineColumns.Add(c);
            }
        }

        private void PredictFullLineRows(int fromRow, int toRowExclusive)
        {
            fullLineRows.Clear();
            for (int r = fromRow; r < toRowExclusive; r++)
            {
                bool isFullLine = true;
                for (int c = 0; c < Size; c++)
                {
                    if (data[r, c] != 1 && data[r, c] != 2)
                    {
                        isFullLine = false;
                        break;
                    }
                }
                if (isFullLine) fullLineRows.Add(r);
            }
        }

        private void HighlightFullLineColumns()
        {
            foreach (var c in fullLineColumns)
            {
                for (int r = 0; r < Size; r++)
                {
                    cells[r, c].Highlight();
                }
            }
        }

        private void HighlightFullLineRows()
        {
            foreach (var r in fullLineRows)
            {
                for (int c = 0; c < Size; c++)
                {
                    cells[r, c].Highlight();
                }
            }
        }

        private void Unhighlight()
        {
            foreach (var c in fullLineColumns)
            {
                for (int r = 0; r < Size; r++)
                {
                    if (data[r, c] == 2) cells[r, c].Normal();
                }
            }
            foreach (var r in fullLineRows)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (data[r, c] == 2) cells[r, c].Normal();
                }
            }
        }

        public bool CheckPlace(int polyominoIndex)
        {
            var polyomino = Polyominos.Get(polyominoIndex);
            int rows = polyomino.GetLength(0);
            int cols = polyomino.GetLength(1);
            for (var r = 0; r < Size - rows; ++r)
            {
                for (var c = 0; c < Size - cols; ++c)
                {
                    if (CheckPlace(c, r, cols, rows, polyomino) == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckPlace(int column, int row, int cols, int rows, int[,] polyomino)
        {
            for (var r = 0; r < rows; ++r)
            {
                for (var c = 0; c < cols; ++c)
                {
                    if (polyomino[r, c] > 0 && data[row + r, column + c] == 2)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void AddScore(int amount)
        {
            score += amount;
            if(score > bestScore)
            {
                bestScore = score;
                PlayerPrefs.SetInt(BestScoreKey, bestScore);
                bestScoreText.text = bestScore.ToString();
            }
            scoreText.text = score.ToString();
            bestScoreText.text = bestScore.ToString();
        }
        public List<int> HighlightPolyominoColumns => highlightPolyominoColumns;
        public List<int> HighlightPolyominoRows => highlightPolyominoRows;

    }
}