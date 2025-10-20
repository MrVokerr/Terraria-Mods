using Terraria.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.GameContent;
using ExileTree.Content.Players;
using ExileTree.Content.Systems;

namespace ExileTree.Content.UI
{
    public class NodeButton : UIElement
    {
        private readonly string _nodeId;
        private static Texture2D Pixel => TextureAssets.MagicPixel.Value;

        // constructor (no texture needed)
        public NodeButton(string nodeId) {
            _nodeId = nodeId;
            Width.Set(24f, 0f);
            Height.Set(24f, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            var dims = GetDimensions();
            var pos = new Vector2(dims.X, dims.Y);

            var player = Main.LocalPlayer.GetModPlayer<ExileTreePlayer>();

            // Retrieve node data
            if (!PassiveTreeSystem.AllNodes.TryGetValue(_nodeId, out var node))
                return;

            bool unlocked = player.unlockedNodes.Contains(_nodeId);
            bool canUnlock = PassiveTreeSystem.CanUnlock(_nodeId, player.unlockedNodes) && player.skillPoints > 0;

            // color logic
            Color tint = unlocked ? Color.LimeGreen : (canUnlock ? Color.Goldenrod : Color.DimGray);

            // adjust size for major/minor
            int size = node.IsMajor ? 40 : 24;
            Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, size, size);

            // draw major as circle, minor as square
            if (node.IsMajor) {
                DrawCircle(spriteBatch, rect.Center.ToVector2(), size / 2f, tint);
            } else {
                spriteBatch.Draw(Pixel, rect, tint);
            }

            // outline (for all nodes)
            Color border = unlocked ? Color.White : Color.Black * 0.7f;
            DrawOutline(spriteBatch, rect, border, node.IsMajor);

            // tooltip
            if (IsMouseHovering) {
                string state = unlocked ? "Unlocked"
                    : (canUnlock ? $"Click to unlock (Points: {player.skillPoints})" : "Locked");

                Main.instance.MouseText($"{node.Name}\n{node.Description}\n{state}");
            }
        }

        public override void LeftClick(UIMouseEvent evt) {
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

        // -------- helper: draw outline --------
        private void DrawOutline(SpriteBatch sb, Rectangle rect, Color color, bool circle)
        {
            if (circle) {
                // draw circular border
                DrawCircle(sb, rect.Center.ToVector2(), rect.Width / 2f, Color.Transparent, color, 2);
            } else {
                // square border
                sb.Draw(Pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), color);
                sb.Draw(Pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), color);
                sb.Draw(Pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), color);
                sb.Draw(Pixel, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), color);
            }
        }

        // -------- helper: draw filled / outlined circle --------
        private void DrawCircle(SpriteBatch sb, Vector2 center, float radius, Color fill, Color? outline = null, int outlineThickness = 0)
        {
            Texture2D pixel = Pixel;
            int segments = 40;
            float increment = MathHelper.TwoPi / segments;
            Vector2[] points = new Vector2[segments + 1];
            for (int i = 0; i <= segments; i++) {
                float angle = i * increment;
                points[i] = center + radius * new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle));
            }

            // fill (approximate with radial quads)
            if (fill.A > 0) {
                for (int i = 0; i < segments; i++) {
                    DrawTriangle(sb, center, points[i], points[i + 1], fill);
                }
            }

            // outline (optional)
            if (outline.HasValue && outlineThickness > 0) {
                for (int i = 0; i < segments; i++) {
                    DrawLine(sb, points[i], points[i + 1], outline.Value, outlineThickness);
                }
            }
        }

        private void DrawTriangle(SpriteBatch sb, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[3];
            verts[0] = new VertexPositionColorTexture(new Vector3(p1, 0f), color, Vector2.Zero);
            verts[1] = new VertexPositionColorTexture(new Vector3(p2, 0f), color, Vector2.Zero);
            verts[2] = new VertexPositionColorTexture(new Vector3(p3, 0f), color, Vector2.Zero);
            // Terraria doesn't support triangle batches directly — simple approximation:
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
