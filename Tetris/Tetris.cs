using System;
using System.Diagnostics;

namespace Tetris;

public static class Tetris
{
    // Set the size for the board
    const int Width = 10, Height = 20;
    public static string[,] board = new String[Width, Height];
    
    public static Dictionary<string, int[,]> Tetrominoes = new Dictionary<string, int[,]>
    {
        { "I", new int[,] { { 1, 1, 1, 1 } } },             // I-Tetromino
        { "O", new int[,] { { 1, 1 }, { 1, 1 } } },         // O-Tetromino
        { "T", new int[,] { { 0, 1, 0 }, { 1, 1, 1 } } },   // T-Tetromino
        { "S", new int[,] { { 0, 1, 1 }, { 1, 1, 0 } } },   // S-Tetromino
        { "Z", new int[,] { { 1, 1, 0 }, { 0, 1, 1 } } },   // Z-Tetromino
        { "J", new int[,] { { 1, 0, 0 }, { 1, 1, 1 } } },   // J-Tetromino
        { "L", new int[,] { { 0, 0, 1 }, { 1, 1, 1 } } }    // L-Tetromino
    };
    
    // int[,] currentTetromino = Tetrominoes["T"]; // T-Tetromino -> To acces the Dictionary
    
    // Randomizer for Tetrominoes
    private static int[,] currentTetromino; // Store the active tetromino
    private static int currentX, currentY; // Position of the active tetromino

    private static Random random = new Random();
    
    // Gravity
    private static Stopwatch gravityTimer = new Stopwatch();
    private static int gravityInterval = 500; // In Mi
    
    // Gameloop boolean
    public static bool gameOver = false;

    
    static void RunGameLoop()
    {
        gameOver = false;
        
        while (!gameOver)
        {
            HandleInput();

            // Spawne neue Tetrominos falls nötig
            if (currentTetromino == null)
            {
                SpawnTetromino();
            }

            if (gravityTimer.ElapsedMilliseconds >= gravityInterval)
            {
                MoveTetromino(0, 1); // Move down
                gravityTimer.Restart();
            }
            
            // TODO: Update the board array with the newly moved Tetrominoes 
            
            DisplayBoard();
        }
        
        // Spiel ist vorbei, Option zum Neustart oder Beenden
        Console.Clear();
        Console.WriteLine("Game Over! Press R to Reset or Escape to Quit.");

        while (true)
        {
            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.R)
            {
                ResetGame();
                RunGameLoop(); // Spiel neu starten
            }
            else if (key == ConsoleKey.Escape)
            {
                break; // Programm beenden
            }
        }
    }

    static void ResetGame()
    {
        // Reset any necessary variables
        gameOver = false;

        // Reinitialize the board or other game state if needed
        InitializeBoard();
    }

    static void SpawnTetromino()
    {
        Debug.WriteLine("Inside of the SpawnTetromino");
        // Pick a Random Tetromino
        var tetrominoKeys = Tetrominoes.Keys.ToList();
        string randomKey = tetrominoKeys[random.Next(tetrominoKeys.Count)];
        currentTetromino = Tetrominoes[randomKey];
        
        // Calculate the starting position (Top Middle)
        currentX = (Width / 2) - (currentTetromino.GetLength(1) / 2); // Center horizontally
        currentY = 0; // Start at the top of the board

        // Can Tetromino be placed
        if (!CanPlaceTetromino(currentTetromino, currentX, currentY))
        {
            gameOver = true; // No space to spawn, game over
            return;
        }
        
        // Place the tetromino on the board
        PlaceTetromino(currentTetromino, currentX, currentY);
    }

    static bool CanPlaceTetromino(int[,] tetromino, int posX, int posY)
    {
        for (int y = 0; y < tetromino.GetLength(0); y++)
        {
            for (int x = 0; x < tetromino.GetLength(1); x++)
            {
                if (tetromino[y, x] == 1) // Only check filled cells
                {
                    int boardX = posX + x;
                    int boardY = posY + y;

                    // Out of bounds or colliding with existing block
                    if (boardX < 0 || boardX >= Width || boardY < 0 || boardY >= Height || board[boardX, boardY] != "░░")
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    
    static void PlaceTetromino(int[,] tetromino, int posX, int posY)
    {
        for (int y = 0; y < tetromino.GetLength(0); y++)
        {
            for (int x = 0; x < tetromino.GetLength(1); x++)
            {
                if (tetromino[y, x] == 1) // Only place filled cells
                {
                    board[posX + x, posY + y] = "██"; // Filled block
                }
            }
        }
    }

    static void MoveTetromino(int deltaX, int deltaY)
    {
        // Calculate the new position
        int newX = currentX + deltaX;
        int newY = currentY + deltaY;
        
        // Check if the new position is valid
        if (CanPlaceTetromino(currentTetromino, newX, newY))
        {
            // Clear the current position
            ClearTetromino(currentTetromino, currentX, currentY);

            // Update the position
            currentX = newX;
            currentY = newY;

            // Place the tetromino at the new position
            PlaceTetromino(currentTetromino, currentX, currentY);
        }
        else if (deltaY > 0) // If it's a downward move and can't proceed
        {
            // Lock the tetromino and spawn a new one
            LockTetromino();
            SpawnTetromino();
        }
    }
    
    static void LockTetromino()
    {
        for (int y = 0; y < currentTetromino.GetLength(0); y++)
        {
            for (int x = 0; x < currentTetromino.GetLength(1); x++)
            {
                if (currentTetromino[y, x] == 1) // Only lock filled cells
                {
                    int boardX = currentX + x;
                    int boardY = currentY + y;

                    // Ensure within bounds before locking
                    if (boardX >= 0 && boardX < Width && boardY >= 0 && boardY < Height)
                    {
                        board[boardY, boardX] = "██"; // Lock the cell
                    }
                }
            }
        }

        // Clear any filled rows
        ClearFilledRows();

        // Reset the current tetromino
        currentTetromino = null;
    }
    
    static void ClearFilledRows()
    {
        for (int y = 0; y < Height; y++)
        {
            bool isRowFull = true;

            for (int x = 0; x < Width; x++)
            {
                if (board[y, x] == "░░") // If any cell is empty, the row isn't full
                {
                    isRowFull = false;
                    break;
                }
            }

            if (isRowFull)
            {
                // Clear the row
                for (int x = 0; x < Width; x++)
                {
                    board[y, x] = "░░"; // Empty the row
                }

                // Move rows above down
                for (int row = y; row > 0; row--)
                {
                    for (int col = 0; col < Width; col++)
                    {
                        board[row, col] = board[row - 1, col];
                    }
                }

                // Clear the topmost row
                for (int x = 0; x < Width; x++)
                {
                    board[0, x] = "░░";
                }
            }
        }
    }
    
    static void ClearTetromino(int[,] tetromino, int posX, int posY)
    {
        for (int y = 0; y < tetromino.GetLength(0); y++)
        {
            for (int x = 0; x < tetromino.GetLength(1); x++)
            {
                if (tetromino[y, x] == 1) // Only clear filled cells
                {
                    int boardX = posX + x;
                    int boardY = posY + y;

                    // Ensure within bounds before clearing
                    if (boardX >= 0 && boardX < Width && boardY >= 0 && boardY < Height)
                    {
                        board[boardY, boardX] = "░░"; // Clear the cell
                    }
                }
            }
        }
    }

    // TODO: Implement rotation 
    static void RotateTetromino()
    {
        
    }

    static void DisplayBoard()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                // Use SetCursorPosition to get rid of the Flickering
                Console.SetCursorPosition(x * 2, y); // x * 2 for wider cells
                Console.Write(board[x, y]);
            }
        }
    }
    
    static void InitializeBoard()
    {
        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
            board[x, y] = "░░"; // Empty cells
    }
    
    static void HandleInput()
    {
        // Exit if Keys are not avalaible
        if (!Console.KeyAvailable) return;
        
        // Save the currently pressed key
        var key = Console.ReadKey(true);

        switch (key.Key)
        {
            case ConsoleKey.A:
                MoveTetromino(-1, 0);
                break;
            case ConsoleKey.D:
                MoveTetromino(1, 0);
                break;
            case ConsoleKey.S:
                MoveTetromino(0, -1);
                break;
            case ConsoleKey.R:
                ResetGame(); // Neustart des Spiels
                RunGameLoop(); // Neustart der Hauptschleife
                break;
            case ConsoleKey.Escape:
                gameOver = true;
                break;
        }
    }
    public static void Run()
    {
        // Remove the Path from the console
        Console.Clear();
        
        // Cursor not Visible
        Console.CursorVisible = false;
        
        // Initialize and display the board immediately
        InitializeBoard();
        DisplayBoard();

        // Run the Game-Loop
        RunGameLoop();
    }
}
