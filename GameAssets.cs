using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A class for referencing all the assets of the game
 */
public class GameAssets : MonoBehaviour
{
    public static GameAssets i;

    public static readonly int lastLevel = 8; 

    private void Awake()
    {
        i = this;
    }

    public Sprite snakeSprite;
    public Sprite headSprite;
    public Sprite foodSprite;
    public Sprite obstacleSprite;

    public GameObject scoreObject;
}
