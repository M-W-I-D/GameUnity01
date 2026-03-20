using UnityEngine;

namespace Game
{
    public class Blocks : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Board board;
        [SerializeField] private Block[] blocks;

        [Header("UI")]
        [SerializeField] private GameObject loseGameObject;

        private int[] polyIndexes;
        private int activeBlockCount;

        // =========================
        // INIT
        // =========================
        private void Start()
        {
            if (blocks == null || blocks.Length == 0)
            {
                Debug.LogError("Blocks not assigned!");
                return;
            }

            polyIndexes = new int[blocks.Length];

            SetupBlocks();
            Generate();
        }

        private void SetupBlocks()
        {
            float width = (float)Board.Size / blocks.Length;
            float cellSize = (float)Board.Size / (Block.Size * blocks.Length + blocks.Length + 1);

            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] == null)
                {
                    Debug.LogError($"Block[{i}] is null!");
                    continue;
                }

                blocks[i].transform.localPosition = new Vector3(
                    width * (i + 0.5f),
                    -0.25f - cellSize * 4f,
                    0f
                );

                blocks[i].transform.localScale = Vector3.one * cellSize;
                blocks[i].Initialize();
            }
        }

        // =========================
        // GENERATE BLOCKS
        // =========================
        private void Generate()
        {
            activeBlockCount = 0;

            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] == null) continue;

                polyIndexes[i] = Random.Range(0, Polyominos.Length);

                blocks[i].gameObject.SetActive(true);
                blocks[i].Show(polyIndexes[i]);

                activeBlockCount++;
            }
        }

        // =========================
        // REMOVE BLOCK
        // =========================
        public void Remove()
        {
            activeBlockCount--;

            if (activeBlockCount <= 0)
            {
                Generate();
            }

            CheckLose();
        }

        // =========================
        // CHECK LOSE
        // =========================
        private void CheckLose()
        {
            if (board == null)
            {
                Debug.LogError("Board not assigned!");
                return;
            }

            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] == null) continue;

                if (!blocks[i].gameObject.activeSelf) continue;

                if (board.CheckPlace(polyIndexes[i]))
                {
                    // còn chỗ đặt → chưa thua
                    return;
                }
            }

            // không block nào đặt được → thua
            Lose();
        }

        // =========================
        // LOSE
        // =========================
        private void Lose()
        {
            if (loseGameObject != null)
            {
                loseGameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Lose UI not assigned!");
            }

            // ❌ không auto reload nữa
        }

        // =========================
        // SORTING
        // =========================
        public void ResetBlocksSortingOrders()
        {
            if (blocks == null) return;

            foreach (var block in blocks)
            {
                if (block == null) continue;
                block.SetSortingOrder(0);
            }
        }
    }
}