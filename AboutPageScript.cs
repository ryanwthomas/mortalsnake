using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AboutPageScript : MonoBehaviour
{
    public GameObject instructionObject;

    // Start is called before the first frame update
    void Start()
    {
        int level = PlayerPrefs.GetInt("Level", 0);

        TextMeshProUGUI text = instructionObject.GetComponent<TextMeshProUGUI>();

        string topText = "";

        if( level <= 0 || level > GameAssets.lastLevel)
        {
            topText += "WASD to MOVE";
        }
        else
        {
            topText += "SPACE to START";
            topText += "\nCLICK to PICK UP/PLACE";
        }

        string bottomText = "P to PAUSE\nR to RESTART\nESC to EXIT";
        text.text = topText + "\n" + bottomText;
    }
}
