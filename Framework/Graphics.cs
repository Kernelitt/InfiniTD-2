using InfiniTD_2;
using System;
using System.Runtime.InteropServices;

public static class Graphics
{
    private static int shaderProgram;
    private static int textureShaderProgram;
    private static int vao;
    private static int vaoTex;
    private static int vbo;
    private static int vboTex;
    private static int currentTexture = 0;

    private const int MAX_VERTICES = 100000;
    private static float[] vertexBuffer = new float[MAX_VERTICES * 6]; // Без UV для примитивов
    private static float[] texVertexBuffer = new float[MAX_VERTICES * 8]; // С UV для текста
    private static int vertexCount = 0;
    private static int texVertexCount = 0;
    private static bool useTexture = false;

    public static int Width { get; private set; } = 1600;
    public static int Height { get; private set; } = 900;

    public static void Init()
    {
        // Создаём VAO и VBO для примитивов
        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();

        // Создаём VAO и VBO для текстур
        vaoTex = GL.GenVertexArray();
        vboTex = GL.GenBuffer();

        // Шейдер без текстуры
        string vsSource = @"
        #version 330 core
        layout (location = 0) in vec2 aPos;
        layout (location = 1) in vec4 aColor;
        out vec4 ourColor;
        void main()
        {
            gl_Position = vec4(aPos, 0.0, 1.0);
            ourColor = aColor;
        }
    ";

        string fsSource = @"
        #version 330 core
        in vec4 ourColor;
        out vec4 FragColor;
        void main()
        {
            FragColor = ourColor;
        }
    ";

        shaderProgram = GL.CreateProgramFromSources(vsSource, fsSource);

        // Шейдер с текстурой
        string texVsSource = @"
        #version 330 core
        layout (location = 0) in vec2 aPos;
        layout (location = 1) in vec2 aTexCoord;
        layout (location = 2) in vec4 aColor;
        out vec2 TexCoord;
        out vec4 Color;
        void main()
        {
            gl_Position = vec4(aPos, 0.0, 1.0);
            TexCoord = aTexCoord;
            Color = aColor;
        }
    ";

        string texFsSource = @"
        #version 330 core
        in vec2 TexCoord;
        in vec4 Color;
        out vec4 FragColor;
        uniform sampler2D textTexture;
        void main()
        {
            vec4 texColor = texture(textTexture, TexCoord);
            FragColor = vec4(Color.rgb, texColor.a * Color.a);
        }
    ";

        textureShaderProgram = GL.CreateProgramFromSources(texVsSource, texFsSource);

        int texLocation = GL.GetUniformLocation(textureShaderProgram, "textTexture");
        GL.UseProgram(textureShaderProgram);
        GL.Uniform1i(texLocation, 0);

        // Настраиваем VAO для примитивов (6 компонентов: pos + color)
        GL.BindVertexArray(vao);
        GL.BindBuffer(GL.GL_ARRAY_BUFFER, vbo);
        GL.BufferData(GL.GL_ARRAY_BUFFER, new IntPtr(MAX_VERTICES * 6 * sizeof(float)), IntPtr.Zero, GL.GL_DYNAMIC_DRAW);

        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(0, 2, GL.GL_FLOAT, false, 6 * sizeof(float), IntPtr.Zero);
        GL.VertexAttribPointer(1, 4, GL.GL_FLOAT, false, 6 * sizeof(float), new IntPtr(2 * sizeof(float)));

        GL.BindVertexArray(0);
        GL.BindBuffer(GL.GL_ARRAY_BUFFER, 0);

        // Настраиваем VAO для текстур (8 компонентов: pos + uv + color)
        GL.BindVertexArray(vaoTex);
        GL.BindBuffer(GL.GL_ARRAY_BUFFER, vboTex);
        GL.BufferData(GL.GL_ARRAY_BUFFER, new IntPtr(MAX_VERTICES * 8 * sizeof(float)), IntPtr.Zero, GL.GL_DYNAMIC_DRAW);

        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(0, 2, GL.GL_FLOAT, false, 8 * sizeof(float), IntPtr.Zero);
        GL.VertexAttribPointer(1, 2, GL.GL_FLOAT, false, 8 * sizeof(float), new IntPtr(2 * sizeof(float)));
        GL.VertexAttribPointer(2, 4, GL.GL_FLOAT, false, 8 * sizeof(float), new IntPtr(4 * sizeof(float)));

        GL.BindVertexArray(0);
        GL.BindBuffer(GL.GL_ARRAY_BUFFER, 0);
    }

    public static void SetViewport(int width, int height)
    {
        Width = width;
        Height = height;
        GL.Viewport(0, 0, width, height);
    }

    public static void SetTexture(int texture)
    {
        if (currentTexture != texture && texVertexCount > 0)
        {
            Flush();
        }
        currentTexture = texture;
        useTexture = texture != 0;
    }
    public static void ResetTexture()
    {
        if (vertexCount > 0 && useTexture)
        {
            Flush();
        }
        currentTexture = 0;
        useTexture = false;
    }


    public static void BeginBatch()
    {
        vertexCount = 0;
        texVertexCount = 0;
        currentTexture = 0;
    }

    public static void AddVertex(float x, float y, float r, float g, float b, float a)
    {
        if (vertexCount >= MAX_VERTICES)
        {
            Flush();
        }

        int idx = vertexCount * 6;

        vertexBuffer[idx] = x;
        vertexBuffer[idx + 1] = y;
        vertexBuffer[idx + 2] = r;
        vertexBuffer[idx + 3] = g;
        vertexBuffer[idx + 4] = b;
        vertexBuffer[idx + 5] = a;
        vertexCount++;
    }

    public static void AddTexturedVertex(float x, float y, float u, float v, float r, float g, float b, float a)
    {
        if (texVertexCount >= MAX_VERTICES)
        {
            Flush();
        }

        int idx = texVertexCount * 8;
        texVertexBuffer[idx] = x;
        texVertexBuffer[idx + 1] = y;
        texVertexBuffer[idx + 2] = u;
        texVertexBuffer[idx + 3] = v;
        texVertexBuffer[idx + 4] = r;
        texVertexBuffer[idx + 5] = g;
        texVertexBuffer[idx + 6] = b;
        texVertexBuffer[idx + 7] = a;
        texVertexCount++;
    }

    public static void Flush()
    {
        if (vertexCount > 0)
        {
            GL.UseProgram(shaderProgram);
            GL.BindVertexArray(vao);
            GL.BindBuffer(GL.GL_ARRAY_BUFFER, vbo);

            GCHandle handle = GCHandle.Alloc(vertexBuffer, GCHandleType.Pinned);
            try
            {
                GL.BufferSubData(GL.GL_ARRAY_BUFFER, IntPtr.Zero, new IntPtr(vertexCount * 6 * sizeof(float)), handle.AddrOfPinnedObject());

                GL.VertexAttribPointer(0, 2, GL.GL_FLOAT, false, 6 * sizeof(float), IntPtr.Zero);
                GL.VertexAttribPointer(1, 4, GL.GL_FLOAT, false, 6 * sizeof(float), new IntPtr(2 * sizeof(float)));

                GL.DrawArrays(GL.GL_TRIANGLES, 0, vertexCount);
            }
            finally
            {
                handle.Free();
            }
        }

        // Потом текстуры
        if (texVertexCount > 0 && currentTexture != 0)
        {
            GL.UseProgram(textureShaderProgram);
            GL.ActiveTexture(GL.GL_TEXTURE0);
            GL.BindTexture(GL.GL_TEXTURE_2D, currentTexture);
            GL.BindVertexArray(vaoTex);
            GL.BindBuffer(GL.GL_ARRAY_BUFFER, vboTex);

            GCHandle handle = GCHandle.Alloc(texVertexBuffer, GCHandleType.Pinned);
            try
            {
                GL.BufferSubData(GL.GL_ARRAY_BUFFER, IntPtr.Zero, new IntPtr(texVertexCount * 8 * sizeof(float)), handle.AddrOfPinnedObject());
                GL.VertexAttribPointer(0, 2, GL.GL_FLOAT, false, 8 * sizeof(float), IntPtr.Zero);
                GL.VertexAttribPointer(1, 2, GL.GL_FLOAT, false, 8 * sizeof(float), new IntPtr(2 * sizeof(float)));
                GL.VertexAttribPointer(2, 4, GL.GL_FLOAT, false, 8 * sizeof(float), new IntPtr(4 * sizeof(float)));

                GL.DrawArrays(GL.GL_TRIANGLE_FAN, 0, texVertexCount);
                
            }
            finally
            {
                handle.Free();
            }
        }

        GL.BindBuffer(GL.GL_ARRAY_BUFFER, 0);
        GL.BindVertexArray(0);

        vertexCount = 0;
        texVertexCount = 0;
        currentTexture = 0;
    }
}