using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows;

namespace Tetris
{
    public partial class GameScreen : Form
    {
        static Graphics g;
        Game game;
        static int cellSize = 32;

        int highscore = 0;

        Thread backgroundThread;
        //Thread musicThread;

        public GameScreen()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            
            //Disables the start button if there is an active game
            if (game != null)
            {
                if (game.isGameActive == true)
                {
                    return;
                }
            }
            //The dispatcher for UI elements
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }
            Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;
            
            ResetGame();
            //Running the game on a background thread
            if (backgroundThread != null)
            {
                backgroundThread.Abort();
            }
            backgroundThread = new Thread(() => game.StartGame(this, dispatcher));
            backgroundThread.Start();

            //Running the music on a background thread
            /*MusicPlayer musicPlayer = new MusicPlayer();
            musicThread = new Thread(() => musicPlayer.PlayBackgroundMusic());
            musicThread.Start();*/
        }

        public static void PaintTile(int xPos, int yPos, string colour)
        {
            //Converts from game to absolute coordinates
            int realXPos = xPos * cellSize + 1;
            int realYPos = (Game.boardHeight - 1) * cellSize - yPos * cellSize + 1;

            //Paints the tile to the display
            try
            {
                g.FillRectangle(new SolidBrush(ColorTranslator.FromHtml(colour)), realXPos, realYPos, cellSize - 2, cellSize - 2);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(String.Format("{0} was caught", e));
            }
        }

        public void UpdateScores()
        {
            int score = game.GetScore();
            scoreData.Text = score.ToString();
            if (score > highscore)
            {
                highscore = score;
                highscoreData.Text = highscore.ToString();
            }
        }

        public void EndGame()
        {
            startButton.Enabled = true;
        }

        public void ResetGame()
        {
            scoreData.Text = "0";

            //Creating graphics and game
            g = gamePanel.CreateGraphics();
            gamePanel.Refresh();
            gamePanel.BackColor = Color.Black;
            startButton.Enabled = false;

            game = new Game();
        }

        private void GameScreen_KeyDown(object sender, KeyEventArgs e)
        {
            //Checks if the game is active
            if (game == null || game.activePiece.isBlank || game.isGameActive == false)
            {
                return;
            }

            //Switches between the held piece and the active piece
            else if (game.canSwitchPiece && e.KeyValue == (char)Keys.C)
            {
                game.SwitchPieces();
            }

            //Roates the current piece clockwise if possible
            else if (e.KeyValue == (char)Keys.X)
            {
                game.RotatePiece(true);
            }

            //Roates the current piece anticlockwise if possible
            else if (e.KeyValue == (char)Keys.Z)
            {
                game.RotatePiece(false);
            }

            //Moves the piece right if possible
            else if (e.KeyValue == (char)Keys.D || e.KeyValue == (char)Keys.Right) {
                game.UpdateBoard(1, 0, true);
            }

            //Moves the piece left if possible
            else if (e.KeyValue == (char)Keys.A || e.KeyValue == (char)Keys.Left)
            {
                game.UpdateBoard(-1, 0, true);
            }

            //Moves the piece down if possible
            else if (e.KeyValue == (char)Keys.S || e.KeyValue == (char)Keys.Down)
            {
                bool isValidMove = game.UpdateBoard(0, -1, true);
                if (isValidMove)
                {
                    game.AddScore(1);
                    UpdateScores();
                }

                //Restarts the move timer if moved down
                game.timerThread.Abort();
            }

            //Moves the piece to the bottom
            else if (e.KeyValue == (char)Keys.Space)
            {
                bool isActive = true;
                while (isActive)
                {
                    isActive = game.UpdateBoard(0, -1, true);
                    if (isActive)
                    {
                        game.AddScore(2);
                        UpdateScores();
                    }
                }

                //Restarts the move timer if moved straight down
                game.timerThread.Abort();
            }
        }
    }
}
