using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExileTree.Content.Players;

namespace ExileTree.Content.Items
{
    // Craftable: refunds all spent points.
    public class OrbOfRespec : ModItem
    {
        // Optional placeholder so you don't need custom art yet:
        // public override string Texture => "Terraria/Images/Item_" + ItemID.Amethyst;

        public override void SetDefaults() {
            Item.width = 28;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 75);
            Item.UseSound = SoundID.Item29;
            Item.maxStack = 99;
        }

        public override bool? UseItem(Player player) {
            player.GetModPlayer<ExileTreePlayer>().RespecAll();
            return true;
        }

        public override void AddRecipes() {
            // Simple, early-ish recipe (tweak to taste)
            CreateRecipe()
                .AddIngredient(ItemID.Amethyst, 5)
                .AddIngredient(ItemID.Bottle, 1)
                .AddTile(TileID.WorkBenches)
                .Register();

            // (Optional alternative recipe example)
            // CreateRecipe()
            //     .AddIngredient(ItemID.FallenStar, 3)
            //     .AddIngredient(ItemID.Glass, 10)
            //     .AddTile(TileID.Anvils)
            //     .Register();
        }
    }
}
