using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonDisc 
{
    public static List<Vector2> GenPoints(float radius, float width, float height, int rejectionCount)
    {
        float cellsize = radius / Mathf.Sqrt(2); //get cell size (c^2 = a^2 - b^2)

        int[,] grid = new int[Mathf.CeilToInt(width/cellsize), Mathf.CeilToInt(height/cellsize)]; //get how many cells are in the grid
        List<Vector2> confirmedPoints = new List<Vector2>(); //hold all generated points
        List<Vector2> activePoints = new List<Vector2>();

        activePoints.Add(new Vector2(width / 2, height / 2)); //add the first spawnpoint in the middle (change to random later)

        while(activePoints.Count > 0)
        {
            int randIndex = Random.Range(0, activePoints.Count);//choose random index 
            Vector2 currentPoint = activePoints[randIndex]; //get its value
            bool newPointPlaced = false;

            //try x amount of times to place a new point 
            for (int i = 0; i < rejectionCount; i++)
            {
                float radianAngle = Random.value * Mathf.PI * 2; //Create a radian angle
                Vector2 dir = new Vector2(Mathf.Cos(radianAngle), Mathf.Sin(radianAngle)); //Convert radian to directiuon
                float distance = Random.Range(radius, radius * 2); //random distance between r and 2r

                Vector2 newPoint = currentPoint + dir * distance;

                if (CanBePlaced(newPoint, confirmedPoints, grid, cellsize, width, height, radius)) //if the point CAN be placed
                {
                    confirmedPoints.Add(newPoint); //add it to the confirmed point list
                    activePoints.Add(newPoint); //also add it to the active point list
                    newPointPlaced = true;
                    //get points location on grid and add set it to point index (so it's not 0)
                    grid[(int)(newPoint.x / cellsize), (int)(newPoint.y / cellsize)] = confirmedPoints.Count;
                    break;
                }
            }

            //if the new point could NOT be placed
            if (!newPointPlaced)
            {
                activePoints.RemoveAt(randIndex); //remove it from the active point list
            }
        }

        return confirmedPoints; //return the noise
    }

    static bool CanBePlaced(Vector2 newPoint, List<Vector2> confirmedPoints, int[,] grid, float cellSize ,float width, float height, float radius)
    {
        //if it is within our bounds
        if(newPoint.x > 0 && newPoint.x < width && newPoint.y > 0 && newPoint.y < height)
        {
            //get cell position of new point
            int cellX = (int)(newPoint.x / cellSize);
            int cellY = (int)(newPoint.y / cellSize);

            //set search parameters
            int startX = Mathf.Max(0, cellX - 2); //start at which is larger, 0, or 2 cells to the left
            int endX = Mathf.Min(grid.GetLength(0)-1,  cellX + 2); //end at which is smaller, the num of cells on x axis or 2 cells to right 
            int startY = Mathf.Max(0, cellY - 2); //start at which is larger, 0, or 2 cells to the top
            int endY = Mathf.Min(grid.GetLength(1) - 1, cellX + 2); //end at which is smaller, the num of cells on Y axis or 2 cells to bottom 


            //check the squares around it (2 either side)
            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    int pointIndex = grid[x, y] - 1; //get index of the point we are currently looping on
                    if(pointIndex != -1) //if there is nothing in the cell
                    {
                        float dst = (newPoint - confirmedPoints[pointIndex]).magnitude; //find dist between new point and old point at that positioni
                        if(dst < radius)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        //cannot be placed
        return false;
    }
}
