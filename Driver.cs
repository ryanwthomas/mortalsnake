// Ryan Thomas
// October 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Debug = UnityEngine.Debug;
using System.Configuration;

// This handles communication between the snake and level grid and has LevelGrid component
// Handles the pause/gameover menu and level's setting
public class Driver : MonoBehaviour
{
    // play area is a dummy object used to establish the bounds of the grid
    public GameObject playArea;

    public GameObject pauseScreen;
    public GameObject gameOverScreen;

    public GameObject snakeGameObject;
    public GameObject leaderboardObject;

    [Range(0f, 1f)]
    public float gridMoveTimerMax = .25f;
    private float gridMoveTimer = 0f;

    private TextMeshPro scoreText;
    private TextMeshPro conditionText;

    private LevelGrid levelGrid;
    private Snake snake;
    private LeaderboardScript leaderboard = null;

    // variables to store from GetInt
    private int startLevel;
    private int globalBest;
    private int playerBest;

    // isPuzzleLevel defines whether the level is normal or a puzzle level
    private bool isPuzzleLevel = false;
    // mortal defines mortality
    private bool mortal = false;

    // started defines whether the snake has started moving
    private bool started = false;
    private bool paused = false;
    private bool ateOwnTail = false;
    private bool gameOver = false;

    // This defines whether objects on the level grid have been baked
    private bool bakedLevelGrid = false;

    private int score = 0;

    // Start is called before the first frame update
    void Start()
    {
        startLevel = PlayerPrefs.GetInt("Level", 0);
        globalBest = PlayerPrefs.GetInt("Best", 0);
        playerBest = PlayerPrefs.GetInt("Score", 0);

        Debug.Log("Settings for Level " + startLevel);

        if (startLevel == 0 || startLevel > GameAssets.lastLevel)
        {
            started = true;
        }
        else
        {
            isPuzzleLevel = true;
        }

        if (startLevel > GameAssets.lastLevel)
        {
            mortal = true;
        }

        // the snake should move faster on puzzle levels
        if (isPuzzleLevel)
        {
            SetSpeed(.10f);
        }
        else
        {
            SetSpeed(.25f);
        }

        scoreText = (GameAssets.i.scoreObject).GetComponent<TextMeshPro>();
        UpdateScoreText();

        pauseScreen.SetActive(false);
        gameOverScreen.SetActive(false);

        levelGrid = playArea.GetComponent<LevelGrid>();

        // ensure leaderboardObject has LeaderboardScript component
        if (leaderboardObject != null)
        {
            leaderboard = leaderboardObject.GetComponent<LeaderboardScript>();
        }

        if (leaderboard != null)
        {
            leaderboard.SetStaticLeaderboard(0);
        }

        snake = snakeGameObject.GetComponent<Snake>();
        snake.SetUp(levelGrid);
        levelGrid.SetSnake(snake);
    }

    private void Update()
    {
        // keyboard shortcuts for window commands
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenMainMenu();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }

        // SECRET DEV COMMAND: This maximizes the snake's speed
        if (Input.GetKeyDown(KeyCode.Z))
        {
            SetSpeed(0.0f);
        }

        // if pause action is initiated
        if (!gameOver && !ateOwnTail && Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        // if a puzzle level is initiated
        if (!started && !paused && Input.GetKeyDown(KeyCode.Space))
        {
            started = true;
        }

        // if the game is active
        if (!gameOver && started && !ateOwnTail && !paused)
        {
            // ensure grid is baked
            if (!bakedLevelGrid)
            {
                levelGrid.Bake();
                bakedLevelGrid = true;
            }

            // get the last directional in 
            string directionalInput = HandleInput();

            // don't take directional inputs on puzzle levels
            if (!isPuzzleLevel && directionalInput != null)
            {
                snake.SetDirection(directionalInput);
            }

            // update time tracker
            gridMoveTimer += Time.deltaTime;

            // if enough time has passed and snake should move
            if (gridMoveTimer >= gridMoveTimerMax)
            {
                gridMoveTimer -= gridMoveTimerMax;
                // attempt to step snake forward
                bool moved = snake.Step(mortal);

                // if snake moved
                if (moved)
                {
                    // SoundPlayer.i.PlaySound("movement");

                    // if snake ate
                    if (snake.MovedOntoFood())
                    {
                        score = score + 1;

                        // if leaderboard exists, update leaderboard
                        if (leaderboard != null)
                        {
                            leaderboard.SetStaticLeaderboard(score);
                        }

                        SoundPlayer.i.PlaySound("ate");

                        // set player's all-time best score
                        if (playerBest < score)
                        {
                            PlayerPrefs.SetInt("Score", score);
                            // set global best score
                            if (globalBest < score)
                            {
                                PlayerPrefs.SetInt("Best", score);
                                globalBest = score;
                            }
                        }

                        UpdateScoreText();

                        // if no place for food to spawn
                        if (!levelGrid.HandleFood(!isPuzzleLevel))
                        {
                            gameOver = true;
                        }
                    }

                    // if snake formed ouroboros
                    if (snake.CheckOuroboros())
                    {
                        Debug.Log("Ouroboros occured:\t" + mortal);
                        // if snake ouroboros'd while immortal, increment level
                        if (!mortal)
                        {
                            PlayerPrefs.SetInt("Level", startLevel + 1);

                            SceneManager.LoadScene("ErrorScreen");
                        }
                        // if snake is mortal, this is a normal game over
                        else
                        {
                            Debug.Log("GAME OVER");
                            gameOver = true;
                        }

                    }
                }
                // snake failed to move (which can only occur if snake is immortal)
                else
                {
                    gameOver = true;
                }

                // if game is now over
                if (gameOver)
                {
                    // immortal snake receive a permanent pause screen
                    if (!mortal)
                    {
                        paused = true;
                        SetPauseScreen();
                    }
                    // mortal snake receive a game over screen
                    else
                    {
                        SetGameOverScreen(levelGrid.GetGameOverState());
                    }
                }// not game over
            }
        } // game is not still active
    } // end of update 

    // NOTE: this text formar is hardcoded for the current display
    private void UpdateScoreText()
    {
        string str = "HIGH\nSCORE\n-----\n" + globalBest + "\n\n\n\n\n\n\n\nSCORE\n-----\n" + score;
        scoreText.text = str;
    }

    private string HandleInput()
    {

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            return "D";
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            return "S";
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            return "A";
        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            return "W";
        }

        return null;
    }

    public void OpenMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Note: If player has beat level, then "restarting" actually means moving onto the next level
    public void RestartLevel()
    {
        if(startLevel < 0 || startLevel > GameAssets.lastLevel)
        {
            SceneManager.LoadScene("Level" + 0);

        }
        else
        {
            // TODO: Put max on level to make sure a level too high isn't erroneous tried to be loaded
            SceneManager.LoadScene("Level" + (startLevel + (ateOwnTail ? 1 : 0)));

        }
    }

    // 
    public void TogglePause()
    {
        paused = gameOver || !paused;

        SetPauseScreen();
    }

    public void SetPauseScreen()
    {
        if (paused)
        {
            pauseScreen.SetActive(true);
        }
        else
        {
            pauseScreen.SetActive(false);
        }
    }

    public void SetGameOverScreen(string s)
    {
        if (gameOverScreen != null && gameOverScreen.transform.GetComponent<GameOverScreenScript>() != null)
        {
            gameOverScreen.SetActive(true);
            (gameOverScreen.transform.GetComponent<GameOverScreenScript>()).SetText("YOU RAN INTO " + s);


        }
    }

    private void SetSpeed(float f)
    {
        gridMoveTimerMax = f;
    }

}
