using Terraria.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.GameContent;
using ExileTree.Content.Players;
using ExileTree.Content.Systems;
using Terraria.ModLoader;

namespace ExileTree.Content.UI
{
    public class NodeButton : UIElement
    {
        private readonly string _nodeId;
        private static Texture2D Pixel => TextureAssets.MagicPixel.Value;

        // Constructor
        public NodeButton(string nodeId)
        {
            _nodeId = nodeId;
            Width.Set(24f, 0f);
            Height.Set(24f, 0f);
        }

        // --------------------------------------------------
        // Draw node (simple color fill + outline)
        // --------------------------------------------------
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var dims = GetDimensions();
            var pos = new Vector2(dims.X, dims.Y);
            var player = Main.LocalPlayer.GetModPlayer<ExileTreePlayer>();

            if (!PassiveTreeSystem.AllNodes.TryGetValue(_nodeId, out var node))
                return;

            bool unlocked = player.unlockedNodes.Contains(_nodeId);
            bool canUnlock = PassiveTreeSystem.CanUnlock(_nodeId, player.unlockedNodes) && player.skillPoints > 0;

            // Base fill colors
            Color fillColor = unlocked
                ? Color.LimeGreen
                : (canUnlock ? Color.Goldenrod : Color.DimGray);

            Color borderColor = unlocked ? Color.White : Color.Black * 0.7f;

            int size = node.IsMajor ? 40 : 24;
            Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, size, size);

            // Draw filled shape
            if (node.IsMajor)
                DrawCircle(spriteBatch, rect.Center.ToVector2(), size / 2f, fillColor, borderColor, 2);
            else
                spriteBatch.Draw(Pixel, rect, fillColor);

            // Draw square border for non-major nodes
            if (!node.IsMajor)
                DrawOutline(spriteBatch, rect, borderColor, false);

            // Tooltip
            if (IsMouseHovering)
            {
                string state = unlocked
                    ? "Unlocked"
                    : (canUnlock ? $"Click to unlock (Points: {player.skillPoints})" : "Locked");

                Main.instance.MouseText($"{node.Name}\n{node.Description}\n{state}");
            }
        }

        // --------------------------------------------------
        // Handle left-click unlocking
        // --------------------------------------------------
        public override void LeftClick(UIMouseEvent evt)
        {
            var player = Main.LocalPlayer.GetModPlayer<ExileTreePlayer>();

            if (!PassiveTreeSystem.AllNodes.TryGetValue(_nodeId, out var node))
                return;

            if (player.unlockedNodes.Contains(_nodeId))
                return;
            if (!PassiveTreeSystem.CanUnlock(_nodeId, player.unlockedNodes))
                return;
            if (player.skillPoints <= 0)
                return;

            player.unlockedNodes.Add(_nodeId);
            player.skillPoints--;
            SoundEngine.PlaySound(SoundID.Unlock);
        }

        // --------------------------------------------------
        // Draw square border
        // --------------------------------------------------
        private void DrawOutline(SpriteBatch sb, Rectangle rect, Color color, bool circle)
        {
            sb.Draw(Pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), color);
            sb.Draw(Pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), color);
            sb.Draw(Pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), color);
            sb.Draw(Pixel, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), color);
        }

        // --------------------------------------------------
        // Draw circular node (for major passives)
        // --------------------------------------------------
        private void DrawCircle(SpriteBatch sb, Vector2 center, float radius, Color fill, Color outline, int outlineThickness = 2)
        {
            Texture2D pixel = Pixel;
            int segments = 40;
            float increment = MathHelper.TwoPi / segments;
            Vector2[] points = new Vector2[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * increment;
                points[i] = center + radius * new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle));
            }

            // Fill
            for (int i = 0; i < segments; i++)
                DrawTriangle(sb, center, points[i], points[i + 1], fill);

            // Outline
            for (int i = 0; i < segments; i++)
                DrawLine(sb, points[i], points[i + 1], outline, outlineThickness);
        }

        private void DrawTriangle(SpriteBatch sb, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            sb.Draw(Pixel, new Rectangle((int)p1.X, (int)p1.Y, 2, 2), color);
            sb.Draw(Pixel, new Rectangle((int)p2.X, (int)p2.Y, 2, 2), color);
            sb.Draw(Pixel, new Rectangle((int)p3.X, (int)p3.Y, 2, 2), color);
        }

        private void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, Color color, int thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)System.Math.Atan2(edge.Y, edge.X);
            sb.Draw(Pixel, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness), null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}
