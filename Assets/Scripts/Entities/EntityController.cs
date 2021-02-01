public class EntityController
{
    public LivingEntity instance;

    public EntityController(LivingEntity instance)
    {
        this.instance = instance;
    }

    public virtual void Tick()
    {
    }
}