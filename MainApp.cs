
using InfiniTD_2.Framework;
using InfiniTD_2.Framework.Audio;
using InfiniTD_2.GameRelated;
using System;
using System.Diagnostics;

namespace InfiniTD_2
{
    public static class MainApp
    {
        private static readonly Stopwatch stopwatch = new Stopwatch();
        private static double lastTime = 0;
        private static double deltaTime = 0;

        static bool isRunning = false;

        public static double DeltaTime => deltaTime;

        public static void Start()
        {
            NativeWindow.OnResize += OnResize;
            NativeWindow.OnKeyDown += OnKeyDown;

            Input.Init();

            ResolutionConv.Init();
            MainMenu.Init();
            LevelEditor.Init();
            AudioSynth.Init();
            Graphics.Init();
            Graphics.SetViewport(1600, 900);
            GL.ClearColor(0.1f, 0.1f, 0.2f, 1.0f);
            // Включаем blending для прозрачности
            GL.Enable(GL.GL_BLEND);
            GL.BlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);


            stopwatch.Start();
            lastTime = stopwatch.Elapsed.TotalSeconds;

            isRunning = true;
            RunGameLoop();

            isRunning = false;
        }



        static void RunGameLoop()
        {
            while (isRunning)
            {
                // Обработка всех накопившихся сообщений
                while (NativeWindow.PeekMessage())
                {
                    if (!isRunning) return;
                }

                double currentTime = stopwatch.Elapsed.TotalSeconds;
                deltaTime = currentTime - lastTime;
                lastTime = currentTime;
                AudioSynth.Update();
                MusicPlayer.Update((float)deltaTime);
                MainMenu.Update();
                Input.Update();
                
                    


                Render();
            }
        }

        static void OnResize(int width, int height)
        {

        }

        static void OnKeyDown(uint vk)
        {
            // Обработка нажатий
        }

        public static void Update()
        {

        }

        public static void Render()
        {
            GL.Clear(GL.GL_COLOR_BUFFER_BIT);
            
            GL.ClearColor(0.1f, 0.1f, 0.2f, 1);
            Graphics.BeginBatch();

            MainMenu.Draw();

            Graphics.Flush();
            NativeWindow.Swap();
        }

    }
}