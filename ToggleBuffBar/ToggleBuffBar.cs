using Terraria;
using Terraria.ModLoader;
using Terraria.GameInput;
using Microsoft.Xna.Framework.Input;

namespace ToggleBuffBar
{
    public class ToggleBuffBar : Mod
    {
        // Static toggle flag for hiding buffs
        public static bool HideAll = false;

        // Static keybind reference
        public static ModKeybind ToggleBuffsHotKey;

        public override void Load()
        {
            // Register the keybind with default key "B"
            ToggleBuffsHotKey = KeybindLoader.RegisterKeybind(this, "ToggleBuffs", "B");
        }

        public override void Unload()
        {
            // Clean up
            ToggleBuffsHotKey = null;
        }
    }

    public class BuffToggleSystem : ModSystem
    {
        public override void PostUpdateInput()
        {
            if (ToggleBuffBar.ToggleBuffsHotKey.JustPressed)
            {
                ToggleBuffBar.HideAll = !ToggleBuffBar.HideAll;

                string status = ToggleBuffBar.HideAll ? "hidden" : "visible";
                Main.NewText($"Buff UI is now {status}.", 255, 255, 0);
            }
        }
    }
}
