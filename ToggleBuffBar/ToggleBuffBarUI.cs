using Terraria;
using Terraria.ModLoader;

namespace ToggleBuffBar
{
    public sealed class BuffUIHook : ModSystem
    {
        private static bool _hooked;

        public override bool IsLoadingEnabled(Mod mod) => !Main.dedServ; // UI-only

        public override void Load()
        {
            if (_hooked) return; // guard against hot-reload double hook
            Terraria.On_Main.DrawInterface_Resources_Buffs += HideAllBuffsHook;
            _hooked = true;
        }

        public override void Unload()
        {
            if (_hooked)
            {
                Terraria.On_Main.DrawInterface_Resources_Buffs -= HideAllBuffsHook;
                _hooked = false;
            }
        }

        private void HideAllBuffsHook(Terraria.On_Main.orig_DrawInterface_Resources_Buffs orig, Main self)
        {
            if (ToggleBuffBar.HideAll) return; // skip drawing: buffs hidden
            orig(self);                        // otherwise draw vanilla buffs
        }
    }
}
