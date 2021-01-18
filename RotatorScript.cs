using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatorScript : MonoBehaviour
{
    public float rotateSpeed = 360f;
    private float z = 180f;

    // Update is called once per frame
    void Update()
    {
        z += Time.deltaTime * -rotateSpeed;

        // Debug.Log(Time.deltaTime * rotateSpeed);

        z = (z % 360f);

        this.transform.localRotation = Quaternion.Euler(0,0,z);
    }
}
