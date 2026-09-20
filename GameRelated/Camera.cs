using System;

namespace InfiniTD_2.GameRelated
{
    public class Camera
    {
        // Константы базового разрешения экрана
        private const float BaseWidth = 1600f;
        private const float BaseHeight = 900f;

        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public float Zoom { get; set; } = 1f;
        public float Rotation { get; set; } = 0f;

        // Центр экрана в мировых координатах
        public float CenterX => X + (BaseWidth / 2f) / Zoom;
        public float CenterY => Y + (BaseHeight / 2f) / Zoom;

        // Границы экрана с учетом зума
        public float Left => X;
        public float Right => X + BaseWidth / Zoom;
        public float Top => Y;
        public float Bottom => Y + BaseHeight / Zoom;

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

        public void ZoomToCenter(float amount)
        {
            // 1. Запоминаем текущую центральную точку мира под камерой
            float oldCenterX = CenterX;
            float oldCenterY = CenterY;

            Zoom += amount;

            // Ограничиваем зум в пределах от 0.25 до 4.0 через if
            if (Zoom < 0.25f)
            {
                Zoom = 0.25f;
            }
            else if (Zoom > 4.0f)
            {
                Zoom = 4.0f;
            }

            // 3. Сдвигаем X и Y так, чтобы центр остался на том же месте мира
            X = oldCenterX - (BaseWidth / 2f) / Zoom;
            Y = oldCenterY - (BaseHeight / 2f) / Zoom;
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

            // Приближение и отдаление центрируются автоматически
            if (Input.IsKeyDown((uint)'C'))
                ZoomToCenter(0.01f); // Измените знак на минус, если зум работает не в ту сторону
            if (Input.IsKeyDown((uint)'X'))
                ZoomToCenter(-0.01f);
        }
    }
}
