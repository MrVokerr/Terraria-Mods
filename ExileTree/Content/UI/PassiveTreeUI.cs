using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using ExileTree.Content.Players;
using ExileTree.Content.Systems;

namespace ExileTree.Content.UI
{
    // Minimal, robust UI: builds clickable nodes that render as solid squares.
    public class PassiveTreeUI : UIState
    {
        private UIElement _root;

        public override void OnInitialize() {
            _root = new UIElement();
            _root.Left.Set(200f, 0f);   // window position
            _root.Top.Set(120f, 0f);
            _root.Width.Set(800f, 0f);  // drawing area
            _root.Height.Set(600f, 0f);
            _root.SetPadding(0);
            Append(_root);

            BuildNodes();
        }

        // Called by ExileTreeUISystem when showing the UI
        public void OnOpen() {
            // Debug: tell us how many nodes exist
            int count = PassiveTreeSystem.AllNodes?.Count ?? 0;
            Main.NewText($"ExileTree: Loaded {count} nodes.");
            if (count == 0)
                PassiveTreeSystem.LoadTree();

            Rebuild();
        }

        private void Rebuild() {
            _root.RemoveAllChildren();
            BuildNodes();
        }

        private void BuildNodes() {
            if (PassiveTreeSystem.AllNodes == null || PassiveTreeSystem.AllNodes.Count == 0)
                return;

            foreach (var node in PassiveTreeSystem.AllNodes.Values) {
                var btn = new NodeButton(node.ID); // iconless; draws solid square
                btn.Left.Set(node.Position.X, 0f);
                btn.Top.Set(node.Position.Y, 0f);
                _root.Append(btn);
            }
        }

        public override void Draw(SpriteBatch spriteBatch) {
            base.Draw(spriteBatch);

            // Small label for skill points (useful while testing)
            var p = Main.LocalPlayer.GetModPlayer<ExileTreePlayer>();
            var origin = new Vector2(_root.GetDimensions().X, _root.GetDimensions().Y);
            Utils.DrawBorderString(spriteBatch, $"Exile Tree — Skill Points: {p.skillPoints}",
                                   origin + new Vector2(0f, -24f), Color.White);
        }
    }
}
