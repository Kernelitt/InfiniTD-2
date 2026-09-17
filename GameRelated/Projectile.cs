using System;

namespace InfiniTD_2.GameRelated
{
    public class Projectile
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Speed { get; set; } = 200f;
        public float Damage { get; set; } = 25f;
        public Enemy Target { get; set; }
        public bool IsActive { get; set; } = true;
        public float Radius { get; set; } = 4f;

        public Projectile(float x, float y, Enemy target, float damage)
        {
            X = x;
            Y = y;
            Target = target;
            Damage = damage;
        }

        public void Update(float deltaTime)
        {
            if (!IsActive || Target == null || !Target.IsActive)
            {
                IsActive = false;
                return;
            }

            float dx = Target.X - X;
            float dy = Target.Y - Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance < 10f)
            {
                // Попадание
                Target.TakeDamage(Damage);
                IsActive = false;
            }
            else
            {
                // Движение к цели
                float moveX = (dx / distance) * Speed * deltaTime;
                float moveY = (dy / distance) * Speed * deltaTime;
                X += moveX;
                Y += moveY;
            }
        }

        public void Draw(Camera camera)
        {
            if (!IsActive) return;

            float screenX = (X - camera.X) * camera.Zoom;
            float screenY = (Y - camera.Y) * camera.Zoom;
            float size = Radius * camera.Zoom;

            // Снаряд (жёлтый круг)
            Primitives.DrawCircle(screenX, screenY, size, 1f, 0.9f, 0.2f, 1f, 8);
        }
    }
}