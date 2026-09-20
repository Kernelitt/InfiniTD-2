using System;

namespace InfiniTD_2.Framework
{
    public class UIButton
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string Text { get; set; }
        public float R { get; set; } = 0.3f;
        public float G { get; set; } = 0.3f;
        public float B { get; set; } = 0.3f;
        public float HoverR { get; set; } = 0.5f;
        public float HoverG { get; set; } = 0.5f;
        public float HoverB { get; set; } = 0.5f;
        public Action OnClick { get; set; }

        private bool isHovered = false;
        private bool wasPressed = false;

        public UIButton(float x, float y, float width, float height, string text)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Text = text;
        }

        public void Update()
        {
            int mx = Input.VirtualMouseX;
            int my = Input.VirtualMouseY;

            isHovered = mx >= X && mx <= X + Width && my >= Y && my <= Y + Height;

            bool isPressed = Input.IsMouseButtonPressed(0);

            if (isHovered && isPressed && !wasPressed)
            {
                OnClick?.Invoke();
            }

            wasPressed = isPressed;
        }

        public void Draw(FontInstance font)
        {
            float r = isHovered ? HoverR : R;
            float g = isHovered ? HoverG : G;
            float b = isHovered ? HoverB : B;

            Primitives.DrawQuad(X, Y, Width, Height, r, g, b, 1f);

            if (!string.IsNullOrEmpty(Text))
            {
                float textX = X + (Width - font.CharWidth * Text.Length / 2) / 2;
                float textY = Y + (Height - font.CharHeight / 2) / 2;
                font.DrawText(Text, textX, textY, 1f, 1f, 1f, 1f);
            }
        }
    }

    public class UICheckbox
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Size { get; set; }
        public string Label { get; set; }
        public bool IsChecked { get; set; }
        public Action<bool> OnChanged { get; set; }

        private bool wasPressed = false;

        public UICheckbox(float x, float y, float size, string label, bool isChecked = false)
        {
            X = x;
            Y = y;
            Size = size;
            Label = label;
            IsChecked = isChecked;
        }

        public void Update()
        {
            int mx = Input.VirtualMouseX;
            int my = Input.VirtualMouseY;

            bool isHovered = mx >= X && mx <= X + Size && my >= Y && my <= Y + Size;
            bool isPressed = Input.IsMouseButtonPressed(0);

            if (isHovered && isPressed && !wasPressed)
            {
                IsChecked = !IsChecked;
                OnChanged?.Invoke(IsChecked);
            }

            wasPressed = isPressed;
        }

        public void Draw(FontInstance font)
        {
            // Фон чекбокса
            Primitives.DrawQuad(X, Y, Size, Size, 0.3f, 0.3f, 0.3f, 1f);

            // Галочка
            if (IsChecked)
            {
                Primitives.DrawQuad(X + 2, Y + 2, Size - 4, Size - 4, 0.2f, 0.8f, 0.2f, 1f);
            }

            // Лейбл
            if (!string.IsNullOrEmpty(Label))
            {
                font.DrawText(Label, X + Size + 5, Y + (Size - font.CharHeight / 2) / 2f, 1f, 1f, 1f, 1f);
            }
        }
    }

    public class UISlider
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float MinValue { get; set; } = 0f;
        public float MaxValue { get; set; } = 1f;
        public float Value { get; set; } = 0.5f;
        public Action<float> OnChanged { get; set; }

        private bool isDragging = false;

        public UISlider(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public void Update()
        {
            int mx = Input.VirtualMouseX;
            int my = Input.VirtualMouseY;

            bool isHovered = mx >= X && mx <= X + Width && my >= Y && my <= Y + Height;
            bool isClicked = Input.IsMouseButtonPressed(0);
            bool isPressed = Input.IsMouseButtonDown(0);

            if (isHovered && isClicked && !isDragging)
            {
                isDragging = true;
            }

            if (!isPressed)
            {
                isDragging = false;
            }

            if (isDragging)
            {
                float t = (mx - X) / Width;
                t = Math.Max(0f, Math.Min(1f, t));
                Value = MinValue + t * (MaxValue - MinValue);
                OnChanged?.Invoke(Value);
            }
        }

        public void Draw(FontInstance font)
        {
            // Фон слайдера
            Primitives.DrawQuad(X, Y + Height / 3f, Width, Height / 3f, 0.3f, 0.3f, 0.3f, 1f);

            // Заполненная часть
            float fillWidth = ((Value - MinValue) / (MaxValue - MinValue)) * Width;
            Primitives.DrawQuad(X, Y + Height / 3f, fillWidth, Height / 3f, 0.2f, 0.6f, 0.9f, 1f);

            // Ручка
            float handleX = X + fillWidth - Height / 2f;
            Primitives.DrawCircle(handleX, Y + Height / 4f, Height / 2f, 0.8f, 0.8f, 0.8f, 1f);

            // Значение
            font.DrawText(Value.ToString("F2"), X + Width + 10, Y, 1f, 1f, 1f, 1f);
        }
    }
}