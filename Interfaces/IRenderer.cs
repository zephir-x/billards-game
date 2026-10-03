namespace BilliardsGame.Interfaces
{
    /// <summary>
    /// Governs visual output sequence generations, consuming the logic domain transparently and flushing pixel data.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>
        /// Links the domain payload dependencies allowing stateless read cycles per frame.
        /// </summary>
        /// <param name="sceneData">Read-only proxy wrapping entity aggregates and rules.</param>
        void Initialize(ISceneParameters sceneData);

        /// <summary>
        /// Forces buffer flushes onto visual displays generating 2D assets using the physics state.
        /// </summary>
        /// <param name="interpolationAlpha">Floating accumulation determining visual blending percentage between discrete physics steps [0.0..1.0].</param>
        void DrawFrame(float interpolationAlpha);
    }
}
