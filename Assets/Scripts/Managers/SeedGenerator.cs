using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class SeedGenerator : MonoBehaviour
{
    public static int SeedByLocation(Location loc)
    {
        return new Random(
                (WorldManager.world.seed.GetHashCode() + ", " + loc.x + ", " + loc.y + ", " + loc.dimension).GetHashCode())
            .Next(int.MinValue, int.MaxValue);
    }
}