using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ErrorScreenScript : MonoBehaviour
{
    // amount of time before error screen automatically unloads itself
    public float timeout = 0f;

    private float timer = 1f;

    private string toDisplay;
    public GameObject textGameObject;
    private TextMeshProUGUI tmp;
    private bool finishedPrinting = false;

    public void Start()
    {
        SoundPlayer.i.PlaySound("glitch");
        SoundPlayer.i.PlaySound("ouroboros");

       string patchVersion;
        int level = PlayerPrefs.GetInt("Level", 0);
        if (level > GameAssets.lastLevel)
        {
            patchVersion = "v1.0";
            timeout = 10f;
        }
        else
        {
            patchVersion = "v0." + level;
        }

        if (level == 1)
        {
            timeout = 10f;
        }else if (level == 2)
        {
            timeout = 3f;
        }

        string patchNotes;

        if (level > GameAssets.lastLevel)
        {
            patchNotes = "- Immortality: REMOVED\n";
            patchNotes += "- Settings: RESET";
        }
        else if( level == 1 ) {
            patchNotes = " - Directional input: REMOVED\n - Automatic start: REMOVED\n - Manual start: IMPLEMENTED\n - Game board status: CORRUPTED\n - Object stability: CORRUPTED\n - Controls page: UPDATED";
        }
        else
        {
            patchNotes = "- Game board: RECONFIGURED";
        }

        toDisplay = "FATAL ERROR OCCURED\n" +
        "Unexpected error occured. Code patch auto-generating.\n" +
        "Patch " + patchVersion + " generating.\n" +
        "Patch " + patchVersion + " installation started.\n" +
        "\nPatch notes:\n" + patchNotes +
        "\n\nPatch " + patchVersion + " successfully installed." +
        "\nProgram rebooting. Please wait.";

        tmp = textGameObject.GetComponent<TextMeshProUGUI>();
    }

    public void Update()
    {
        if (toDisplay.Length > 0)
        {
            tmp.text = tmp.text + toDisplay.Substring(0, 1);
            toDisplay = toDisplay.Substring(1);
        }
        else
        {
            finishedPrinting = true;
        }

        if( finishedPrinting)
        {
            timer += Time.deltaTime;

            if (timer >= timeout)
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

    }


}
