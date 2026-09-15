using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace InfiniTD_2
{
    public static class Input
    {
        // --- Состояния клавиш ---
        private static readonly bool[] keysDown = new bool[256];
        private static readonly bool[] keysPressed = new bool[256];
        private static readonly bool[] keysReleased = new bool[256];

        // --- Состояния мыши ---
        private static readonly bool[] mouseDown = new bool[3]; // 0 = ЛКМ, 1 = ПКМ, 2 = СКМ
        private static readonly bool[] mousePressed = new bool[3];
        private static readonly bool[] mouseReleased = new bool[3];

        // --- Позиция мыши ---
        private static int mouseX = 0;
        private static int mouseY = 0;
        private static int mouseDeltaX = 0;
        private static int mouseDeltaY = 0;
        private static int lastMouseX = 0;
        private static int lastMouseY = 0;

        // --- Колесо мыши ---
        private static float mouseWheelDelta = 0;

        public static int MouseX => mouseX;
        public static int MouseY => mouseY;
        public static int MouseDeltaX => mouseDeltaX;
        public static int MouseDeltaY => mouseDeltaY;
        public static float MouseWheelDelta => mouseWheelDelta;
        private static int virtualMouseX = 0;
        private static int virtualMouseY = 0;

        public static int VirtualMouseX => virtualMouseX;
        public static int VirtualMouseY => virtualMouseY;
        // --- Инициализация ---
        public static void Init()
        {
            NativeWindow.OnKeyDown += OnKeyDownHandler;
            NativeWindow.OnKeyUp += OnKeyUpHandler;
            NativeWindow.OnMouseDown += OnMouseDownHandler;
            NativeWindow.OnMouseUp += OnMouseUpHandler;
            NativeWindow.OnMouseMove += OnMouseMoveHandler;
            NativeWindow.OnMouseWheel += OnMouseWheelHandler;
        }

        public static void Update()
        {
            Array.Clear(keysPressed, 0, keysPressed.Length);
            Array.Clear(mousePressed, 0, mousePressed.Length);
            Array.Clear(mouseReleased, 0, mouseReleased.Length);

            mouseDeltaX = mouseX - lastMouseX;
            mouseDeltaY = mouseY - lastMouseY;
            lastMouseX = mouseX;
            lastMouseY = mouseY;

            mouseWheelDelta = 0;

            // Конвертация в виртуальные координаты
            virtualMouseX = (int)ResolutionConv.ToVirtualX(mouseX);
            virtualMouseY = (int)ResolutionConv.ToVirtualY(mouseY);
        }

        // --- Обработчики событий ---
        private static void OnKeyDownHandler(uint vk)
        {
            if (vk < 256)
            {
                if (!keysDown[vk])
                {
                    keysPressed[vk] = true;
                }
                keysDown[vk] = true;
            }
        }

        private static void OnKeyUpHandler(uint vk)
        {
            if (vk < 256)
            {
                if (keysDown[vk])
                {
                    keysReleased[vk] = true;
                }
                keysDown[vk] = false;
            }
        }

        private static void OnMouseDownHandler(uint button)
        {
            if (button < 3)
            {
                if (!mouseDown[button])
                {
                    mousePressed[button] = true;
                }
                mouseDown[button] = true;
            }
        }

        private static void OnMouseUpHandler(uint button)
        {
            if (button < 3)
            {
                if (mouseDown[button])
                {
                    mouseReleased[button] = true;
                }
                mouseDown[button] = false;
            }
        }

        private static void OnMouseMoveHandler(int x, int y)
        {
            mouseX = x;
            mouseY = y;
        }

        private static void OnMouseWheelHandler(float delta)
        {
            mouseWheelDelta = delta;
        }

        // --- API для проверки состояний ---
        public static bool IsKeyDown(uint vk) => vk < 256 && keysDown[vk];
        public static bool IsKeyPressed(uint vk) => vk < 256 && keysPressed[vk];
        public static bool IsKeyReleased(uint vk) => vk < 256 && keysReleased[vk];

        public static bool IsMouseButtonDown(uint button) => button < 3 && mouseDown[button];
        public static bool IsMouseButtonPressed(uint button) => button < 3 && mousePressed[button];
        public static bool IsMouseButtonReleased(uint button) => button < 3 && mouseReleased[button];

        // --- Виртуальные коды клавиш ---
        public const uint VK_LBUTTON = 0x01;
        public const uint VK_RBUTTON = 0x02;
        public const uint VK_MBUTTON = 0x04;
        public const uint VK_ESCAPE = 0x1B;
        public const uint VK_SPACE = 0x20;
        public const uint VK_LEFT = 0x25;
        public const uint VK_UP = 0x26;
        public const uint VK_RIGHT = 0x27;
        public const uint VK_DOWN = 0x28;
        public const uint VK_A = 0x41;
        public const uint VK_D = 0x44;
        public const uint VK_W = 0x57;
        public const uint VK_S = 0x53;
    }
}