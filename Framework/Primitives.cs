using System;
using System.Runtime.InteropServices;

namespace InfiniTD_2.Framework
{
    public static class Primitives
    {
        public static void DrawTriangle(float x1, float y1, float x2, float y2, float x3, float y3,
            float r1, float g1, float b1, float a1,
            float r2, float g2, float b2, float a2,
            float r3, float g3, float b3, float a3)
        {
            float glX1 = (x1 / Graphics.Width) * 2f - 1f;
            float glY1 = -(y1 / Graphics.Height) * 2f + 1f;
            float glX2 = (x2 / Graphics.Width) * 2f - 1f;
            float glY2 = -(y2 / Graphics.Height) * 2f + 1f;
            float glX3 = (x3 / Graphics.Width) * 2f - 1f;
            float glY3 = -(y3 / Graphics.Height) * 2f + 1f;

            Graphics.AddVertex(glX1, glY1, r1, g1, b1, a1);
            Graphics.AddVertex(glX2, glY2, r2, g2, b2, a2);
            Graphics.AddVertex(glX3, glY3, r3, g3, b3, a3);
        }

        public static void DrawQuad(float x, float y, float width, float height,
            float r1, float g1, float b1, float a1,
            float r2, float g2, float b2, float a2,
            float r3, float g3, float b3, float a3,
            float r4, float g4, float b4, float a4)
        {
            float glX = (x / Graphics.Width) * 2f - 1f;
            float glY = -(y / Graphics.Height) * 2f + 1f;
            float glW = (width / Graphics.Width) * 2f;
            float glH = (height / Graphics.Height) * 2f;

            // 2 треугольника
            Graphics.AddVertex(glX, glY + glH, r1, g1, b1, a1);
            Graphics.AddVertex(glX + glW, glY + glH, r2, g2, b2, a2);
            Graphics.AddVertex(glX, glY, r4, g4, b4, a4);

            Graphics.AddVertex(glX + glW, glY + glH, r2, g2, b2, a2);
            Graphics.AddVertex(glX + glW, glY, r3, g3, b3, a3);
            Graphics.AddVertex(glX, glY, r4, g4, b4, a4);
        }

        public static void DrawTexturedQuad(float x, float y, float width, float height,
    float u0, float v0, float u1, float v1,
    float r, float g, float b, float a)
        {
            float glX = (x / Graphics.Width) * 2f - 1f;
            float glY = -(y / Graphics.Height) * 2f + 1f;
            float glW = (width / Graphics.Width) * 2f;
            float glH = (height / Graphics.Height) * 2f;

            // 4 вершины для GL_TRIANGLE_FAN
            Graphics.AddTexturedVertex(glX, glY - glH, u0, v1, r, g, b, a);  // Bottom-left
            Graphics.AddTexturedVertex(glX, glY, u0, v0, r, g, b, a);        // Top-left
            Graphics.AddTexturedVertex(glX + glW, glY, u1, v0, r, g, b, a);  // Top-right
            Graphics.AddTexturedVertex(glX + glW, glY - glH, u1, v1, r, g, b, a); // Bottom-right
        }

        public static void DrawCircle(float centerX, float centerY, float radius, float r, float g, float b, float a, int segments = 32)
        {
            float glR = (radius / Graphics.Width) * 2f;
            float glCX = ((centerX + radius / 2) / Graphics.Width) * 2f - 1f;
            float glCY = -((centerY + radius / 2) / Graphics.Height) * 2f + 1f;

            // Центр
            

            // Вершины по окружности
            for (int i = 0; i <= segments; i++)
            {
                Graphics.AddVertex(glCX, glCY, r, g, b, a);

                for (int j = 0; j <= 1; j++)
                {
                    double angle = (2.0 * Math.PI * (i + j)) / (segments + 1);
                    float vx = (float)(glCX + glR * Math.Cos(angle));
                    float vy = (float)(glCY + glR * (Graphics.Width / (float)Graphics.Height) * Math.Sin(angle));

                    Graphics.AddVertex(vx, vy, r, g, b, a);
                }
            }
        }

        public static void DrawQuad(float x, float y, float width, float height, float r, float g, float b, float a)
        {
            // x, y - это top-left угол
            float glX = (x / Graphics.Width) * 2f - 1f;
            float glY = -(y / Graphics.Height) * 2f + 1f;
            float glW = (width / Graphics.Width) * 2f;
            float glH = (height / Graphics.Height) * 2f;

            // 2 треугольника (6 вершин)
            Graphics.AddVertex(glX, glY, r, g, b, a);           // Top-left
            Graphics.AddVertex(glX + glW, glY, r, g, b, a);     // Top-right
            Graphics.AddVertex(glX, glY - glH, r, g, b, a);     // Bottom-left

            Graphics.AddVertex(glX + glW, glY, r, g, b, a);     // Top-right
            Graphics.AddVertex(glX + glW, glY - glH, r, g, b, a); // Bottom-right
            Graphics.AddVertex(glX, glY - glH, r, g, b, a);     // Bottom-left
        }

        public static void DrawLine(float x1, float y1, float x2, float y2, float width, float r, float g, float b, float a)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);

            if (length == 0) return;

            float nx = -dy / length * (width / 2f);
            float ny = dx / length * (width / 2f);

            float leftX1 = x1 + nx;
            float leftY1 = y1 + ny;
            float rightX1 = x1 - nx;
            float rightY1 = y1 - ny;
            float leftX2 = x2 + nx;
            float leftY2 = y2 + ny;
            float rightX2 = x2 - nx;
            float rightY2 = y2 - ny;

            float glLeftX1 = (leftX1 / Graphics.Width) * 2f - 1f;
            float glLeftY1 = -(leftY1 / Graphics.Height) * 2f + 1f;
            float glRightX1 = (rightX1 / Graphics.Width) * 2f - 1f;
            float glRightY1 = -(rightY1 / Graphics.Height) * 2f + 1f;
            float glLeftX2 = (leftX2 / Graphics.Width) * 2f - 1f;
            float glLeftY2 = -(leftY2 / Graphics.Height) * 2f + 1f;
            float glRightX2 = (rightX2 / Graphics.Width) * 2f - 1f;
            float glRightY2 = -(rightY2 / Graphics.Height) * 2f + 1f;

            Graphics.AddVertex(glLeftX1, glLeftY1, r, g, b, a);
            Graphics.AddVertex(glRightX1, glRightY1, r, g, b, a);
            Graphics.AddVertex(glLeftX2, glLeftY2, r, g, b, a);

            Graphics.AddVertex(glRightX1, glRightY1, r, g, b, a);
            Graphics.AddVertex(glRightX2, glRightY2, r, g, b, a);
            Graphics.AddVertex(glLeftX2, glLeftY2, r, g, b, a);
        }

    }
}