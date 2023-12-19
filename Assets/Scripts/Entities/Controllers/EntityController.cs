public class EntityController
{
    protected readonly LivingEntity instance;
    protected readonly EntityMovement movement;


    public EntityController(LivingEntity instance)
    {
        this.instance = instance;
        this.movement = instance.GetComponent<EntityMovement>();
    }

    public virtual void Tick()
    {
    }
}