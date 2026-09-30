using System.Numerics;
using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;
using Raylib_cs;

namespace BilliardsGame.App
{
    public class RaylibInputProvider : IInputProvider
    {
        private bool _ignoreUntilReleased = false;

        private System.Func<Vector2, Vector2>? _screenToWorldMapper;

        public void SetCoordinateMapper(System.Func<Vector2, Vector2> screenToWorldMapper)
        {
            _screenToWorldMapper = screenToWorldMapper;
        }

        public Vector2 MouseWorldPosition 
        {
            get
            {
                var screenPos = Raylib.GetMousePosition();
                return _screenToWorldMapper != null ? _screenToWorldMapper(screenPos) : screenPos;
            }
        }
        
        public bool WasLeftMousePressed 
        {
            get 
            {
                if (_ignoreUntilReleased) return false;
                return Raylib.IsMouseButtonPressed(MouseButton.Left);
            }
        }
        
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