using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGeneration
{
    public int[] triangles;
    public Vector3[] vertexArr;
    public Vector2[] uvArr;
    public int vertexAmount;
    public int simplificationLevel;

    public Mesh GenMesh(float[,] noiseMap, float heightMultiplier, int simplificationLevel)
    {
        int width = noiseMap.GetLength(0); //get x length of array
        int height = noiseMap.GetLength(1); //get y length of array

        if (simplificationLevel == 1)
        {
            simplificationLevel = 1;
        }
        else if (simplificationLevel == 2)
        {
            simplificationLevel = 2;
        }
        else if (simplificationLevel == 3)
        {
            simplificationLevel = 4;
        }
        else if (simplificationLevel == 4)
        {
            simplificationLevel = 8;
        }

        vertexAmount = (width - 1) / simplificationLevel + 1;

        triangles = new int[(vertexAmount - 1) * (vertexAmount - 1) * 6]; //fill triangle array size
        vertexArr = new Vector3[vertexAmount * vertexAmount]; //fill vertex array size
        uvArr = new Vector2[vertexAmount * vertexAmount]; //fill uv array size

        MakeGrid(width, height, noiseMap, heightMultiplier, simplificationLevel); //Create the grid separetly to the triangles
        MakeTriangles(vertexAmount, vertexAmount); //Make triangles AFTER so I can mess with simplification


        //ADD EVERYTHING TO THE MESH OBJECT
        Mesh mesh = new Mesh();
        mesh.vertices = vertexArr; //assign it the variables I created
        mesh.triangles = triangles;
        mesh.uv = uvArr;
        mesh.RecalculateNormals(); //do the lighting again
        return mesh; //GIMMIE. DAT. MESH.

    }

    void MakeGrid(int width, int height, float[,] noiseMap, float heightMultiplier, int simplificationLevel)
    {
        int currentVert = 0; //keep track of what vertex I am on
        for (int x = 0; x < width; x+= simplificationLevel)
        {
            for (int y = 0; y < height; y+= simplificationLevel) //loop through the grid points
            {

                vertexArr[currentVert] = new Vector3(x, noiseMap[x, y] * heightMultiplier, y); //make a point on the grid at the height of the noiseMap
                //Debug.Log("Vertex Arr:" + vertexArr[currentVert].y);
                uvArr[currentVert] = new Vector2(x / (float)width, y / (float)height); //divided as the result needs to be between 0 and 1 due to how UV's work
                currentVert++;
            }
        }

    }


    void MakeTriangles(int width, int height)
    {
        int currentVert = 0; //keep track of what vertex I am on
        int currentTrianglePoint = 0; //keep track of which triangle point I am on

        for(int y=0; y<height-1; y++) //-1 so I don't make triangles outside of the array (Don't need last column)
        {
            for (int x=0; x<width-1; x++) //loop through the mesh points
            {
                //create triangles to make a square per vertex
                //Triangle 1
                triangles[currentTrianglePoint] = currentVert; //top left
                triangles[currentTrianglePoint+1] = currentVert + vertexAmount + 1; //bottom right
                triangles[currentTrianglePoint+2] = currentVert + vertexAmount; //bottom left
                //Triangle 2
                triangles[currentTrianglePoint+3] = currentVert + 1 + vertexAmount; //top left
                triangles[currentTrianglePoint+4] = currentVert; //top right
                triangles[currentTrianglePoint+5] = currentVert + 1; //bottom right
               
                currentTrianglePoint += 6; //move 6 points across since we used 6
                currentVert++; //move to the next vertex
            }

            currentVert++; //at the end of every row, move up one so there isn't a LONG triangle between rows
        } 
    }

    

}

