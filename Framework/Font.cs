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

        // Теперь храним 4 UV-координаты отдельно для каждого символа
        private float[] texCoords;
        private bool isInitialized = false;

        public FontInstance(string fontName, float fontSize)
        {
            FontName = fontName;
            FontSize = fontSize;
            FirstChar = 32;
            LastChar = 126;
            CharCount = LastChar - FirstChar + 1;
            texCoords = new float[CharCount * 4];
        }

        // В начало класса FontInstance добавьте поле:
        private Point[] savedCharPositions;

        public void Init()
        {
            if (isInitialized) return;

            // 1. Измеряем точные максимальные габариты символов шрифта
            using (var font = new System.Drawing.Font(FontName, FontSize, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var dummyBitmap = new Bitmap(1, 1))
            using (var dummyGfx = System.Drawing.Graphics.FromImage(dummyBitmap))
            {
                var stringFormat = System.Drawing.StringFormat.GenericTypographic;
                SizeF sizeM = dummyGfx.MeasureString("M", font, PointF.Empty, stringFormat);
                SizeF sizeW = dummyGfx.MeasureString("W", font, PointF.Empty, stringFormat);

                CharWidth = (int)Math.Ceiling(Math.Max(sizeM.Width, sizeW.Width)) + 2;
                CharHeight = (int)Math.Ceiling(Math.Max(sizeM.Height, sizeW.Height)) + 2;
            }

            // Инициализируем массив для хранения пиксельных позиций
            savedCharPositions = new Point[CharCount];

            // 2. Бронируем место в атласе для каждого символа по очереди
            for (int c = 0; c < CharCount; c++)
            {
                savedCharPositions[c] = FontAtlas.AllocateCharSpace(CharWidth, CharHeight);
            }

            // 3. Рисуем символы на общем холсте атласа
            FontAtlas.DrawToAtlas((gfx) =>
            {
                using (var font = new System.Drawing.Font(FontName, FontSize, FontStyle.Regular, GraphicsUnit.Pixel))
                using (var brush = new SolidBrush(Color.White))
                {
                    var stringFormat = System.Drawing.StringFormat.GenericTypographic;
                    stringFormat.FormatFlags |= System.Drawing.StringFormatFlags.NoWrap;
                    stringFormat.Alignment = System.Drawing.StringAlignment.Center;
                    stringFormat.LineAlignment = System.Drawing.StringAlignment.Center;

                    for (int c = 0; c < CharCount; c++)
                    {
                        char ch = (char)(FirstChar + c);
                        string text = ch.ToString();

                        RectangleF targetRect = new RectangleF(
                            savedCharPositions[c].X,
                            savedCharPositions[c].Y,
                            CharWidth,
                            CharHeight
                        );

                        gfx.DrawString(text, font, brush, targetRect, stringFormat);
                    }
                }
            });

            // 4. Синхронизируем обновленный холст с OpenGL
            FontAtlas.UpdateTexture();

            isInitialized = true;

            // 5. Обновляем UV координаты для ВСЕХ шрифтов через FontManager
            FontManager.RefreshAllFontUVs();
        }


        public void RefreshUVCoordinates()
        {
            if (!isInitialized) return;

            float atlasW = FontAtlas.Width;
            float atlasH = FontAtlas.Height;

            // В коде инициализации нам понадобятся сохраненные пиксельные позиции.
            // Давайте добавим приватный массив в класс, чтобы помнить их:
            // private Point[] savedCharPositions; (измените код Init, как показано на Шаге 2)

            for (int c = 0; c < CharCount; c++)
            {
                float px = savedCharPositions[c].X;
                float py = savedCharPositions[c].Y;

                texCoords[c * 4 + 0] = px / atlasW;                 // u0
                texCoords[c * 4 + 1] = py / atlasH;                 // v0
                texCoords[c * 4 + 2] = (px + CharWidth) / atlasW;   // u1
                texCoords[c * 4 + 3] = (py + CharHeight) / atlasH;  // v1
            }
        }

        public void DrawText(string text, float x, float y, float scale = 1f, float r = 1f, float g = 1f, float b = 1f, float a = 1f)
        {
            if (string.IsNullOrEmpty(text)) return;

            Graphics.SetTexture(FontAtlas.TextureID);

            // ВЫНОСИМ МАТЕМАТИКУ NDC НАРУЖУ: Считаем коэффициенты перевода один раз на всю строку
            float invWidth2 = 2f / Graphics.Width;
            float invHeight2 = 2f / Graphics.Height;

            // Переводим начальные пиксельные координаты в NDC
            float glX = x * invWidth2 - 1f;
            float glY = -(y * invHeight2) + 1f;

            // Вычисляем размер одного символа в пространстве NDC
            float glW = (CharWidth * scale / 2f) * invWidth2;
            float glH = (CharHeight * scale / 2f) * invHeight2;

            // Шаг смещения каретки (в NDC)
            float glStepX = glW * 0.75f;

            float currentGlX = glX;

            foreach (char ch in text)
            {
                if (ch < FirstChar || ch > LastChar) continue;

                int charIndex = ch - FirstChar;
                int coordIdx = charIndex * 4;

                float u0 = texCoords[coordIdx + 0];
                float v0 = texCoords[coordIdx + 1];
                float u1 = texCoords[coordIdx + 2];
                float v1 = texCoords[coordIdx + 3];

                // Крайние точки символа в пространстве NDC
                float left = currentGlX;
                float right = currentGlX + glW;
                float top = glY;
                float bottom = glY - glH;

                // Добавляем вершины НАПРЯМУЮ в Graphics, минуя лишние вызовы методов и деления
                // Треугольник 1
                Graphics.AddTexturedVertex(left, bottom, u0, v1, r, g, b, a);
                Graphics.AddTexturedVertex(left, top, u0, v0, r, g, b, a);
                Graphics.AddTexturedVertex(right, top, u1, v0, r, g, b, a);

                // Треугольник 2
                Graphics.AddTexturedVertex(left, bottom, u0, v1, r, g, b, a);
                Graphics.AddTexturedVertex(right, top, u1, v0, r, g, b, a);
                Graphics.AddTexturedVertex(right, bottom, u1, v1, r, g, b, a);

                // Смещаем каретку на следующий символ в NDC
                currentGlX += glStepX;
            }
        }


        public void Dispose() { }
    }

    public static class FontManager
    {
        private static readonly Dictionary<string, FontInstance> fonts = new Dictionary<string, FontInstance>();

        public static FontInstance GetFont(string fontName, float fontSize)
        {
            string key = $"{fontName}_{fontSize}";

            if (!fonts.ContainsKey(key))
            {
                var font = new FontInstance(fontName, fontSize);

                // СНАЧАЛА регистрируем в словаре, чтобы методы обновления видели этот шрифт
                fonts[key] = font;

                // ТЕПЕРЬ безопасно инициализируем
                font.Init();
            }

            return fonts[key];
        }

        public static void RefreshAllFontUVs()
        {
            foreach (var font in fonts.Values)
            {
                font.RefreshUVCoordinates();
            }
        }

        public static void Clear()
        {
            fonts.Clear();
            FontAtlas.Clear();
        }
    }


    public static class FontAtlas
    {
        public static int TextureID { get; private set; }

        // Фиксированная ширина, идеальная для кэша видеокарт
        public const int Width = 2048;

        // Текущая рабочая высота атласа (растет динамически)
        public static int Height { get; private set; } = 128;

        private static Bitmap atlasBitmap;
        private static System.Drawing.Graphics atlasGraphics;
        private static bool isInitialized = false;

        // Координаты для упаковки следующего символа
        private static int currentX = 0;
        private static int currentY = 0;
        private static int maxRowHeight = 0;

        public static void Init()
        {
            if (isInitialized) return;

            // Стартуем с маленького экономного размера
            atlasBitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            atlasGraphics = System.Drawing.Graphics.FromImage(atlasBitmap);
            atlasGraphics.Clear(Color.Transparent);

            TextureID = GL.GenTexture();

            isInitialized = true;
        }

        // Запрос места под ОДИН конкретный символ
        public static Point AllocateCharSpace(int charWidth, int charHeight)
        {
            Init();

            // Если символ не влезает в текущую строку по горизонтали — перенос строки
            if (currentX + charWidth > Width)
            {
                currentX = 0;
                currentY += maxRowHeight;
                maxRowHeight = 0;
            }

            // Если символы выходят за текущую высоту атласа — динамически увеличиваем Bitmap
            if (currentY + charHeight > Height)
            {
                ResizeAtlas(Height + Math.Max(charHeight, 256));
            }

            Point allocatedPos = new Point(currentX, currentY);

            // Сдвигаем координату X для следующего символа
            currentX += charWidth;

            // Запоминаем самый высокий символ в текущей строке
            if (charHeight > maxRowHeight)
            {
                maxRowHeight = charHeight;
            }

            return allocatedPos;
        }

        // Метод отрисовки на холсте
        public static void DrawToAtlas(Action<System.Drawing.Graphics> drawAction)
        {
            if (atlasGraphics == null) return;

            // Задаем настройки четкости
            atlasGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            atlasGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            atlasGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            atlasGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            drawAction(atlasGraphics);
        }

        // Динамическое расширение памяти атласа
        // Измените метод ResizeAtlas в классе FontAtlas:
        private static void ResizeAtlas(int newHeight)
        {
            Bitmap newBitmap = new Bitmap(Width, newHeight, PixelFormat.Format32bppArgb);
            using (var g = System.Drawing.Graphics.FromImage(newBitmap))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(atlasBitmap, 0, 0);
            }

            atlasGraphics.Dispose();
            atlasBitmap.Dispose();

            atlasBitmap = newBitmap;
            atlasGraphics = System.Drawing.Graphics.FromImage(atlasBitmap);
            Height = newHeight;

            // ИСПРАВЛЕНИЕ: Оповещаем менеджер, что высота изменилась и нужно пересчитать UV
            FontManager.RefreshAllFontUVs();
        }


        // Загрузка измененного холста в OpenGL
        public static void UpdateTexture()
        {
            GL.BindTexture(GL.GL_TEXTURE_2D, TextureID);

            BitmapData data = atlasBitmap.LockBits(
                new Rectangle(0, 0, Width, Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            int bytesPerPixel = 4;
            byte[] rgbaData = new byte[Width * Height * bytesPerPixel];

            unsafe
            {
                byte* srcPtr = (byte*)data.Scan0;
                for (int y = 0; y < Height; y++)
                {
                    byte* srcRow = srcPtr + (y * data.Stride);
                    int destRowOffset = y * Width * bytesPerPixel;

                    for (int x = 0; x < Width; x++)
                    {
                        int srcIdx = x * bytesPerPixel;
                        int destIdx = destRowOffset + (x * bytesPerPixel);

                        rgbaData[destIdx] = srcRow[srcIdx + 2]; // R
                        rgbaData[destIdx + 1] = srcRow[srcIdx + 1]; // G
                        rgbaData[destIdx + 2] = srcRow[srcIdx];     // B
                        rgbaData[destIdx + 3] = srcRow[srcIdx + 3]; // A
                    }
                }
            }

            atlasBitmap.UnlockBits(data);

            IntPtr dataPtr = Marshal.AllocHGlobal(rgbaData.Length);
            Marshal.Copy(rgbaData, 0, dataPtr, rgbaData.Length);

            GL.TexImage2D(
                GL.GL_TEXTURE_2D, 0, GL.GL_RGBA,
                Width, Height, 0,
                GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, dataPtr
            );

            Marshal.FreeHGlobal(dataPtr);

            GL.TexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, (int)GL.GL_LINEAR);
            GL.TexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, (int)GL.GL_LINEAR);
            GL.TexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, (int)GL.GL_CLAMP_TO_EDGE);
            GL.TexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, (int)GL.GL_CLAMP_TO_EDGE);
        }

        public static void Clear()
        {
            if (TextureID != 0) { GL.DeleteTexture(TextureID); TextureID = 0; }
            if (atlasGraphics != null) atlasGraphics.Dispose();
            if (atlasBitmap != null) atlasBitmap.Dispose();
            currentX = 0; currentY = 0; maxRowHeight = 0; Height = 128;
            isInitialized = false;
        }
    }
}