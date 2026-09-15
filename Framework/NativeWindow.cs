using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace InfiniTD_2
{
    public static class NativeWindow
    {
        // --- Win32 константы ---
        const uint CS_OWNDC = 0x0020;
        const uint CW_USEDEFAULT = 0x80000000;
        const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;
        const uint WS_VISIBLE = 0x10000000;
        const uint WS_POPUP = 0x80000000;
        const uint SW_SHOW = 5;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOZORDER = 0x0004;

        const uint WM_DESTROY = 0x0002;
        const uint WM_SIZE = 0x0005;
        const uint WM_KEYDOWN = 0x0100;

        const int VK_F11 = 0x7A;
        const int VK_ESCAPE = 0x1B;

        // --- Форматы пикселей OpenGL ---
        const int PFD_DRAW_TO_WINDOW = 4;
        const int PFD_SUPPORT_OPENGL = 32;
        const int PFD_DOUBLEBUFFER = 1;
        const int PFD_TYPE_RGBA = 0;
        const int PFD_MAIN_PLANE = 0;

        [StructLayout(LayoutKind.Sequential)]
        struct PIXELFORMATDESCRIPTOR
        {
            public ushort nSize;
            public ushort nVersion;
            public uint dwFlags;
            public byte iPixelType;
            public byte cColorBits;
            public byte cRedBits, cRedShift;
            public byte cGreenBits, cGreenShift;
            public byte cBlueBits, cBlueShift;
            public byte cAlphaBits, cAlphaShift;
            public byte cAccumBits, cAccumRedBits, cAccumGreenBits, cAccumBlueBits;
            public byte cDepthBits, cStencilBits, cAuxBuffers;
            public byte iLayerType;
            public byte bReserved;
            public uint dwLayerMask;
            public uint dwVisibleMask;
            public uint dwDamageMask;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct WNDCLASS
        {
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public IntPtr lpszMenuName;
            public string lpszClassName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public int pt_x;
            public int pt_y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left, top, right, bottom;
        }

        // --- P/Invoke ---
        [DllImport("user32.dll", SetLastError = true)]
        static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr CreateWindowEx(
            uint dwExStyle,
            string lpClassName,
            string lpWindowName,
            uint dwStyle,
            int X, int Y,
            int nWidth, int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        [DllImport("user32.dll")]
        static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        static extern bool PostQuitMessage(int nExitCode);

        [DllImport("user32.dll")]
        static extern IntPtr DefWindowProc(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool AdjustWindowRect(ref RECT lpRect, uint dwStyle, bool bMenu);

        [DllImport("user32.dll")]
        static extern bool SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        static extern int ChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR pfd);

        [DllImport("gdi32.dll")]
        static extern bool SetPixelFormat(IntPtr hdc, int format, ref PIXELFORMATDESCRIPTOR pfd);

        [DllImport("gdi32.dll")]
        static extern bool SwapBuffers(IntPtr hdc);

        [DllImport("opengl32.dll")]
        static extern IntPtr wglCreateContext(IntPtr hdc);

        [DllImport("opengl32.dll")]
        static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        delegate IntPtr WndProcDelegate(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // --- Глобальные для окна ---
        static IntPtr hwnd;
        static IntPtr hdc;
        static IntPtr hglrc;
        static WNDCLASS wc;
        static WndProcDelegate wndProcDelegate;

        static bool isFullScreen = false;
        static uint windowedStyle;
        static RECT windowedRect;

        public static IntPtr Handle => hwnd;
        public static IntPtr GLContext => hglrc;
        public static int Width { get; private set; }
        public static int Height { get; private set; }

        public static Action<int, int> OnResize;
        public static Action<uint> OnKeyDown;

        // --- События ввода ---
        public static Action<uint> OnKeyUp;
        public static Action<uint> OnMouseDown;
        public static Action<uint> OnMouseUp;
        public static Action<int, int> OnMouseMove;
        public static Action<float> OnMouseWheel;

        // --- Константы сообщений ---
        const uint WM_KEYUP = 0x0101;
        const uint WM_LBUTTONDOWN = 0x0201;
        const uint WM_LBUTTONUP = 0x0202;
        const uint WM_RBUTTONDOWN = 0x0204;
        const uint WM_RBUTTONUP = 0x0205;
        const uint WM_MBUTTONDOWN = 0x0207;
        const uint WM_MBUTTONUP = 0x0208;
        const uint WM_MOUSEMOVE = 0x0200;
        const uint WM_MOUSEWHEEL = 0x020A;
        const uint WM_QUIT = 0x0012;
        private const uint PM_REMOVE = 1;

        public static bool PeekMessage()
        {
            if (PeekMessage(out MSG msg, IntPtr.Zero, 0, 0, PM_REMOVE))
            {
                if (msg.message == WM_QUIT)
                {
                    return false;
                }

                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
                return true;
            }
            return false;
        }

        // --- WndProc ---
        static IntPtr WndProc(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam)
        {
            switch (Msg)
            {
                case WM_DESTROY:
                    wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                    PostQuitMessage(0);
                    return IntPtr.Zero;
                case WM_SIZE:
                    {
                        GetClientRect(hWnd, out var rect);
                        int newWidth = rect.right - rect.left;
                        int newHeight = rect.bottom - rect.top;

                        if (newWidth > 0 && newHeight > 0)
                        {
                            Width = newWidth;
                            Height = newHeight;
                            wglMakeCurrent(hdc, hglrc);
                            if (GL.IsInitialized)
                            {
                                GL.Viewport(0, 0, Width, Height);
                            }
                            OnResize?.Invoke(Width, Height);
                        }

                        break;
                    }

                case WM_KEYDOWN:
                    {
                        uint vk = (uint)wParam;
                        if (vk == VK_F11)
                        {
                            ToggleFullScreen();
                        }
                        else if (vk == VK_ESCAPE && isFullScreen)
                        {
                            ToggleFullScreen();
                        }
                        OnKeyDown?.Invoke(vk);
                        break;
                    }

                case WM_KEYUP:
                    {
                        uint vk = (uint)wParam;
                        OnKeyUp?.Invoke(vk);
                        break;
                    }

                case WM_LBUTTONDOWN:
                    OnMouseDown?.Invoke(0); // ЛКМ
                    break;
                case WM_LBUTTONUP:
                    OnMouseUp?.Invoke(0);
                    break;
                case WM_RBUTTONDOWN:
                    OnMouseDown?.Invoke(1); // ПКМ
                    break;
                case WM_RBUTTONUP:
                    OnMouseUp?.Invoke(1);
                    break;
                case WM_MBUTTONDOWN:
                    OnMouseDown?.Invoke(2); // СКМ
                    break;
                case WM_MBUTTONUP:
                    OnMouseUp?.Invoke(2);
                    break;
                case WM_MOUSEMOVE:
                    {
                        int lParamInt = lParam.ToInt32();
                        int x = (short)(lParamInt & 0xFFFF);
                        int y = (short)(lParamInt >> 16 & 0xFFFF);
                        OnMouseMove?.Invoke(x, y);
                        break;
                    }
                case WM_MOUSEWHEEL:
                    {
                        int delta = (int)((lParam.ToInt32() >> 16) & 0xFFFF);
                        if (delta > 32767) delta -= 65536; // Преобразуем в знаковое
                        OnMouseWheel?.Invoke(delta / 120f);
                        break;
                    }
            }

            return DefWindowProc(hWnd, Msg, wParam, lParam);
        }

        const uint SWP_FRAMECHANGED = 0x0020;

        static void ToggleFullScreen()
        {
            isFullScreen = !isFullScreen;

            if (isFullScreen)
            {
                GetClientRect(hwnd, out windowedRect);
                windowedStyle = WS_OVERLAPPEDWINDOW;

                int screenWidth = GetSystemMetrics(0);
                int screenHeight = GetSystemMetrics(1);

                SetWindowLong(hwnd, -16, WS_POPUP);

                // Добавлен флаг SWP_FRAMECHANGED для пересчёта зоны окна
                SetWindowPos(hwnd, IntPtr.Zero, 0, 0, screenWidth, screenHeight, SWP_NOZORDER | SWP_FRAMECHANGED);
                ShowWindow(hwnd, SW_SHOW);
            }
            else
            {
                SetWindowLong(hwnd, -16, windowedStyle);

                int x = windowedRect.left;
                int y = windowedRect.top;
                int w = windowedRect.right - windowedRect.left;
                int h = windowedRect.bottom - windowedRect.top;

                var rect = new RECT { left = x, top = y, right = x + w, bottom = y + h };
                AdjustWindowRect(ref rect, windowedStyle, false);

                int adjustedW = rect.right - rect.left;
                int adjustedH = rect.bottom - rect.top;

                SetWindowPos(hwnd, IntPtr.Zero, x, y, adjustedW, adjustedH, SWP_NOZORDER | SWP_FRAMECHANGED);
                ShowWindow(hwnd, SW_SHOW);
            }
        }

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        public static void Create(string title = "Hello Windows", int width = 800, int height = 600)
        {
            var hInstance = GetModuleHandle(null);

            wndProcDelegate = WndProc;
            wc = new WNDCLASS
            {
                style = CS_OWNDC,
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProcDelegate),
                hInstance = hInstance,
                hCursor = IntPtr.Zero,
                hbrBackground = IntPtr.Zero,
                lpszClassName = "MainWindowClass"
            };

            if (RegisterClass(ref wc) == 0)
                throw new System.ComponentModel.Win32Exception();

            hwnd = CreateWindowEx(
                0,
                "MainWindowClass",
                title,
                WS_OVERLAPPEDWINDOW | WS_VISIBLE,
                unchecked((int)CW_USEDEFAULT),
                unchecked((int)CW_USEDEFAULT),
                width,
                height,
                IntPtr.Zero,
                IntPtr.Zero,
                hInstance,
                IntPtr.Zero);

            if (hwnd == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();

            ShowWindow(hwnd, SW_SHOW);
            UpdateWindow(hwnd);

            hdc = GetDC(hwnd);

            var pfd = new PIXELFORMATDESCRIPTOR
            {
                nSize = (ushort)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(),
                nVersion = 1,
                dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER,
                iPixelType = PFD_TYPE_RGBA,
                cColorBits = 32,
                cDepthBits = 24,
                iLayerType = PFD_MAIN_PLANE
            };

            int pf = ChoosePixelFormat(hdc, ref pfd);
            if (pf == 0) throw new Exception("ChoosePixelFormat failed");
            if (!SetPixelFormat(hdc, pf, ref pfd)) throw new Exception("SetPixelFormat failed");

            hglrc = wglCreateContext(hdc);
            if (hglrc == IntPtr.Zero) throw new Exception("wglCreateContext failed");
            if (!wglMakeCurrent(hdc, hglrc)) throw new Exception("wglMakeCurrent failed");

            GetClientRect(hwnd, out var rect);
            Width = rect.right - rect.left;
            Height = rect.bottom - rect.top;
            // Убрали вызов GL.Viewport отсюда
        }

        public static void Swap()
        {
            SwapBuffers(hdc);
        }

        public static bool RunMessageLoop()
        {
            if (GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
                return true;
            }

            return false;
        }

        public static void Cleanup()
        {
            if (hglrc != IntPtr.Zero)
            {
                wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            }
            if (hdc != IntPtr.Zero && hwnd != IntPtr.Zero)
            {
                ReleaseDC(hwnd, hdc);
            }

        }
    }
}