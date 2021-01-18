// Ryan Thomas
// October 2020

using System;
using Random = System.Random;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

// tracks obstacles and food placement/generation
public class LevelGrid : MonoBehaviour
{
    private int width;
    private int height;

    public List<GameObject> foodObjects;
    public List<GameObject> obstacleObjects;
    private Snake snake;

    private List<Vector2Int> playAreaPositions = new List<Vector2Int>();
    public List<Vector2Int> obstaclePositions = new List<Vector2Int>();

    // empty object that is parent of all obstacles (used for workplace sorting purpose)
    private static Transform obstacleParent;

    private static readonly string LAYER_NAME = "Enviroment";

    public void Start()
    {
        width = (int)(this.transform.lossyScale).x;
        height = (int)(this.transform.lossyScale).y;

        // This ensures the width and height are odd.
        width = width | 1;
        height = height | 1;

        int halfWidth = (width) / 2;
        int halfHeight = (height) / 2;

        Debug.Log("w = " + width + "\th =" + height);

        obstacleParent = this.transform;

        int counter = 1;

        for (int i = (height * width) - 1; i >= 0; i--)
        {
            Vector2Int vec = new Vector2Int((i % width) - halfWidth, (i / width) - halfHeight);
            playAreaPositions.Add(vec);
        }

        // extend height and width by one for borders
        halfHeight++;
        halfWidth++;

        // Create obstacles around level grid so snake stays contained to grid.
        // get top and bottom obstacles
        for (int i = -halfWidth; i <= halfWidth; i++)
        {
            MakeObstacle(i, halfHeight, counter++);
            MakeObstacle(i, -halfHeight, counter++);
        }

        // get left and right obstacles
        for (int j = -(halfHeight - 1); j <= (halfHeight - 1); j++)
        {
            MakeObstacle(halfWidth, j, counter++);
            MakeObstacle(-halfWidth, j, counter++);
        }

        // if no food has been spawned in, spawn food in out of bounds
        if (foodObjects.Count == 0)
        {
            GameObject foodGameObject = new GameObject("Food", typeof(SpriteRenderer));
            foodGameObject.GetComponent<SpriteRenderer>().sprite = GameAssets.i.foodSprite;
            foodGameObject.transform.position = new Vector3(-100, -100, 0);

            foodObjects.Add(foodGameObject);
        }

        // snap each food object to grid
        foreach (GameObject foodGameObject in foodObjects)
        {
            SnapToGrid(foodGameObject);
        }
    }

    public void SetSnake(Snake snake)
    {
        this.snake = snake;
    }

    public bool Place(PickUp p, Vector2Int v)
    {
        // check if the position is in the level grid
        if (playAreaPositions.IndexOf(v) < 0)
        {
            return false;
        }

        // check object to be placed is not in snake
        List<Vector2Int> tempList = new List<Vector2Int>();
        tempList.AddRange(snake.GetAllCoords());

        // if position is in snake, object can NOT place
        if (tempList.IndexOf(v) >= 0)
        {
            return false;
        }

        // check position doesn't overlap with (other) food objects
        foreach (GameObject go in foodObjects)
        {
            if (ToVector2Int(go.transform.position) == v &&
                p != go.GetComponent<PickUp>())
            {
                return false;
            }
        }

        // check position doesn't overlap with (other) obstacle objects
        foreach (GameObject go in obstacleObjects)
        {
            if (ToVector2Int(go.transform.position) == v &&
                p != go.GetComponent<PickUp>())
            {
                return false;
            }
        }

        return true;
    }

    // make sure object's position is marked by integers, not floats
    private bool SnapToGrid(GameObject go)
    {
        Vector3 v = go.transform.position;

        if (go.transform.GetComponent<PickUp>() == null)
        {
            go.transform.position = new Vector3((int)v.x, (int)v.y, v.z);
            return true;
        }
        return false;
    }

    // make sure all obstacles and food are snapped to grid and are no longer movable
    public void Bake()
    {
        Debug.Log("Baking Start");
        foreach (GameObject go in foodObjects)
        {
            SpriteRenderer sr = go.transform.GetComponent<SpriteRenderer>();
            sr.sprite = GameAssets.i.foodSprite;
            sr.sortingLayerName = LAYER_NAME;
        }

        foreach (GameObject go in obstacleObjects)
        {
            if (go.transform.GetComponent<PickUp>() != null)
            {
                PickUp pu = go.transform.GetComponent<PickUp>();
                Place(pu, ToVector2Int(go.transform.position));
            }

            SpriteRenderer sr = go.transform.GetComponent<SpriteRenderer>();
            sr.sprite = GameAssets.i.obstacleSprite;
            sr.sortingLayerName = LAYER_NAME;

            Vector2Int v = ToVector2Int(go.transform.position);
            Debug.Log(go.transform.position + "\t" + v);
            obstaclePositions.Add(v);
        }

        playAreaPositions = playAreaPositions.Except(obstaclePositions).ToList();
    }

    // generate obstacle at given position
    // int n is used for behind the scenes naming
    private void MakeObstacle(int x, int y, int n)
    {
        Vector2Int position = new Vector2Int(x, y);

        GameObject temp = new GameObject("Obstacle #" + n);
        temp.layer = 9;

        SpriteRenderer obstacleSR = temp.AddComponent<SpriteRenderer>();

        obstacleSR.sprite = GameAssets.i.obstacleSprite;
        obstacleSR.sortingLayerName = LAYER_NAME;
        temp.transform.parent = obstacleParent.transform;
        temp.transform.position = (new Vector3(x, y, 0)) - this.transform.position;

        obstacleObjects.Add(temp);
    }

    // This function handles food object
    public bool HandleFood(bool spawnNew)
    {
        GameObject snakeAte = null;
        for (int i = 0; i < foodObjects.Count(); i++)
        {
            GameObject go = foodObjects[i];
            Vector2Int v = ToVector2Int(go.transform.position);
            // we only need to check if food is on top of snake's head
            if (snake.GetHeadPosition() == v)
            {
                snakeAte = go;
                foodObjects.RemoveAt(i);
                // this break assumes only 1 food should be at any given position
                break;
            }
        }

        // if snake ate food
        if (snakeAte != null)
        {
            // if another food is supposed to spawn after a food has been eaten
            // "Spawn" here is a misnomer. New food objects are never spawned in. "Eaten" food objects are simply moved
            if (spawnNew)
            {
                Debug.Log("Spawning New Food");
                List<Vector2Int> tempList = GetOpenSpaces();

                // if there is room for food to spawn
                if (tempList.Count > 0)
                {
                    Random rnd = new Random();
                    int i = rnd.Next(tempList.Count);
                    Vector2Int foodGridPosition = tempList[i];
                    snakeAte.transform.position = (Vector2)foodGridPosition;
                    foodObjects.Add(snakeAte);

                    Debug.Log("Food set");
                    return true;
                }
                else
                {
                    Debug.Log("Play area :" + ((width - 1) * (height - 1)));
                    Debug.Log("Snake's Length:" + (snake.GetLength() + 1));

                    snakeAte.transform.position = new Vector3(-100, -100);
                    snakeAte.SetActive(false);
                    return false;
                }

            }
            // if food isn't supposed to respawn after eaten
            else
            {
                Destroy(snakeAte);
                return true;
            }
        }
        return false;
    }

    private List<Vector2Int> GetOpenSpaces()
    {
        List<Vector2Int> tempList = new List<Vector2Int>();
        tempList.AddRange(playAreaPositions);

        tempList = tempList.Except(snake.GetAllCoords()).ToList();
        return tempList;
    }

    public string GetGameOverState()
    {
        if (GetOpenSpaces().Count <= 0)
        {
            return "A LACK OF OPTIONS";
        }
        else if (IntersectsObstacle(snake.GetHeadPosition()))
        {
            return "A WALL";
        }
        return "YOURSELF";
    }

    public bool IntersectsObstacle(Vector2Int givenPosition)
    {
        // iterate through all the obstacles on the grid
        return obstaclePositions.IndexOf(givenPosition) >= 0;
    }

    public bool OnFood(Vector2Int givenPosition)
    {
        foreach (GameObject foodGameObject in foodObjects)
        {
            if (givenPosition == ToVector2Int(foodGameObject.transform.position))
            {
                return true;
            }
        }
        return false;
    }

    private Vector2Int ToVector2Int(Vector3 v)
    {
        return new Vector2Int((int)Math.Round(v.x), (int)Math.Round(v.y));
    }
}
