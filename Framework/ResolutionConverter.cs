using System;

namespace InfiniTD_2
{
    public static class ResolutionConv
    {
        // Виртуальное разрешение (базовое)
        public const int VirtualWidth = 1600;
        public const int VirtualHeight = 900;

        // Реальное разрешение окна
        public static int RealWidth => NativeWindow.Width;
        public static int RealHeight => NativeWindow.Height;

        // Коэффициенты масштабирования
        public static float ScaleX { get; private set; } = 1f;
        public static float ScaleY { get; private set; } = 1f;
        public static float Scale { get; private set; } = 1f;

        public static void Init()
        {
            NativeWindow.OnResize += OnResize;
            UpdateScale();
        }

        private static void OnResize(int width, int height)
        {
            UpdateScale();
        }

        private static void UpdateScale()
        {
            ScaleX = (float)RealWidth / VirtualWidth;
            ScaleY = (float)RealHeight / VirtualHeight;
            Scale = Math.Min(ScaleX, ScaleY); // Сохраняем соотношение сторон
        }

        // Конвертация виртуальных координат в реальные
        public static float ToRealX(float virtualX) => virtualX * ScaleX;
        public static float ToRealY(float virtualY) => virtualY * ScaleY;
        public static float ToRealSize(float virtualSize) => virtualSize * Scale;

        // Конвертация реальных координат в виртуальные (для ввода)
        public static float ToVirtualX(float realX) => realX / ScaleX;
        public static float ToVirtualY(float realY) => realY / ScaleY;
    }
}