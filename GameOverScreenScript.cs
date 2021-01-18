using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverScreenScript : MonoBehaviour
{
    public TextMeshProUGUI text;

    // Set the game over screen's text to display why the player died
    public void SetText(string s)
    {
        text.text = s;
    }
}
