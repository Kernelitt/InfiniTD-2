using System;
using System.Runtime.InteropServices;
using System.Text;

namespace InfiniTD_2
{
    public static class GL
    {
        // --- OpenGL константы ---
        public const uint GL_CLEAR = 0x00004000;
        public const uint GL_COLOR_BUFFER_BIT = 0x00004000;
        public const uint GL_DEPTH_BUFFER_BIT = 0x00000100;
        public const uint GL_FLOAT = 0x1406;
        public const uint GL_TRIANGLES = 0x0004;
        public const uint GL_QUADS = 0x0007;
        public const uint GL_ARRAY_BUFFER = 0x8892;
        public const uint GL_STATIC_DRAW = 0x88E4;
        public const uint GL_FRAGMENT_SHADER = 0x8B30;
        public const uint GL_VERTEX_SHADER = 0x8B31;
        public const uint GL_COMPILE_STATUS = 0x8B81;
        public const uint GL_LINK_STATUS = 0x8B82;
        public const uint GL_INFO_LOG_LENGTH = 0x8B84;
        public const uint GL_TRUE = 1;
        public const uint GL_FALSE = 0;
        public const uint GL_TEXTURE_2D = 0x0DE1;
        public const uint GL_TEXTURE_MIN_FILTER = 0x2801;
        public const uint GL_TEXTURE_MAG_FILTER = 0x2800;
        public const uint GL_TEXTURE_WRAP_S = 0x2802;
        public const uint GL_TEXTURE_WRAP_T = 0x2803;
        public const uint GL_NEAREST = 0x2600;
        public const uint GL_LINEAR = 0x2601;
        public const uint GL_CLAMP_TO_EDGE = 0x812F;
        public const uint GL_REPEAT = 0x2901;
        public const uint GL_TEXTURE0 = 0x84C0;
        public const int GL_RGBA = 0x1908;
        public const int GL_UNSIGNED_BYTE = 0x1401;
        public const uint GL_TRIANGLE_FAN = 0x0006;
        public const uint GL_BLEND = 0x0BE2;
        public const uint GL_SRC_ALPHA = 0x0302;
        public const uint GL_ONE_MINUS_SRC_ALPHA = 0x0303;
        public const uint GL_VERSION = 0x1F02;
        public const uint GL_SHADING_LANGUAGE_VERSION = 0x8B8C;
        public const uint GL_DYNAMIC_DRAW = 0x88E8;
        // --- Делегаты функций OpenGL ---
        delegate void GlClearColorDelegate(float red, float green, float blue, float alpha);
        delegate void GlClearDelegate(uint mask);
        delegate void GlViewportDelegate(int x, int y, int width, int height);
        delegate void GlGenBuffersDelegate(int n, out int buffers);
        delegate void GlBindBufferDelegate(uint target, int buffer);
        delegate void GlBufferDataDelegate(uint target, IntPtr size, IntPtr data, uint usage);
        delegate void GlEnableVertexAttribArrayDelegate(uint index);
        delegate void GlVertexAttribPointerDelegate(uint index, int size, uint type, bool normalized, int stride, IntPtr pointer);
        delegate void GlDrawArraysDelegate(uint mode, int first, int count);
        delegate int GlCreateShaderDelegate(uint type);
        delegate void GlShaderSourceDelegate(int shader, int count, ref string source, IntPtr length);
        delegate void GlCompileShaderDelegate(int shader);
        delegate void GlGetShaderivDelegate(int shader, uint pname, out int @params);
        delegate void GlGetShaderInfoLogDelegate(int shader, int bufSize, out int length, StringBuilder infoLog);
        delegate int GlCreateProgramDelegate();
        delegate void GlAttachShaderDelegate(int program, int shader);
        delegate void GlLinkProgramDelegate(int program);
        delegate void GlGetProgramivDelegate(int program, uint pname, out int @params);
        delegate void GlGetProgramInfoLogDelegate(int program, int bufSize, out int length, StringBuilder infoLog);
        delegate void GlUseProgramDelegate(int program);
        delegate int GlGetUniformLocationDelegate(int program, string name);
        delegate void GlUniform1fDelegate(int location, float v0);
        delegate void GlUniform2fDelegate(int location, float v0, float v1);
        delegate void GlUniform3fDelegate(int location, float v0, float v1, float v2);
        delegate void GlUniform4fDelegate(int location, float v0, float v1, float v2, float v3);
        delegate void GlUniformMatrix4fvDelegate(int location, int count, bool transpose, ref float value);
        delegate void GlUniform1iDelegate(int location, int v0);
        delegate void GlDisableVertexAttribArrayDelegate(uint index);
        delegate void GlEnableDelegate(uint cap);
        delegate void GlDisableDelegate(uint cap);
        delegate void GlBlendFuncDelegate(uint sfactor, uint dfactor);
        delegate void GlGenTexturesDelegate(int n, out int textures);
        delegate void GlBindTextureDelegate(uint target, int texture);
        delegate void GlTexImage2DDelegate(uint target, int level, int internalFormat, int width, int height, int border, uint format, uint type, IntPtr pixels);
        delegate void GlTexParameteriDelegate(uint target, uint pname, int param);
        delegate void GlActiveTextureDelegate(uint texture);
        delegate void GlDeleteTexturesDelegate(int n, ref int textures);
        delegate IntPtr GlGetStringDelegate(uint name);
        delegate void GlColor4fDelegate(float r, float g, float b, float a);
        delegate void GlBeginDelegate(uint mode);
        delegate void GlVertex2fDelegate(float x, float y);
        delegate void GlEndDelegate();
        delegate void GlGenVertexArraysDelegate(int n, out int arrays);
        delegate void GlBindVertexArrayDelegate(int array);
        delegate void GlBufferSubDataDelegate(uint target, IntPtr offset, IntPtr size, IntPtr data);
        delegate void GlDeleteBuffersDelegate(int n, ref int buffers);
        // --- Функции OpenGL ---
        static GlClearColorDelegate glClearColor;
        static GlClearDelegate glClear;
        static GlViewportDelegate glViewport;
        static GlGenBuffersDelegate glGenBuffers;
        static GlBindBufferDelegate glBindBuffer;
        static GlBufferDataDelegate glBufferData;
        static GlEnableVertexAttribArrayDelegate glEnableVertexAttribArray;
        static GlVertexAttribPointerDelegate glVertexAttribPointer;
        static GlDrawArraysDelegate glDrawArrays;
        static GlCreateShaderDelegate glCreateShader;
        static GlShaderSourceDelegate glShaderSource;
        static GlCompileShaderDelegate glCompileShader;
        static GlGetShaderivDelegate glGetShaderiv;
        static GlGetShaderInfoLogDelegate glGetShaderInfoLog;
        static GlCreateProgramDelegate glCreateProgram;
        static GlAttachShaderDelegate glAttachShader;
        static GlLinkProgramDelegate glLinkProgram;
        static GlGetProgramivDelegate glGetProgramiv;
        static GlGetProgramInfoLogDelegate glGetProgramInfoLog;
        static GlUseProgramDelegate glUseProgram;
        static GlGetUniformLocationDelegate glGetUniformLocation;
        static GlUniform1fDelegate glUniform1f;
        static GlUniform2fDelegate glUniform2f;
        static GlUniform3fDelegate glUniform3f;
        static GlUniform4fDelegate glUniform4f;
        static GlUniformMatrix4fvDelegate glUniformMatrix4fv;
        static GlUniform1iDelegate glUniform1i;
        static GlDisableVertexAttribArrayDelegate glDisableVertexAttribArray;
        static GlEnableDelegate glEnable;
        static GlDisableDelegate glDisable;
        static GlBlendFuncDelegate glBlendFunc;
        static GlGenTexturesDelegate glGenTextures;
        static GlBindTextureDelegate glBindTexture;
        static GlTexImage2DDelegate glTexImage2D;
        static GlTexParameteriDelegate glTexParameteri;
        static GlActiveTextureDelegate glActiveTexture;
        static GlDeleteTexturesDelegate glDeleteTextures;
        static GlGetStringDelegate glGetString;
        static GlColor4fDelegate glColor4f;
        static GlBeginDelegate glBegin;
        static GlVertex2fDelegate glVertex2f;
        static GlGenVertexArraysDelegate glGenVertexArrays;
        static GlBindVertexArrayDelegate glBindVertexArray;
        static GlEndDelegate glEnd;
        static GlBufferSubDataDelegate glBufferSubData;
        static GlDeleteBuffersDelegate glDeleteBuffers;

        [DllImport("opengl32.dll", CharSet = CharSet.Ansi)]
        static extern IntPtr wglGetProcAddress(string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        static IntPtr GetGLProc(string name)
        {
            // Сначала пробуем wglGetProcAddress
            var ptr = wglGetProcAddress(name);
            if (ptr != IntPtr.Zero) return ptr;

            // Fallback на opengl32.dll для базовых функций
            var hModule = GetModuleHandle("opengl32.dll");
            if (hModule == IntPtr.Zero)
            {
                hModule = LoadLibrary("opengl32.dll");
            }
            return GetProcAddress(hModule, name);
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        public static bool IsInitialized { get; private set; }

        public static void Init()
        {
            if (IsInitialized) return;

            glClearColor = GetDelegate<GlClearColorDelegate>("glClearColor");
            glClear = GetDelegate<GlClearDelegate>("glClear");
            glViewport = GetDelegate<GlViewportDelegate>("glViewport");
            glGenBuffers = GetDelegate<GlGenBuffersDelegate>("glGenBuffers");
            glBindBuffer = GetDelegate<GlBindBufferDelegate>("glBindBuffer");
            glBufferData = GetDelegate<GlBufferDataDelegate>("glBufferData");
            glEnableVertexAttribArray = GetDelegate<GlEnableVertexAttribArrayDelegate>("glEnableVertexAttribArray");
            glVertexAttribPointer = GetDelegate<GlVertexAttribPointerDelegate>("glVertexAttribPointer");
            glDrawArrays = GetDelegate<GlDrawArraysDelegate>("glDrawArrays");
            glCreateShader = GetDelegate<GlCreateShaderDelegate>("glCreateShader");
            glShaderSource = GetDelegate<GlShaderSourceDelegate>("glShaderSource");
            glCompileShader = GetDelegate<GlCompileShaderDelegate>("glCompileShader");
            glGetShaderiv = GetDelegate<GlGetShaderivDelegate>("glGetShaderiv");
            glGetShaderInfoLog = GetDelegate<GlGetShaderInfoLogDelegate>("glGetShaderInfoLog");
            glCreateProgram = GetDelegate<GlCreateProgramDelegate>("glCreateProgram");
            glAttachShader = GetDelegate<GlAttachShaderDelegate>("glAttachShader");
            glLinkProgram = GetDelegate<GlLinkProgramDelegate>("glLinkProgram");
            glGetProgramiv = GetDelegate<GlGetProgramivDelegate>("glGetProgramiv");
            glGetProgramInfoLog = GetDelegate<GlGetProgramInfoLogDelegate>("glGetProgramInfoLog");
            glUseProgram = GetDelegate<GlUseProgramDelegate>("glUseProgram");
            glGetUniformLocation = GetDelegate<GlGetUniformLocationDelegate>("glGetUniformLocation");
            glUniform1f = GetDelegate<GlUniform1fDelegate>("glUniform1f");
            glUniform2f = GetDelegate<GlUniform2fDelegate>("glUniform2f");
            glUniform3f = GetDelegate<GlUniform3fDelegate>("glUniform3f");
            glUniform4f = GetDelegate<GlUniform4fDelegate>("glUniform4f");
            glUniformMatrix4fv = GetDelegate<GlUniformMatrix4fvDelegate>("glUniformMatrix4fv");
            glGenTextures = GetDelegate<GlGenTexturesDelegate>("glGenTextures");
            glBindTexture = GetDelegate<GlBindTextureDelegate>("glBindTexture");
            glTexImage2D = GetDelegate<GlTexImage2DDelegate>("glTexImage2D");
            glTexParameteri = GetDelegate<GlTexParameteriDelegate>("glTexParameteri");
            glActiveTexture = GetDelegate<GlActiveTextureDelegate>("glActiveTexture");
            glDeleteTextures = GetDelegate<GlDeleteTexturesDelegate>("glDeleteTextures");
            glUniform1i = GetDelegate<GlUniform1iDelegate>("glUniform1i");
            glDisableVertexAttribArray = GetDelegate<GlDisableVertexAttribArrayDelegate>("glDisableVertexAttribArray");
            glEnable = GetDelegate<GlEnableDelegate>("glEnable");
            glDisable = GetDelegate<GlDisableDelegate>("glDisable");
            glBlendFunc = GetDelegate<GlBlendFuncDelegate>("glBlendFunc");
            glGetString = GetDelegate<GlGetStringDelegate>("glGetString");
            glColor4f = GetDelegate<GlColor4fDelegate>("glColor4f");
            glBegin = GetDelegate<GlBeginDelegate>("glBegin");
            glVertex2f = GetDelegate<GlVertex2fDelegate>("glVertex2f");
            glEnd = GetDelegate<GlEndDelegate>("glEnd");
            glGenVertexArrays = GetDelegate<GlGenVertexArraysDelegate>("glGenVertexArrays");
            glBindVertexArray = GetDelegate<GlBindVertexArrayDelegate>("glBindVertexArray");
            glBufferSubData = GetDelegate<GlBufferSubDataDelegate>("glBufferSubData");
            glDeleteBuffers = GetDelegate<GlDeleteBuffersDelegate>("glDeleteBuffers");

            IsInitialized = true;
        }

        static T GetDelegate<T>(string name) where T : Delegate
        {
            var ptr = GetGLProc(name);
            if (ptr == IntPtr.Zero)
                throw new Exception($"Failed to load OpenGL function: {name}");
            return Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }

        // --- Обёртки функций ---
        public static void ClearColor(float r, float g, float b, float a) => glClearColor(r, g, b, a);
        public static void Clear(uint mask) => glClear(mask);
        public static void Viewport(int x, int y, int w, int h) => glViewport(x, y, w, h);
        public static void Enable(uint cap) => glEnable(cap);
        public static void Disable(uint cap) => glDisable(cap);
        public static void BlendFunc(uint sfactor, uint dfactor) => glBlendFunc(sfactor, dfactor);
        public static int GenBuffer()
        {
            glGenBuffers(1, out int buffer);
            return buffer;
        }
        public static string GetString(uint name)
        {
            if (glGetString == null) return "NULL";

            IntPtr ptr = glGetString(name);
            return ptr == IntPtr.Zero ? "ZERO" : Marshal.PtrToStringAnsi(ptr);
        }
        public static void BindBuffer(uint target, int buffer) => glBindBuffer(target, buffer);
        public static void BufferData(uint target, IntPtr size, IntPtr data, uint usage) => glBufferData(target, size, data, usage);

        public static void EnableVertexAttribArray(uint index) => glEnableVertexAttribArray(index);
        public static void VertexAttribPointer(uint index, int size, uint type, bool normalized, int stride, IntPtr pointer) =>
            glVertexAttribPointer(index, size, type, normalized, stride, pointer);

        public static void DrawArrays(uint mode, int first, int count) => glDrawArrays(mode, first, count);

        public static int CreateShader(uint type) => glCreateShader(type);
        public static void ShaderSource(int shader, string source) => glShaderSource(shader, 1, ref source, IntPtr.Zero);
        
        public static void CompileShader(int shader) => glCompileShader(shader);
        public static int GenVertexArray()
        {
            glGenVertexArrays(1, out int vao);
            return vao;
        }
        public static void DeleteBuffer(int buffer) => glDeleteBuffers(1, ref buffer);
        public static void BindVertexArray(int vao) => glBindVertexArray(vao);
        public static void GetShaderiv(int shader, uint pname, out int @params) => glGetShaderiv(shader, pname, out @params);

        public static string GetShaderInfoLog(int shader)
        {
            glGetShaderiv(shader, GL_INFO_LOG_LENGTH, out int logLength);
            if (logLength <= 0) return string.Empty;
            var log = new StringBuilder(logLength);
            glGetShaderInfoLog(shader, logLength, out _, log);
            return log.ToString();
        }

        public static int CreateProgram() => glCreateProgram();
        public static void AttachShader(int program, int shader) => glAttachShader(program, shader);
        public static void LinkProgram(int program) => glLinkProgram(program);

        public static void GetProgramiv(int program, uint pname, out int @params) => glGetProgramiv(program, pname, out @params);

        public static string GetProgramInfoLog(int program)
        {
            glGetProgramiv(program, GL_INFO_LOG_LENGTH, out int logLength);
            if (logLength <= 0) return string.Empty;
            var log = new StringBuilder(logLength);
            glGetProgramInfoLog(program, logLength, out _, log);
            return log.ToString();
        }
        public static void BufferSubData(uint target, IntPtr offset, IntPtr size, IntPtr data) =>
    glBufferSubData(target, offset, size, data);
        public static void UseProgram(int program) => glUseProgram(program);
        public static int GetUniformLocation(int program, string name) => glGetUniformLocation(program, name);
        public static void Uniform1f(int location, float v0) => glUniform1f(location, v0);
        public static void Uniform2f(int location, float v0, float v1) => glUniform2f(location, v0, v1);
        public static void Uniform3f(int location, float v0, float v1, float v2) => glUniform3f(location, v0, v1, v2);
        public static void Uniform4f(int location, float v0, float v1, float v2, float v3) => glUniform4f(location, v0, v1, v2, v3);
        public static void UniformMatrix4fv(int location, int count, bool transpose, ref float value) =>
            glUniformMatrix4fv(location, count, transpose, ref value);
        public static void Uniform1i(int location, int v0) => glUniform1i(location, v0);

        public static void DisableVertexAttribArray(uint index) => glDisableVertexAttribArray(index);
        public static int GenTexture()
        {
            glGenTextures(1, out int texture);
            return texture;
        }
        public static void BindTexture(uint target, int texture) => glBindTexture(target, texture);

        public static void TexImage2D(uint target, int level, int internalFormat, int width, int height, int border, uint format, uint type, IntPtr pixels) =>
            glTexImage2D(target, level, internalFormat, width, height, border, format, type, pixels);

        public static void TexParameteri(uint target, uint pname, int param) => glTexParameteri(target, pname, param);

        public static void ActiveTexture(uint texture) => glActiveTexture(texture);

        public static void DeleteTexture(int texture) => glDeleteTextures(1, ref texture);
        // --- Утилиты ---
        public static int CreateProgramFromSources(string vertexSource, string fragmentSource)
        {
            int vs = CreateShader(GL_VERTEX_SHADER);
            ShaderSource(vs, vertexSource);
            CompileShader(vs);
            GetShaderiv(vs, GL_COMPILE_STATUS, out int vsCompiled);
            if (vsCompiled != GL_TRUE)
                throw new Exception("Vertex shader compile error:\n" + GetShaderInfoLog(vs));

            int fs = CreateShader(GL_FRAGMENT_SHADER);
            ShaderSource(fs, fragmentSource);
            CompileShader(fs);
            GetShaderiv(fs, GL_COMPILE_STATUS, out int fsCompiled);
            if (fsCompiled != GL_TRUE)
                throw new Exception("Fragment shader compile error:\n" + GetShaderInfoLog(fs));

            int program = CreateProgram();
            AttachShader(program, vs);
            AttachShader(program, fs);
            LinkProgram(program);
            GetProgramiv(program, GL_LINK_STATUS, out int linked);
            if (linked != GL_TRUE)
                throw new Exception("Program link error:\n" + GetProgramInfoLog(program));

            return program;
        }

        public static void Color4(float r, float g, float b, float a) => glColor4f(r, g, b, a);
        public static void Begin(uint mode) => glBegin(mode);
        public static void Vertex2(float x, float y) => glVertex2f(x, y);
        public static void End() => glEnd();
    }
}