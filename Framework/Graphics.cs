using InfiniTD_2;
using System;
using System.Runtime.InteropServices;

public static class Graphics
{
    private static int shaderProgram;
    private static int vao;
    private static int vbo;
    private static int currentTexture = 0;
    private static int whiteTexture = 0; // Текстура 1x1 для примитивов

    private const int MAX_VERTICES = 200000; // Увеличили размер буфера
    // Каждая вершина теперь ВСЕГДА имеет 8 компонентов: X, Y, U, V, R, G, B, A
    private static float[] vertexBuffer = new float[MAX_VERTICES * 8];
    private static int vertexCount = 0;

    public static int Width { get; private set; } = 1600;
    public static int Height { get; private set; } = 900;

    public static void Init()
    {
        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();

        // Универсальный шейдер, который работает всегда с текстурой
        string vsSource = @"
        #version 330 core
        layout (location = 0) in vec2 aPos;
        layout (location = 1) in vec2 aTexCoord;
        layout (location = 2) in vec4 aColor;
        out vec2 TexCoord;
        out vec4 Color;
        void main() {
            gl_Position = vec4(aPos, 0.0, 1.0);
            TexCoord = aTexCoord;
            Color = aColor;
        }
        ";

        string fsSource = @"
        #version 330 core
        in vec2 TexCoord;
        in vec4 Color;
        out vec4 FragColor;
        uniform sampler2D mainTexture;
        void main() {
            vec4 texColor = texture(mainTexture, TexCoord);
            // Умножаем цвет текстуры на цвет вершины (подходит и для текста, и для спрайтов)
            FragColor = vec4(Color.rgb * texColor.rgb, texColor.a * Color.a);
        }
        ";

        shaderProgram = GL.CreateProgramFromSources(vsSource, fsSource);

        int texLocation = GL.GetUniformLocation(shaderProgram, "mainTexture");
        GL.UseProgram(shaderProgram);
        GL.Uniform1i(texLocation, 0);

        // Настраиваем один универсальный VAO (8 компонентов: pos(2) + uv(2) + color(4))
        GL.BindVertexArray(vao);
        GL.BindBuffer(GL.GL_ARRAY_BUFFER, vbo);
        GL.BufferData(GL.GL_ARRAY_BUFFER, new IntPtr(MAX_VERTICES * 8 * sizeof(float)), IntPtr.Zero, GL.GL_DYNAMIC_DRAW);

        GL.EnableVertexAttribArray(0); // Позиция
        GL.EnableVertexAttribArray(1); // UV
        GL.EnableVertexAttribArray(2); // Цвет

        GL.VertexAttribPointer(0, 2, GL.GL_FLOAT, false, 8 * sizeof(float), IntPtr.Zero);
        GL.VertexAttribPointer(1, 2, GL.GL_FLOAT, false, 8 * sizeof(float), new IntPtr(2 * sizeof(float)));
        GL.VertexAttribPointer(2, 4, GL.GL_FLOAT, false, 8 * sizeof(float), new IntPtr(4 * sizeof(float)));

        GL.BindVertexArray(0);
        GL.BindBuffer(GL.GL_ARRAY_BUFFER, 0);

        // Создаем белую текстуру 1x1 для отрисовки примитивов без текстуры
        CreateWhiteTexture();
    }

    private static void CreateWhiteTexture()
    {
        whiteTexture = GL.GenTexture();
        GL.BindTexture(GL.GL_TEXTURE_2D, whiteTexture);

        byte[] whitePixel = { 255, 255, 255, 255 };
        IntPtr ptr = Marshal.AllocHGlobal(whitePixel.Length);
        Marshal.Copy(whitePixel, 0, ptr, whitePixel.Length);

        GL.TexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA, 1, 1, 0, GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, ptr);
        Marshal.FreeHGlobal(ptr);

        GL.TexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, (int)GL.GL_NEAREST);
        GL.TexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, (int)GL.GL_NEAREST);
    }

    public static void SetViewport(int width, int height)
    {
        Width = width;
        Height = height;
        GL.Viewport(0, 0, width, height);
    }

    public static void SetTexture(int texture)
    {
        int targetTexture = texture == 0 ? whiteTexture : texture;

        if (currentTexture != targetTexture && vertexCount > 0)
        {
            Flush();
        }
        currentTexture = targetTexture;
    }

    public static void ResetTexture()
    {
        if (currentTexture != whiteTexture && vertexCount > 0)
        {
            Flush();
        }
        currentTexture = whiteTexture;
    }

    public static void BeginBatch()
    {
        vertexCount = 0;
        currentTexture = whiteTexture; // По умолчанию активируем белую текстуру
    }

    // Для обычных примитивов (просто передаем UV = 0)
    public static void AddVertex(float x, float y, float r, float g, float b, float a)
    {
        if (currentTexture != whiteTexture && vertexCount > 0)
        {
            Flush();
            currentTexture = whiteTexture;
        }

        AddTexturedVertex(x, y, 0f, 0f, r, g, b, a);
    }

    // Универсальный метод добавления вершины
    public static void AddTexturedVertex(float x, float y, float u, float v, float r, float g, float b, float a)
    {
        if (vertexCount >= MAX_VERTICES)
        {
            Flush();
        }

        int idx = vertexCount * 8;
        vertexBuffer[idx] = x;
        vertexBuffer[idx + 1] = y;
        vertexBuffer[idx + 2] = u;
        vertexBuffer[idx + 3] = v;
        vertexBuffer[idx + 4] = r;
        vertexBuffer[idx + 5] = g;
        vertexBuffer[idx + 6] = b;
        vertexBuffer[idx + 7] = a;
        vertexCount++;
    }

    public static void Flush()
    {
        if (vertexCount == 0) return;

        GL.UseProgram(shaderProgram);
        GL.ActiveTexture(GL.GL_TEXTURE0);
        GL.BindTexture(GL.GL_TEXTURE_2D, currentTexture);
        GL.BindVertexArray(vao);
        GL.BindBuffer(GL.GL_ARRAY_BUFFER, vbo);

        unsafe
        {
            fixed (float* ptr = vertexBuffer)
            {
                GL.BufferSubData(GL.GL_ARRAY_BUFFER, IntPtr.Zero,
                    new IntPtr(vertexCount * 8 * sizeof(float)), (IntPtr)ptr);
            }
        }

        GL.DrawArrays(GL.GL_TRIANGLES, 0, vertexCount);

        GL.BindBuffer(GL.GL_ARRAY_BUFFER, 0);
        GL.BindVertexArray(0);

        vertexCount = 0;
    }
}
