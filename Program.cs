using System;
using System.Runtime.InteropServices;

namespace InfiniTD_2
{
    internal class Program
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [STAThread]
        static void Main(string[] args)
        {

#if DEBUG
            Console.WriteLine("Debug Build!");
#endif

#if !DEBUG
            ShowWindow(GetConsoleWindow(), 0);
            FreeConsole();
#endif

            NativeWindow.Create("InfiniTD 2", 1600, 900);
            GL.Init();
            Console.WriteLine($"OpenGL Version: {GL.GetString(GL.GL_VERSION)}");
            Console.WriteLine($"GLSL Version: {GL.GetString(GL.GL_SHADING_LANGUAGE_VERSION)}");
            Console.WriteLine($"Vendor: {GL.GetString(0x1F00)}"); // GL_VENDOR
            Console.WriteLine($"Renderer: {GL.GetString(0x1F01)}"); // GL_RENDERER
#if !NO_MOD_SUPPORT
            Framework.ModManager.InitAndLoadMods("mods/");
#endif

            MainApp.Start();

            NativeWindow.Cleanup();



        }
    }
}
