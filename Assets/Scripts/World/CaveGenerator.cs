using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    private const int perlinSnakeAmount = 6;
    private const int perlinSnakeMinLength = 48;
    private const int perlinSnakeMaxLength = 92;
    private const int perlinSnakeMaxAngleChange = 40;
    private const float perlinSnakeVericalAngleStopChance = 0.2f;
    
    private const int maxCaveHeight = 50;
    
    public static void GenerateCavesForRegion(ChunkPosition chunkPos)
    {
        Dimension dim = chunkPos.dimension;
        int region; //region position

        if (chunkPos.chunkX >= 0)
            region = (int) (chunkPos.chunkX / (float) Chunk.AmountOfChunksInRegion);
        else
            region = Mathf.CeilToInt(((float) chunkPos.chunkX + 1) / Chunk.AmountOfChunksInRegion) - 1;

        if (WorldManager.instance.caveGeneratedRegions.Contains(region))
            return;
        
        
        int minX = region * Chunk.AmountOfChunksInRegion * Chunk.Width; //first block of the region
        int maxX = minX + (Chunk.AmountOfChunksInRegion * Chunk.Width); //last block of the region

        //Random, with a seed unique to the chunk position and the world seed
        System.Random r = new System.Random(chunkPos.GetHashCode() + WorldManager.world.seed);

        for (int snakeAmount = 0; snakeAmount < perlinSnakeAmount; snakeAmount++)
        {
            Location snakeSegmentLocation = new Location(r.Next(minX, maxX), r.Next(0, maxCaveHeight), dim);
            float angle = r.Next(0, 360);
            int snakeSize = 2;
            int snakeLength = r.Next(perlinSnakeMinLength, perlinSnakeMaxLength);
            
            for (int currentLength = 0; currentLength < snakeLength; currentLength++)
            {
                //Hollow out
                for (int x = -snakeSize; x < snakeSize; x++)
                {
                    for (int y = -snakeSize; y < snakeSize; y++)
                    {
                        WorldManager.instance.caveHollowBlocks.Add(snakeSegmentLocation + new Location(x, y, dim));
                    }
                }
                
                //Decide new location
                if (((angle % 360 > 340 || angle % 360 < 20) || (angle % 360 > 160 && angle % 360 < 200)) &&
                    r.NextDouble() < perlinSnakeVericalAngleStopChance)
                    angle = 90 * (r.Next(0, 1 + 1) == 0 ? -1 : 1);
                else
                    angle += r.Next(-perlinSnakeMaxAngleChange, perlinSnakeMaxAngleChange+1);
                
                float angleRadians = angle * Mathf.Deg2Rad;
                Vector2 deltaLocation = new Vector2(Mathf.Sin(angleRadians), Mathf.Cos(angleRadians)) * 2;
                snakeSegmentLocation += Location.LocationByPosition(deltaLocation);

                //Decide new size
            }
        }
        
        WorldManager.instance.caveGeneratedRegions.Add(region);
    }
}
