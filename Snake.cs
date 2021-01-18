// Ryan Thomas
// October 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using System;

/*
 * SNAKE SPECIFICATIONS
 * - keeps track of snake's path
 * - snake grows if it eats food
 * - snake may be mortal or immortal
 * - if snake is immortal, snake will "dodge" obstacles
 * - if snake is mortal, snake will die
 */

public class Snake : MonoBehaviour
{
    // starting length of snake
    public int startLength = 3;
    public Direction startDirection = Direction.Right;

    // number of links in snake
    private int length;
    
    // queue of game objects that compose the links of the snake
    private Queue<GameObject> body = new Queue<GameObject>();
    
    // queue of game objects that connect the links aesthetically
    /* NOTE: connectors are inefficient, and should preferably be replaced by 
     * making body sprites x1.5 longer and rotating them to fill gaps between
     * individual links.
    */
    private Queue<GameObject> connectors = new Queue<GameObject>();
    
    // head of the snake
    private GameObject head;

    private GameObject connectorParent;

    // the direction the snake will "dodge" in
    bool clockwise = false;
    bool ateOnPreviousFrame = false;

    private LevelGrid levelGrid;
    public enum Direction
    {
        Left, Right, Up, Down, None
    };

    private Direction headDir;
    private Direction prevDir;

    // hashmap used to convert a direction enum to vector
    public static Dictionary<Direction, Vector2Int> dirToVector = new Dictionary<Direction, Vector2Int>() {
        { Direction.Left,   new Vector2Int(-1, 0)},
        { Direction.Right,  new Vector2Int(1, 0)},
        { Direction.Up,     new Vector2Int(0, 1)},
        { Direction.Down,   new Vector2Int(0, -1)}
    };

    // hashmap used to converts string to direction enum
    public static Dictionary<string, Direction> strToDir = new Dictionary<string, Direction>() {
        { "A", Direction.Left },
        { "D", Direction.Right },
        { "W", Direction.Up },
        { "S", Direction.Down }
    };

    // arrays used to determine which direction to go when "dodging"
    Direction[] upRotations = {Direction.Up, Direction.Right, Direction.Down, Direction.Left};
    Direction[] rightRotations = { Direction.Right, Direction.Down, Direction.Left, Direction.Up};
    Direction[] downRotations = { Direction.Down, Direction.Left, Direction.Up, Direction.Right};
    Direction[] leftRotations = { Direction.Left, Direction.Up, Direction.Right, Direction.Down};

    public void Start()
    {
        // snake may be spawned in with a dummy sprite. We delete the dummy sprate
        if ( this.GetComponent<SpriteRenderer>() != null)
        {
            Destroy(GetComponent<SpriteRenderer>());
        }

        // snake must be at least 1 link long
        if (startLength < 1)
        {
            startLength = 1;
        }

        length = startLength;
        prevDir = (headDir = startDirection);
        Vector2Int startPosition = ToVector2Int( this.transform.position );

        // snap snake to grid
        this.transform.position = (Vector2) startPosition;

        // for connectors
        Vector2 prevPos = startPosition;
        connectorParent = new GameObject("Connector Parent");
        connectorParent.transform.parent = this.transform;

        // generate body of snake
        // link are generated in reverse because the tail should be first in the queue
        for (int i = startLength - 1; i >= 0; i--)
        {
            GameObject snakeGO = new GameObject("BodyPiece #" + i);

            snakeGO.transform.parent = this.transform;
            SpriteRenderer snakeSR = snakeGO.AddComponent<SpriteRenderer>();

            Vector2 bodyPosition = (startPosition -
                i * dirToVector[startDirection]);

            snakeGO.transform.position = bodyPosition;

            // if the piece isn't first
            if (i > 0)
            {
                snakeSR.sprite = GameAssets.i.snakeSprite;
            }
            // if the piece is first
            else
            {
                // set head
                snakeSR.sprite = GameAssets.i.headSprite;
                head = snakeGO;
            }

            body.Enqueue(snakeGO);

            // generate connectors in between links
            if ( i < startLength-1 )
            {
                Vector2 midPoint = ((Vector2)(prevPos + bodyPosition)) * 0.5f;

                GameObject go = new GameObject("Connector #"+i);
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                // sr.sortingLayerName = LAYER_NAME;
                sr.sprite = GameAssets.i.snakeSprite;

                go.transform.position = midPoint;
                go.transform.parent = connectorParent.transform;

                connectors.Enqueue(go);
            }
            prevPos = bodyPosition;
        }
    }

    public void SetUp(LevelGrid levelGrid)
    {
        this.levelGrid = levelGrid;
    }

    public bool SetDirection(string s)
    {
        Direction directionalInput = strToDir[s];
        /* only change direction if the input is new and
         * wouldn't make the snake go backwards (e.g. change from right to left)
         */
        if (directionalInput != headDir && !IsFlipped(directionalInput))
        {
            headDir = directionalInput;
            return true;
        }
        return false;
    }

    public bool Step(bool mortal)
    {
        bool movable = true;

        // change clockwise direction if the snake turned
        if (IsClockwise(prevDir, headDir))
        {
            clockwise = true;
        }
        else if (IsWiddershins(prevDir, headDir))
        {
            clockwise = false;
        }

        Vector2Int pos = GetPosition(head);

        // if the original direction would crash (and snake is immortal)
        if (!mortal && !CanMove(headDir, pos + dirToVector[headDir]))
        {
            movable = false;
            Direction[] otherDirs;
            // check in opposite direction
            if (!clockwise)
            {
                Direction[] x = { RotateClockwise(headDir, 1),
                        RotateClockwise(headDir, 2), RotateClockwise(headDir, 3) };
                otherDirs = x;
            }
            else
            {
                // reverse order for widdershins
                Direction[] x = { RotateClockwise(headDir, 3),
                        RotateClockwise(headDir, 2), RotateClockwise(headDir, 1) };
                otherDirs = x;
            }

            // check other directions for a direction to "dodge" in
            for (int i = 0; i < 3; i++)
            {
                if (CanMove(otherDirs[i], pos + dirToVector[otherDirs[i]]))
                {
                    movable = true;
                    // the snake dodged a crash
                    //TODO play dodging sound?
                    headDir = otherDirs[i];

                    break;
                }
            }

            // OH NO! No solution found!!
            if (!movable)
            {
                return false;
            }
        }

        // if mortal and moving would cause death
        if( mortal )
        {
            bool toReturn = CanMove(headDir, pos + dirToVector[headDir]);
            this.MoveDirection();
            return toReturn;
        }
        else
        {
            // this should always return true
            bool moved = this.MoveDirection();
            // Debug.Log("X" + moved);
            return moved;
        }
    }
    
    public Vector2Int GetPosition(GameObject go)
    {
        Vector3 tempPos = go.transform.position;
        Vector2Int pos = ToVector2Int(tempPos);
        return pos;
    }

    public bool CheckOuroboros()
    {
        // if head is on tail, OUROBOROS has occured
        if (GetPosition(head) == GetPosition(body.Peek()))
        {
            GameObject tail = body.Peek();
            SetSprite(tail, GameAssets.i.headSprite );
            return true;
        }
        return false;
    }

    public bool MoveDirection()
    {
        Direction givenDirection = headDir;

        Vector3 newPosition = (Vector2)(GetPosition(head) +
            dirToVector[givenDirection]);

        Vector3 midPosition = ((Vector3)(Vector2)GetPosition(head) + newPosition)*0.5f;

        // update curving
        if (IsClockwise(prevDir, headDir))
        {
            clockwise = true;
        }
        else if (IsWiddershins(prevDir, headDir))
        {
            clockwise = false;
        }

        prevDir = headDir;

        // current head will not be a head on next frame, so change sprite
        SetSprite(head, GameAssets.i.snakeSprite);

        // if the snake should grow
        if (ateOnPreviousFrame)
        {
            // generate new link and connector and place them at the front of the queue

            ateOnPreviousFrame = false;
            // generate new link
            {
                GameObject snakeGO = new GameObject("BodyPiece #" + length);
                snakeGO.transform.parent = this.transform;
                SpriteRenderer snakeSR = snakeGO.AddComponent<SpriteRenderer>();

                snakeSR.sprite = GameAssets.i.headSprite;
                snakeGO.transform.position = newPosition;

                // change old head sprite
                body.Enqueue(snakeGO);

                head = snakeGO;
            }
            // generate new connector
            {
                GameObject connectorGO = new GameObject("Connector #" + length);
                connectorGO.transform.parent = connectorParent.transform;
                SpriteRenderer connectorSR = connectorGO.AddComponent<SpriteRenderer>();

                connectorSR.sprite = GameAssets.i.snakeSprite;
                connectorGO.transform.position = midPosition;

                connectors.Enqueue(connectorGO);
            }

            length++;
        }
        // if the snake doesn't grow
        else
        {
            /* remove link and connector from the front of the queue and
             * move them to the back of the queue
             */
            GameObject newHead = body.Dequeue();
            head = newHead;

            newHead.transform.position = newPosition;
            body.Enqueue(newHead);

            GameObject newConnector = connectors.Dequeue();
            newConnector.transform.position = midPosition;

            connectors.Enqueue( newConnector );
        }

        SetSprite(head, GameAssets.i.headSprite);

        // if current head is on food
        if (levelGrid.OnFood(GetPosition(head)))
        {
            ateOnPreviousFrame = true;
        }

        return true;
    }

    public bool MovedOntoFood()
    {
        return ateOnPreviousFrame;
    }   

    /* TODO: this current runs in O(length) time.
     * This could be optimized by having list of link coordinates that's updated when snake moves.
     * Though, ultimately O(100) is neglible.
     */
    public bool HasCoord(Vector2Int vec, bool includingTail)
    {
        // if given coord is on tail
        if( vec == GetPosition(body.Peek()))
        {
            return includingTail;
        }

        IEnumerator i = body.GetEnumerator();

        while ( i.MoveNext() )
        {
            // TODO: There has to be a way to do this without casting, right?
            if ( GetPosition((GameObject)i.Current) == vec )
            {
                return true;
            }
        }

        return false;
    }
    
    public Vector2Int[] GetAllCoords()
    {
        Vector2Int[] toReturn = new Vector2Int[ body.Count ];
        int index = 0;

        IEnumerator i = body.GetEnumerator();
        while (i.MoveNext())
        {
            // this line of code sucks
            toReturn[index++] = GetPosition((GameObject)i.Current );
        }
        return toReturn;
    }

    public Vector2Int GetHeadPosition()
    {
        return GetPosition(this.head);
    }

    public int GetLength()
    {
        return length;
    }

    private Vector2Int ToVector2Int(Vector3 v)
    {
        return new Vector2Int((int)Math.Round(v.x), (int)Math.Round(v.y));
    }

    private void SetSprite(GameObject go, Sprite sp)
    {
        SpriteRenderer snakeSR = go.transform.GetComponent<SpriteRenderer>();

        snakeSR.sprite = sp;
    }

    private bool CanMove(Direction givenDirection, Vector2Int newPosition)
    {
        /*
         * make sure heading in the given direction doesn't
         * A) flip the previous direction
         *      (e.g. can't go from up -> back, left -> right, or vice versa)
         * 
         * B) intersect with a section of body (not including the tail)
         * 
         * C) intersect with an obstacle on level grid
        */

        if (IsFlipped(givenDirection) ||
            HasCoord(newPosition, false) ||
            levelGrid.IntersectsObstacle(newPosition))
        {
            return false;
        }
        else
        {
            // else, snake can move!
            return true;
        }
    }

    private Direction RotateClockwise(Direction d, int turns)
    {
        switch (d)
        {
            case Direction.Left: return leftRotations[turns % 4];
            case Direction.Up: return upRotations[turns % 4];
            case Direction.Right: return rightRotations[turns % 4];
            case Direction.Down: return downRotations[turns % 4];
            default: return d;
        }
    }

    private bool IsFlipped(Direction d)
    {
        switch (prevDir)
        {
            case Direction.Right:
                return Direction.Left == d;
            case Direction.Up:
                return Direction.Down == d;
            case Direction.Left:
                return Direction.Right == d;
            case Direction.Down:
                return Direction.Up == d;
            default:
                return false;
        }
    }

    private bool IsClockwise(Direction d1, Direction d2)
    {
        switch (d1)
        {
            case Direction.Right: return d2 == Direction.Down;
            case Direction.Down: return d2 == Direction.Left;
            case Direction.Left: return d2 == Direction.Up;
            case Direction.Up: return d2 == Direction.Right;
            default: return false;
        }
    }

    private bool IsWiddershins(Direction d1, Direction d2)
    {
        switch (d1)
        {
            case Direction.Right: return d2 == Direction.Up;
            case Direction.Up: return d2 == Direction.Left;
            case Direction.Left: return d2 == Direction.Down;
            case Direction.Down: return d2 == Direction.Right;
            default: return false;
        }
    }
}
