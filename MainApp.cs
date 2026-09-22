
using InfiniTD_2.Framework;
using InfiniTD_2.Framework.Audio;
using InfiniTD_2.GameRelated;
using System;
using System.Diagnostics;
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

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
            NativeWindow.SetVSync(SaveSystem.LoadSetting<bool>(SaveSystem.SettingType.VSync));
            NativeWindow.SetFPSLimit(SaveSystem.LoadSetting<int>(SaveSystem.SettingType.MaxFPS));

            GL.ClearColor(0.1f, 0.1f, 0.2f, 1.0f);

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
            Profiler.BeginFrame();

            // Замер очистки экрана
            Profiler.Start("GL.Clear");
            GL.Clear(GL.GL_COLOR_BUFFER_BIT);
            Profiler.Stop("GL.Clear");

            // Замер подготовки батча
            Profiler.Start("Graphics.BeginBatch");
            Graphics.BeginBatch();
            Profiler.Stop("Graphics.BeginBatch");

            // Замер логики отрисовки всего меню/игры
            Profiler.Start("MainMenu.Draw");
            MainMenu.Draw();
            Profiler.Stop("MainMenu.Draw");

            // Замер отправки данных в GPU (очень важный показатель!)
            Profiler.Start("Graphics.Flush");
            Graphics.Flush();
            Profiler.Stop("Graphics.Flush");


            // Замер ожидания вертикальной синхронизации и смены буферов
            Profiler.Start("NativeWindow.Swap");
            NativeWindow.Swap();
            Profiler.Stop("NativeWindow.Swap");

            // 2. Завершаем кадр профайлера
            Profiler.EndFrame();

            // 3. Выводим результат (для начала можно в консоль отладки, либо на экран через DrawText)
            // Чтобы не спамить в консоль каждый кадр, можно делать это, например, раз в 60 кадров
            Console.WriteLine(Profiler.GetSummary());
        }

    }
}