using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    //OBJECT POOLING, WHAT IS IT, AND CAN IT BE USED HERE?

    public const float maxViewDist = 384;
    public Transform player;

    public static Vector2 playerPos;

    int chunksize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, Chunk> chunkDict = new Dictionary<Vector2, Chunk>(); //Dictionary of Chunks
    List<Chunk> ChunksVisibleLastUpdate = new List<Chunk>(); //list of chunks that need to be destroyed


    // Start is called before the first frame update
    void Start()
    {
        chunksize = TerrainController.chunkSize - 1; //fetch chunk size and set properly
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDist / chunksize); //round up how many chunks you can see
    }

    private void Update()
    {
        playerPos = new Vector2(player.position.x, player.position.z); //update player pos
        UpdateVisibleChunks(); //update chunks
    }

    void UpdateVisibleChunks()
    {
        //hide all old chunks and clear the list so we don't constantly clear the old old ones
        for(int i =0; i < ChunksVisibleLastUpdate.Count; i++)
        {
            ChunksVisibleLastUpdate[i].SetVisible(false);
        }
        ChunksVisibleLastUpdate.Clear();

        //get coord of the chunk the player is in
        int currentChunkCoordX = Mathf.RoundToInt(playerPos.x / chunksize);
        int currentChunkCoordY = Mathf.RoundToInt(playerPos.y / chunksize);

        //loop through the surrounding chunks (8 around current chunk)
        for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
        {
            for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset); //getting coord of chunk

                //if that chunk is already in our dictionary, update it, if not, add it
                if (chunkDict.ContainsKey(viewedChunkCoord))
                {
                    chunkDict[viewedChunkCoord].UpdateChunk();
                    if (chunkDict[viewedChunkCoord].IsVisible())
                    {
                        ChunksVisibleLastUpdate.Add(chunkDict[viewedChunkCoord]);
                    }
                }
                else
                {
                    chunkDict.Add(viewedChunkCoord, new Chunk(viewedChunkCoord, chunksize, transform));
                }
            }
        }
    }

    public class Chunk
    {
        Vector2 pos;
        GameObject meshObj;
        Bounds bounds;

        public Chunk(Vector2 coord, int size, Transform parent)
        {
            pos = coord * size;
            Vector3 posV3 = new Vector3(pos.x, 0, pos.y);
            bounds = new Bounds(pos, Vector2.one * size);

            meshObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObj.transform.position = posV3;
            meshObj.transform.localScale = Vector3.one * size / 10f;
            meshObj.transform.parent = parent;
            SetVisible(false);
        }

        public void UpdateChunk()
        {
            float playerDistFromNewestEdge = Mathf.Sqrt(bounds.SqrDistance(playerPos));
            bool isVisible = playerDistFromNewestEdge <= maxViewDist; 
            SetVisible(isVisible);
        }

        public void SetVisible(bool visible)
        {
            meshObj.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObj.activeSelf;
        }
    }

}
