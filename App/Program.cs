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
            Raylib.InitWindow(800, 600, "2D Billiards Game");
            Raylib.SetTargetFPS(144); 
            Raylib.SetExitKey(KeyboardKey.Null);
            
            var physicsEngine = new PhysicsEngine(1.2f, 8.0f);

            
            // Table Main Cushions
            physicsEngine.AddBody(new Cushion(10, new Vector2(130, 100), new Vector2(370, 100), 0.9f));
            physicsEngine.AddBody(new Cushion(11, new Vector2(430, 100), new Vector2(670, 100), 0.9f));
            physicsEngine.AddBody(new Cushion(12, new Vector2(700, 130), new Vector2(700, 470), 0.9f));
            physicsEngine.AddBody(new Cushion(13, new Vector2(670, 500), new Vector2(430, 500), 0.9f));
            physicsEngine.AddBody(new Cushion(14, new Vector2(370, 500), new Vector2(130, 500), 0.9f));
            physicsEngine.AddBody(new Cushion(15, new Vector2(100, 470), new Vector2(100, 130), 0.9f));

            // Table Corner and Middle Jaws (Pocket funnels)
            physicsEngine.AddBody(new Cushion(20, new Vector2(100, 130), new Vector2(80, 110), 0.9f));
            physicsEngine.AddBody(new Cushion(21, new Vector2(110, 80), new Vector2(130, 100), 0.9f));
            physicsEngine.AddBody(new Cushion(22, new Vector2(370, 100), new Vector2(380, 75), 0.9f));
            physicsEngine.AddBody(new Cushion(23, new Vector2(420, 75), new Vector2(430, 100), 0.9f));
            physicsEngine.AddBody(new Cushion(24, new Vector2(670, 100), new Vector2(690, 80), 0.9f));
            physicsEngine.AddBody(new Cushion(25, new Vector2(720, 110), new Vector2(700, 130), 0.9f));
            physicsEngine.AddBody(new Cushion(26, new Vector2(700, 470), new Vector2(720, 490), 0.9f));
            physicsEngine.AddBody(new Cushion(27, new Vector2(690, 520), new Vector2(670, 500), 0.9f));
            physicsEngine.AddBody(new Cushion(28, new Vector2(430, 500), new Vector2(420, 525), 0.9f));
            physicsEngine.AddBody(new Cushion(29, new Vector2(380, 525), new Vector2(370, 500), 0.9f));
            physicsEngine.AddBody(new Cushion(30, new Vector2(130, 500), new Vector2(110, 520), 0.9f));
            physicsEngine.AddBody(new Cushion(31, new Vector2(80, 490), new Vector2(100, 470), 0.9f));

            // Pockets
            physicsEngine.AddPocket(new Hole(new Vector2(90, 90), 24f));
            physicsEngine.AddPocket(new Hole(new Vector2(400, 80), 24f));
            physicsEngine.AddPocket(new Hole(new Vector2(710, 90), 24f));
            physicsEngine.AddPocket(new Hole(new Vector2(90, 510), 24f));
            physicsEngine.AddPocket(new Hole(new Vector2(400, 520), 24f));
            physicsEngine.AddPocket(new Hole(new Vector2(710, 510), 24f));

            Action setupBalls = () => 
            {
                var bodies = physicsEngine.GetBodies().ToList();
                foreach (var b in bodies)
                {
                    if (b.Id == 0 || b.Id == 1)
                        physicsEngine.RemoveBody(b);
                }

                var whiteBall = new Ball(0, new Vector2(250, 300), 0.15f, 10f, 0.8f);
                var blackBall = new Ball(1, new Vector2(550, 300), 0.15f, 10f, 0.8f);
                
                physicsEngine.AddBody(whiteBall);
                physicsEngine.AddBody(blackBall);
            };

            setupBalls();

            var inputProvider = new RaylibInputProvider();
            var cueController = new CueController();
            var gameManager = new GameManager(physicsEngine, cueController, inputProvider);
            gameManager.OnScratchFoul += () => 
            {
                var whiteBall = new Ball(0, new Vector2(250, 300), 0.15f, 10f, 0.8f);
                physicsEngine.AddBody(whiteBall);
            };

            var sceneData = new SceneParameters(gameManager, physicsEngine, cueController);
            var renderer = new RaylibRenderer();
            renderer.Initialize(sceneData);

            bool isPaused = false;
            bool shouldExit = false;

            renderer.OnPlayClicked = () => {
                inputProvider.ConsumeClickForUI();
                gameManager.StartGame();
            };
            renderer.OnExitClicked = () => {
                shouldExit = true;
            };
            renderer.OnContinueClicked = () => {
                inputProvider.ConsumeClickForUI();
                isPaused = false;
            };
            renderer.OnRestartClicked = () => {
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
                if (gameManager.CurrentState != BilliardsGame.Interfaces.GameState.Menu && 
                    gameManager.CurrentState != BilliardsGame.Interfaces.GameState.GameOver)
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                    {
                        isPaused = !isPaused;
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

                float alpha = isPaused ? 0f : (accumulator / dt);
                renderer.DrawFrame(alpha);
            }

            Raylib.CloseWindow();
        }
    }
}