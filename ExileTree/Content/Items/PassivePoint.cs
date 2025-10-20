using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExileTree.Content.Players;

namespace ExileTree.Content.Items
{
    // Admin/spawn-only: grants +1 passive point. No recipe.
    public class PassivePoint : ModItem
    {
        // Optional placeholder so you don't need custom art yet:
        // public override string Texture => "Terraria/Images/Item_" + ItemID.FallenStar;

        public override void SetDefaults() {
            Item.width = 28;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(silver: 50);
            Item.UseSound = SoundID.Item4;
            Item.maxStack = 999;
        }

        public override bool? UseItem(Player player) {
            player.GetModPlayer<ExileTreePlayer>().AddSkillPoint(1);
            Main.NewText("Gained +1 Exile Tree point!");
            return true;
        }

        // INTENTIONALLY no AddRecipes() → uncraftable
    }
}
