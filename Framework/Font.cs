using InfiniTD_2.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace InfiniTD_2
{
    public class FontInstance
    {
        public string FontName { get; private set; }
        public float FontSize { get; private set; }
        public int CharWidth { get; private set; }
        public int CharHeight { get; private set; }
        public int FirstChar { get; private set; }
        public int LastChar { get; private set; }
        public int CharCount { get; private set; }

        private int fontTexture;
        private float[] texCoords;
        private int shaderProgram;
        private int vbo;
        private bool isInitialized = false;

        public FontInstance(string fontName, float fontSize, int charWidth, int charHeight)
        {
            FontName = fontName;
            FontSize = fontSize;
            CharWidth = charWidth;
            CharHeight = charHeight;
            FirstChar = 32;
            LastChar = 126;
            CharCount = LastChar - FirstChar + 1;
        }

        public void Init()
        {
            if (isInitialized) return;

            GenerateFontTexture();
            GenerateTexCoords();
            CreateShader();
            CreateBuffer();

            isInitialized = true;
        }

        private void GenerateFontTexture()
        {
            int textureWidth = CharWidth * CharCount;
            int textureHeight = CharHeight;

            using (var bitmap = new Bitmap(textureWidth, textureHeight, PixelFormat.Format32bppArgb))
            using (var gfx = System.Drawing.Graphics.FromImage(bitmap))
            {
                gfx.Clear(Color.Transparent);

                using (var font = new System.Drawing.Font(FontName, FontSize, FontStyle.Regular, GraphicsUnit.Pixel))
                {
                    for (int c = 0; c < CharCount; c++)
                    {
                        char ch = (char)(FirstChar + c);
                        string text = ch.ToString();

                        using (var brush = new SolidBrush(Color.White))
                        {
                            gfx.DrawString(text, font, brush, c * CharWidth, 0);
                        }
                    }
                }

                BitmapData data = bitmap.LockBits(
                    new Rectangle(0, 0, textureWidth, textureHeight),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                byte[] fontData = new byte[data.Stride * data.Height];
                Marshal.Copy(data.Scan0, fontData, 0, fontData.Length);
                bitmap.UnlockBits(data);

                for (int i = 0; i < fontData.Length; i += 4)
                {
                    byte b = fontData[i];
                    byte g = fontData[i + 1];
                    byte r = fontData[i + 2];
                    byte a = fontData[i + 3];

                    fontData[i] = r;
                    fontData[i + 1] = g;
                    fontData[i + 2] = b;
                    fontData[i + 3] = a;
                }

                fontTexture = GL.GenTexture();
                GL.BindTexture(GL.GL_TEXTURE_2D, fontTexture);

                IntPtr dataPtr = Marshal.AllocHGlobal(fontData.Length);
                Marshal.Copy(fontData, 0, dataPtr, fontData.Length);

                GL.TexImage2D(
                    GL.GL_TEXTURE_2D,
                    0,
                    GL.GL_RGBA,
                    textureWidth,
                    textureHeight,
                    0,
                    GL.GL_RGBA,
                    GL.GL_UNSIGNED_BYTE,
                    dataPtr);

                Marshal.FreeHGlobal(dataPtr);

                GL.TexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, (int)GL.GL_LINEAR);
                GL.TexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, (int)GL.GL_LINEAR);
                GL.TexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, (int)GL.GL_CLAMP_TO_EDGE);
                GL.TexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, (int)GL.GL_CLAMP_TO_EDGE);
            }
        }

        private void GenerateTexCoords()
        {
            texCoords = new float[CharCount * 4];
            float texWidth = CharWidth * CharCount;
            float texHeight = CharHeight;

            for (int c = 0; c < CharCount; c++)
            {
                float u0 = (c * CharWidth) / texWidth;
                float u1 = ((c + 1) * CharWidth) / texWidth;
                float v0 = 0f / texHeight;
                float v1 = CharHeight / texHeight;

                texCoords[c * 4 + 0] = u0;
                texCoords[c * 4 + 1] = v0;
                texCoords[c * 4 + 2] = u1;
                texCoords[c * 4 + 3] = v1;
            }
        }

        private void CreateShader()
        {
            string vsSource = @"
                #version 330 core
                layout(location = 0) in vec2 aPos;
                layout(location = 1) in vec2 aTexCoord;
                layout(location = 2) in vec4 aColor;
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
                uniform sampler2D textTexture;
                void main() {
                    vec4 texColor = texture(textTexture, TexCoord);
                    FragColor = vec4(Color.rgb, texColor.a * Color.a);
                }
            ";

            shaderProgram = GL.CreateProgramFromSources(vsSource, fsSource);

            int texLocation = GL.GetUniformLocation(shaderProgram, "textTexture");
            GL.UseProgram(shaderProgram);
            GL.Uniform1i(texLocation, 0);
        }

        private void CreateBuffer()
        {
            vbo = GL.GenBuffer();
        }

        public void DrawText(string text, float x, float y, float scale = 1f, float r = 1f, float g = 1f, float b = 1f, float a = 1f)
        {
            if (string.IsNullOrEmpty(text)) return;


             // СНАЧАЛА устанавливаем текстуру

            float currentX = x;
            float currentY = y;

            foreach (char ch in text)
            {
                if (ch < FirstChar || ch > LastChar) continue;
                Graphics.SetTexture(fontTexture);
                int charIndex = ch - FirstChar;
                float u0 = texCoords[charIndex * 4 + 0];
                float v0 = texCoords[charIndex * 4 + 1];
                float u1 = texCoords[charIndex * 4 + 2];
                float v1 = texCoords[charIndex * 4 + 3];

                float w = CharWidth * scale / 2;
                float h = CharHeight * scale / 2;

                Primitives.DrawTexturedQuad(currentX, currentY, w, h, u0, v0, u1, v1, r, g, b, a);
                Graphics.Flush();
                currentX += w * 0.7f;
            }

            
            Graphics.ResetTexture();
        }

        public void Dispose()
        {
            if (fontTexture != 0)
            {
                GL.DeleteTexture(fontTexture);
            }
        }
    }

    public static class FontManager
    {
        private static readonly Dictionary<string, FontInstance> fonts = new Dictionary<string, FontInstance>();

        public static FontInstance GetFont(string fontName, float fontSize, int charWidth, int charHeight)
        {
            string key = $"{fontName}_{fontSize}_{charWidth}_{charHeight}";

            if (!fonts.ContainsKey(key))
            {
                var font = new FontInstance(fontName, fontSize, charWidth, charHeight);
                font.Init();
                fonts[key] = font;
            }

            return fonts[key];
        }

        public static void Clear()
        {
            foreach (var font in fonts.Values)
            {
                font.Dispose();
            }
            fonts.Clear();
        }
    }
}