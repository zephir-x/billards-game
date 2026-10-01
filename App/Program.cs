using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;
using System;
using System.Numerics;
using System.Linq;
using BilliardsGame.App;
using BilliardsGame.Core;
using BilliardsGame.Physics;
using Raylib_cs;

namespace BilliardsGame.App
{
    class Program
    {
        static void Main()
        {
            Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
            Raylib.InitWindow(1600, 900, "2D Billiards Game");
            Raylib.SetTargetFPS(144); 
            Raylib.SetExitKey(KeyboardKey.Null);

            var audioManager = new AudioManager();
            audioManager.Initialize();

            Action[] loadSteps = new Action[]
            {
                () => audioManager.LoadBGM("menu", "Audio/BGM/bgm_menu.mp3"),
                () => audioManager.LoadBGM("gameplay", "Audio/BGM/bgm_gameplay.mp3"),
                () => audioManager.LoadSFX("collision", "Audio/SFX/sfx_collision.mp3"),
                () => audioManager.LoadSFX("cushion", "Audio/SFX/sfx_cushion.mp3"),
                () => audioManager.LoadSFX("pocket", "Audio/SFX/sfx_pocket.mp3"),
                () => audioManager.LoadSFX("cue_hit", "Audio/SFX/sfx_cue_hit.mp3"),
                () => audioManager.LoadSFX("game_over", "Audio/SFX/sfx_game_over.mp3"),
                () => audioManager.LoadSFX("ui_click", "Audio/SFX/sfx_ui_click.mp3"),
                () => audioManager.LoadSFX("foul", "Audio/SFX/sfx_faul.mp3"),
                () => audioManager.LoadSFX("announcement", "Audio/SFX/sfx_announcement.mp3")
            };

            for (int i = 0; i < loadSteps.Length; i++)
            {
                // Render spinning loading screen
                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(5, 5, 12, 255));
                Raylib.DrawRectangleGradientV(0, 0, 1600, 900, new Color(5, 5, 25, 255), new Color(40, 5, 5, 255));
                
                string txt = "Loading Resources";
                int fSize = 36;
                int wText = Raylib.MeasureText(txt, fSize);
                
                Raylib.DrawText(txt, 1600 / 2 - wText / 2, 900 / 2 - 80, fSize, Color.RayWhite);
                
                float t = (float)Raylib.GetTime() * 360f;
                Raylib.DrawRing(new Vector2(1600 / 2, 900 / 2 + 20), 20f, 26f, t, t + 100f, 32, Color.Gold);
                Raylib.DrawRing(new Vector2(1600 / 2, 900 / 2 + 20), 20f, 26f, t + 180f, t + 280f, 32, Color.Gold);
                
                Raylib.EndDrawing();

                // Load one step
                loadSteps[i].Invoke();
            }

            audioManager.PlayBGM("menu", 0.6f);
            var physicsEngine = new PhysicsEngine(1.2f, 8.0f);
            physicsEngine.OnCollisionOccurred += audioManager.PlayCollision;
            physicsEngine.OnCushionHit += audioManager.PlayCushionHit;
            physicsEngine.OnBallPocketed += audioManager.PlayPocketed;

            
            // Table Main Cushions
            physicsEngine.AddBody(new Cushion(10, new Vector2(230, 200), new Vector2(470, 200), 0.9f));
            physicsEngine.AddBody(new Cushion(11, new Vector2(530, 200), new Vector2(770, 200), 0.9f));
            physicsEngine.AddBody(new Cushion(12, new Vector2(800, 230), new Vector2(800, 570), 0.9f));
            physicsEngine.AddBody(new Cushion(13, new Vector2(770, 600), new Vector2(530, 600), 0.9f));
            physicsEngine.AddBody(new Cushion(14, new Vector2(470, 600), new Vector2(230, 600), 0.9f));
            physicsEngine.AddBody(new Cushion(15, new Vector2(200, 570), new Vector2(200, 230), 0.9f));

            // Table Corner and Middle Jaws (Pocket funnels)
            physicsEngine.AddBody(new Cushion(20, new Vector2(200, 230), new Vector2(180, 210), 0.9f));
            physicsEngine.AddBody(new Cushion(21, new Vector2(210, 180), new Vector2(230, 200), 0.9f));
            physicsEngine.AddBody(new Cushion(22, new Vector2(470, 200), new Vector2(480, 175), 0.9f));
            physicsEngine.AddBody(new Cushion(23, new Vector2(520, 175), new Vector2(530, 200), 0.9f));
            physicsEngine.AddBody(new Cushion(24, new Vector2(770, 200), new Vector2(790, 180), 0.9f));
            physicsEngine.AddBody(new Cushion(25, new Vector2(820, 210), new Vector2(800, 230), 0.9f));
            physicsEngine.AddBody(new Cushion(26, new Vector2(800, 570), new Vector2(820, 590), 0.9f));
            physicsEngine.AddBody(new Cushion(27, new Vector2(790, 620), new Vector2(770, 600), 0.9f));
            physicsEngine.AddBody(new Cushion(28, new Vector2(530, 600), new Vector2(520, 625), 0.9f));
            physicsEngine.AddBody(new Cushion(29, new Vector2(480, 625), new Vector2(470, 600), 0.9f));
            physicsEngine.AddBody(new Cushion(30, new Vector2(230, 600), new Vector2(210, 620), 0.9f));
            physicsEngine.AddBody(new Cushion(31, new Vector2(180, 590), new Vector2(200, 570), 0.9f));

            // Pockets
            physicsEngine.AddPocket(new Hole(new Vector2(190, 190), 24f));
            physicsEngine.AddPocket(new Hole(new Vector2(500, 180), 24f));
            physicsEngine.AddPocket(new Hole(new Vector2(810, 190), 24f));
            physicsEngine.AddPocket(new Hole(new Vector2(190, 610), 24f));
            physicsEngine.AddPocket(new Hole(new Vector2(500, 620), 24f));
            physicsEngine.AddPocket(new Hole(new Vector2(810, 610), 24f));

            Action setupBalls = () => 
            {
                var bodies = physicsEngine.GetBodies().ToList();
                foreach (var b in bodies)
                {
                    if (b is BilliardsGame.Interfaces.ICircleBody)
                        physicsEngine.RemoveBody(b);
                }

                float r = 10f;
                float startX = 650f;
                float startY = 400f;
                float dx = r * MathF.Sqrt(3);
                float dy = r;

                var numbers = new int[] 
                {
                    1,
                    9, 2,
                    10, 8, 3,
                    11, 4, 12, 5,
                    6, 14, 7, 15, 13
                };

                int idx = 0;
                for (int col = 0; col < 5; col++)
                {
                    float x = startX + col * dx;
                    float yStart = startY - col * dy;

                    for (int row = 0; row <= col; row++)
                    {
                        float y = yStart + row * 2 * dy;
                        int number = numbers[idx++];
                        BallType type;
                        if (number == 8) type = BallType.Black;
                        else if (number >= 1 && number <= 7) type = BallType.Solid;
                        else type = BallType.Striped;

                        var ball = new Ball(number, new Vector2(x, y), 0.15f, r, 0.8f, type, number);
                        physicsEngine.AddBody(ball);
                    }
                }

                var whiteBall = new Ball(0, new Vector2(350, 400), 0.15f, r, 0.8f, BallType.Cue, 0);
                physicsEngine.AddBody(whiteBall);
            };

            setupBalls();

            var inputProvider = new RaylibInputProvider();
            var cueController = new CueController();
            cueController.OnCueHit += audioManager.PlayCueHit;
            var gameManager = new GameManager(physicsEngine, cueController, inputProvider);
            gameManager.OnNotificationEvent += (isFoul) => {
                if (isFoul) audioManager.PlaySFX("foul", 0.08f);
                else audioManager.PlaySFX("announcement", 0.08f);
            };
            gameManager.OnPlaceCueBall += (pos) => 
            {
                var whiteBall = new Ball(0, pos, 0.15f, 10f, 0.8f, BallType.Cue, 0);
                physicsEngine.AddBody(whiteBall);
                audioManager.PlaySFX("collision", 0.7f);
            };

            gameManager.OnGameOver += () => { audioManager.StopBGM("gameplay"); audioManager.PlayGameOver(); audioManager.PlayBGM("menu", 0.6f); };
            var sceneData = new SceneParameters(gameManager, physicsEngine, cueController);
            var renderer = new RaylibRenderer();
            renderer.Initialize(sceneData);
            renderer.OnButtonHovered = () => audioManager.PlayUIHover();
            
            inputProvider.SetCoordinateMapper((screenPos) => Raylib.GetScreenToWorld2D(screenPos, renderer.MainCamera));

            bool isPaused = false;
            bool shouldExit = false;

            renderer.OnPlayClicked = () => { audioManager.PlayUIClick(); audioManager.StopBGM("menu"); audioManager.PlayBGM("gameplay", 0.4f);
                inputProvider.ConsumeClickForUI();
                gameManager.StartGame();
            };
            renderer.OnExitClicked = () => { audioManager.PlayUIClick();
                shouldExit = true;
            };
            renderer.OnContinueClicked = () => { audioManager.PlayUIClick(); audioManager.StopBGM("menu"); audioManager.PlayBGM("gameplay");
                inputProvider.ConsumeClickForUI();
                isPaused = false;
            };
            renderer.OnRestartClicked = () => { audioManager.PlayUIClick(); audioManager.StopBGM("menu"); audioManager.PlayBGM("gameplay", 0.4f);
                inputProvider.ConsumeClickForUI();
                isPaused = false;
                setupBalls();
                gameManager.StartGame();
            };

            float accumulator = 0f;
            const float dt = 1f / 480f;

            while (!Raylib.WindowShouldClose() && !shouldExit)
            {
                inputProvider.Update();
                if (gameManager.CurrentState != GameState.Menu && 
                    gameManager.CurrentState != GameState.GameOver)
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                    {
                        isPaused = !isPaused;
                        if (isPaused) { audioManager.StopBGM("gameplay"); audioManager.PlayBGM("menu"); }
                        else { audioManager.StopBGM("menu"); audioManager.PlayBGM("gameplay"); }
                    }
                }
                else
                {
                    isPaused = false;
                }

                float frameTime = Raylib.GetFrameTime();
                if (frameTime > 0.1f) frameTime = 0.1f;
                
                renderer.IsPaused = isPaused;

                if (!isPaused)
                {
                    accumulator += frameTime;
                    gameManager.UpdateLogic(frameTime);

                    while (accumulator >= dt)
                    {
                        physicsEngine.Step(dt);
                        accumulator -= dt;
                    }
                }

                audioManager.Update();
                float alpha = isPaused ? 0f : (accumulator / dt);
                renderer.DrawFrame(alpha);
            }

            audioManager.Deinitialize();
            Raylib.CloseWindow();
        }
    }
}
