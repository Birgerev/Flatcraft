using UnityEngine;

public class Monster : LivingEntity
{
    protected virtual bool burnUnderSun { get; } = false;

    public override void Tick()
    {
        base.Tick();

        if (burnUnderSun)
            TryBurnUnderSun();
        TryDespawn();
    }
    
    private void TryDespawn()
    {
        if (Time.time % 10f - Time.deltaTime > 0)
            return;
        
        foreach (Player player in Player.players)
        {
            Location playerLoc = player.Location;

            if (Location.dimension != playerLoc.dimension)
                continue;

            float distanceFromPlayer = Vector2.Distance(Location.GetPosition(), playerLoc.GetPosition());
            if (distanceFromPlayer < 60)
                return;
        }
        
        Unload();
    }

    public override void Save()
    {
        
    }


    private void TryBurnUnderSun()
    {
        if (WorldManager.GetTimeOfDay() != TimeOfDay.Day || Time.time % 10f - Time.deltaTime > 0)
            return;
                
        Block topmostBlock = Chunk.GetTopmostBlock(Location.x, Location.dimension, true);

        if (topmostBlock == null || Location.y < topmostBlock.location.y)
            return;
        
        fireTime = 10;
    }

    public override void Hit(float damage, Entity source)
    {
        base.Hit(damage, source);

        Particle.Spawn_Number(transform.position + new Vector3(1, 2), (int) damage, Color.red);
    }
}