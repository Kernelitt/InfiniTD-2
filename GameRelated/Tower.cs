using InfiniTD_2.Framewok.Audio;
using System;
using System.Collections.Generic;

namespace InfiniTD_2.GameRelated
{
    public class Tower
    {
        public short X { get; set; }
        public short Y { get; set; }
        public int Cost { get; set; }
        public float Range { get; set; } = 200f;
        public float Damage { get; set; } = 25f;
        public float FireRate { get; set; } = 1.4f; // Выстрелов в секунду
        public float Rotation { get; set; } = 0f;
        public float TargetRotation { get; set; } = 0f;
        public float RotationSpeed { get; set; } = 15f;
        public float ReloadTimer { get; set; } = 0f;
        public Enemy Target { get; set; } = null;
        public bool IsActive { get; set; } = true;
        public TargetingMode TargetingMode { get; set; } = TargetingMode.First;
        public bool ShowMenu { get; set; } = false;

        public Tower(short x, short y, int cost)
        {
            X = x;
            Y = y;
            Cost = cost;
        }

        public void Update(List<Enemy> enemies, float deltaTime, List<Projectile> projectiles)
        {
            if (!IsActive) return;

            // Выбор цели
            Target = SelectTarget(enemies);

            if (Target != null && Target.IsActive)
            {
                // Вычисление угла до цели
                float targetX = Target.X;
                float targetY = Target.Y;
                float towerX = X * 50 + 25;
                float towerY = Y * 50 + 25;

                float dx = targetX - towerX;
                float dy = targetY - towerY;
                TargetRotation = (float)Math.Atan2(dy, dx);

                // Плавный поворот башни
                float angleDiff = TargetRotation - Rotation;
                while (angleDiff > Math.PI) angleDiff -= 2 * (float)Math.PI;
                while (angleDiff < -Math.PI) angleDiff += 2 * (float)Math.PI;

                if (Math.Abs(angleDiff) < 0.05f)
                {
                    Rotation = TargetRotation;
                }
                else
                {
                    Rotation += Math.Sign(angleDiff) * RotationSpeed * deltaTime;
                }

                // Перезарядка и выстрел
                ReloadTimer += deltaTime;
                if (ReloadTimer >= 1f / FireRate)
                {
                    ReloadTimer = 0f;
                    Shoot(projectiles);
                }
            }
        }

        private Enemy SelectTarget(List<Enemy> enemies)
        {
            Enemy bestTarget = null;
            float bestPriority = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsActive) continue;

                float enemyX = enemy.X;
                float enemyY = enemy.Y;
                float towerX = X * 50 + 25;
                float towerY = Y * 50 + 25;

                float dx = enemyX - towerX;
                float dy = enemyY - towerY;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                if (distance <= Range)
                {
                    float priority = 0f;

                    switch (TargetingMode)
                    {
                        case TargetingMode.First:
                            priority = -enemy.PathIndex; // Ближайший к базе
                            break;
                        case TargetingMode.Last:
                            priority = enemy.PathIndex; // Самый далёкий от базы
                            break;
                        case TargetingMode.Strong:
                            priority = -enemy.Health; // Самое здоровье
                            break;
                        case TargetingMode.Weak:
                            priority = enemy.Health; // Самое слабое
                            break;
                        case TargetingMode.Close:
                            priority = distance; // Ближайший
                            break;
                        case TargetingMode.Far:
                            priority = -distance; // Самый далёкий
                            break;
                    }

                    if (priority < bestPriority)
                    {
                        bestPriority = priority;
                        bestTarget = enemy;
                    }
                }
            }

            return bestTarget;
        }

        private void Shoot(List<Projectile> projectiles)
        {
            if (Target != null && Target.IsActive)
            {
                float towerX = X * 50 + 25;
                float towerY = Y * 50 + 25;
                projectiles.Add(new Projectile(towerX, towerY, Target, Damage));
            }
        }

        public void Draw(Camera camera, FontInstance font)
        {
            if (!IsActive) return;

            float screenX = (X * 50 + 25 - camera.X) * camera.Zoom;
            float screenY = (Y * 50 + 25 - camera.Y) * camera.Zoom;
            float size = 20 * camera.Zoom;

            // Основание башни
            Primitives.DrawQuad(
                screenX - size,
                screenY - size,
                size * 2,
                size * 2,
                0.5f, 0.5f, 0.5f, 1f);

            // Орудие (линия)
            float barrelLength = 28 * camera.Zoom;
            float endX = screenX + (float)Math.Cos(Rotation) * barrelLength;
            float endY = screenY + (float)Math.Sin(Rotation) * barrelLength;

            Primitives.DrawLine(screenX, screenY, endX, endY, 1 * camera.Zoom, 0.3f, 0.3f, 0.8f, 1f);

            // Круг башни
            Primitives.DrawCircle(screenX - size/4f, screenY - size/4f, size/2f, 0.4f, 0.4f, 0.6f, 1f, 16);

            // Радиус действия (если выбрано меню)
            if (ShowMenu)
            {
                float rangeScreen = Range * camera.Zoom;
                Primitives.DrawCircle(screenX-rangeScreen/2, screenY - rangeScreen / 2, rangeScreen, 0.8f, 0.8f, 0.2f, 0.3f, 32);
            }

            // Меню башни
            if (ShowMenu)
            {
                DrawMenu(camera, font);
            }
        }

        private void DrawMenu(Camera camera, FontInstance font)
        {
            float menuWidth = 250;
            float menuHeight = 700;
            float menuX = 1350;
            float menuY = 100;

            // Фон меню
            Primitives.DrawQuad(menuX, menuY, menuWidth, menuHeight, 0.1f, 0.1f, 0.15f, 0.9f);
            Primitives.DrawLine(menuX, menuY, menuX + menuWidth, menuY, 2, 0.5f, 0.5f, 0.8f, 1f);
            Primitives.DrawLine(menuX + menuWidth, menuY, menuX + menuWidth, menuY + menuHeight, 2, 0.5f, 0.5f, 0.8f, 1f);
            Primitives.DrawLine(menuX + menuWidth, menuY + menuHeight, menuX, menuY + menuHeight, 2, 0.5f, 0.5f, 0.8f, 1f);
            Primitives.DrawLine(menuX, menuY + menuHeight, menuX, menuY, 2, 0.5f, 0.5f, 0.8f, 1f);

            // Заголовок
            font.DrawText("Tower Menu", menuX + 10, menuY + 10, 0.5f, 1f, 1f, 1f);

            // Статистика
            font.DrawText($"Damage: {Damage}", menuX + 10, menuY + 50, 0.4f, 0.8f, 0.8f, 0.8f);
            font.DrawText($"Range: {Range}", menuX + 10, menuY + 80, 0.4f, 0.8f, 0.8f, 0.8f);
            font.DrawText($"Fire Rate: {FireRate:F1}/s", menuX + 10, menuY + 110, 0.4f, 0.8f, 0.8f, 0.8f);

            // Выбор цели
            font.DrawText("Targeting:", menuX + 10, menuY + 160, 0.45f, 1f, 1f, 0.5f);

            string[] modes = new string[] { "First", "Last", "Strong", "Weak", "Close", "Far" };
            for (int i = 0; i < modes.Length; i++)
            {
                float y = menuY + 190 + i * 25;
                bool isSelected = (int)TargetingMode == i;

                if (isSelected)
                {
                    Primitives.DrawQuad(menuX + 10, y - 2, menuWidth - 20, 18, 0.3f, 0.3f, 0.5f, 0.5f);
                    font.DrawText(modes[i], menuX + 15, y, 0.35f, 1f, 1f, 1f);
                }
                else
                {
                    font.DrawText(modes[i], menuX + 15, y, 0.35f, 0.7f, 0.7f, 0.7f);
                }
            }

            // Подсказка
            font.DrawText("Click mode to change", menuX + 10, menuY + 280, 0.3f, 0.5f, 0.5f, 0.5f);
        }

        public bool IsMouseOver(float mouseX, float mouseY)
        {
            float towerX = X * 50 + 25;
            float towerY = Y * 50 + 25;
            float dx = mouseX - towerX;
            float dy = mouseY - towerY;
            return Math.Sqrt(dx * dx + dy * dy) < 25;
        }

        public bool IsMenuClicked(float mouseX, float mouseY)
        {
            if (!ShowMenu) return false;

            float menuWidth = 250;
            float menuHeight = 700;
            float menuX = 1350;
            float menuY = 50;

            return mouseX >= menuX && mouseX <= menuX + menuWidth &&
                   mouseY >= menuY && mouseY <= menuY + menuHeight;
        }

        public void ToggleMenu()
        {
            ShowMenu = !ShowMenu;
        }

        public void SetTargetingMode(int mode)
        {
            if (mode >= 0 && mode <= 5)
            {
                TargetingMode = (TargetingMode)mode;
            }
        }
    }

    public enum TargetingMode
    {
        First,    // Ближайший к базе
        Last,     // Самый далёкий от базы
        Strong,   // Самое здоровье
        Weak,     // Самое слабое
        Close,    // Ближайший к башне
        Far       // Самый далёкий от башни
    }
}