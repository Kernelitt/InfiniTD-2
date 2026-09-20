using InfiniTD_2.Framework;
using InfiniTD_2.Framework.Audio;
using System;
using System.Collections.Generic;

namespace InfiniTD_2.GameRelated
{
    [Serializable]
    public abstract class Tower
    {
        public short X { get; set; }
        public short Y { get; set; }
        public int Cost { get; set; }
        public float Range { get; set; }
        public float Damage { get; set; }
        public float FireRate { get; set; }
        public float Rotation { get; set; } = 0f;
        public float TargetRotation { get; set; } = 0f;
        public float RotationSpeed { get; set; } = 3f;
        public float ReloadTimer { get; set; } = 0f;
        public Enemy Target { get; set; } = null;
        public bool IsActive { get; set; } = true;
        public TargetingMode TargetingMode { get; set; } = TargetingMode.First;
        public bool ShowMenu { get; set; } = false;
        public int Level { get; set; } = 1;
        public int UpgradeCost { get; set; }
        public bool CanUpgrade { get; set; } = true;
        public int MaxLevel { get; set; } = 10;

        public float BaseR { get; set; } = 0.5f;
        public float BaseG { get; set; } = 0.5f;
        public float BaseB { get; set; } = 0.5f;

        public abstract string TowerName { get; }

        [NonSerialized] public UIButton upgradeButton;
        [NonSerialized] public List<UIButton> targetingButtons;

        protected Tower(short x, short y, int cost, float range, float damage, float fireRate, int upgradeCost)
        {
            X = x;
            Y = y;
            Cost = cost;
            Range = range;
            Damage = damage;
            FireRate = fireRate;
            UpgradeCost = upgradeCost;

            InitializeButtons();
        }

        protected virtual void InitializeButtons()
        {
            float menuX = 1250;
            float menuY = 0;

            // Кнопка улучшения
            upgradeButton = new UIButton(menuX + 10, menuY + 140, 330, 30, "Upgrade")
            {
                R = 0.2f,
                G = 0.6f,
                B = 0.2f,
                HoverR = 0.3f,
                HoverG = 0.8f,
                HoverB = 0.3f,
                OnClick = () => OnUpgradeClicked()
            };

            // Кнопки выбора цели
            targetingButtons = new List<UIButton>
            {
                new UIButton(menuX + 10, menuY + 230, 330, 20, "First") { OnClick = () => SetTargetingMode(0) },
                new UIButton(menuX + 10, menuY + 255, 330, 20, "Last") { OnClick = () => SetTargetingMode(1) },
                new UIButton(menuX + 10, menuY + 280, 330, 20, "Strong") { OnClick = () => SetTargetingMode(2) },
                new UIButton(menuX + 10, menuY + 305, 330, 20, "Weak") { OnClick = () => SetTargetingMode(3) },
                new UIButton(menuX + 10, menuY + 330, 330, 20, "Close") { OnClick = () => SetTargetingMode(4) },
                new UIButton(menuX + 10, menuY + 355, 330, 20, "Far") { OnClick = () => SetTargetingMode(5) }
            };
        }

        protected virtual void OnUpgradeClicked()
        {
            // Будет вызвано из MainGame
        }

        public virtual void UpdateButtons()
        {
            if (upgradeButton != null)
            {
                upgradeButton.Update();
            }

            if (targetingButtons != null)
            {
                foreach (var btn in targetingButtons)
                {
                    btn.Update();
                }
            }
        }

        public virtual void Update(List<Enemy> enemies, float deltaTime, List<Projectile> projectiles)
        {
            if (!IsActive) return;

            Target = SelectTarget(enemies);

            if (Target != null && Target.IsActive)
            {
                float targetX = Target.X;
                float targetY = Target.Y;
                float towerX = X * 50 + 25;
                float towerY = Y * 50 + 25;

                float dx = targetX - towerX;
                float dy = targetY - towerY;
                TargetRotation = (float)Math.Atan2(dy, dx);

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

                ReloadTimer += deltaTime;
                if (ReloadTimer >= 1f / FireRate)
                {
                    ReloadTimer = 0f;
                    Shoot(projectiles);
                }
            }
        }

        protected virtual Enemy SelectTarget(List<Enemy> enemies)
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
                            priority = -enemy.PathIndex;
                            break;
                        case TargetingMode.Last:
                            priority = enemy.PathIndex;
                            break;
                        case TargetingMode.Strong:
                            priority = -enemy.Health;
                            break;
                        case TargetingMode.Weak:
                            priority = enemy.Health;
                            break;
                        case TargetingMode.Close:
                            priority = distance;
                            break;
                        case TargetingMode.Far:
                            priority = -distance;
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

        protected virtual void Shoot(List<Projectile> projectiles)
        {
            if (Target != null && Target.IsActive)
            {
                float towerX = X * 50 + 25;
                float towerY = Y * 50 + 25;
                float endX = towerX + (float)Math.Cos(Rotation) * 28;
                float endY = towerY + (float)Math.Sin(Rotation) * 28;
                projectiles.Add(new Projectile(endX, endY, Target, Damage));
                MusicPlayer.PlayShootSound();
            }
        }

        public virtual void Draw(Camera camera, FontInstance font)
        {
            if (!IsActive) return;

            float screenX = (X * 50 + 25 - camera.X) * camera.Zoom;
            float screenY = (Y * 50 + 25 - camera.Y) * camera.Zoom;
            float size = 20 * camera.Zoom;

            Primitives.DrawQuad(screenX - size, screenY - size, size * 2, size * 2, BaseR, BaseG, BaseB, 1f);

            float barrelLength = 28 * camera.Zoom;
            float endX = screenX + (float)Math.Cos(Rotation) * barrelLength;
            float endY = screenY + (float)Math.Sin(Rotation) * barrelLength;

            Primitives.DrawLine(screenX, screenY, endX, endY, 4 * camera.Zoom, BaseR * 0.6f, BaseG * 0.6f, BaseB * 1.2f, 1f);

            Primitives.DrawCircle(screenX - size / 4f, screenY - size / 4f, size / 2f, BaseR * 0.8f, BaseG * 0.8f, BaseB * 1.2f, 1f, 16);

            if (ShowMenu)
            {
                float rangeScreen = Range * camera.Zoom;
                Primitives.DrawCircle(screenX - rangeScreen / 2, screenY - rangeScreen / 2, rangeScreen, 0.8f, 0.8f, 0.2f, 0.3f, 32);
            }

            if (ShowMenu)
            {
                DrawMenu(font);
            }

            if (Level > 1)
            {
                font.DrawText($"Lv.{Level}", screenX - 15 * camera.Zoom, screenY - 25 * camera.Zoom, 0.7f * camera.Zoom, 1f, 1f, 0.3f);
            }
        }

        protected virtual void DrawMenu(FontInstance font)
        {
            float menuWidth = 350;
            float menuHeight = 900;
            float menuX = 1250;
            float menuY = 0;

            Primitives.DrawQuad(menuX, menuY, menuWidth, menuHeight, 0.1f, 0.1f, 0.15f, 0.9f);
            Primitives.DrawLine(menuX, menuY, menuX + menuWidth, menuY, 2, 0.5f, 0.5f, 0.8f, 1f);
            Primitives.DrawLine(menuX + menuWidth, menuY, menuX + menuWidth, menuY + menuHeight, 2, 0.5f, 0.5f, 0.8f, 1f);
            Primitives.DrawLine(menuX + menuWidth, menuY + menuHeight, menuX, menuY + menuHeight, 2, 0.5f, 0.5f, 0.8f, 1f);
            Primitives.DrawLine(menuX, menuY + menuHeight, menuX, menuY, 2, 0.5f, 0.5f, 0.8f, 1f);

            font.DrawText($"{TowerName} (Lv.{Level}/{MaxLevel})", menuX + 10, menuY + 10, 1.2f, 1f, 1f, 1f);

            font.DrawText($"Damage: {Damage:F0}", menuX + 10, menuY + 50, 0.9f, 0.8f, 0.8f, 0.8f);
            font.DrawText($"Range: {Range:F0}", menuX + 10, menuY + 80, 0.9f, 0.8f, 0.8f, 0.8f);
            font.DrawText($"Fire Rate: {FireRate:F1}/s", menuX + 10, menuY + 110, 0.9f, 0.8f, 0.8f, 0.8f);

            // Обновление текста кнопки улучшения
            if (CanUpgrade)
            {
                upgradeButton.Text = $"Upgrade - ${UpgradeCost}";
                upgradeButton.R = 0.2f; upgradeButton.G = 0.6f; upgradeButton.B = 0.2f;
                upgradeButton.HoverR = 0.3f; upgradeButton.HoverG = 0.8f; upgradeButton.HoverB = 0.3f;
            }
            else
            {
                upgradeButton.Text = "MAX LEVEL";
                upgradeButton.R = 0.4f; upgradeButton.G = 0.4f; upgradeButton.B = 0.4f;
                upgradeButton.HoverR = 0.4f; upgradeButton.HoverG = 0.4f; upgradeButton.HoverB = 0.4f;
            }

            if (upgradeButton != null)
            {
                upgradeButton.Draw(font);
            }

            font.DrawText("Targeting:", menuX + 10, menuY + 210, 0.8f, 1f, 1f, 0.5f);

            if (targetingButtons != null)
            {
                for (int i = 0; i < targetingButtons.Count; i++)
                {
                    var btn = targetingButtons[i];
                    bool isSelected = (int)TargetingMode == i;

                    if (isSelected)
                    {
                        btn.R = 0.3f; btn.G = 0.3f; btn.B = 0.5f;
                        btn.HoverR = 0.4f; btn.HoverG = 0.4f; btn.HoverB = 0.7f;
                    }
                    else
                    {
                        btn.R = 0.2f; btn.G = 0.2f; btn.B = 0.2f;
                        btn.HoverR = 0.3f; btn.HoverG = 0.3f; btn.HoverB = 0.3f;
                    }

                    btn.Draw(font);
                }
            }
        }

        public virtual bool Upgrade()
        {
            if (Level >= MaxLevel || !CanUpgrade) return false;

            Level++;
            Damage *= 1.3f;
            Range *= 1.1f;
            FireRate *= 1.05f;
            UpgradeCost = (int)(UpgradeCost * 1.5f);

            if (Level >= MaxLevel)
            {
                CanUpgrade = false;
                UpgradeCost = 0;
            }

            return true;
        }

        public virtual bool IsMouseOver(float mouseX, float mouseY)
        {
            float towerX = X * 50 + 25;
            float towerY = Y * 50 + 25;
            float dx = mouseX - towerX;
            float dy = mouseY - towerY;
            return Math.Sqrt(dx * dx + dy * dy) < 25;
        }

        public virtual bool IsMenuClicked(float mouseX, float mouseY)
        {
            if (!ShowMenu) return false;

            float menuWidth = 250;
            float menuHeight = 700;
            float menuX = 1350;
            float menuY = 50;

            return mouseX >= menuX && mouseX <= menuX + menuWidth &&
                   mouseY >= menuY && mouseY <= menuY + menuHeight;
        }

        public virtual void ToggleMenu()
        {
            ShowMenu = !ShowMenu;
        }

        public virtual void SetTargetingMode(int mode)
        {
            if (mode >= 0 && mode <= 5)
            {
                TargetingMode = (TargetingMode)mode;
            }
        }
    }

    [Serializable]
    public class BasicTower : Tower
    {
        public override string TowerName => "Basic Tower";

        public BasicTower(short x, short y) : base(x, y, 50, 150f, 20f, 1.5f, 50)
        {
            BaseR = 0.3f;
            BaseG = 0.2f;
            BaseB = 0.5f;
        }
    }

    [Serializable]
    public class SniperTower : Tower
    {
        public override string TowerName => "Sniper Tower";

        public SniperTower(short x, short y) : base(x, y, 120, 600f, 90f, 0.3f, 150)
        {
            BaseR = 0.2f;
            BaseG = 0.5f;
            BaseB = 0.3f;
            MaxLevel = 7;
            RotationSpeed = 1.5f;
        }

        public override void Draw(Camera camera, FontInstance font)
        {
            if (!IsActive) return;

            float screenX = (X * 50 + 25 - camera.X) * camera.Zoom;
            float screenY = (Y * 50 + 25 - camera.Y) * camera.Zoom;
            float size = 20 * camera.Zoom;

            // Основание башни
            Primitives.DrawTriangle(
                screenX - size, screenY + size,
                screenX, screenY - size,
                screenX + size, screenY + size,

                BaseR, BaseG, BaseB, 1f,
                BaseR, BaseG * 0.8f, BaseB, 1f,
                BaseR, BaseG * 0.9f, BaseB, 1f
                );

            // Орудие
            float barrelLength = 28 * camera.Zoom;
            float endX = screenX + (float)Math.Cos(Rotation) * barrelLength;
            float endY = screenY + (float)Math.Sin(Rotation) * barrelLength;
            Primitives.DrawLine(screenX, screenY, endX, endY, 4 * camera.Zoom, BaseR * 0.5f, BaseG * 0.5f, BaseB * 0.5f, 1f);
            if (Target != null && Target.IsActive)
            {
                float targetScreenX = (Target.X - camera.X) * camera.Zoom;
                float targetScreenY = (Target.Y - camera.Y) * camera.Zoom;
                Primitives.DrawLine(endX, endY, targetScreenX, targetScreenY, 1 * camera.Zoom, 1, 0, 0, 0.3f);
            }
            

            // Круг башни
            Primitives.DrawCircle(screenX - size / 6f, screenY - size / 6f, size / 3f, BaseR * 0.8f, BaseG * 0.8f, BaseB * 1.2f, 1f, 16);

            // Радиус действия
            if (ShowMenu)
            {
                float rangeScreen = Range * camera.Zoom;
                Primitives.DrawCircle(screenX - rangeScreen / 2, screenY - rangeScreen / 2, rangeScreen, 0.8f, 0.8f, 0.2f, 0.3f, 32);
            }

            // Меню
            if (ShowMenu)
            {
                DrawMenu(font);
            }

            // Уровень
            if (Level > 1)
            {
                font.DrawText($"Lv.{Level}", screenX - 15 * camera.Zoom, screenY - 25 * camera.Zoom, 0.7f * camera.Zoom, 1f, 1f, 0.3f);
            }
        }

        protected override void Shoot(List<Projectile> projectiles)
        {
            if (Target != null && Target.IsActive)
            {
                Target.TakeDamage(Damage);
                MusicPlayer.PlayShootSound();
            }
        }
    }

    [Serializable]
    public enum TargetingMode
    {
        First,
        Last,
        Strong,
        Weak,
        Close,
        Far
    }
}