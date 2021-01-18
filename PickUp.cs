using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

// Applied to objects that can be picked up via mouse click by the player
public class PickUp : MonoBehaviour
{
    public GameObject levelGridObject;

    private static readonly string LAYER_NAME_UP = "PickUpUp";
    private static readonly string LAYER_NAME_DOWN = "PickUpDown";
    LevelGrid levelGrid;

    // pressed is true if the object has been clicked on
    bool pressed = false;
    // placed is true if the object has been snapped to grid
    bool placed = false;

    Vector3 startingPos;
    Vector3 jitterSum = Vector3.zero;

    void Start()
    {
        startingPos = transform.position;

        levelGrid = levelGridObject.transform.GetComponent<LevelGrid>();
    }

    void OnMouseDown()
    {
        // if (!pressed && !paused)
        if (!pressed )
        {
            pressed = true;
            placed = false;
            SetSortingLayer(LAYER_NAME_UP);
        }
    }

    void OnMouseUp()
    {
        // if (pressed && !paused)
        if (pressed)
        {
            placed = TryPlace();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pressed)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // make z=0 so object doesnt clip with camera
            mousePos.z = 0;
            this.transform.position = mousePos;

            jitterSum = Vector3.zero;
        }
        else if (!placed)
        {
            Jitter();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // spacebar has been pressed, the object MUST BE put down
            if (!placed)
            {
                TryPlace();
            }
            Destroy(this);
        }
    }

    private bool TryPlace()
    {
        pressed = false;

        int x = (int)Math.Round(this.transform.position.x);
        int y = (int)Math.Round(this.transform.position.y);

        SetSortingLayer(LAYER_NAME_DOWN);

        // if the object can NOT be placed, send it back to home
        if (levelGrid.Place(this, new Vector2Int(x, y)))
        {
            this.transform.position = new Vector2(x, y);
            return true;
        }
        else
        {
            this.transform.position = startingPos;
            return false;
        }
    }

    private void SetSortingLayer(string layerName)
    {
        (this.transform.GetComponent<SpriteRenderer>()).sortingLayerName = layerName;
    }

    // jitter the object around to catch players' attention
    private void Jitter()
    {
        float d = .05f;
        float maxWanderDistance = .25f;
        // Random rnd = new Random();
        Vector3 v = new Vector3(Random.Range(0f, d) - (d / 2),
            Random.Range(0f, d) - (d / 2), 0);

        jitterSum = jitterSum + v;
        
        // if the object wanders too far, snap it back to its starting position
        if (jitterSum.magnitude > maxWanderDistance)
        {
            this.transform.position = startingPos;
            jitterSum = Vector3.zero;
        }
        else
        {
            this.transform.position = this.transform.position + v;
        }
    }
}
