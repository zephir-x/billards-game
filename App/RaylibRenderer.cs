using System;
using System.Numerics;
using BilliardsGame.Interfaces;
using Raylib_cs;

namespace BilliardsGame.App
{
    public class RaylibRenderer : IRenderer
    {
        private ISceneParameters? _sceneData;

        public bool IsPaused { get; set; }
        
        public Action? OnPlayClicked { get; set; }
        public Action? OnExitClicked { get; set; }
        
        public Action? OnContinueClicked { get; set; }
        public Action? OnRestartClicked { get; set; }

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
                // Draw Cushions
                foreach (var body in _sceneData.Bodies)
                {
                    if (body is ISegmentBody segmentBody)
                    {
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
            if (_sceneData != null && _sceneData.CurrentState != GameState.Menu && _sceneData.CurrentState != GameState.GameOver)
            {
                string turnText = _sceneData.CurrentTurnPlayer != null 
                    ? "Turn: " + _sceneData.CurrentTurnPlayer.Name
                    : "Turn: None";
                
                Raylib.DrawText(turnText, 10, 10, 20, Color.RayWhite);

                // If charging shot, draw the cue
                if (_sceneData.CurrentState == GameState.ChargingShot || _sceneData.CurrentState == GameState.PlayerTurn)
                {
                    var cueBall = GetCueBall();
                    if (cueBall != null && _sceneData.CueInfo != null)
                    {
                        Vector2 cueBallRenderedPos = (cueBall.PreviousPosition * (1f - interpolationAlpha)) + (cueBall.Position * interpolationAlpha);
                        
                        float offset = cueBall.Radius + 8f + (_sceneData.CueInfo.Power * 50f);
                        Vector2 cueDir = _sceneData.CueInfo.CueDirection;
                        if (cueDir != Vector2.Zero)
                        {
                            Vector2 cueStart = cueBallRenderedPos - (cueDir * offset);
                            Vector2 cueEnd = cueBallRenderedPos - (cueDir * (offset + 150f));

                            Raylib.DrawLineEx(cueStart, cueEnd, 6f, new Color(210, 180, 140, 255));
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
            
            if (_sceneData != null)
            {
                if (_sceneData.CurrentState == GameState.Menu)
                {
                    DrawMainMenu();
                }
                else if (_sceneData.CurrentState == GameState.GameOver)
                {
                    DrawGameOverMenu();
                }
                else if (IsPaused)
                {
                    DrawPauseMenu();
                }
            }

            Raylib.EndDrawing();
        }
        
        private void DrawMainMenu()
        {
            Raylib.DrawRectangle(0, 0, 800, 600, new Color(0, 0, 0, 150));
            Vector2 mousePos = Raylib.GetMousePosition();
            bool isClick = Raylib.IsMouseButtonPressed(MouseButton.Left);

            Rectangle playRect = new Rectangle(200, 250, 150, 80);
            Rectangle exitRect = new Rectangle(450, 250, 150, 80);

            bool playHover = Raylib.CheckCollisionPointRec(mousePos, playRect);
            bool exitHover = Raylib.CheckCollisionPointRec(mousePos, exitRect);

            Raylib.DrawRectangleRec(playRect, playHover ? Color.DarkGray : Color.Gray);
            Raylib.DrawRectangleRec(exitRect, exitHover ? Color.DarkGray : Color.Gray);

            int playTextWidth = Raylib.MeasureText("Play", 30);
            int exitTextWidth = Raylib.MeasureText("Exit", 30);
            
            Raylib.DrawText("Play", 200 + 75 - playTextWidth/2, 250 + 25, 30, Color.RayWhite);
            Raylib.DrawText("Exit", 450 + 75 - exitTextWidth/2, 250 + 25, 30, Color.RayWhite);
            
            Raylib.DrawText("MAIN MENU", 400 - Raylib.MeasureText("MAIN MENU", 40)/2, 120, 40, Color.RayWhite);

            if (playHover && isClick) OnPlayClicked?.Invoke();
            if (exitHover && isClick) OnExitClicked?.Invoke();
        }

        private void DrawPauseMenu()
        {
            Raylib.DrawRectangle(0, 0, 800, 600, new Color(0, 0, 0, 150));
            Vector2 mousePos = Raylib.GetMousePosition();
            bool isClick = Raylib.IsMouseButtonPressed(MouseButton.Left);

            Rectangle continueRect = new Rectangle(300, 200, 200, 60);
            Rectangle restartRect = new Rectangle(300, 280, 200, 60);
            Rectangle exitRect = new Rectangle(300, 360, 200, 60);

            bool continueHover = Raylib.CheckCollisionPointRec(mousePos, continueRect);
            bool restartHover = Raylib.CheckCollisionPointRec(mousePos, restartRect);
            bool exitHover = Raylib.CheckCollisionPointRec(mousePos, exitRect);

            Raylib.DrawRectangleRec(continueRect, continueHover ? Color.DarkGray : Color.Gray);
            Raylib.DrawRectangleRec(restartRect, restartHover ? Color.DarkGray : Color.Gray);
            Raylib.DrawRectangleRec(exitRect, exitHover ? Color.DarkGray : Color.Gray);

            int continueTextWidth = Raylib.MeasureText("Continue", 30);
            int restartTextWidth = Raylib.MeasureText("Restart", 30);
            int exitTextWidth = Raylib.MeasureText("Exit", 30);

            Raylib.DrawText("Continue", 300 + 100 - continueTextWidth/2, 200 + 15, 30, Color.RayWhite);
            Raylib.DrawText("Restart", 300 + 100 - restartTextWidth/2, 280 + 15, 30, Color.RayWhite);
            Raylib.DrawText("Exit", 300 + 100 - exitTextWidth/2, 360 + 15, 30, Color.RayWhite);
            
            Raylib.DrawText("PAUSED", 400 - Raylib.MeasureText("PAUSED", 40)/2, 100, 40, Color.RayWhite);

            if (continueHover && isClick) OnContinueClicked?.Invoke();
            if (restartHover && isClick) OnRestartClicked?.Invoke();
            if (exitHover && isClick) OnExitClicked?.Invoke();
        }
        
        private void DrawGameOverMenu()
        {
            Raylib.DrawRectangle(0, 0, 800, 600, new Color(0, 0, 0, 180));
            Raylib.DrawText("GAME OVER", 400 - Raylib.MeasureText("GAME OVER", 50)/2, 150, 50, Color.Red);
            
            if (_sceneData?.Winner != null)
            {
                string winnerText = "Winner: " + _sceneData.Winner.Name;
                Raylib.DrawText(winnerText, 400 - Raylib.MeasureText(winnerText, 40)/2, 230, 40, Color.Gold);
            }
            
            Vector2 mousePos = Raylib.GetMousePosition();
            bool isClick = Raylib.IsMouseButtonPressed(MouseButton.Left);

            Rectangle restartRect = new Rectangle(300, 320, 200, 60);
            Rectangle exitRect = new Rectangle(300, 400, 200, 60);

            bool restartHover = Raylib.CheckCollisionPointRec(mousePos, restartRect);
            bool exitHover = Raylib.CheckCollisionPointRec(mousePos, exitRect);

            Raylib.DrawRectangleRec(restartRect, restartHover ? Color.DarkGray : Color.Gray);
            Raylib.DrawRectangleRec(exitRect, exitHover ? Color.DarkGray : Color.Gray);
            
            int restartTextWidth = Raylib.MeasureText("Restart", 30);
            int exitTextWidth = Raylib.MeasureText("Exit", 30);

            Raylib.DrawText("Restart", 300 + 100 - restartTextWidth/2, 320 + 15, 30, Color.RayWhite);
            Raylib.DrawText("Exit", 300 + 100 - exitTextWidth/2, 400 + 15, 30, Color.RayWhite);
            
            if (restartHover && isClick) OnRestartClicked?.Invoke();
            if (exitHover && isClick) OnExitClicked?.Invoke();
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
