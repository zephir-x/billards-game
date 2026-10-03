using System;
using System.Numerics;
using BilliardsGame.Interfaces;
using Raylib_cs;

namespace BilliardsGame.App
{
    /// <summary>
    /// Serves as the primary hardware layer abstraction channeling C-Language Mouse integration bindings into managed C# vectors.
    /// Safely isolates input poll loops from the abstract game logic via the <see cref=""IInputProvider""/> interface.
    /// </summary>
    public class RaylibInputProvider : IInputProvider
    {
        // Internal flagging system forcing the hardware domain to selectively blur active polls when UIs demand focus.
        private bool _ignoreUntilReleased = false;

        private Func<Vector2, Vector2>? _screenToWorldMapper;

        /// <inheritdoc />
        public void SetCoordinateMapper(Func<Vector2, Vector2> screenToWorldMapper)
        {
            _screenToWorldMapper = screenToWorldMapper;
        }

        /// <inheritdoc />
        public Vector2 MouseWorldPosition 
        {
            get
            {
                // Capture raw frame-pixels scaling dynamically through the projection matrix callback.
                var screenPos = Raylib.GetMousePosition();
                return _screenToWorldMapper != null ? _screenToWorldMapper(screenPos) : screenPos;
            }
        }
        
        /// <inheritdoc />
        public bool WasLeftMousePressed 
        {
            get 
            {
                if (_ignoreUntilReleased) return false;
                return Raylib.IsMouseButtonPressed(MouseButton.Left);
            }
        }
        
        /// <inheritdoc />
        public bool IsLeftMouseDown 
        {
            get 
            {
                if (_ignoreUntilReleased) return false;
                return Raylib.IsMouseButtonDown(MouseButton.Left);
            }
        }
        
        /// <inheritdoc />
        public bool WasLeftMouseReleased 
        {
            get 
            {
                if (_ignoreUntilReleased) return false;
                return Raylib.IsMouseButtonReleased(MouseButton.Left);
            }
        }

        /// <summary>
        /// Emits a strict masking overlay preventing the physical play field from tracking shots while manipulating the UI overlay menus.
        /// </summary>
        public void ConsumeClickForUI()
        {
            _ignoreUntilReleased = true;
        }

        /// <summary>
        /// Explicitly triggers verification cycles removing UI masking when the physical button hardware registers a full disconnection.
        /// </summary>
        public void Update()
        {
            if (_ignoreUntilReleased && !Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                _ignoreUntilReleased = false;
            }
        }
    }
}
