using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria;
using Terraria.UI;
using Terraria.GameInput;
using ExileTree.Content.Players;
using ExileTree.Content.Systems;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace ExileTree.Content.UI
{
    public class PassiveTreeUI : UIState
    {
        private UIPanel _window;         // The draggable window
        private UIElement _container;    // Container for the tree
        private bool _dragging;
        private Vector2 _dragStart;
        private Vector2 _offset;         // Pan offset
        private const float _zoom = 1f;  // Fixed zoom level

        public override void OnInitialize()
        {
            // Calculate the bounds of all nodes to determine window size
            Vector2 minPos = new Vector2(float.MaxValue);
            Vector2 maxPos = new Vector2(float.MinValue);
            foreach (var node in PassiveTreeSystem.AllNodes.Values)
            {
                minPos = Vector2.Min(minPos, node.Position);
                maxPos = Vector2.Max(maxPos, node.Position);
            }

            // Calculate tree dimensions
            Vector2 treeSize = maxPos - minPos;
            const float minPadding = 150f; // Minimum padding around the tree
            
            // Create the main window
            _window = new UIPanel();
            
            // Calculate the aspect ratio of the tree
            float treeAspect = treeSize.X / treeSize.Y;
            float screenAspect = (float)Main.screenWidth / Main.screenHeight;
            
            // Start with a base size that's 70% of the screen height
            float baseHeight = Main.screenHeight * 0.7f;
            float baseWidth = baseHeight * treeAspect;
            
            // If width is too wide, scale based on width instead
            if (baseWidth > Main.screenWidth * 0.7f)
            {
                baseWidth = Main.screenWidth * 0.7f;
                baseHeight = baseWidth / treeAspect;
            }
            
            // Calculate final percentages with minimum padding
            float widthPercent = (baseWidth + minPadding * 2) / Main.screenWidth;
            float heightPercent = (baseHeight + minPadding * 2) / Main.screenHeight;
            
            // Ensure we don't exceed 85% of screen size
            widthPercent = Math.Min(widthPercent, 0.85f);
            heightPercent = Math.Min(heightPercent, 0.85f);
            
            _window.Width.Set(0, widthPercent);
            _window.Height.Set(0, heightPercent);
            
            // Center the window
            _window.Left.Set(0, (1f - widthPercent) / 2);
            _window.Top.Set(0, (1f - heightPercent) / 2);
            
            _window.SetPadding(6);
            Append(_window);

            // Create the container for the tree immediately after window setup

            // Create the container for the tree
            _container = new UIElement();
            _container.Width.Set(-20, 1f);    // Full width minus padding
            _container.Height.Set(-20, 1f);   // Full height minus padding
            _container.Left.Set(10, 0f);      // Add padding from edges
            _container.Top.Set(10, 0f);       // Add padding from edges
            _container.SetPadding(0);
            _container.OverflowHidden = true; // Clip content to container bounds
            _window.Append(_container);

            BuildNodes();
        }

        public void OnOpen()
        {
            int count = PassiveTreeSystem.AllNodes?.Count ?? 0;
            if (count == 0)
                PassiveTreeSystem.LoadTree();

            // Reset view
            _offset = Vector2.Zero;  // Reset panning offset
            
            Rebuild();
        }

        private void Rebuild()
        {
            _container.RemoveAllChildren();
            BuildNodes();
        }

        private void BuildNodes()
        {
            if (PassiveTreeSystem.AllNodes == null || PassiveTreeSystem.AllNodes.Count == 0)
                return;

            _container.RemoveAllChildren();

            var dims = _container.GetDimensions();
            float centerX = dims.Width * 0.5f;
            float centerY = dims.Height * 0.5f;

            // Calculate the bounds of all nodes
            Vector2 minPos = new Vector2(float.MaxValue);
            Vector2 maxPos = new Vector2(float.MinValue);
            foreach (var node in PassiveTreeSystem.AllNodes.Values)
            {
                minPos = Vector2.Min(minPos, node.Position);
                maxPos = Vector2.Max(maxPos, node.Position);
            }

            // Calculate the center of all nodes
            Vector2 treeCenter = (minPos + maxPos) * 0.5f;

            // Position all nodes relative to container center
            foreach (var node in PassiveTreeSystem.AllNodes.Values)
            {
                var btn = new NodeButton(node.ID);
                Vector2 centeredPos = (node.Position - treeCenter) * _zoom;
                btn.Left.Set(centerX + centeredPos.X + _offset.X, 0f);
                btn.Top.Set(centerY + centeredPos.Y + _offset.Y, 0f);
                btn.SetZoom(_zoom);
                _container.Append(btn);
            }

            // Position all nodes relative to the window center
            foreach (var node in PassiveTreeSystem.AllNodes.Values)
            {
                var btn = new NodeButton(node.ID);
                // Center the node relative to both window and tree center
                btn.Left.Set(centerX + (node.Position.X - treeCenter.X) * _zoom, 0f);
                btn.Top.Set(centerY + (node.Position.Y - treeCenter.Y) * _zoom, 0f);
                _container.Append(btn);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_window.IsMouseHovering)
            {
                if (Main.mouseMiddle)
                {
                    if (!_dragging)
                    {
                        _dragging = true;
                        _dragStart = Main.MouseScreen;
                    }
                    else
                    {
                        Vector2 delta = Main.MouseScreen - _dragStart;
                        
                        // Calculate new offset
                        Vector2 newOffset = _offset + delta;
                        
                        // Get window bounds
                        var dims = _window.GetDimensions();
                        float maxOffsetX = dims.Width * 0.5f;
                        float maxOffsetY = dims.Height * 0.5f;
                        
                        // Confine to window bounds
                        newOffset.X = Math.Clamp(newOffset.X, -maxOffsetX, maxOffsetX);
                        newOffset.Y = Math.Clamp(newOffset.Y, -maxOffsetY, maxOffsetY);
                        
                        _offset = newOffset;
                        _dragStart = Main.MouseScreen;
                    }
                }
                else
                {
                    _dragging = false;
                }
            }

            // Update node positions based on zoom and offset
            foreach (UIElement element in _container.Children)
            {
                if (element is NodeButton btn)
                {
                    if (PassiveTreeSystem.AllNodes.TryGetValue(btn.NodeId, out var node))
                    {
                        var dims = _container.GetDimensions();
                        float centerX = dims.Width * 0.5f;
                        float centerY = dims.Height * 0.5f;

                        // Calculate the bounds and center of all nodes
                        Vector2 minPos = new Vector2(float.MaxValue);
                        Vector2 maxPos = new Vector2(float.MinValue);
                        foreach (var n in PassiveTreeSystem.AllNodes.Values)
                        {
                            minPos = Vector2.Min(minPos, n.Position);
                            maxPos = Vector2.Max(maxPos, n.Position);
                        }
                        Vector2 treeCenter = (minPos + maxPos) * 0.5f;

                        // Position relative to container center
                        Vector2 relativePos = node.Position - treeCenter;
                        btn.Left.Set(centerX + relativePos.X + _offset.X, 0f);
                        btn.Top.Set(centerY + relativePos.Y + _offset.Y, 0f);
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            var dims = _window.GetDimensions();
            var p = Main.LocalPlayer.GetModPlayer<ExileTreePlayer>();
            
            // Draw window title with skill points
            string title = $"Exile Tree — Skill Points: {p.skillPoints}";
            Vector2 pos = new Vector2(dims.X + dims.Width * 0.5f, dims.Y - 20f);
            Vector2 measurement = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(title);
            Utils.DrawBorderString(spriteBatch, title, pos - new Vector2(measurement.X * 0.5f, 0), Color.White);
        }
    }
}
