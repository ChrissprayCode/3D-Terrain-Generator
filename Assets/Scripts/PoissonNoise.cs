using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonNoise : MonoBehaviour
{

    public static List<Vector2> GenPoissonNoise(int width, int height, float radius)
    {
        width -= 1;
        height -= 1;
        float[,] grid = new float[width, height];
        float cellWidth = radius / Mathf.Sqrt(2);
        List<Vector2> activePoints = new List<Vector2>();
        List<Vector2> poissonNoise = new List<Vector2>();

        const int k = 30; //amount of times to choose new location before giving up

 //initialise grid with all -1;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = -1;
            }
        }

 //generate a random starting point
        int randX = Random.Range(0, width);
        int randY = Random.Range(0, height);
        Vector2 StartPoint = new Vector2(randX, randY);
        //add it to the active points list and change it's grid val to 1
        grid[randX, randY] = 1;
        activePoints.Add(StartPoint);

 //whilst there is a point that is active
        while (activePoints.Count > 0)
        {
            int randIndex = Random.Range(0, activePoints.Count);//choose random index 
            Vector2 current = activePoints[randIndex]; //get its value
            int failedAttempts = 0;

            for (int j = 0; j <= k; j++) //generate up to k points
            {
                float angle = Random.Range(0, 360); //create random angle
                float radian = angle * Mathf.PI / 180; //convert angle to radians
                float distance = Random.Range(radius, radius * 2); //random distance between r and 2r

                float endingX = current.x + distance * Mathf.Cos(radian); //xposition to place
                float endingY = current.y + distance * Mathf.Sin(radian); //yposition to place

                if(endingX < 0)
                {
                    endingX = 0;
                }
                if (endingX > width)
                {
                    endingX = width;
                }
                if (endingY < 0)
                {
                    endingY = 0;
                }
                if (endingY > width)
                {
                    endingY = width;
                }

                Vector2 toPlace = new Vector2(endingX, endingY); //save previously generated points to a vector2

                //generate it's position in the grid
                int gridX = Mathf.FloorToInt(toPlace.x); 
                int gridY = Mathf.FloorToInt(toPlace.y);


//Checking a square of the 9 cells around it(8 cells AND the current cell point)
                bool canBePlaced = true;
                for (int x=-1; x <=1; x++)
                {
                    for(int y=-1; y <=1; y++)
                    {
                        int gridRow = gridX + x; //check the grid x pos
                        int gridCol = gridY + y; //check the grid y pos

                        //clamp the values (if point is being palced on the edge, cannot check outside the array)
                        if (gridRow > 127) 
                        {
                            gridRow = 127;
                        }
                        if (gridRow < 0)
                        {
                            gridRow = 0;
                        }
                        if (gridCol > 127)
                        {
                            gridCol = 127;
                        }
                        if (gridCol < 0)
                        {
                            gridCol = 0;
                        }

                        //if the grid point IS OCCUPIED
                        if (grid[gridRow,gridCol] == 1)
                        {
                            //Debug.Log("NO");
                            canBePlaced = false; 
                            failedAttempts++; //increment failure counter by 1
                        }
                    }
                }

                if (gridX > 127)
                {
                    gridX = 127;
                }
                if (gridX < 0)
                {
                    gridX = 0;
                }
                if (gridY > 127)
                {
                    gridY = 127;
                }
                if (gridY < 0)
                {
                    gridY = 0;
                }
                //Processing what to do with points based on if they can be placed or not
                if (canBePlaced) //if it can be placed
                {
                    grid[gridX, gridY] = 1;
                    activePoints.Add(toPlace); //add it to the active points list
                }
                if(failedAttempts == k) //if failedAttempts = max num of tries (every point couldn't be placed), remove the point 
                {
                    poissonNoise.Add(current);
                    activePoints.RemoveAt(randIndex); //remove it from the list
                    
                }
                
            }

        }
        
        return poissonNoise;
    }

}

