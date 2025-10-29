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

            int currentRank = player.nodeRanks.TryGetValue(_nodeId, out var rank) ? rank : 0;
            bool unlocked = currentRank > 0;
            bool canUnlock = PassiveTreeSystem.CanUnlock(_nodeId, player.unlockedNodes, player.nodeRanks) && 
                           player.skillPoints >= node.SkillPointCost &&
                           currentRank < node.MaxRank;

            Color borderColor = unlocked ? new Color(255, 255, 255, 230) : (canUnlock ? Color.Goldenrod : Color.Gray);
            Color tint = unlocked ? Color.White : (canUnlock ? Color.LightGray : Color.DimGray);

            int size = node.IsMajor ? 32 : 21;
            var nodeRect = new Rectangle((int)pos.X, (int)pos.Y, size, size);
            
            float scaledSize = size * _zoom;
            Width.Set(scaledSize, 0f);
            Height.Set(scaledSize, 0f);

            if (!string.IsNullOrEmpty(node.IconPath) && ModContent.HasAsset(node.IconPath))
            {
                Texture2D iconTexture = ModContent.Request<Texture2D>(node.IconPath).Value;
                float scale = (node.IsMajor ? 1f : 0.644f) * _zoom;
                Vector2 iconPosition = pos + new Vector2(size / 2f);
                
                Color iconTint = unlocked ? Color.White * 1.25f : tint;
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
                        scale * 1f,
                        SpriteEffects.None,
                        0f
                    );
                }
            }

            // Draw rank display for multi-rank nodes
            if (node.MaxRank > 1)
            {
                string rankText = $"{currentRank}/{node.MaxRank}";
                Vector2 textSize = FontAssets.MouseText.Value.MeasureString(rankText);
                Vector2 textPos = pos + new Vector2(size / 2f - textSize.X / 2f, size + 2);
                Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, rankText, 
                    textPos.X, textPos.Y, unlocked ? Color.White : Color.Gray, Color.Black, Vector2.Zero, 0.7f);
            }

            if (IsMouseHovering)
            {
                string costText = node.SkillPointCost > 1 ? $" (Cost: {node.SkillPointCost})" : "";
                string rankInfo = node.MaxRank > 1 ? $" [{currentRank}/{node.MaxRank}]" : "";
                
                // Add tree investment progress for major nodes (capstones)
                string treeProgressText = "";
                if (node.IsMajor && !unlocked)
                {
                    int pointsSpentInTree = CalculateTreeInvestment(_nodeId, player);
                    treeProgressText = $"\nTree Investment: {pointsSpentInTree}/6";
                }
                
                string state = unlocked && currentRank >= node.MaxRank
                    ? "Max Rank"
                    : (canUnlock ? $"Click to unlock{costText} (Points: {player.skillPoints})" : "Locked");

                Main.instance.MouseText($"{node.Name}{rankInfo}\n{node.Description}\n{state}{treeProgressText}");
            }
        }

        private int CalculateTreeInvestment(string nodeId, ExileTreePlayer player)
        {
            int pointsSpentInTree = 0;
            string treeIdentifier = "";
            
            if (nodeId.Contains("Melee")) treeIdentifier = "Melee";
            else if (nodeId.Contains("Magic")) treeIdentifier = "Magic";
            else if (nodeId.Contains("Ranged") || nodeId.Contains("Move")) treeIdentifier = "Ranged";
            else if (nodeId.Contains("Summon")) treeIdentifier = "Summon";
            else if (nodeId.Contains("DR") || nodeId.Contains("Life") || nodeId.Contains("Regen")) treeIdentifier = "Health";
            
            foreach (var kvp in player.nodeRanks)
            {
                if (PassiveTreeSystem.AllNodes.TryGetValue(kvp.Key, out var treeNode) && !treeNode.IsMajor)
                {
                    bool inSameTree = false;
                    if (treeIdentifier == "Melee" && kvp.Key.Contains("Melee")) inSameTree = true;
                    else if (treeIdentifier == "Magic" && kvp.Key.Contains("Magic")) inSameTree = true;
                    else if (treeIdentifier == "Ranged" && (kvp.Key.Contains("Ranged") || kvp.Key.Contains("Move"))) inSameTree = true;
                    else if (treeIdentifier == "Summon" && kvp.Key.Contains("Summon")) inSameTree = true;
                    else if (treeIdentifier == "Health" && (kvp.Key.Contains("DR") || kvp.Key.Contains("Life") || kvp.Key.Contains("Regen"))) inSameTree = true;
                    
                    if (inSameTree)
                    {
                        pointsSpentInTree += kvp.Value * treeNode.SkillPointCost;
                    }
                }
            }
            
            return pointsSpentInTree;
        }

        // Handle left-click unlocking
        public override void LeftClick(UIMouseEvent evt)
        {
            var player = Main.LocalPlayer.GetModPlayer<ExileTreePlayer>();

            if (!PassiveTreeSystem.AllNodes.TryGetValue(_nodeId, out var node))
                return;

            int currentRank = player.nodeRanks.TryGetValue(_nodeId, out var rank) ? rank : 0;
            
            // Check if already at max rank
            if (currentRank >= node.MaxRank)
                return;
            
            // Check if can unlock (prerequisites met)
            if (!PassiveTreeSystem.CanUnlock(_nodeId, player.unlockedNodes, player.nodeRanks))
                return;
            
            // Check if player has enough skill points
            if (player.skillPoints < node.SkillPointCost)
                return;

            // Increase rank
            player.nodeRanks[_nodeId] = currentRank + 1;
            player.skillPoints -= node.SkillPointCost;
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
