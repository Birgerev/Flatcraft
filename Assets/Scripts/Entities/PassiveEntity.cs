using UnityEngine;

public class PassiveEntity : LivingEntity
{
    public override void Hit(float damage, Entity source)
    {
        base.Hit(damage, source);

        DamageNumberEffect((int)damage, Color.green);
    }
}