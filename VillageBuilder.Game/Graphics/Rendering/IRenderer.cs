namespace VillageBuilder.Game.Graphics.Rendering
{
    /// <summary>
    /// Generic renderer interface for rendering specific entity types.
    /// Enables Strategy Pattern for flexible rendering architecture.
    /// </summary>
    /// <typeparam name="T">The type of entity this renderer handles</typeparam>
    public interface IRenderer<T>
    {
        /// <summary>
        /// Render a single entity with the given context
        /// </summary>
        /// <param name="entity">The entity to render</param>
        /// <param name="context">Rendering context with camera, time, etc.</param>
        void Render(T entity, RenderContext context);
        
        /// <summary>
        /// Check if this entity should be rendered (visibility, culling, etc.)
        /// </summary>
        /// <param name="entity">The entity to check</param>
        /// <param name="context">Rendering context</param>
        /// <returns>True if entity should be rendered</returns>
        bool ShouldRender(T entity, RenderContext context);
    }
    
    /// <summary>
    /// Batch renderer interface for rendering collections of entities efficiently
    /// </summary>
    /// <typeparam name="T">The type of entities this renderer handles</typeparam>
    public interface IBatchRenderer<T>
    {
        /// <summary>
        /// Render a collection of entities in a single pass
        /// </summary>
        /// <param name="entities">Collection of entities to render</param>
        /// <param name="context">Rendering context</param>
        void RenderBatch(IEnumerable<T> entities, RenderContext context);
    }
}
