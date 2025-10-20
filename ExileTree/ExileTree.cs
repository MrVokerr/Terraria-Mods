using Terraria.ModLoader;
using Terraria.GameInput;

namespace ExileTree
{
    public class ExileTree : Mod
    {
        public static ModKeybind ToggleTreeKeybind;

        public override void Load() {
            ToggleTreeKeybind = KeybindLoader.RegisterKeybind(this, "Toggle Passive Tree", "P");
        }

        public override void Unload() {
            ToggleTreeKeybind = null;
        }
    }
}
