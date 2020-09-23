using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedGenerator : MonoBehaviour
{
    private static Dictionary<Location, int> cachedRandomSeeds = new Dictionary<Location, int>();

    public static int SeedByLocation(Location loc)
    {
        int seed = 0;

        if (cachedRandomSeeds.ContainsKey(loc))
            seed = cachedRandomSeeds[loc];
        else
            seed = GenerateSeedByLocation(loc);
        
        return seed;
    }

    private static int GenerateSeedByLocation(Location loc)
    {
        int seed = new System.Random((WorldManager.world.seed + ", " + loc.x + ", " + loc.y + ", " + loc.dimension).GetHashCode()).Next(int.MinValue, int.MaxValue);
        cachedRandomSeeds[loc] = seed;
        
        return seed;
    }

    public static void Reset()
    {
        cachedRandomSeeds.Clear();
    }
}
