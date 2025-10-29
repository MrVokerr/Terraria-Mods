using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using ExileTree.Content.UI;
using ExileTree.Content.Systems;

namespace ExileTree.Content.Systems
{
    // Creates, updates, and draws the Passive Tree UI
    public class ExileTreeUISystem : ModSystem
    {
        private static UserInterface _ui;
        private static PassiveTreeUI _treeState;
        public static bool Visible { get; private set; }

        public override void Load() {
            if (Main.dedServ)
                return;

            // Ensure the tree data is available as soon as the mod loads
            PassiveTreeSystem.LoadTree();

            _ui = new UserInterface();
            _treeState = new PassiveTreeUI();
            _treeState.Activate();
        }

        public override void Unload() {
            _ui = null;
            _treeState = null;
        }

        public override void UpdateUI(GameTime gameTime) {
            if (Visible && _ui?.CurrentState != null) {
                _ui.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1) {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "ExileTree: PassiveTreeUI",
                    delegate {
                        if (Visible) {
                            _ui.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }

        public static void Toggle() {
            if (_ui == null || _treeState == null)
                return;

            // Safety: (re)load the tree if it was never loaded or got cleared
            if (PassiveTreeSystem.AllNodes == null || PassiveTreeSystem.AllNodes.Count == 0) {
                PassiveTreeSystem.LoadTree();
            }

            Visible = !Visible;
            _ui.SetState(Visible ? _treeState : null);

            // When opening, force the UI to rebuild its buttons from the tree data
            if (Visible) {
                _treeState.OnOpen();
            }
        }

        // Force refresh the UI if it's currently open
        public static void RefreshIfOpen() {
            if (Visible && _treeState != null) {
                _treeState.ForceRefresh();
            }
        }
    }
}
