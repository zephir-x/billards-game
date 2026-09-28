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
            Raylib.ClearBackground(new Color(20, 105, 50, 255)); // Classic billiards green 

            if (_sceneData?.Bodies != null)
            {
                // Draw Cushions first
                foreach (var body in _sceneData.Bodies)
                {
                    if (body is ISegmentBody segmentBody)
                    {
                        // Visually extend the line by half the thickness (7.5f) on both ends so thick corners overlap perfectly
                        Vector2 direction = segmentBody.EndPoint - segmentBody.StartPoint;
                        if (direction.LengthSquared() > 0.0001f)
                        {
                            direction = Vector2.Normalize(direction);
                            float extension = 7.5f;
                            Vector2 drawStart = segmentBody.StartPoint - direction * extension;
                            Vector2 drawEnd = segmentBody.EndPoint + direction * extension;
                            
                            Raylib.DrawLineEx(drawStart, drawEnd, 15f, new Color(139, 69, 19, 255)); // Saddle Brown
                        }
                    }
                }

                
                // Draw Pockets
                if (_sceneData.Pockets != null)
                {
                    foreach (var pocket in _sceneData.Pockets)
                    {
                        Raylib.DrawCircleV(pocket.Position, pocket.Radius, Color.Black);
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
                string turnText = _sceneData.CurrentTurnPlayer != null 
                    ? "Turn: " + _sceneData.CurrentTurnPlayer.Name
                    : "Turn: None";
                
                Raylib.DrawText(turnText, 10, 10, 20, Color.RayWhite);

                // If charging shot, draw the cue
                if (_sceneData.CurrentState.ToString() == "ChargingShot" || _sceneData.CurrentState.ToString() == "PlayerTurn")
                {
                    var cueBall = GetCueBall();
                    if (cueBall != null && _sceneData.CueInfo != null)
                    {
                        Vector2 cueBallRenderedPos = (cueBall.PreviousPosition * (1f - interpolationAlpha)) + (cueBall.Position * interpolationAlpha);
                        
                        // Drawn cue stick (opposite to CueDirection) 
                        float offset = cueBall.Radius + 8f + (_sceneData.CueInfo.Power * 50f);
                        Vector2 cueDir = _sceneData.CueInfo.CueDirection;
                        if (cueDir != Vector2.Zero)
                        {
                            Vector2 cueStart = cueBallRenderedPos - (cueDir * offset);
                            Vector2 cueEnd = cueBallRenderedPos - (cueDir * (offset + 150f));

                            Raylib.DrawLineEx(cueStart, cueEnd, 6f, new Color(210, 180, 140, 255)); // Light brown stick
                        }
                    }
                }

                // PowerBar UI
                if (_sceneData.CueInfo != null)
                {
                    int barX = 250;
                    int barY = 30;
                    int barWidth = 300;
                    int barHeight = 20;

                    Raylib.DrawRectangle(barX, barY, barWidth, barHeight, Color.DarkGray);
                    
                    int fillWidth = (int)(barWidth * _sceneData.CueInfo.Power);
                    Color fillColor = _sceneData.CueInfo.IsOverheated ? Color.Orange : new Color(100, 150, 200, 255);
                    
                    if (fillWidth > 0)
                    {
                        Raylib.DrawRectangle(barX, barY, fillWidth, barHeight, fillColor);
                    }
                    Raylib.DrawRectangleLines(barX, barY, barWidth, barHeight, Color.LightGray);
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