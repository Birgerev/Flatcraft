using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Location
{
    public int x;
    public int y;
    public Dimension dimension;


    public Location(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.dimension = Dimension.Overworld;
    }

    public Location(int x, int y, Dimension dimension)
    {
        this.x = x;
        this.y = y;
        this.dimension = dimension;
    }

    public static Location locationByPosition(Vector3 pos, Dimension dimension)
    {
        return new Location(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), dimension);
    }
    
    public Vector2 getPosition()
    {
        return new Vector2(x, y);
    }

    public static Location operator +(Location a, Location b)
    {
        return new Location(a.x + b.x, a.y + b.y, a.dimension);
    }
    public static Location operator -(Location a, Location b)
    {
        return new Location(a.x - b.x, a.y - b.y, a.dimension);
    }
}
