using UnityEngine;

namespace Game
{
    public static class Polyominos
    {
        private static readonly int[][,] polyominos = new int[][,]
        {
        // L shape
        new int[,]
        {
            {0,0,1},
            {0,0,1},
            {1,1,1}
        },

        new int[,]
        {
            {1,1,1},
            {1,0,0},
            {1,0,0}
        },

        new int[,]
        {
            {1,1,1},
            {0,0,1},
            {0,0,1}
        },

        new int[,]
        {
            {0,0,1},
            {0,0,1},
            {1,1,1}
        },

        // Square
        new int[,]
        {
            {1,1},
            {1,1}
        },

        // Line 4
        new int[,]
        {
            {1,1,1,1}
        },

        // Line vertical
        new int[,]
        {
            {1},
            {1},
            {1},
            {1}
        },

        // T shape
        new int[,]
        {
            {1,1,1},
            {0,1,0}
        },

        // Z shape
        new int[,]
        {
            {1,1,0},
            {0,1,1}
        },

        // S shape
        new int[,]
        {
            {0,1,1},
            {1,1,0}
        },

        // Plus shape
        new int[,]
        {
            {0,1,0},
            {1,1,1},
            {0,1,0}
        },

        // Small L
        new int[,]
        {
            {1,0},
            {1,1}
        },

        // Rectangle 2x3
        new int[,]
        {
            {1,1,1},
            {1,1,1}
        }
        };

        static Polyominos()
        {
            foreach (var polyomino in polyominos)
            {
                ReverseRows(polyomino);
            }
        }

        public static int[,] Get(int index) => polyominos[index];

        public static int Length => polyominos.Length;

        private static void ReverseRows(int[,] polyomino)
        {
            var polyominoRows = polyomino.GetLength(0);
            var polyominoColumns = polyomino.GetLength(1);

            for (var r = 0; r < polyominoRows / 2; ++r)
            {
                var topRow = r;
                var bottomRow = polyominoRows - 1 - r;

                for (var c = 0; c < polyominoColumns; ++c)
                {
                    var tmp = polyomino[topRow, c];
                    polyomino[topRow, c] = polyomino[bottomRow, c];
                    polyomino[bottomRow, c] = tmp;
                }
            }
        }

    }
}
