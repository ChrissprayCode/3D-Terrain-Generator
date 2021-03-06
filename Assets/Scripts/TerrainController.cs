using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainController : MonoBehaviour
{
    public const int chunkSize = 129;

    //Meshes and Prefabs
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Renderer textureRenderer;
    public GameObject treeParent;
    public GameObject tree;
    public MeshGeneration meshGen = new MeshGeneration();
    public GameObject meshObj;

    //Perlin Noise variables
    [Header("Perlin Noise")]
    [Min(0)] public int seed;
    [Range (20f, 70f)]public float scale;
    [Range (1,10)]public int octaves;
    [Range (0.3f, 0.7f)]public float persistance;
    [Range(1.5f, 3f)] public float lacunarity;
    [Range(6, 30)] public int heightMultiplier;
    [Range(1, 4)] public int meshSimplificationLevel;
    [Range(2f, 5f)] public float curve;

    //Colouring
    [Header("Colouring")]
    public Gradient gradient;
    public gradientFilter ColourType;
    string enumResult;
    Color[] gradientColour = new Color[chunkSize * chunkSize]; //make new colour array the size of the map
    

    //Trees
    [Header("Trees")]
    public bool spawnTrees;
    public int forestStartKey;
    public int forestEndKey;
    [Range(1f, 2f)] public float radius = 1.5f;
    public int rejectionCount = 30;

    public float treeLowSpawn = 1f;
    public float treeHighSpawn = 30f;

    [System.Serializable]
    public enum gradientFilter
    {
        Blend,
        Fixed
    }
    
    public void GenerateTerrain()
    {
        float[,] noiseMap = PerlinNoise.genNoise(chunkSize, chunkSize, seed, octaves, scale, persistance, lacunarity, curve); //Make the noise map
        ColourMap(noiseMap); //colour the noise map
        
        Mesh mesh = meshGen.GenMesh(noiseMap, heightMultiplier, meshSimplificationLevel); //make a mesh with the noisemap

        //Clear old mesh and make new
        if (meshObj.TryGetComponent(typeof(MeshCollider), out Component comp))
        {
            DestroyImmediate(comp);
        }
        
        meshFilter.sharedMesh = mesh; //assign the mesh to filtered mesh so it can be shown in editor
        var possibleMeshCol = meshObj.AddComponent<MeshCollider>();
        meshFilter.sharedMesh = possibleMeshCol.sharedMesh;


        //spawn trees if chosen to
        if (spawnTrees) {
            TreePlacer(chunkSize - 1, chunkSize - 1, forestStartKey, forestEndKey, tree, noiseMap);
        }
        else if (!spawnTrees)
        {
            DestroyTrees();
        }

    }

    public void ColourMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height); //create a new empty texture

        //Colouring the map
        
        enumResult = ColourType.ToString(); //get the result from the enum choice

        //change the gradientMode based on the result, as well as the filterMode that suits it best
        if (enumResult == "Blend")
        {
            gradient.mode = GradientMode.Blend; //change gradient mode to blend
            texture.filterMode = FilterMode.Bilinear; //change gradient filter to Bilinear
        }
        else if (enumResult == "Fixed")
        {
            gradient.mode = GradientMode.Fixed; //change gradient to fixed
            texture.filterMode = FilterMode.Point; //change filter to point
        }

        //apply the colour to the map based on the height
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noiseHeight = noiseMap[x, y]; //store the height
                
                gradientColour[y * width + x] = gradient.Evaluate(noiseHeight); //put the right colour in

            }
        }

        //stop colour from wrapping to opposite side
        texture.wrapMode = TextureWrapMode.Clamp;

        //set the colour to the texture
        texture.SetPixels(gradientColour);
        texture.Apply();
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(width, 1, height);
    }

    public void OnValidate()
    {
        scale = Mathf.RoundToInt(scale);
    }

    //destroy trees
    public void DestroyTrees()
    {
        var trees = GameObject.FindGameObjectsWithTag("tree"); 
        foreach (var newTree in trees) 
        { 
            DestroyImmediate(newTree); 
        }
    }


    void TreePlacer(int width, int height, int forestStart, int forestEnd, GameObject tree, float[,] noiseMap)
    {
        DestroyTrees();

        float maxHeight = 0;
        float minHeight = -10;

        //get the spawn points
        List<Vector2> points = PoissonNoise2.GeneratePoints(radius, new Vector2(127, 127), rejectionCount);


        //catch max and minimum height
        for (int x = 0; x < width; x += meshSimplificationLevel)
        {
            
            for (int y = 0; y < height; y += meshSimplificationLevel)
            {
                if (noiseMap[x, y] * heightMultiplier > maxHeight)
                {
                    maxHeight = noiseMap[x, y] * heightMultiplier;
                }
                else if (noiseMap[x, y] * heightMultiplier < minHeight)
                {
                    minHeight = noiseMap[x, y] * heightMultiplier;
                }
            }
        }

        SpawnTree2(tree, points, minHeight, maxHeight, forestStart, forestEnd);

    }

    void SpawnTree(GameObject tree, List<Vector2> points, float minHeight, float maxHeight, int forestStart, int forestEnd)
    {
        //create an array for gradient keys
        GradientColorKey[] gradKeys;
        gradKeys = gradient.colorKeys;


        foreach (var point in points)
        {
            Vector3 pointPos = new Vector3(point.x, 30, point.y);

            Ray ray = new Ray(pointPos, Vector3.down * 30);
            RaycastHit hit;
            float spawnY = 0;

            if (Physics.Raycast(ray, out hit))
            {
                spawnY = hit.point.y;
            }

            //normalise spawnY
            float normalisedSpawnY = ((spawnY - minHeight) / (maxHeight - minHeight));

            if (normalisedSpawnY > gradKeys[forestStart].time && normalisedSpawnY < gradKeys[forestEnd].time)
            {
                Instantiate(tree, new Vector3(point.x, spawnY, point.y), Quaternion.Euler(270, Random.Range(0f, 360f), 0));
            }
        }

        //get all trees and parent them under an empty obj
        var spawnedTrees = GameObject.FindGameObjectsWithTag("tree");
        foreach (var newTree in spawnedTrees)
        {
            float size = Random.Range(0.2f, 0.5f);
            newTree.gameObject.transform.localScale = new Vector3(size, size, size);
            newTree.transform.parent = treeParent.transform;
        }
    }

    void SpawnTree2(GameObject tree, List<Vector2> points, float minHeight, float maxHeight, int forestStart, int forestEnd)
    {

        foreach (var point in points)
        {
            Vector3 pointPos = new Vector3(point.x, 30, point.y);

            Ray ray = new Ray(pointPos, Vector3.down * 30);
            RaycastHit hit;
            float spawnY = 0;

            if (Physics.Raycast(ray, out hit) && hit.transform.tag == "ground")
            {
                spawnY = hit.point.y;
            }

            //normalise spawnY
            float normalisedSpawnY = ((spawnY - minHeight) / (maxHeight - minHeight));

            if (normalisedSpawnY > treeLowSpawn && normalisedSpawnY < treeHighSpawn)
            {
                Instantiate(tree, new Vector3(point.x, spawnY, point.y), Quaternion.Euler(270, Random.Range(0f, 360f), 0));
            }
        }

        //get all trees and parent them under an empty obj
        var spawnedTrees = GameObject.FindGameObjectsWithTag("tree");
        foreach (var newTree in spawnedTrees)
        {
            float size = Random.Range(0.2f, 0.5f);
            newTree.gameObject.transform.localScale = new Vector3(size, size, size);
            newTree.transform.parent = treeParent.transform;
        }
    }


    void PlaceTrees(int width, int height, int forestStart, int forestEnd, GameObject tree, float[,] noiseMap, List<Vector2> points)
    {
        DestroyTrees();

        //create new variables
        Vector3[] treeLocations = new Vector3[width*height];
        forestStart -= 1; 
        forestEnd -= 1; 
        int counter = 0;

        float maxHeight = 10;
        float minHeight = 10;
        float[] normalizedHeight = new float[width * height];

        //create an array for gradient keys
        GradientColorKey[] gradKeys;
        gradKeys = gradient.colorKeys;

        //ensure that the forest at the correct positions
        if(forestEnd < forestStart)
        {
            forestEnd = forestStart + 1;
        }

        int treeCounter = 0;//start tree counter


        for (int x = 0; x < width; x+= meshSimplificationLevel)
        {
            //catch max and minimum height
            for (int y = 0; y < height; y+= meshSimplificationLevel)
            {
                if (noiseMap[x, y] * heightMultiplier > maxHeight)
                {
                    maxHeight = noiseMap[x, y] * heightMultiplier;
                }
                else if (noiseMap[x, y] * heightMultiplier < minHeight)
                {
                    minHeight = noiseMap[x, y] * heightMultiplier;
                }

                //create new array for each tree location based on poisson list
                if (treeCounter < points.Count)
                {
                    treeLocations[counter] = new Vector3(points[treeCounter].x, noiseMap[x, y] * heightMultiplier, points[treeCounter].y);
                    treeCounter++;
                }

                normalizedHeight[counter] = (((treeLocations[counter].y) - minHeight) / (maxHeight - minHeight)); //Poisson Noise
                    

                treeLocations[counter] = new Vector3(x, noiseMap[x, y] * heightMultiplier, y); // GRID SPAWNING
                normalizedHeight[counter] = (((noiseMap[x, y] * heightMultiplier) - minHeight) / (maxHeight - minHeight));

                //if it's in the right height zone, spawn the trees
                if (normalizedHeight[counter] > gradKeys[forestStart].time && normalizedHeight[counter] < gradKeys[forestEnd].time)
                {
                    Instantiate(tree, treeLocations[counter], Quaternion.Euler(270, Random.Range(0f, 360f), 0));
                    counter ++;
                }
            }
                
        }

        // set trees to random size
        var spawnedTrees = GameObject.FindGameObjectsWithTag("tree");
        foreach (var newTree in spawnedTrees)
        {
            float size = Random.Range(7f, 15f);
            newTree.gameObject.transform.localScale = new Vector3(size, size, size);
            newTree.transform.parent = treeParent.transform;
        }
    }
    
}
