using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise 
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i<octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;

            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Clamp scale value to avoid division by 0
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        // to keep track of min and max values
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        // make noiseScale zoom into middle of map
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;
        
        // Loop through noiseMap 
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                float amplitude = 1;
                float frequency = 1; //the higher the freq, the further apart the sample points --> the height value will change more rapidly
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    // sample coordinates
                    float sampleX = (x-halfWidth)/scale * frequency + octaveOffsets[i].x; // divide by scale to get non-integer values, and different values
                    float sampleY = (y-halfHeight)/scale * frequency + octaveOffsets[i].y;

                    // create perlinValue
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2-1; // *2-1 is to get neg values as well
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance; //decreases each octave, persistance is between 1 and 0
                    frequency *= lacunarity; // increases each octave, lacunarity is greater than 1

                }

                // keep track of min and max noiseHeight
                if(noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                //apply noiseHeight to noiseMap
                noiseMap[x, y] = noiseHeight;
            }
        }

        // Normalize noiseMap
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;

    }

}
