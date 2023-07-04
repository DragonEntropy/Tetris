using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;

namespace Tetris
{
    internal class Game
    {
        //Height and width of the tetris board
        public static int boardHeight = 20;
        public static int boardWidth = 10;

        //Important booleans for the game and pieces
        public bool canSwitchPiece = true;
        public bool isGameActive = true;
        public bool skipTimer = true;

        //The pieces and the board
        public Piece activePiece;
        public Piece heldPiece;
        public int[,] tileArray;

        //Elements external to the game
        Random rand;
        Dispatcher dispatcher;
        GameScreen gameScreen;

        //The background thread for the timer
        public Thread timerThread;

        //Other constants
        public static int levelLines = 4;
        static double timerMultiplier = 0.9;

        //Game data
        int level = 1;
        int score = 0;
        int combo = 0;
        int levelLinesCleared = 0;

        //Starts the tetris game
        public void StartGame(GameScreen screen, Dispatcher disp)
        {
            /*
             * Stores the values of each tile 
             * 0 - denotes the tile is unoccupied
             * 1 - denotes the tile is occupied by the active piece
             * 2 or greater - denotes the tile is occupied by a stationary piece
             */
            tileArray = new int[boardHeight, boardWidth];
            
            //Setting up random generation and the initial piece
            rand = new Random();
            activePiece = PieceData.GeneratePiece(rand);

            //Setting up connections to the game screen
            dispatcher = disp;
            gameScreen = screen;

            //Runs while the game is active
            while (isGameActive)
            {
                //Timer thread
                timerThread = new Thread(() => Timer());
                timerThread.Start();
                timerThread.Join();
            }

            //End of game process
            dispatcher.Invoke(() => gameScreen.EndGame());
            Console.WriteLine("The game has finished!");
        }

        //The main automatic process of processing tile movements
        void MainGameLoop()
        {
            //Updates the board every second
            bool isPieceActive = UpdateBoard(0, -1, false);
            if (!isPieceActive)
            {
                DestroyCompleteRows();
                activePiece = PieceData.GeneratePiece(rand);
                canSwitchPiece = true;

                //Updates game scores
                dispatcher.Invoke(() => gameScreen.UpdateScores());

                isGameActive = InitialPlace();
                isPieceActive = true;
            }
        }

        //The timer that the automatic downwards tile movements use
        void Timer()
        {
            //The timer is skipped if a new piece is made
            if (!skipTimer)
            {
                //Waits for a variable amount of time based off the level
                Thread.Sleep(CalculateTime());
            }

            skipTimer = false;
            MainGameLoop();
        }

        //Places a tile on the screen and adds its to the tile array
        public static void PlaceTile(int xPos, int yPos, string colour, int[,] tileArray, int tileValue)
        {
            //Writes the tile to the tile array
            tileArray[yPos, xPos] = tileValue;

            //Paints the tile to the display
            GameScreen.PaintTile(xPos, yPos, colour);

            //Console.WriteLine(String.Format("Placed tile at {0}, {1}!", xPos, yPos));
        }

        //Removes a tile on the screen and removes it from the tile array
        public void RemoveTile(int xPos, int yPos)
        {
            PlaceTile(xPos, yPos, PieceData.black, tileArray, 0);
        }

        //Removes the current piece from the screen and tile array after each move
        public void RemoveTempTiles()
        {
            for (int row = 0; row < boardHeight; row++)
            {
                for (int col = 0; col < boardWidth; col++)
                {
                    if (tileArray[row, col] == 1)
                    {
                        RemoveTile(col, row);
                    }
                }
            }
        }

        //Updates the board when the current piece moves
        public bool UpdateBoard(int xShift, int yShift, bool isPlayer)
        {
            //If the piece doesn't exist, nothing happens
            if (activePiece.isBlank)
            {
                return false;
            }

            //Records the success status of attempting to move the piece
            int moveStatus = activePiece.MovePiece(xShift, yShift, tileArray);
            RemoveTempTiles();

            //Restarts the timer if the player moved
            /*if (isPlayer)
            {
                timerThread.Abort();
            }*/

            //Deals with each potential status from attempting to move the piece
            switch (moveStatus)
            {
                //The piece moved successfully
                case 0:
                    //Console.WriteLine("Normal move!");
                    activePiece.PlacePiece(tileArray, 1);
                    break;

                //The piece hit a wall
                case 1:
                    //Console.WriteLine("Hit a wall!");
                    activePiece.PlacePiece(tileArray, 1);
                    break;

                //The piece hit the floor
                case 2:
                    //Console.WriteLine("Hit the floor!");
                    activePiece.PlacePiece(tileArray, 2 + activePiece.GetId());
                    AbandonPiece();
                    return false;

                //The piece hit another tile's side 
                case 3:
                    //Console.WriteLine("Hit another tile's side!");
                    activePiece.PlacePiece(tileArray, 1);
                    break;

                //The piece hit another tile's top
                case 4:
                    //Console.WriteLine("Hit another tile's side!");
                    activePiece.PlacePiece(tileArray, 2 + activePiece.GetId());
                    AbandonPiece();
                    return false;
            }

            return true;
        }

        //Tries to place the tile in its initial position (otherwise game ends)
        bool InitialPlace()
        {
            int moveStatus = activePiece.MovePiece(0, 0, tileArray);
            activePiece.PlacePiece(tileArray, 1);

            if (moveStatus == 0)
            {
                return true;
            }

            return false;
        }

        //No longer allows the player to control the piece, assigning a new piece
        void AbandonPiece()
        {
            skipTimer = true;
            activePiece = PieceData.BlankPiece();
        }

        //Switches the active piece with the held piece
        public void SwitchPieces()
        {
            if (heldPiece == null)
            {
                heldPiece = activePiece;
                activePiece = PieceData.GeneratePiece(rand);
            }
            else
            {
                Piece tempPiece = activePiece;
                activePiece = heldPiece;
                heldPiece = tempPiece;
                activePiece.Reset();
            }

            RemoveTempTiles();
            isGameActive = InitialPlace();
            canSwitchPiece = false;
        }

        //Destroys all complete rows and shifts above rows down
        void DestroyCompleteRows()
        {
            int linesDestroyed = 0;

            //First find complete rows
            for (int row = boardHeight - 1; row >= 0; row--)
            {
                bool isComplete = true;
                for (int col = 0; col < boardWidth; col++)
                {
                    if (tileArray[row, col] < 2)
                    {
                        isComplete = false;
                        break;
                    }
                }

                if (isComplete)
                {
                    //Adds to lines destroyed
                    linesDestroyed += 1;

                    //Then destroy the complete row
                    for (int col = 0; col < boardWidth; col++)
                    {
                        RemoveTile(col, row);
                    }

                    //Then shifts all other rows
                    for (int y = row + 1; y < boardHeight; y++)
                    {
                        for (int x = 0; x < boardWidth; x++)
                        {
                            int tileValue = tileArray[y, x];
                            string colour = PieceData.black;
                            if (tileValue >= 2)
                            {
                                colour = PieceData.cellColours[tileValue - 2];
                            }
                            PlaceTile(x, y - 1, colour, tileArray, tileValue);
                        }
                    }

                    //Clears the top row
                    for (int col = 0; col < boardWidth; col++)
                    {
                        RemoveTile(col, boardHeight - 1);
                    }
                }
            }

            //Calculates score and adds combo if lines were destroyed
            if (linesDestroyed > 0)
            {
                levelLinesCleared += linesDestroyed;
                CalculateScore(linesDestroyed);
                combo += 1;
            }

            //Resets combo if no lines were destroyed
            else
            {
                combo = 0;
            }

            //Checks if the player should level up
            if (levelLinesCleared >= levelLines)
            {
                levelLinesCleared -= levelLines;
                level += 1;
                Console.WriteLine($"Level up! level {level} reached!");
            }
        }

        //Rotates a piece clockwise or anticlockwise if possible
        public void RotatePiece(bool isClockwise)
        {
            int[,] rotatedShape = new int[Piece.maxPieceSize, Piece.maxPieceSize];

            if (isClockwise)
            {
                activePiece.ClockwiseRotate(rotatedShape);
            }
            else
            {
                activePiece.AnticlockwiseRotate(rotatedShape);
            }

            bool isValidRotation = activePiece.ValidateRotation(tileArray, rotatedShape);
            if (isValidRotation)
            {
                RemoveTempTiles();
                InitialPlace();
            }
        }
        
        //Calculates the score after a line clear
        public void CalculateScore(int lines)
        {
            int lineScore = 100 * level;
            switch (lines)
            {
                case 1:
                    lineScore *= 1;
                    break;
                case 2:
                    lineScore *= 3;
                    break;
                case 3:
                    lineScore *= 5;
                    break;
                case 4:
                    lineScore *= 8;
                    break;
            }
            int comboScore = 50 * combo * level;

            score += lineScore + comboScore;
        }

        //Adds to the score
        public void AddScore(int extraScore)
        {
            score += extraScore;
        }

        //Resets the score
        public void ResetScore()
        {
            score = 0;
        }

        //Gets the score
        public int GetScore()
        {
            return score;
        }

        //Calculates the time for each automatic game cycle
        int CalculateTime()
        {
            return (int)(1000 * Math.Pow(timerMultiplier, level));
        }

        //Prints the tile array (for debug)
        void PrintTileArray()
        {
            for (int row = boardHeight - 1; row >= 0; row--)
            {
                for (int col = 0; col < boardWidth; col++)
                {
                    Console.Write(String.Format("{0} ", tileArray[row, col]));
                }
                Console.WriteLine("");
            }
            Console.WriteLine("");
        }
    }
}
