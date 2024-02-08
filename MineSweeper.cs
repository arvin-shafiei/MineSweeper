// Team 8
// Ahmed Youssef 2507690@dundee.ac.uk
// Arvin Shafiei 2503389@dundee.ac.uk
// Usama Fakhar 2530869@dundee.ac.uk

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MinesweeperGame
{
    public partial class MainForm : Form
    {
        // Enumeration to represent different difficulty levels
        private enum Difficulty { Easy, Medium, Hard }

        // Size of the game grid (number of rows and columns)
        private int gridSize = 10;

        // Variables to store game-related information
        private int mineCount;          // Number of mines on the grid
        private int scoreMultiplier;    // Score multiplier based on difficulty
        private int score;              // Player's current score
        private int topScore;           // Top score achieved
        private float aiBombChance;     // Chance for AI to ignore a mine

        // Arrays to represent the game grid and mines
        private Button[,] buttons;      // Array of Buttons for the game grid
        private bool[,] mines;          // Array to track the presence of mines

        // Flag to indicate whether the game has ended
        private bool gameEnded;

        // ToolStripStatusLabel controls for displaying score information
        private ToolStripStatusLabel scoreLabel;     // Label for displaying the player's score
        private ToolStripStatusLabel topScoreLabel;  // Label for displaying the top score

        Font arialBoldFont = new Font("Arial", 10, FontStyle.Bold);

        public MainForm()
        {
            // Initialize the menu strip for game options
            InitializeMenuStrip();

            // Initialize the status strip for displaying score information
            InitializeStatusStrip();

            InitializeComponent(); // Initializes form components

            this.ClientSize = new Size(gridSize * 30, gridSize * 30 + 50);

            InitializeGame(Difficulty.Easy); // Initialize game settings

            CenterToScreen(); // Center the form on the screen
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "MainForm";
            this.Text = "Minesweeper";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        private void InitializeMenuStrip()
        {
            // Create a new MenuStrip control
            MenuStrip menuStrip = new MenuStrip();

            // Create a ToolStripMenuItem for the "Game" menu
            ToolStripMenuItem gameMenu = new ToolStripMenuItem("Game");

            // Add sub-items for starting new games with different difficulties
            gameMenu.DropDownItems.Add("New Game Easy", null, (sender, e) => InitializeGame(Difficulty.Easy));
            gameMenu.DropDownItems.Add("New Game Medium", null, (sender, e) => InitializeGame(Difficulty.Medium));
            gameMenu.DropDownItems.Add("New Game Hard", null, (sender, e) => InitializeGame(Difficulty.Hard));

            // Create a ToolStripMenuItem for the "About" menu
            ToolStripMenuItem aboutMenu = new ToolStripMenuItem("About and Help");

            // Add sub-items for About and Simple Rules under "About"
            aboutMenu.DropDownItems.Add("What's this game about", null, (sender, e) => MessageBox.Show("This is similar to normal Minesweeper where the mines are hidden randomly under the buttons, the safe squares have a number on them which tell you how many mines that button is touching. You have to use the number on the button to finish the game and reveal all the safe squares. If you click on a button with a mine you lose. We have put a twist on this and made it so you are also up against the AI, and whoever can do better wins."));

            // Add the "Game" and "About" menus to the MenuStrip's Items collection
            menuStrip.Items.AddRange(new ToolStripItem[] { gameMenu, aboutMenu });

            // Set the MainMenuStrip property of the form to the created MenuStrip
            this.MainMenuStrip = menuStrip;

            // Add the MenuStrip control to the form's Controls collection
            this.Controls.Add(menuStrip);
        }

        private void InitializeStatusStrip()
        {
            // Create a new StatusStrip control
            StatusStrip statusStrip = new StatusStrip();

            // Create ToolStripStatusLabel controls for displaying score information
            scoreLabel = new ToolStripStatusLabel("Score: 0");
            topScoreLabel = new ToolStripStatusLabel("Top Score: 0");

            // Add the scoreLabel and topScoreLabel to the StatusStrip's Items collection
            statusStrip.Items.Add(scoreLabel);
            statusStrip.Items.Add(topScoreLabel);

            // Add the StatusStrip control to the form's Controls collection
            this.Controls.Add(statusStrip);
        }

        private void InitializeGame(Difficulty difficulty)
        {
            // Dispose of any existing buttons on the game grid
            DisposeExistingButtons();

            // Define game settings based on the selected difficulty
            DefineGameSettings(difficulty);

            // Initialize the buttons and mines arrays based on the gridSize
            buttons = new Button[gridSize, gridSize];
            mines = new bool[gridSize, gridSize];

            // Create and set up the game grid with buttons
            InitializeGrid();

            // Randomly place mines on the game grid
            PlaceMines();

            // Reset the game state, including scores and gameEnded flag
            ResetGame();
        }

        private void DisposeExistingButtons()
        {
            // Check if the buttons array is not null
            if (buttons != null)
            {
                // Loop through the grid to dispose of existing buttons
                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        // Check if the button at (x, y) is not null
                        if (buttons[x, y] != null)
                        {
                            // Remove the button from the form's Controls collection to hide it
                            this.Controls.Remove(buttons[x, y]);

                            // Dispose of the button to release its resources
                            buttons[x, y].Dispose();
                        }
                    }
                }
            }
        }

        private void DefineGameSettings(Difficulty difficulty)
        {
            // Use a switch statement to define game settings based on the selected difficulty
            switch (difficulty)
            {
                case Difficulty.Easy:
                    mineCount = 10;          // Number of mines for easy difficulty
                    scoreMultiplier = 1;    // Score multiplier for easy difficulty
                    aiBombChance = 0.15f;   // AI bomb chance for easy difficulty (15% chance)
                    break;

                case Difficulty.Medium:
                    mineCount = 20;          // Number of mines for medium difficulty
                    scoreMultiplier = 2;    // Score multiplier for medium difficulty
                    aiBombChance = 0.1f;    // AI bomb chance for medium difficulty (10% chance)
                    break;

                case Difficulty.Hard:
                    mineCount = 30;          // Number of mines for hard difficulty
                    scoreMultiplier = 3;    // Score multiplier for hard difficulty
                    aiBombChance = 0.05f;   // AI bomb chance for hard difficulty (5% chance)
                    break;
            }

            // Initialize the buttons array based on the gridSize
            buttons = new Button[gridSize, gridSize];

            // Initialize the mines array based on the gridSize
            mines = new bool[gridSize, gridSize];
        }

        private void AITurn()
        {
            // Set gameEnded to false to ensure no more player actions during AI's turn
            gameEnded = false;

            // Create a new instance of the Random class for AI's random decisions
            Random rand = new Random();

            // Initialize AI's score
            int aiScore = 0;

            // Flag to track if it's the AI's first turn
            bool isFirstTurn = true;

            // Variables to store the last clicked coordinates
            int lastX = -1, lastY = -1;

            // Loop through all the cells in the grid and execute AI's actions
            for (int i = 0; i < gridSize * gridSize && !gameEnded; i++)
            {
                int x, y;

                // If it's the first turn or there are no adjacent non-mine squares, choose a random square
                if (isFirstTurn || !TryGetAdjacentNonMineSquare(lastX, lastY, out x, out y))
                {
                    x = rand.Next(gridSize);
                    y = rand.Next(gridSize);
                    isFirstTurn = false;
                }

                // Get the button at the selected coordinates
                Button aiButton = buttons[x, y];

                // Ensure that AI does not select the same square twice
                if (!aiButton.Enabled)
                    continue;

                // Update the last clicked coordinates
                lastX = x;
                lastY = y;

                // Simulate AI clicking a button with a chance to ignore mines
                if (!mines[x, y] || rand.NextDouble() < aiBombChance)
                {
                    // Process the AI's selection and update AI's score
                    ProcessAISelection(x, y, ref aiScore);
                }
            }

            // Compare AI's score with the player's score and declare the winner
            FinalizeAIScore(aiScore);
        }

        private bool TryGetAdjacentNonMineSquare(int x, int y, out int adjX, out int adjY)
        {
            // Create a new instance of the Random class to generate random numbers
            Random rand = new Random();

            // Create a list to store possible moves (coordinates) of adjacent non-mine squares
            List<Tuple<int, int>> possibleMoves = new List<Tuple<int, int>>();

            // Loop through neighboring cells using two nested loops for dx and dy
            for (int dx = -1; dx <= 2; dx++)
            {
                for (int dy = -1; dy <= 2; dy++)
                {
                    // Calculate the coordinates of the neighboring cell
                    int newX = x + dx;
                    int newY = y + dy;

                    // Check if the neighboring cell is within the grid bounds, is enabled, and does not contain a mine
                    if (newX >= 0 && newX < gridSize && newY >= 0 && newY < gridSize && buttons[newX, newY].Enabled && !mines[newX, newY])
                    {
                        // Add the coordinates of the valid neighboring cell to the list of possible moves
                        possibleMoves.Add(new Tuple<int, int>(newX, newY));
                    }
                }
            }

            // Check if there are any valid adjacent non-mine squares
            if (possibleMoves.Count > 0)
            {
                // Randomly select one of the valid moves from the list
                var move = possibleMoves[rand.Next(possibleMoves.Count)];

                // Retrieve the coordinates of the selected move
                adjX = move.Item1;
                adjY = move.Item2;

                // Return true to indicate that a valid move was found
                return true;
            }

            // If no valid moves are found, set adjX and adjY to -1 and return false
            adjX = -1;
            adjY = -1;
            return false;
        }

        private Image ResizeImage(Image img, int width, int height)
        {
            // Create a new Bitmap with the specified size
            Bitmap bmp = new Bitmap(width, height);
            // Use Graphics to draw the resized image
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(img, 0, 0, width, height);
            }
            return bmp;
        }


        private void ProcessAISelection(int x, int y, ref int aiScore)
        {
            // Get the button at the specified coordinates for AI's selection
            Button aiButton = buttons[x, y];

            // Check if the AI's selection contains a mine
            if (mines[x, y])
            {
                // If AI hits a mine, set the button's background color to red
                aiButton.BackColor = Color.Red;

                // Resize image cause its HUMONGOUS
                Image resizedBombImage = ResizeImage(MineSweeperGame.Properties.Resources.conflict_explosion_icon, 12, 12);

                // Set image on the button
                aiButton.Image = resizedBombImage;

                // Set image to middle of button
                aiButton.ImageAlign = ContentAlignment.MiddleCenter;

                // Display a message indicating that AI hit a mine, along with scores
                MessageBox.Show($"AI hit a mine. Final AI Score: {aiScore}. Player Score: {score}");

                // Set the gameEnded flag to indicate that the game is over
                gameEnded = true;
            }
            else
            {
                // If AI's selection is safe (no mine)

                // Calculate the number of adjacent mines
                int adjacentMines = CountAdjacentMines(x, y);

                // Disable the AI's selected button to prevent further interaction
                aiButton.Enabled = false;

                // Increase the AI's score by the scoreMultiplier value
                aiScore += scoreMultiplier;

                if (adjacentMines > 0)
                {
                    // If there are adjacent mines, display the count on the button
                    aiButton.Text = adjacentMines.ToString();
                }
                else
                {
                    // If there are no adjacent mines, reveal adjacent cells
                    RevealAdjacent(x, y);
                }
            }
        }

        private void FinalizeAIScore(int aiScore)
        {
            // Check if the game has not ended (AI didn't hit a mine)
            if (!gameEnded)
            {
                // Display a message with the final AI Score and Player Score
                MessageBox.Show($"AI finished. Final AI Score: {aiScore}. Player Score: {score}");
            }

            // Compare AI's score with the player's score
            if (aiScore > score)
            {
                // If AI's score is higher, display a message indicating AI wins
                MessageBox.Show("AI Wins!");
            }
            else if (aiScore < score)
            {
                // If player's score is higher, display a message indicating the player wins
                MessageBox.Show("YOU WIN, CONGRATS, YOU BEAT THE AI!");
            }
            else
            {
                // If both scores are equal, it's a tie, so display a tie message
                MessageBox.Show("Offt that's a tough one its a tie!");
            }
        }

        private void InitializeGrid()
        {
            // Loop through the grid to create buttons for each cell
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    // Create a new Button control for the cell
                    buttons[x, y] = new Button
                    {
                        // Set the size of the button to 30x30 pixels
                        Size = new Size(30, 30),

                        // Calculate the location of the button based on grid coordinates
                        Location = new Point(x * 30, y * 30 + 24),

                        // Put a custom font on the buttons
                        Font = arialBoldFont
                    };

                    // Attach the Button_Click event handler to handle button clicks
                    buttons[x, y].Click += Button_Click;

                    // Add the button to the form's Controls collection for display
                    this.Controls.Add(buttons[x, y]);
                }
            }
        }

        private void PlaceMines()
        {
            // Create a new instance of the Random class to generate random numbers
            Random rand = new Random();

            // Initialize a variable to keep track of the number of mines placed
            int minesPlaced = 0;

            // Continue placing mines until the desired mineCount is reached
            while (minesPlaced < mineCount)
            {
                // Generate random coordinates (x, y) within the grid size
                int x = rand.Next(gridSize);
                int y = rand.Next(gridSize);

                // Check if there is no mine at the selected coordinates
                if (!mines[x, y])
                {
                    // Place a mine at the selected coordinates
                    mines[x, y] = true;

                    // Increment the count of placed mines
                    minesPlaced++;
                }
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            // Check if the game has ended; if so, do nothing
            if (gameEnded)
                return;

            // Cast the sender as a Button to get the clicked button
            Button clickedButton = sender as Button;

            // Calculate the grid coordinates (x, y) of the clicked button based on its location
            int x = clickedButton.Location.X / 30;
            int y = (clickedButton.Location.Y - 24) / 30; // Adjust for menu strip height

            // Check if the clicked cell contains a mine
            if (mines[x, y])
            {
                // If a mine is clicked, set the button's background color to red
                clickedButton.BackColor = Color.Red;

                // Resize image, cause it's stupid big originally
                Image resizedBombImage = ResizeImage(MineSweeperGame.Properties.Resources.conflict_explosion_icon, 12, 12);

                // Set resized image on hte button
                clickedButton.Image = resizedBombImage;

                // Align image to the middle
                clickedButton.ImageAlign = ContentAlignment.MiddleCenter;

                // Call the GameOver method to handle the game-ending logic
                GameOver();
            }
            else
            {
                // If the clicked cell does not contain a mine
                // Calculate the number of adjacent mines
                int adjacentMines = CountAdjacentMines(x, y);

                // Disable the clicked button to prevent further interaction
                clickedButton.Enabled = false;

                if (adjacentMines > 0)
                {
                    // If there are adjacent mines, display the count on the button
                    clickedButton.Text = adjacentMines.ToString();
                }
                else
                {
                    // If there are no adjacent mines, reveal adjacent cells recursively
                    RevealAdjacent(x, y);
                }

                // Increase the player's score
                IncreaseScore();
            }
        }

        private void IncreaseScore()
        {
            // Increment the current score by the scoreMultiplier value
            score += scoreMultiplier;

            // Check if the updated score is greater than the topScore
            if (score > topScore)
            {
                // If the updated score is higher, update the topScore to the new value
                topScore = score;
            }

            // Update the score labels to reflect the changes
            UpdateScoreLabels();
        }

        private void GameOver()
        {
            // Set the gameEnded flag to indicate that the game is over
            gameEnded = true;

            // Display a message to inform the player that the game is over
            MessageBox.Show("Game Over! Now it's AI's turn.");

            // Dispose of the existing buttons on the grid (clean up)
            DisposeExistingButtons();

            // Reinitialize the grid for AI's turn
            InitializeGrid();

            // Place mines on the grid for AI's turn
            PlaceMines();

            // Start AI's turn
            AITurn();

            // Dispose of the existing buttons on the grid (clean up)
            DisposeExistingButtons();

            // Reinitialize the grid for a new game
            InitializeGrid();

            // Place mines for a new game
            PlaceMines();

            // Reset the game (presumably resetting any game-specific variables)
            ResetGame();
        }

        private int CountAdjacentMines(int x, int y)
        {
            // Initialize a count to keep track of adjacent mines
            int count = 0;

            // Loop through neighboring cells using two nested loops for dx and dy
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Calculate the coordinates of the neighboring cell
                    int nx = x + dx;
                    int ny = y + dy;

                    // Check if the neighboring cell is within the grid bounds and contains a mine
                    if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize && mines[nx, ny])
                    {
                        // If there is a mine in the neighboring cell, increment the count
                        count++;
                    }
                }
            }

            // Return the total count of adjacent mines
            return count;
        }

        private void RevealAdjacent(int x, int y)
        {
            // Loop through neighboring cells using two nested loops for dx and dy
            for (int dx = -1; dx <= 1; dx++)
            {
                // BIG O NOTATION GOING WILD HERE
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Calculate the coordinates of the neighboring cell
                    int nx = x + dx;
                    int ny = y + dy;

                    // Check if the neighboring cell is within the grid bounds and is still enabled
                    if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize && buttons[nx, ny].Enabled)
                    {
                        // Count the number of adjacent mines for the neighboring cell
                        int adjacentMines = CountAdjacentMines(nx, ny);

                        // Disable the button to prevent further interaction
                        buttons[nx, ny].Enabled = false;

                        // Check if there are adjacent mines
                        if (adjacentMines > 0)
                        {
                            // If there are adjacent mines, display the count on the button
                            buttons[nx, ny].Text = adjacentMines.ToString();
                        }
                        else
                        {
                            // If there are no adjacent mines, recursively reveal adjacent cells
                            RevealAdjacent(nx, ny);
                        }
                    }
                }
            }
        }

        private void ResetGame()
        {
            // Reset the score and game state
            score = 0;
            gameEnded = false;
            UpdateScoreLabels();

            DisposeExistingButtons();

            // Reinitialize buttons and mines arrays
            buttons = new Button[gridSize, gridSize];
            mines = new bool[gridSize, gridSize];

            // Recreate the buttons and place mines
            InitializeGrid();
            PlaceMines();

            // Force the form to redraw itself
            this.Invalidate();
            this.Update();
        }

        private void UpdateScoreLabels()
        {
            scoreLabel.Text = $"Score: {score}";
            topScoreLabel.Text = $"Top Score: {topScore}";
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
