namespace BilliardsGame.Interfaces
{
    public interface IRenderer
    {
        void Initialize(ISceneParameters sceneData);
        void DrawFrame(float interpolationAlpha);
    }
}
