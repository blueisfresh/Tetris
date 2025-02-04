using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Tetris
{
    public static class Tetris
    {
        // Board size (columns x rows)
        const int Width = 10;
        const int Height = 20;
        public static string[,] board = new string[Width, Height];

        // Definition of tetromino shapes
        public static Dictionary<string, int[,]> Tetrominoes = new Dictionary<string, int[,]>
        {
            { "I", new int[,] { { 1, 1, 1, 1 } } },
            { "O", new int[,] { { 1, 1 }, { 1, 1 } } },
            { "T", new int[,] { { 0, 1, 0 }, { 1, 1, 1 } } },
            { "S", new int[,] { { 0, 1, 1 }, { 1, 1, 0 } } },
            { "Z", new int[,] { { 1, 1, 0 }, { 0, 1, 1 } } },
            { "J", new int[,] { { 1, 0, 0 }, { 1, 1, 1 } } },
            { "L", new int[,] { { 0, 0, 1 }, { 1, 1, 1 } } }
        };

        // Active tetromino and its position
        private static int[,] currentTetromino;
        private static int currentX, currentY;
        private static Random random = new Random();

        // Timer for gravity (e.g., 500ms per step)
        private static Stopwatch gravityTimer = new Stopwatch();
        private static int gravityInterval = 500; // in milliseconds

        // Flags for game over and exit
        public static bool gameOver = false;
        private static bool exitGame = false;

        // Thread-safe queue for keyboard inputs
        private static ConcurrentQueue<ConsoleKey> inputQueue = new ConcurrentQueue<ConsoleKey>();

        /// <summary>
        /// Starts the game. The game state is fully reset and two threads are started:
        /// one for input and one for the game loop.
        /// </summary>
        public static void Run()
        {
            ResetGameState();

            // Clear any leftover key inputs (flush the input buffer)
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }

            // Prepare the console
            Console.Clear();
            Console.CursorVisible = false;

            // Start the input thread (background thread)
            Thread inputThread = new Thread(InputLoop) { IsBackground = true };
            inputThread.Start();

            // Run the game loop on the current thread
            GameLoop();

            // After the game ends, make the cursor visible again
            Console.CursorVisible = true;
        }

        /// <summary>
        /// Resets all relevant game variables to their initial state.
        /// </summary>
        public static void ResetGameState()
        {
            gameOver = false;
            exitGame = false;
            currentTetromino = null;
            gravityTimer.Reset();
            gravityTimer.Start();
            InitializeBoard();
        }

        /// <summary>
        /// Initializes the board by marking all cells as empty ("░░").
        /// </summary>
        public static void InitializeBoard()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    board[x, y] = "░░";
        }

        /// <summary>
        /// Continuously reads keyboard input and places it into a thread-safe queue.
        /// Pressing the Enter key will exit the game.
        /// </summary>
        public static void InputLoop()
        {
            while (!exitGame)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Enter)
                    {
                        exitGame = true;
                        break;
                    }
                    inputQueue.Enqueue(key);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        /// Main game loop. Processes input, moves the tetromino (including gravity),
        /// redraws the board, and checks if any rows are full.
        /// </summary>
        public static void GameLoop()
        {
            while (!exitGame)
            {
                ProcessInput();

                // If no active tetromino exists, spawn a new one.
                if (currentTetromino == null)
                {
                    SpawnTetromino();
                }

                // Gravity: Move the tetromino down after the interval has passed.
                if (gravityTimer.ElapsedMilliseconds >= gravityInterval)
                {
                    MoveTetromino(0, 1);
                    gravityTimer.Restart();
                }

                DisplayBoard();
                Thread.Sleep(50);

                if (gameOver)
                {
                    exitGame = true;
                    Console.SetCursorPosition(0, Height + 1);
                    Console.WriteLine("Game Over. Press any key to exit.");
                    Console.ReadKey(true);
                }
            }
        }

        /// <summary>
        /// Processes all key presses stored in the queue.
        /// Supports left (A), right (D), down (S) and rotation (W).
        /// </summary>
        public static void ProcessInput()
        {
            while (inputQueue.TryDequeue(out ConsoleKey key))
            {
                switch (key)
                {
                    case ConsoleKey.A:
                        MoveTetromino(-1, 0);
                        break;
                    case ConsoleKey.D:
                        MoveTetromino(1, 0);
                        break;
                    case ConsoleKey.S:
                        MoveTetromino(0, 1);
                        break;
                    case ConsoleKey.W:
                        RotateTetromino();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Spawns a new random tetromino and sets it at the starting position.
        /// If the tetromino cannot be placed, the game is set to game over.
        /// </summary>
        public static void SpawnTetromino()
        {
            var keys = Tetrominoes.Keys.ToList();
            string randomKey = keys[random.Next(keys.Count)];
            currentTetromino = Tetrominoes[randomKey];
            currentX = (Width / 2) - (currentTetromino.GetLength(1) / 2);
            currentY = 0;

            if (!CanPlaceTetromino(currentTetromino, currentX, currentY))
            {
                gameOver = true;
                return;
            }
            PlaceTetromino(currentTetromino, currentX, currentY);
        }

        /// <summary>
        /// Checks if a tetromino can be placed at the desired position.
        /// It verifies that there are no collisions and that the tetromino stays within the board.
        /// </summary>
        public static bool CanPlaceTetromino(int[,] tetromino, int posX, int posY)
        {
            for (int y = 0; y < tetromino.GetLength(0); y++)
            {
                for (int x = 0; x < tetromino.GetLength(1); x++)
                {
                    if (tetromino[y, x] == 1)
                    {
                        int boardX = posX + x;
                        int boardY = posY + y;
                        if (boardX < 0 || boardX >= Width || boardY < 0 || boardY >= Height)
                            return false;
                        if (board[boardX, boardY] != "░░")
                            return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Places the given tetromino on the board at the specified position (posX, posY).
        /// </summary>
        public static void PlaceTetromino(int[,] tetromino, int posX, int posY)
        {
            for (int y = 0; y < tetromino.GetLength(0); y++)
            {
                for (int x = 0; x < tetromino.GetLength(1); x++)
                {
                    if (tetromino[y, x] == 1)
                    {
                        int boardX = posX + x;
                        int boardY = posY + y;
                        board[boardX, boardY] = "██";
                    }
                }
            }
        }

        /// <summary>
        /// Clears the tetromino from its current position on the board.
        /// </summary>
        public static void ClearTetromino(int[,] tetromino, int posX, int posY)
        {
            for (int y = 0; y < tetromino.GetLength(0); y++)
            {
                for (int x = 0; x < tetromino.GetLength(1); x++)
                {
                    if (tetromino[y, x] == 1)
                    {
                        int boardX = posX + x;
                        int boardY = posY + y;
                        board[boardX, boardY] = "░░";
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to move the active tetromino in the desired direction.
        /// If the movement (especially downward) is not possible, the tetromino is locked and a new one is spawned.
        /// </summary>
        public static void MoveTetromino(int deltaX, int deltaY)
        {
            int newX = currentX + deltaX;
            int newY = currentY + deltaY;

            if (CanPlaceTetromino(currentTetromino, newX, newY))
            {
                ClearTetromino(currentTetromino, currentX, currentY);
                currentX = newX;
                currentY = newY;
                PlaceTetromino(currentTetromino, currentX, currentY);
            }
            else
            {
                // If the tetromino cannot move down further, lock it and spawn a new one.
                if (deltaY == 1)
                {
                    LockTetromino();
                    currentTetromino = null;
                }
            }
        }

        /// <summary>
        /// Locks the current tetromino (making it permanent on the board)
        /// and then checks for full rows.
        /// </summary>
        public static void LockTetromino()
        {
            ClearFilledRows();
        }

        /// <summary>
        /// Checks from the bottom up if any row is full. If a row is full,
        /// it clears the row and shifts all rows above it downward.
        /// </summary>
        public static void ClearFilledRows()
        {
            for (int y = Height - 1; y >= 0; y--)
            {
                bool full = true;
                for (int x = 0; x < Width; x++)
                {
                    if (board[x, y] == "░░")
                    {
                        full = false;
                        break;
                    }
                }
                if (full)
                {
                    // Shift all rows above the current row down by one.
                    for (int row = y; row > 0; row--)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            board[x, row] = board[x, row - 1];
                        }
                    }
                    // Clear the top row.
                    for (int x = 0; x < Width; x++)
                    {
                        board[x, 0] = "░░";
                    }
                    // Re-check the same row as it now contains the shifted row.
                    y++;
                }
            }
        }

        /// <summary>
        /// Rotates the current tetromino 90° clockwise, if there is enough space at the current position.
        /// </summary>
        public static void RotateTetromino()
        {
            int rows = currentTetromino.GetLength(0);
            int cols = currentTetromino.GetLength(1);
            int[,] rotated = new int[cols, rows];

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    rotated[x, rows - y - 1] = currentTetromino[y, x];
                }
            }

            if (CanPlaceTetromino(rotated, currentX, currentY))
            {
                ClearTetromino(currentTetromino, currentX, currentY);
                currentTetromino = rotated;
                PlaceTetromino(currentTetromino, currentX, currentY);
            }
        }

        /// <summary>
        /// Displays the board on the console.
        /// </summary>
        public static void DisplayBoard()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Console.SetCursorPosition(x * 2, y); // Multiply x by 2 for wider cells
                    Console.Write(board[x, y]);
                }
            }
        }
    }
}