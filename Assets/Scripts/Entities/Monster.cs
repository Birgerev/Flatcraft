using UnityEngine;

public class Monster : LivingEntity
{
    protected virtual bool burnUnderSun { get; } = false;

    public override void Tick()
    {
        base.Tick();

        if (burnUnderSun && WorldManager.GetTimeOfDay() == TimeOfDay.Day && Time.time % 10f - Time.deltaTime <= 0)
        {
            Block topmostBlock = Chunk.GetTopmostBlock(Location.x, Location.dimension, true);
            if (Location.y > topmostBlock.location.y)
            {
                fireTime = 10;
            } 
        }
    }

    public override void Hit(float damage, Entity source)
    {
        base.Hit(damage, source);

        Particle.Spawn_Number(transform.position + new Vector3(1, 2), (int) damage, Color.red);
    }
}