namespace VillageBuilder.Engine.Entities.Wildlife
{
    /// <summary>
    /// Current behavior state of a wildlife entity
    /// </summary>
    public enum WildlifeBehavior
    {
        Idle,           // Standing still, doing nothing
        Grazing,        // Eating vegetation (prey animals)
        Wandering,      // Moving randomly within territory
        Fleeing,        // Running away from danger
        Hunting,        // Chasing prey (predators)
        Eating,         // Consuming killed prey
        Resting,        // Recovering energy
        Breeding,       // Mating behavior
        Dead            // Animal has died
    }
}
