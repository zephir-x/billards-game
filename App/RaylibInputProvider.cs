using System.Numerics;
using BilliardsGame.Interfaces;
using Raylib_cs;

namespace BilliardsGame.App
{
    public class RaylibInputProvider : IInputProvider
    {
        public Vector2 MouseWorldPosition => Raylib.GetMousePosition();
        public bool IsLeftMouseDown => Raylib.IsMouseButtonDown(MouseButton.Left);
        public bool WasLeftMouseReleased => Raylib.IsMouseButtonReleased(MouseButton.Left);
    }
}
