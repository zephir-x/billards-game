using System.Numerics;
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
            
            // tableFriction = 1.2f, sleepVelocityThreshold = 8.0f
            var physicsEngine = new PhysicsEngine(1.2f, 8.0f);

            var topCushion = new Cushion(10, new Vector2(100, 100), new Vector2(700, 100), 0.9f);
            var rightCushion = new Cushion(11, new Vector2(700, 100), new Vector2(700, 500), 0.9f);
            var bottomCushion = new Cushion(12, new Vector2(700, 500), new Vector2(100, 500), 0.9f);
            var leftCushion = new Cushion(13, new Vector2(100, 500), new Vector2(100, 100), 0.9f);

            physicsEngine.AddBody(topCushion);
            physicsEngine.AddBody(rightCushion);
            physicsEngine.AddBody(bottomCushion);
            physicsEngine.AddBody(leftCushion);

            var hole1 = new Hole(new Vector2(100, 100), 16f);
            var hole2 = new Hole(new Vector2(400, 100), 16f);
            var hole3 = new Hole(new Vector2(700, 100), 16f);
            var hole4 = new Hole(new Vector2(100, 500), 16f);
            var hole5 = new Hole(new Vector2(400, 500), 16f);
            var hole6 = new Hole(new Vector2(700, 500), 16f);

            physicsEngine.AddPocket(hole1);
            physicsEngine.AddPocket(hole2);
            physicsEngine.AddPocket(hole3);
            physicsEngine.AddPocket(hole4);
            physicsEngine.AddPocket(hole5);
            physicsEngine.AddPocket(hole6);
            
            var whiteBall = new Ball(0, new Vector2(250, 300), 0.15f, 10f, 0.8f);
            var blackBall = new Ball(1, new Vector2(550, 300), 0.15f, 10f, 0.8f);
            
            physicsEngine.AddBody(whiteBall);
            physicsEngine.AddBody(blackBall);

            var inputProvider = new RaylibInputProvider();
            var cueController = new CueController();
            var gameManager = new GameManager(physicsEngine, cueController, inputProvider);
            gameManager.StartGame();

            var sceneData = new SceneParameters(gameManager, physicsEngine, cueController);
            var renderer = new RaylibRenderer();
            renderer.Initialize(sceneData);

            float accumulator = 0f;
            const float dt = 1f / 480f;

            while (!Raylib.WindowShouldClose())
            {
                float frameTime = Raylib.GetFrameTime();
                accumulator += frameTime;

                gameManager.UpdateLogic(frameTime);

                while (accumulator >= dt)
                {
                    physicsEngine.Step(dt);
                    accumulator -= dt;
                }

                float alpha = accumulator / dt;
                renderer.DrawFrame(alpha);
            }

            Raylib.CloseWindow();
        }
    }
}