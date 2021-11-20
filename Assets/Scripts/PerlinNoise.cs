using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    public static float[,] genNoise(int width, int height, int seed, int octaves, float scale, float persistence, float lacunarity, float curve)
    {
        float[,] noiseMap = new float[width, height]; //create empty noisemap array

        Random.InitState(seed); //plant seed into prng
        int randX = Random.Range(-5000, 5000); //generate random X coord
        int randY = Random.Range(-5000, 5000); //generate random Y coord
        
        float maxHeight = 0;
        float minHeight = 0;

        for (int x=0; x < width; x++)
        {
                for (int y=0; y < width; y++)
                {
                    float amp = 1;
                    float freq = 1;
                    float noise = 0;

                    for (int i=0; i<octaves; i++)
                    {
                        float perlinX = x / scale * freq + randX;
                        float perlinY = y / scale * freq + randY;

                        noise += Mathf.PerlinNoise(perlinX, perlinY) * amp;
                        //decrease the amp, increase the freq
                        amp *= persistence;
                        freq *= lacunarity;
                    }

                    //smooth out the land
                    noise = Mathf.Pow(noise, curve);

                    //catch the lowest and highest values for normalising later
                    if (noise > maxHeight)
                    {
                        maxHeight = noise;
                    }
                    else if (noise < minHeight)
                    {
                        minHeight = noise;
                    }
                        noiseMap[x, y] = noise;
                }
        }

        //loop through every point
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minHeight, maxHeight, noiseMap[x, y]); //normalise the number for colouring later
            }
        }
        return noiseMap;
    }






}
