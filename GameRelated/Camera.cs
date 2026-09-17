using System;

namespace InfiniTD_2.GameRelated
{
    public class Camera
    {
        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public float Zoom { get; set; } = 1f;
        public float Rotation { get; set; } = 0f;

        public float Left => X;
        public float Right => X + 1600 / Zoom;
        public float Top => Y;
        public float Bottom => Y + 900 / Zoom;

        public void Move(float dx, float dy)
        {
            X += dx;
            Y += dy;
        }

        public void SetPosition(float x, float y)
        {
            X = x;
            Y = y;
        }

        public void ZoomIn(float amount)
        {
            Zoom = Math.Max(0.25f, Zoom - amount);
        }

        public void ZoomOut(float amount)
        {
            Zoom = Math.Min(4f, Zoom + amount);
        }

        public void Update()
        {
            // Управление камерой с клавиатуры
            float moveSpeed = 10f / Zoom;

            if (Input.IsKeyDown(Input.VK_W) || Input.IsKeyDown(Input.VK_UP))
                Move(0, -moveSpeed);

            if (Input.IsKeyDown(Input.VK_S) || Input.IsKeyDown(Input.VK_DOWN))
                Move(0, moveSpeed);

            if (Input.IsKeyDown(Input.VK_A) || Input.IsKeyDown(Input.VK_LEFT))
                Move(-moveSpeed, 0);

            if (Input.IsKeyDown(Input.VK_D) || Input.IsKeyDown(Input.VK_RIGHT))
                Move(moveSpeed, 0);

            // Зум колёсиком мыши
            if (Input.IsKeyDown((uint)'C'))
                ZoomIn(0.1f);
            if (Input.IsKeyDown((uint)'X'))
                ZoomOut(0.1f);


            // Ограничение камеры (опционально)
            // X = Math.Max(0, Math.Min(X, mapWidth - Graphics.VirtualWidth / Zoom));
            // Y = Math.Max(0, Math.Min(Y, mapHeight - Graphics.VirtualHeight / Zoom));
        }
    }
}