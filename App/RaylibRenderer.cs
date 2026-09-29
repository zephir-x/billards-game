using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;
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
        public Camera2D MainCamera { get; private set; }
        
        public Action? OnPlayClicked { get; set; }
        public Action? OnExitClicked { get; set; }
        
        public Action? OnContinueClicked { get; set; }
        public Action? OnRestartClicked { get; set; }

        public void Initialize(ISceneParameters sceneData)
        {
            _sceneData = sceneData;
            MainCamera = new Camera2D
            {
                Target = new Vector2(500, 420),
                Offset = new Vector2(1600 / 2f, 900 / 2f + 85),
                Rotation = 0.0f,
                Zoom = 1.35f
            };
        }

        public void DrawFrame(float interpolationAlpha)
        {
            Raylib.BeginDrawing();
            
            // Draw Environment Background
            Raylib.ClearBackground(new Color(5, 5, 20, 255));
            Raylib.DrawRectangleGradientH(0, 0, 1600, 900, new Color(5, 5, 25, 255), new Color(40, 5, 5, 255));

            Raylib.BeginMode2D(MainCamera);

            // 1. Outer Dark Wood Base
            Rectangle outerWood = new Rectangle(140, 140, 720, 520);
            Raylib.DrawRectangleRounded(outerWood, 0.15f, 30, new Color(60, 30, 10, 255));

            // 2. Inner Green Cloth
            Rectangle cloth = new Rectangle(180, 180, 640, 440);
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
            var tl1 = new Vector2(210, 180);
            var tl2 = new Vector2(230, 200);
            var tl3 = new Vector2(470, 200);
            var tl4 = new Vector2(480, 180);
            DrawQuad(tl1, tl2, tl3, tl4, railColor);
            DrawQuadLines(tl1, tl2, tl3, tl4, 3f, railBorder);

            // Top Right Rail
            var tr1 = new Vector2(520, 180);
            var tr2 = new Vector2(530, 200);
            var tr3 = new Vector2(770, 200);
            var tr4 = new Vector2(790, 180);
            DrawQuad(tr1, tr2, tr3, tr4, railColor);
            DrawQuadLines(tr1, tr2, tr3, tr4, 3f, railBorder);

            // Bottom Left Rail
            var bl1 = new Vector2(230, 600); 
            var bl2 = new Vector2(210, 620);
            var bl3 = new Vector2(480, 620);
            var bl4 = new Vector2(470, 600);
            DrawQuad(bl1, bl2, bl3, bl4, railColor);
            DrawQuadLines(bl1, bl2, bl3, bl4, 3f, railBorder);

            // Bottom Right Rail
            var br1 = new Vector2(530, 600);
            var br2 = new Vector2(520, 620);
            var br3 = new Vector2(790, 620);
            var br4 = new Vector2(770, 600);
            DrawQuad(br1, br2, br3, br4, railColor);
            DrawQuadLines(br1, br2, br3, br4, 3f, railBorder);

            // Left Rail
            var l1 = new Vector2(180, 210);
            var l2 = new Vector2(180, 590);
            var l3 = new Vector2(200, 570);
            var l4 = new Vector2(200, 230);
            DrawQuad(l1, l2, l3, l4, railColor);
            DrawQuadLines(l1, l2, l3, l4, 3f, railBorder);

            // Right Rail
            var r1 = new Vector2(800, 230);
            var r2 = new Vector2(800, 570);
            var r3 = new Vector2(820, 590);
            var r4 = new Vector2(820, 210);
            DrawQuad(r1, r2, r3, r4, railColor);
            DrawQuadLines(r1, r2, r3, r4, 3f, railBorder);

            // 4. Pocket Holes
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
                        
                        Color ballColor = Color.White;
                        int n = circleBody.Number;
                        if (circleBody.BallType == BallType.Cue) 
                        {
                            ballColor = Color.White;
                        }
                        else if (circleBody.BallType == BallType.Black)
                        {
                            ballColor = new Color(20, 20, 20, 255);
                        }
                        else
                        {
                            int colorIndex = n > 8 ? n - 8 : n;
                            ballColor = colorIndex switch 
                            {
                                1 => new Color(255, 215, 0, 255),
                                2 => new Color(0, 0, 255, 255),
                                3 => new Color(255, 0, 0, 255),
                                4 => new Color(128, 0, 128, 255),
                                5 => new Color(255, 140, 0, 255),
                                6 => new Color(0, 128, 0, 255),
                                7 => new Color(128, 0, 0, 255),
                                _ => Color.White
                            };
                        }

                        if (circleBody.BallType == BallType.Cue)
                        {
                            Raylib.DrawCircleV(renderedPos, circleBody.Radius, Color.RayWhite);
                        }
                        else if (circleBody.BallType == BallType.Solid || circleBody.BallType == BallType.Black)
                        {
                            Raylib.DrawCircleV(renderedPos, circleBody.Radius, ballColor);
                            Raylib.DrawCircleV(renderedPos, circleBody.Radius * 0.55f, Color.RayWhite);
                        }
                        else if (circleBody.BallType == BallType.Striped)
                        {
                            Raylib.DrawCircleV(renderedPos, circleBody.Radius, Color.RayWhite);
                            
                            float stripeHalf = circleBody.Radius * 0.65f;
                            for (float dy = -stripeHalf; dy <= stripeHalf; dy += 0.5f)
                            {
                                float chordX = MathF.Sqrt(circleBody.Radius * circleBody.Radius - dy * dy);
                                Raylib.DrawLineV(
                                    new Vector2(renderedPos.X - chordX, renderedPos.Y + dy),
                                    new Vector2(renderedPos.X + chordX, renderedPos.Y + dy),
                                    ballColor);
                            }
                            Raylib.DrawCircleV(renderedPos, circleBody.Radius * 0.55f, Color.RayWhite);
                        }
                        
                        if (circleBody.BallType != BallType.Cue)
                        {
                            string numStr = n.ToString();
                            Font font = Raylib.GetFontDefault();
                            float fontSize = 9f;
                            float spacing = 1.0f;
                            Vector2 textSize = Raylib.MeasureTextEx(font, numStr, fontSize, spacing);
                            Vector2 textPos = new Vector2(renderedPos.X - textSize.X / 2f, renderedPos.Y - (textSize.Y / 2f) + 1.0f);
                            Raylib.DrawTextEx(font, numStr, textPos, fontSize, spacing, Color.Black);
                        }
                    }
                }
            }

            if (_sceneData?.CurrentState == GameState.BallInHand)
            {
                Vector2 mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), MainCamera);
                Raylib.DrawCircleV(mouseWorld, 10f, new Color(255, 255, 255, 120));
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
            Raylib.EndMode2D();

            // Draw Statistics
            if (_sceneData != null && _sceneData.CurrentState != GameState.Menu && _sceneData.CurrentState != GameState.GameOver)
            {
                string turnText = _sceneData.CurrentTurnPlayer != null  ? "Turn: " + _sceneData.CurrentTurnPlayer.Name : "Turn: None";
                int turnTextWidth = Raylib.MeasureText(turnText, 36); 
                Raylib.DrawText(turnText, 1600 / 2 - turnTextWidth / 2, 20, 36, Color.RayWhite);

                var p1 = _sceneData.Player1;
                var p2 = _sceneData.Player2;
                string p1Type = p1?.AssignedType != null ? p1.AssignedType.ToString()! : "None";
                int p1Pocketed = p1?.Score ?? 0;
                
                string p2Type = p2?.AssignedType != null ? p2.AssignedType.ToString()! : "None";
                int p2Pocketed = p2?.Score ?? 0;

                int boxW = 280;
                int boxH = 130;
                int pad = 20;
                int p1X = pad; int p1Y = pad;
                int p2X = 1600 - boxW - pad; int p2Y = pad;
                int titleSize = 28;
                int valSize = 22;

                // Player 1 Box
                Raylib.DrawRectangle(p1X, p1Y, boxW, boxH, new Color(30, 30, 30, 200));
                if (_sceneData.CurrentTurnPlayer?.Id == 1) Raylib.DrawRectangleLines(p1X, p1Y, boxW, boxH, Color.Orange);
                else Raylib.DrawRectangleLines(p1X, p1Y, boxW, boxH, Color.DarkGray);
                
                string txtP1 = "Player 1";
                string txtP1Type = $"Type: {p1Type}";
                string txtP1Pock = $"Pocketed: {p1Pocketed}/7";

                Raylib.DrawText(txtP1, p1X + boxW/2 - Raylib.MeasureText(txtP1, titleSize)/2, p1Y + 15, titleSize, Color.RayWhite);
                Raylib.DrawText(txtP1Type, p1X + boxW/2 - Raylib.MeasureText(txtP1Type, valSize)/2, p1Y + 55, valSize, Color.LightGray);
                Raylib.DrawText(txtP1Pock, p1X + boxW/2 - Raylib.MeasureText(txtP1Pock, valSize)/2, p1Y + 90, valSize, Color.LightGray);

                // Player 2 Box
                Raylib.DrawRectangle(p2X, p2Y, boxW, boxH, new Color(30, 30, 30, 200));
                if (_sceneData.CurrentTurnPlayer?.Id == 2) Raylib.DrawRectangleLines(p2X, p2Y, boxW, boxH, Color.Orange);
                else Raylib.DrawRectangleLines(p2X, p2Y, boxW, boxH, Color.DarkGray);

                string txtP2 = "Player 2";
                string txtP2Type = $"Type: {p2Type}";
                string txtP2Pock = $"Pocketed: {p2Pocketed}/7";

                Raylib.DrawText(txtP2, p2X + boxW/2 - Raylib.MeasureText(txtP2, titleSize)/2, p2Y + 15, titleSize, Color.RayWhite);
                Raylib.DrawText(txtP2Type, p2X + boxW/2 - Raylib.MeasureText(txtP2Type, valSize)/2, p2Y + 55, valSize, Color.LightGray);
                Raylib.DrawText(txtP2Pock, p2X + boxW/2 - Raylib.MeasureText(txtP2Pock, valSize)/2, p2Y + 90, valSize, Color.LightGray);

                // PowerBar UI
                if (_sceneData.CueInfo != null)
                {
                    int barWidth = 600;
                    int barHeight = 24;
                    int barX = 1600/2 - barWidth / 2;
                    int barY = 70;

                    Raylib.DrawRectangle(barX, barY, barWidth, barHeight, Color.DarkGray);
                    int fillWidth = (int)(barWidth * _sceneData.CueInfo.Power);
                    Color fillColor = _sceneData.CueInfo.IsOverheated ? Color.Orange : new Color(100, 150, 200, 255);
                    if (fillWidth > 0) Raylib.DrawRectangle(barX, barY, fillWidth, barHeight, fillColor);
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
            int screenW = 1600; int screenH = 900;
            Raylib.DrawRectangle(0, 0, screenW, screenH, new Color(0, 0, 0, 150));
            Vector2 mousePos = Raylib.GetMousePosition();
            bool isClick = Raylib.IsMouseButtonPressed(MouseButton.Left);

            int btnWidth = 150; int btnHeight = 80;
            int gap = 50;
            int totalWidth = btnWidth * 2 + gap;
            int startX = (screenW - totalWidth) / 2;
            int startY = screenH / 2 - btnHeight / 2 + 50;

            Rectangle playRect = new Rectangle(startX, startY, btnWidth, btnHeight);
            Rectangle exitRect = new Rectangle(startX + btnWidth + gap, startY, btnWidth, btnHeight);

            bool playHover = Raylib.CheckCollisionPointRec(mousePos, playRect);
            bool exitHover = Raylib.CheckCollisionPointRec(mousePos, exitRect);

            Raylib.DrawRectangleRec(playRect, playHover ? Color.DarkGray : Color.Gray);
            Raylib.DrawRectangleRec(exitRect, exitHover ? Color.DarkGray : Color.Gray);

            int playTextWidth = Raylib.MeasureText("Play", 30);
            int exitTextWidth = Raylib.MeasureText("Exit", 30);
            
            Raylib.DrawText("Play", startX + btnWidth/2 - playTextWidth/2, startY + btnHeight/2 - 15, 30, Color.RayWhite);
            Raylib.DrawText("Exit", (startX + btnWidth + gap) + btnWidth/2 - exitTextWidth/2, startY + btnHeight/2 - 15, 30, Color.RayWhite);
            
            Raylib.DrawText("MAIN MENU", screenW/2 - Raylib.MeasureText("MAIN MENU", 50)/2, screenH/3 - 50, 50, Color.RayWhite);

            if (playHover && isClick) OnPlayClicked?.Invoke();
            if (exitHover && isClick) OnExitClicked?.Invoke();
        }

        private void DrawPauseMenu()
        {
            int screenW = 1600; int screenH = 900;
            Raylib.DrawRectangle(0, 0, screenW, screenH, new Color(0, 0, 0, 150));
            Vector2 mousePos = Raylib.GetMousePosition();
            bool isClick = Raylib.IsMouseButtonPressed(MouseButton.Left);

            int btnWidth = 200; int btnHeight = 60; int gap = 20;
            int startX = screenW / 2 - btnWidth / 2;
            int startY = screenH / 2 - (btnHeight * 3 + gap * 2) / 2 + 50;

            Rectangle continueRect = new Rectangle(startX, startY, btnWidth, btnHeight);
            Rectangle restartRect = new Rectangle(startX, startY + btnHeight + gap, btnWidth, btnHeight);
            Rectangle exitRect = new Rectangle(startX, startY + (btnHeight + gap) * 2, btnWidth, btnHeight);

            bool continueHover = Raylib.CheckCollisionPointRec(mousePos, continueRect);
            bool restartHover = Raylib.CheckCollisionPointRec(mousePos, restartRect);
            bool exitHover = Raylib.CheckCollisionPointRec(mousePos, exitRect);

            Raylib.DrawRectangleRec(continueRect, continueHover ? Color.DarkGray : Color.Gray);
            Raylib.DrawRectangleRec(restartRect, restartHover ? Color.DarkGray : Color.Gray);
            Raylib.DrawRectangleRec(exitRect, exitHover ? Color.DarkGray : Color.Gray);

            int continueTextWidth = Raylib.MeasureText("Continue", 30);
            int restartTextWidth = Raylib.MeasureText("Restart", 30);
            int exitTextWidth = Raylib.MeasureText("Exit", 30);

            Raylib.DrawText("Continue", startX + btnWidth/2 - continueTextWidth/2, startY + btnHeight/2 - 15, 30, Color.RayWhite);
            Raylib.DrawText("Restart", startX + btnWidth/2 - restartTextWidth/2, startY + btnHeight + gap + btnHeight/2 - 15, 30, Color.RayWhite);
            Raylib.DrawText("Exit", startX + btnWidth/2 - exitTextWidth/2, startY + (btnHeight + gap)*2 + btnHeight/2 - 15, 30, Color.RayWhite);
            
            Raylib.DrawText("PAUSED", screenW/2 - Raylib.MeasureText("PAUSED", 50)/2, screenH/3 - 50, 50, Color.RayWhite);

            if (continueHover && isClick) OnContinueClicked?.Invoke();
            if (restartHover && isClick) OnRestartClicked?.Invoke();
            if (exitHover && isClick) OnExitClicked?.Invoke();
        }
        
        private void DrawGameOverMenu()
        {
            int screenW = 1600; int screenH = 900;
            Raylib.DrawRectangle(0, 0, screenW, screenH, new Color(0, 0, 0, 180));
            Raylib.DrawText("GAME OVER", screenW/2 - Raylib.MeasureText("GAME OVER", 60)/2, screenH/3 - 60, 60, Color.Red);
            
            if (_sceneData?.Winner != null)
            {
                string winnerText = "Winner: " + _sceneData.Winner.Name;
                Raylib.DrawText(winnerText, screenW/2 - Raylib.MeasureText(winnerText, 40)/2, screenH/3 + 20, 40, Color.Gold);
            }
            
            Vector2 mousePos = Raylib.GetMousePosition();
            bool isClick = Raylib.IsMouseButtonPressed(MouseButton.Left);

            int btnWidth = 200; int btnHeight = 60; int gap = 20;
            int startX = screenW / 2 - btnWidth / 2;
            int startY = screenH / 2 + 50;

            Rectangle restartRect = new Rectangle(startX, startY, btnWidth, btnHeight);
            Rectangle exitRect = new Rectangle(startX, startY + btnHeight + gap, btnWidth, btnHeight);

            bool restartHover = Raylib.CheckCollisionPointRec(mousePos, restartRect);
            bool exitHover = Raylib.CheckCollisionPointRec(mousePos, exitRect);

            Raylib.DrawRectangleRec(restartRect, restartHover ? Color.DarkGray : Color.Gray);
            Raylib.DrawRectangleRec(exitRect, exitHover ? Color.DarkGray : Color.Gray);
            
            int restartTextWidth = Raylib.MeasureText("Restart", 30);
            int exitTextWidth = Raylib.MeasureText("Exit", 30);

            Raylib.DrawText("Restart", startX + btnWidth/2 - restartTextWidth/2, startY + btnHeight/2 - 15, 30, Color.RayWhite);
            Raylib.DrawText("Exit", startX + btnWidth/2 - exitTextWidth/2, startY + btnHeight + gap + btnHeight/2 - 15, 30, Color.RayWhite);
            
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
