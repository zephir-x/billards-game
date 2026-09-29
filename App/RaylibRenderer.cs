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
            
            // Draw Environment Background
            Raylib.ClearBackground(new Color(5, 5, 20, 255));
            Raylib.DrawRectangleGradientH(0, 0, 800, 600, new Color(5, 5, 25, 255), new Color(40, 5, 5, 255));

            // 1. Outer Dark Wood Base
            Rectangle outerWood = new Rectangle(40, 40, 720, 520);
            Raylib.DrawRectangleRounded(outerWood, 0.15f, 30, new Color(60, 30, 10, 255));

            // 2. Inner Green Cloth
            Rectangle cloth = new Rectangle(80, 80, 640, 440);
            Raylib.DrawRectangleRounded(cloth, 0.05f, 10, new Color(20, 105, 50, 255));

            // 3. Custom polygons for table rails (V-cuts and 45-degree corner pockets)
            Color railColor = new Color(139, 69, 19, 255);
            Color railBorder = new Color(60, 30, 10, 255);
            
            Action<Vector2, Vector2, Vector2, Vector2, Color> DrawQuad = (v1, v2, v3, v4, col) => 
            {
                // Draw all permutations to bypass face culling issues
                Raylib.DrawTriangle(v1, v2, v3, col);
                Raylib.DrawTriangle(v1, v3, v2, col);
                Raylib.DrawTriangle(v1, v3, v4, col);
                Raylib.DrawTriangle(v1, v4, v3, col);
            };
            
            Action<Vector2, Vector2, Vector2, Vector2, float, Color> DrawQuadLines = (v1, v2, v3, v4, t, col) => 
            {
                // Draw outline and circular caps for smooth joints
                Raylib.DrawLineEx(v1, v2, t, col);
                Raylib.DrawLineEx(v2, v3, t, col);
                Raylib.DrawLineEx(v3, v4, t, col);
                Raylib.DrawLineEx(v4, v1, t, col);
                
                Raylib.DrawCircleV(v1, t/2, col);
                Raylib.DrawCircleV(v2, t/2, col);
                Raylib.DrawCircleV(v3, t/2, col);
                Raylib.DrawCircleV(v4, t/2, col);
            };

            // Top Left Rail
            var tl1 = new Vector2(110, 80);
            var tl2 = new Vector2(130, 100);
            var tl3 = new Vector2(370, 100);
            var tl4 = new Vector2(380, 80);
            DrawQuad(tl1, tl2, tl3, tl4, railColor);
            DrawQuadLines(tl1, tl2, tl3, tl4, 3f, railBorder);

            // Top Right Rail
            var tr1 = new Vector2(420, 80);
            var tr2 = new Vector2(430, 100);
            var tr3 = new Vector2(670, 100);
            var tr4 = new Vector2(690, 80);
            DrawQuad(tr1, tr2, tr3, tr4, railColor);
            DrawQuadLines(tr1, tr2, tr3, tr4, 3f, railBorder);

            // Bottom Left Rail
            var bl1 = new Vector2(130, 500); 
            var bl2 = new Vector2(110, 520);
            var bl3 = new Vector2(380, 520);
            var bl4 = new Vector2(370, 500);
            DrawQuad(bl1, bl2, bl3, bl4, railColor);
            DrawQuadLines(bl1, bl2, bl3, bl4, 3f, railBorder);

            // Bottom Right Rail
            var br1 = new Vector2(430, 500);
            var br2 = new Vector2(420, 520);
            var br3 = new Vector2(690, 520);
            var br4 = new Vector2(670, 500);
            DrawQuad(br1, br2, br3, br4, railColor);
            DrawQuadLines(br1, br2, br3, br4, 3f, railBorder);

            // Left Rail
            var l1 = new Vector2(80, 110);
            var l2 = new Vector2(80, 490);
            var l3 = new Vector2(100, 470);
            var l4 = new Vector2(100, 130);
            DrawQuad(l1, l2, l3, l4, railColor);
            DrawQuadLines(l1, l2, l3, l4, 3f, railBorder);

            // Right Rail
            var r1 = new Vector2(700, 130);
            var r2 = new Vector2(700, 470);
            var r3 = new Vector2(720, 490);
            var r4 = new Vector2(720, 110);
            DrawQuad(r1, r2, r3, r4, railColor);
            DrawQuadLines(r1, r2, r3, r4, 3f, railBorder);

            // 4. Pocket Holes (Drawn on top to stencil out perfect wood cutouts)
                if (_sceneData?.Pockets != null)
                {
                    foreach (var pocket in _sceneData.Pockets)
                    {
                        float visualRadius = 23f; // Cover jaws cutout seamlessly
                        Raylib.DrawCircleV(pocket.Position, visualRadius, Color.Black);
                        Raylib.DrawCircleLines((int)pocket.Position.X, (int)pocket.Position.Y, visualRadius * 0.9f, new Color(40, 40, 40, 200));
                        Raylib.DrawCircleLines((int)pocket.Position.X, (int)pocket.Position.Y, visualRadius * 0.6f, new Color(20, 20, 20, 100));
                    }
                }


                if (_sceneData?.Bodies != null)
            {
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

            

                // If charging shot, draw the cue
                if (_sceneData != null && (_sceneData.CurrentState == GameState.ChargingShot || _sceneData.CurrentState == GameState.PlayerTurn))
                {
                    var cueBall = GetCueBall();
                    if (cueBall != null && _sceneData.CueInfo != null)
                    {
                        Vector2 cueBallRenderedPos = (cueBall.PreviousPosition * (1f - interpolationAlpha)) + (cueBall.Position * interpolationAlpha);
                        
                        float offset = cueBall.Radius + 8f + (_sceneData.CueInfo.Power * 50f);
                        Vector2 cueDir = _sceneData.CueInfo.CueDirection;
                        if (cueDir != Vector2.Zero)
                        {
                            // Ghost Guide Line (White dotted)
                            for (float d = cueBall.Radius + 12f; d < 120f; d += 15f)
                            {
                                Vector2 ghostPoint = cueBallRenderedPos + (cueDir * d);
                                Raylib.DrawCircleV(ghostPoint, 2f, new Color(255, 255, 255, 120));
                            }

                            float cueLength = 300f; // Longer cue
                            Vector2 cueStart = cueBallRenderedPos - (cueDir * offset);
                            Vector2 cueEnd = cueBallRenderedPos - (cueDir * (offset + cueLength));

                            // 1. Dark thick outline/shadow
                            Raylib.DrawLineEx(cueStart, cueEnd, 8f, new Color(30, 15, 5, 255));

                            // 2. Base wood color
                            Raylib.DrawLineEx(cueStart, cueEnd, 6f, new Color(222, 184, 135, 255));

                            // 3. Subtle decorative line down the center
                            Raylib.DrawLineEx(cueStart, cueEnd, 2f, new Color(110, 55, 15, 255));

                            // 4. Distinct cue butt (handle) at the back
                            float buttLength = 100f;
                            Vector2 cueHandleStart = cueBallRenderedPos - (cueDir * (offset + cueLength - buttLength));
                            Raylib.DrawLineEx(cueHandleStart, cueEnd, 6f, new Color(25, 25, 25, 255)); // Black grip
                            Raylib.DrawLineEx(cueHandleStart, cueEnd, 2f, new Color(50, 50, 50, 255)); // Grip highlight

                            // 5. Cue tip (blue chalk)
                            Vector2 cueTipEnd = cueStart + (cueDir * 4f);
                            Raylib.DrawLineEx(cueStart, cueTipEnd, 6f, new Color(100, 150, 255, 255));
                        }
                    }
                }
            // Draw Statistics
            if (_sceneData != null && _sceneData.CurrentState != GameState.Menu && _sceneData.CurrentState != GameState.GameOver)
            {
                string turnText = _sceneData.CurrentTurnPlayer != null 
                    ? "Turn: " + _sceneData.CurrentTurnPlayer.Name
                    : "Turn: None";
                
                int turnTextWidth = Raylib.MeasureText(turnText, 24);
                Raylib.DrawText(turnText, 400 - turnTextWidth / 2, 10, 24, Color.RayWhite);

                // Player 1 Box (Top Left)
                Raylib.DrawRectangle(10, 10, 160, 60, new Color(30, 30, 30, 200));
                if (_sceneData.CurrentTurnPlayer?.Id == 1)
                    Raylib.DrawRectangleLines(10, 10, 160, 60, Color.Orange);
                else
                    Raylib.DrawRectangleLines(10, 10, 160, 60, Color.DarkGray);
                
                Raylib.DrawText("Player 1", 20, 15, 20, Color.RayWhite);
                Raylib.DrawText("Balls Pocketed: 0", 20, 45, 14, Color.LightGray);
                
                // Player 2 Box (Top Right)
                int p2BoxX = 800 - 170;
                Raylib.DrawRectangle(p2BoxX, 10, 160, 60, new Color(30, 30, 30, 200));
                if (_sceneData.CurrentTurnPlayer?.Id == 2)
                    Raylib.DrawRectangleLines(p2BoxX, 10, 160, 60, Color.Orange);
                else
                    Raylib.DrawRectangleLines(p2BoxX, 10, 160, 60, Color.DarkGray);
                
                Raylib.DrawText("Player 2", p2BoxX + 10, 15, 20, Color.RayWhite);
                Raylib.DrawText("Balls Pocketed: 0", p2BoxX + 10, 45, 14, Color.LightGray);

                // PowerBar UI
                if (_sceneData.CueInfo != null)
                {
                    int barWidth = 300;
                    int barHeight = 15;
                    int barX = 400 - barWidth / 2;
                    int barY = 45;

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
