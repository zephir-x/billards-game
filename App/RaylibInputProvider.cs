using System.Numerics;
using BilliardsGame.Interfaces;
using Raylib_cs;

namespace BilliardsGame.App
{
    public class RaylibInputProvider : IInputProvider
    {
        private bool _ignoreUntilReleased = false;

        public Vector2 MouseWorldPosition => Raylib.GetMousePosition();
        
        public bool IsLeftMouseDown 
        {
            get 
            {
                if (_ignoreUntilReleased) return false;
                return Raylib.IsMouseButtonDown(MouseButton.Left);
            }
        }
        
        public bool WasLeftMouseReleased 
        {
            get 
            {
                if (_ignoreUntilReleased) return false;
                return Raylib.IsMouseButtonReleased(MouseButton.Left);
            }
        }

        public void ConsumeClickForUI()
        {
            _ignoreUntilReleased = true;
        }

        public void Update()
        {
            if (_ignoreUntilReleased && !Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                _ignoreUntilReleased = false;
            }
        }
    }
}
