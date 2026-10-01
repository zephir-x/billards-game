using System;
using System.Numerics;

namespace BilliardsGame.Interfaces
{
    public interface IInputProvider
    {
        void SetCoordinateMapper(System.Func<System.Numerics.Vector2, System.Numerics.Vector2> screenToWorldMapper);
        Vector2 MouseWorldPosition { get; }
        bool WasLeftMousePressed { get; }
        bool IsLeftMouseDown { get; }
        bool WasLeftMouseReleased { get; }
    }
}