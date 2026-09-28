using System.Numerics;
using BilliardsGame.Interfaces;
using Raylib_cs;

namespace BilliardsGame.App
{
    public class RaylibRenderer : IRenderer
    {
        private ISceneParameters? _sceneData;

        public void Initialize(ISceneParameters sceneData)
        {
            _sceneData = sceneData;
        }

        public void DrawFrame(float interpolationAlpha)
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(34, 139, 34, 255)); // Forest Green table 

            if (_sceneData?.Bodies != null)
            {
                // Draw Cushions first
                foreach (var body in _sceneData.Bodies)
                {
                    if (body is ISegmentBody segmentBody)
                    {
                        Raylib.DrawLineEx(segmentBody.StartPoint, segmentBody.EndPoint, 10f, new Color(139, 69, 19, 255)); // Saddle Brown
                    }
                }

                // Draw Balls
                foreach (var body in _sceneData.Bodies)
                {
                    if (body is ICircleBody circleBody)
                    {
                        Vector2 renderedPos = (body.PreviousPosition * (1f - interpolationAlpha)) + (body.Position * interpolationAlpha);
                        Color color = body.Id == 0 ? Color.White : Color.Black;
                        Raylib.DrawCircleV(renderedPos, circleBody.Radius, color);
                    }
                }
            }

            // Draw Statistics
            if (_sceneData != null)
            {
                string stateText = "State: " + _sceneData.CurrentState.ToString();
                string turnText = _sceneData.CurrentTurnPlayer != null 
                    ? "Turn: " + _sceneData.CurrentTurnPlayer.Name
                    : "Turn: None";
                
                Raylib.DrawText(stateText, 10, 10, 20, Color.RayWhite);
                Raylib.DrawText(turnText, 10, 35, 20, Color.RayWhite);

                // If charging shot, draw the cue
                if (_sceneData.CurrentState == GameState.ChargingShot || _sceneData.CurrentState == GameState.PlayerTurn)
                {
                    var cueBall = GetCueBall();
                    if (cueBall != null && _sceneData.CueInfo != null)
                    {
                        Vector2 cueBallRenderedPos = (cueBall.PreviousPosition * (1f - interpolationAlpha)) + (cueBall.Position * interpolationAlpha);
                        
                        // Drawn cue stick (opposite to CueDirection) 
                        // Offset from white ball: Radius + (CueInfo.Power * 50f)
                        float offset = cueBall.Radius + (_sceneData.CueInfo.Power * 50f);
                        Vector2 cueDir = _sceneData.CueInfo.CueDirection;
                        if (cueDir != Vector2.Zero)
                        {
                            Vector2 cueStart = cueBallRenderedPos - (cueDir * offset);
                            Vector2 cueEnd = cueBallRenderedPos - (cueDir * (offset + 150f));

                            Raylib.DrawLineEx(cueStart, cueEnd, 6f, new Color(205, 133, 63, 255)); // Peru brown stick
                        }
                    }
                }
            }

            Raylib.EndDrawing();
        }

        private ICircleBody? GetCueBall()
        {
            if (_sceneData?.Bodies == null) return null;
            foreach (var body in _sceneData.Bodies)
            {
                if (body.Id == 0 && body is ICircleBody circle)
                    return circle;
            }
            return null;
        }
    }
}
