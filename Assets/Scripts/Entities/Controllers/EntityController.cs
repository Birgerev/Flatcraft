public class EntityController
{
    protected readonly LivingEntity instance;


    public EntityController(LivingEntity instance)
    {
        this.instance = instance;
    }

    public virtual void Tick()
    {
    }
}