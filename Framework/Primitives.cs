using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace InfiniTD_2
{
    public static class Primitives
    {
        private const int Width = 1600, Height = 900;
        private static int shaderProgram;
        private static int vbo;
        private static bool isInitialized = false;

        public static void Init()
        {
            if (isInitialized) return;

            CreateShader();
            vbo = GL.GenBuffer();

            isInitialized = true;
        }

        private static void CreateShader()
        {
            string vsSource = @"
                #version 330 core
                layout(location = 0) in vec2 aPos;
                layout(location = 1) in vec4 aColor;
                out vec4 Color;
                void main() {
                    gl_Position = vec4(aPos, 0.0, 1.0);
                    Color = aColor;
                }
            ";

            string fsSource = @"
                #version 330 core
                in vec4 Color;
                out vec4 FragColor;
                void main() {
                    FragColor = Color;
                }
            ";

            shaderProgram = GL.CreateProgramFromSources(vsSource, fsSource);
        }

        public static void DrawTriangle(float x1, float y1, float x2, float y2, float x3, float y3,
            float r1, float g1, float b1, float a1,
            float r2, float g2, float b2, float a2,
            float r3, float g3, float b3, float a3)
        {

            float glX1 =  (x1 / Width) * 2f - 1f;
            float glY1 = -(y1 / Height) * 2f + 1f;
            float glX2 =  (x2 / Width) * 2f - 1f;
            float glY2 = -(y2 / Height) * 2f + 1f;
            float glX3 =  (x3 / Width) * 2f - 1f;
            float glY3 = -(y3 / Height) * 2f + 1f;

            float[] vertices = new float[]
            {
        glX1, glY1, r1, g1, b1, a1,
        glX2, glY2, r2, g2, b2, a2,
        glX3, glY3, r3, g3, b3, a3,
            };

            DrawPrimitive(vertices, 3);
        }

        public static void DrawQuad(float x, float y, float width, float height, float r, float g, float b, float a)
        {
            float glX = (x / Width) * 2f - 1f;
            float glY =-(y / Height) * 2f + 1f;
            float glW = (width / Width) * 2f;
            float glH = (height / Height) * 2f;

            float[] vertices = new float[]
            {
        glX,       glY - glH,          r, g, b, a,
        glX + glW, glY - glH,    r, g, b, a,
        glX + glW, glY, r, g, b, a,
        glX,       glY,    r, g, b, a,
            };

            DrawPrimitive(vertices, 4);
        }


        public static void DrawQuad(float x, float y, float width, float height,
            float r1, float g1, float b1, float a1,
            float r2, float g2, float b2, float a2,
            float r3, float g3, float b3, float a3,
            float r4, float g4, float b4, float a4)
        {

            float glX = (x / Width) * 2f - 1f;
            float glY =-(y / Height) * 2f + 1f;
            float glW = (width / Width) * 2f;
            float glH = (height / Height) * 2f;

            float[] vertices = new float[]
            {
                glX,       glY - glH,          r1, g1, b1, a1,
                glX + glW, glY - glH,     r2, g2, b2, a2,
                glX + glW, glY,    r3, g3, b3, a3,
                glX,       glY,    r4, g4, b4, a4,
            };

            DrawPrimitive(vertices, 4);
        }

        public static void DrawCircle(float centerX, float centerY, float radius, float r, float g, float b, float a, int segments = 32)
        {
            float glR = (radius / Width) * 2f;
            float glCX = ((centerX + radius / 2) / Width) * 2f - 1f;
            float glCY = -((centerY + radius / 2) / Height) * 2f + 1f;
            

            float[] vertices = new float[(segments + 2) * 6];

            // Центр круга
            vertices[0] = glCX;
            vertices[1] = glCY;
            vertices[2] = r;
            vertices[3] = g;
            vertices[4] = b;
            vertices[5] = a;

            // Вершины по окружности
            for (int i = 0; i <= segments; i++)
            {
                double angle = (2.0 * Math.PI * i) / segments;
                float vx = (float)(glCX + glR * Math.Cos(angle));
                float vy = (float)(glCY + (glR * 16 / 9) * Math.Sin(angle));

                int idx = (i + 1) * 6;
                vertices[idx + 0] = vx;
                vertices[idx + 1] = vy;
                vertices[idx + 2] = r;
                vertices[idx + 3] = g;
                vertices[idx + 4] = b;
                vertices[idx + 5] = a;
            }

            DrawPrimitive(vertices, segments + 2);
        }

        public static void DrawLine(float x1, float y1, float x2, float y2, float width, float r, float g, float b, float a)
        {
            // Линия как тонкий прямоугольник
            float dx = x2 - x1;
            float dy = y2 - y1;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            float nx = -dy / length * (width / 2f);
            float ny = dx / length * (width / 2f);

            DrawQuad(x1 + nx, y1 + ny, x2 - x1 + 2 * nx, y2 - y1 + 2 * ny, r, g, b, a);
        }

        private static void DrawPrimitive(float[] vertices, int vertexCount)
        {
            GL.Enable(GL.GL_BLEND);
            GL.BlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);

            GL.UseProgram(shaderProgram);
            GL.BindBuffer(GL.GL_ARRAY_BUFFER, vbo);

            IntPtr dataPtr = Marshal.AllocHGlobal(vertices.Length * sizeof(float));
            Marshal.Copy(vertices, 0, dataPtr, vertices.Length);
            GL.BufferData(GL.GL_ARRAY_BUFFER, new IntPtr(vertices.Length * sizeof(float)), dataPtr, GL.GL_STATIC_DRAW);
            Marshal.FreeHGlobal(dataPtr);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(0, 2, GL.GL_FLOAT, false, 6 * sizeof(float), IntPtr.Zero);
            GL.VertexAttribPointer(1, 4, GL.GL_FLOAT, false, 6 * sizeof(float), new IntPtr(2 * sizeof(float)));

            GL.DrawArrays(GL.GL_TRIANGLE_FAN, 0, vertexCount);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);

            GL.Disable(GL.GL_BLEND);
        }
    }
}