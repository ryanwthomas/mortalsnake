using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class BasicSceneScript : MonoBehaviour
{
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Esc Pressed from BSS");
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void OpenMainMenu()
    {
        Debug.Log("Button clicked");
        SceneManager.LoadScene("MainMenu");
    }
}
