using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Random = UnityEngine.Random;
using System.Collections.Specialized;

// Code adapted from Kemble Software
// https://www.youtube.com/watch?v=FXMqUdP3XcE

public class ColorfulText : MonoBehaviour
{
    TextMeshProUGUI temp;
 //   float speed = 20f;
//    float amplitude = .005f;

    Color32 specialColor = new Color32((byte)215, (byte)0, (byte)0, 255);

    void Awake()
    {
        temp = this.transform.GetComponent<TextMeshProUGUI>();
        temp.ForceMeshUpdate();
    }


    void Update()
    {
        var textInfo = temp.textInfo;

        var guard = Math.Min(4 + 8, temp.textInfo.characterCount);
        for (int i = 4; i < guard; ++i)
        {
            var charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
            {
                continue;
            }

            var meshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];

            for (int j = 0; j < 4; j++)
            {
                var index = charInfo.vertexIndex + j;
                /*
                var orig = meshInfo.vertices[index];
                meshInfo.vertices[index] = orig +
                    new Vector3(0,
                    Mathf.Sin(Time.time * speed + orig.x * 0.01f) * amplitude,
                    0);
                */
                // meshInfo.colors32[index] = new Color32((byte)Random.Range(0, 124), (byte)Random.Range(0, 124), (byte)(Random.Range(0, 124) + 170), 127);
                meshInfo.colors32[index] = specialColor;
                //meshInfo.colors32[index] = Color.red;
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; ++i)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            meshInfo.mesh.colors32 = meshInfo.colors32;
            temp.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
