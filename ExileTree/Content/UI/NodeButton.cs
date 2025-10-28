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
        private float _zoom = 1f;
        private static Texture2D Pixel => TextureAssets.MagicPixel.Value;
        
        public string NodeId => _nodeId; // Public accessor for nodeId

        public NodeButton(string nodeId)
        {
            _nodeId = nodeId;
            
            // Set the initial size based on whether it's a major node
            if (PassiveTreeSystem.AllNodes.TryGetValue(nodeId, out var node))
            {
                float size = node.IsMajor ? 32f : 21f; // Major stays 32, Minor increased to 21 (15% bigger than 18)
                Width.Set(size, 0f);
                Height.Set(size, 0f);
            }
            else
            {
                Width.Set(21f, 0f); // Default to minor node size
                Height.Set(21f, 0f);
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var dims = GetDimensions();
            var pos = new Vector2(dims.X, dims.Y);
            var player = Main.LocalPlayer.GetModPlayer<ExileTreePlayer>();

            if (!PassiveTreeSystem.AllNodes.TryGetValue(_nodeId, out var node))
                return;

            bool unlocked = player.unlockedNodes.Contains(_nodeId);
            bool canUnlock = PassiveTreeSystem.CanUnlock(_nodeId, player.unlockedNodes) && player.skillPoints > 0;

            Color borderColor = unlocked ? new Color(255, 255, 255, 230) : (canUnlock ? Color.Goldenrod : Color.Gray); // Almost solid white for allocated nodes
            Color tint = unlocked ? Color.White : (canUnlock ? Color.LightGray : Color.DimGray);

            int size = node.IsMajor ? 32 : 21; // Major stays 32, Minor increased to 21
            var nodeRect = new Rectangle((int)pos.X, (int)pos.Y, size, size);
            
            // Update the UI element's size to match (ensures hitbox is correct) and apply zoom
            float scaledSize = size * _zoom;
            Width.Set(scaledSize, 0f);
            Height.Set(scaledSize, 0f);

            // We no longer draw the background or borders since we have proper icons for all nodes
            // The hitbox is maintained by the nodeRect and Width/Height settings above

            // Only draw the icon if we have a valid asset
            if (!string.IsNullOrEmpty(node.IconPath) && ModContent.HasAsset(node.IconPath))
            {
                Texture2D iconTexture = ModContent.Request<Texture2D>(node.IconPath).Value;
                float scale = (node.IsMajor ? 1f : 0.644f) * _zoom; // Apply zoom to the base scale
                Vector2 iconPosition = pos + new Vector2(size / 2f);
                
                // Draw the icon centered with enhanced brightness for allocated nodes
                Color iconTint = unlocked ? Color.White * 1.25f : tint; // Make allocated nodes 25% brighter
                spriteBatch.Draw(
                    iconTexture, 
                    iconPosition, 
                    null, 
                    iconTint, 
                    0f, 
                    new Vector2(iconTexture.Width / 2f, iconTexture.Height / 2f),
                    scale, 
                    SpriteEffects.None, 
                    0f
                );

                // Add a subtle pulsing glow effect for unlocked nodes
                if (unlocked)
                {
                    float pulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f;
                    spriteBatch.Draw(
                        iconTexture,
                        iconPosition,
                        null,
                        Color.White * 0.4f * pulse,
                        0f,
                        new Vector2(iconTexture.Width / 2f, iconTexture.Height / 2f),
                        scale * 1f, // Slightly larger for glow effect
                        SpriteEffects.None,
                        0f
                    );
                }
            }

            // --- Tooltip ---
            if (IsMouseHovering)
            {
                string state = unlocked
                    ? "Unlocked"
                    : (canUnlock ? $"Click to unlock (Points: {player.skillPoints})" : "Locked");

                Main.instance.MouseText($"{node.Name}\n{node.Description}\n{state}");
            }
        }

        // Handle left-click unlocking
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

        private void DrawOutline(SpriteBatch sb, Rectangle rect, Color color)
        {
            sb.Draw(Pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), color);
            sb.Draw(Pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), color);
            sb.Draw(Pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), color);
            sb.Draw(Pixel, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), color);
        }

        public void SetZoom(float zoom)
        {
            _zoom = zoom;
            if (PassiveTreeSystem.AllNodes.TryGetValue(_nodeId, out var node))
            {
                float size = node.IsMajor ? 32f : 21f;
                float scaledSize = size * _zoom;
                Width.Set(scaledSize, 0f);
                Height.Set(scaledSize, 0f);
            }
        }
    }
}
