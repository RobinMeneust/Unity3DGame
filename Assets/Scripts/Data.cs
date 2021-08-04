using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Data
{
    public static readonly int chunkWidth=16;
    public static readonly int chunkHeight=16;
    public static readonly int textureAtlasSizeInBlocks=16; //16x16 blocks on the atlas
    public static float normalizedAtlasSizeOfOneBlock{
        get { return 1f/textureAtlasSizeInBlocks;}
    }


    public static readonly Vector3[] verticesCoord = new Vector3[8]{
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
    };

    public static readonly int[,] triangles = new int[6,4]
    {
        //0 3 1 1 3 2
        {0, 3, 1, 2}, //Front
        {5, 6, 4, 7}, //Back
        {3, 7, 2, 6}, //Top
        {4, 0, 5, 1}, //Bot
        {1, 2, 5, 6}, //Right
        {4, 7, 0, 3} //Left
    };

    public static readonly Vector3[] checkFacesArray = new Vector3[6]{
        new Vector3(0.0f, 0.0f, -1.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, -1.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(-1.0f, 0.0f, 0.0f),
    };
}
