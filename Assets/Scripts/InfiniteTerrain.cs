using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{

    public const float maxViewDist = 300;
    public Transform viewer;

    public static Vector2 viewerPos;

    int chunksize;
    int chunksVisibleInViewDistance;

    // Start is called before the first frame update
    void Start()
    {
        chunksize = TerrainController.chunkSize - 1; //fetch chunk size and set properly
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDist / chunksize); //round up how many chunks you can see
    }

    void UpdateVisibleChunks()
    {
        int currentChunkCoordX = Mathf.RoundToInt(viewerPos.x / chunksize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPos.y / chunksize);
    }
}
