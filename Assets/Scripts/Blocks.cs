using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class Blocks : MonoBehaviour
    {
        [SerializeField] private Board board;

        [SerializeField] private Block[] blocks;

        [Space(8.0f)]
        [SerializeField] private GameObject loseGameObject;

        private int[] polyominoIndexes;

        private int blockCount = 0;

        private void Start()
        {
            if (blocks == null || blocks.Length == 0)
            {
                Debug.LogError("Blocks array is not assigned or empty on " + name + ". Initialization aborted.");
                return;
            }

            var blockWidth = (float)Board.Size / blocks.Length;
            var cellSize = (float)Board.Size / (Block.Size * blocks.Length + blocks.Length + 1);
            for (var i = 0; i < blocks.Length; ++i)
            {
                if (blocks[i] == null)
                {
                    Debug.LogError($"blocks[{i}] is null on {name}.");
                    continue;
                }

                blocks[i].transform.localPosition = new(blockWidth * (i + 0.5f), -0.25f - cellSize * 4.0f, 0.0f);
                blocks[i].transform.localScale = new(cellSize, cellSize, cellSize);
                blocks[i].Initialize();
            }

            polyominoIndexes = new int[blocks.Length];

            Generate();
        }

        private void Generate()
        {
            if (blocks == null || blocks.Length == 0) return;

            blockCount = 0;
            for (var i = 0; i < blocks.Length; ++i)
            {
                polyominoIndexes[i] = Random.Range(0, Polyominos.Length);
                if (blocks[i] == null) continue;
                blocks[i].gameObject.SetActive(true);
                blocks[i].Show(polyominoIndexes[i]);
                ++blockCount;
            }
        }

        public void Remove()
        {
            --blockCount;
            if (blockCount <= 0)
            {
                blockCount = 0;
                Generate();
            }

            if (board == null)
            {
                Debug.LogError("Board reference not set on " + name + ". Cannot check for lose condition.");
                return;
            }

            var lose = true;
            for (var i = 0; i < blocks.Length; ++i)
            {
                if (blocks[i] == null) continue;
                if (blocks[i].gameObject.activeSelf == true && board.CheckPlace(polyominoIndexes[i]) == true)
                {
                    lose = false;
                    break;
                }
            }

            if (lose == true)
            {
                Lose();
            }
        }

        public void ResetBlocksSortingOrders()
        {
            if (blocks == null) return;
            for (var i = 0; i < blocks.Length; ++i)
            {
                if (blocks[i] == null) continue;
                blocks[i].SetSortingOrder(0);
            }
        }

        private void Lose()
        {
            if (loseGameObject != null)
            {
                loseGameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("loseGameObject is not assigned.");
            }

            StartCoroutine(DelayAndLoseCoroutine());
        }

        private IEnumerator DelayAndLoseCoroutine()
        {
            yield return new WaitForSeconds(3.0f);
            SceneManager.LoadScene("Game");
        }
    }
}