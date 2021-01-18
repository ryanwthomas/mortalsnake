using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetPrefsScript : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("MainMenu");
    }
}
