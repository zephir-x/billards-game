using System;
using System.Numerics;

namespace BilliardsGame.Interfaces
{
    public interface IInputProvider
    {
        Vector2 MouseWorldPosition { get; }
        bool IsLeftMouseDown { get; }
        bool WasLeftMouseReleased { get; }
    }
}
