using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace InfiniTD_2.Framework
{
    public static class Profiler
    {
        private static readonly Dictionary<string, Stopwatch> timers = new Dictionary<string, Stopwatch>();
        private static readonly Dictionary<string, double> frameResults = new Dictionary<string, double>();
        private static readonly List<string> order = new List<string>();

        private static Stopwatch totalFrameTimer = new Stopwatch();
        private static double lastTotalFrameTime = 0;

        /// <summary>
        /// Вызывать в самом начале метода Render() или игрового цикла
        /// </summary>
        public static void BeginFrame()
        {
            totalFrameTimer.Restart();
            timers.Clear();
            frameResults.Clear();
            order.Clear();
        }

        /// <summary>
        /// Начать замер определенного участка кода
        /// </summary>
        public static void Start(string sectionName)
        {
            if (!timers.TryGetValue(sectionName, out var sw))
            {
                sw = new Stopwatch();
                timers[sectionName] = sw;
                order.Add(sectionName);
            }
            sw.Start();
        }

        /// <summary>
        /// Остановить замер определенного участка кода
        /// </summary>
        public static void Stop(string sectionName)
        {
            if (timers.TryGetValue(sectionName, out var sw))
            {
                sw.Stop();
                // Сохраняем время в миллисекундах с плавающей запятой
                frameResults[sectionName] = (double)sw.ElapsedTicks / TimeSpan.TicksPerMillisecond;
            }
        }

        /// <summary>
        /// Вызывать в самом конце метода Render(), ПОСЛЕ NativeWindow.Swap()
        /// </summary>
        public static void EndFrame()
        {
            totalFrameTimer.Stop();
            lastTotalFrameTime = (double)totalFrameTimer.ElapsedTicks / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Получить форматированную строку со всеми замерами для вывода на экран или в консоль
        /// </summary>
        public static string GetSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"--- PROFILER (Total Frame: {lastTotalFrameTime:F2} ms) ---");

            foreach (var section in order)
            {
                if (frameResults.TryGetValue(section, out double time))
                {
                    double percent = (time / lastTotalFrameTime) * 100;
                    sb.AppendLine($"{section}: {time:F3} ms ({percent:F1}%)");
                }
            }
            return sb.ToString();
        }
    }
}
