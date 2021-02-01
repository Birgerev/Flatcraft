using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class SeedGenerator : MonoBehaviour
{
    private static readonly Dictionary<Location, int> cachedRandomSeeds = new Dictionary<Location, int>();

    public static int SeedByLocation(Location loc)
    {
        var seed = 0;

        if (cachedRandomSeeds.ContainsKey(loc))
            seed = cachedRandomSeeds[loc];
        else
            seed = GenerateSeedByLocation(loc);

        return seed;
    }

    private static int GenerateSeedByLocation(Location loc)
    {
        var seed = new Random(
                (WorldManager.world.seed + ", " + loc.x + ", " + loc.y + ", " + loc.dimension).GetHashCode())
            .Next(int.MinValue, int.MaxValue);
        cachedRandomSeeds.Add(loc, seed);

        return seed;
    }

    public static void Reset()
    {
        cachedRandomSeeds.Clear();
    }
}