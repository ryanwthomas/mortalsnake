using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text.RegularExpressions;

public class MainMenuScript : MonoBehaviour
{
    public GameObject titleTMP;
    public GameObject versionTMP;

    private int level;

    string konamiString = "";

    public void Start()
    {
        TextMeshProUGUI titleText = titleTMP.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI versionText = versionTMP.GetComponent<TextMeshProUGUI>();

        // Set all PlayerPrefs
        if (!PlayerPrefs.HasKey("Score"))
        {
            PlayerPrefs.SetInt("Score", 0);
        }
        // TODO: THIS IS HARD CODED BEWARE
        if (!PlayerPrefs.HasKey("Best"))
        {
            PlayerPrefs.SetInt("Best", 64);
        }

        level = PlayerPrefs.GetInt("Level", 0);

        // if game has been beaten
        if (level > GameAssets.lastLevel )
        {
            // this is to cap level at max
            level = GameAssets.lastLevel + 1;
            PlayerPrefs.SetInt("Level", level);

            titleText.text = "MORTAL\nSNAKE";
            versionText.text = "";
        }
        // if ouroboros has never been  formed
        else if (level <= 0)
        {
            // this is to cap level at min
            level = 0;
            versionText.text = "";
        }
        // normal case 
        else{
            versionText.text = "v0." + level;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }

        // input is tracked for Konami code debug
        if ( Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.R))
        {
            konamiString += "R";
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.D))
        {
            konamiString += "D";
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.L))
        {
            konamiString += "L";
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.U))
        {
            konamiString += "U";
        }
        else if (Input.GetKeyDown(KeyCode.A) )
        {
            konamiString += "A";
        }
        else if (Input.GetKeyDown(KeyCode.B) )
        {
            konamiString += "B";
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            Match m = Regex.Match(konamiString, "UU+DD+L+R+L+R+B+A+");
            if ( m.Success )
            {
                SceneManager.LoadScene("DeletePrefs");
            }

            m = Regex.Match(konamiString, "A+B+R+L+R+L+DD+UU+");
            if (m.Success)
            {
                Debug.Log("Score cleared");
                PlayerPrefs.SetInt("Score", 0);
            }

            konamiString = "";
        }
    }

    public void PlayGame()
    {
        // If level is less than 0, default to level0.
        // If the player has beaten all the levels, then level0 is loaded but with immortality turned off.
        if( level < 0 || level > GameAssets.lastLevel )
        {
            level = 0;
        }

        SceneManager.LoadScene("Level"+level);
    }
    
    public void OpenAbout()
    {
        SceneManager.LoadScene("AboutScene");
    }

    public void OpenLeaderboard()
    {
        SceneManager.LoadScene("LeaderboardScene");
    }

    // because the game is now published in WebGL, there's no need to close application
    public void QuitGame()
    {
        // Application.Quit();
    }

}
