using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExileTree.Content.Players;

namespace ExileTree.Content.Items
{
    // Craftable: refunds all spent points.
    public class OrbOfRespec : ModItem
    {
        public override void SetStaticDefaults() {
            ItemID.Sets.IsFood[Type] = false; // This is not a food item
            Item.ResearchUnlockCount = 5; // Amount needed for Journey Mode research
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips.Add(new TooltipLine(Mod, "OrbOfRespecTooltip", "Refunds all spent passive skill points"));
            tooltips.Add(new TooltipLine(Mod, "OrbOfRespecUsage", "Right click to use"));
        }
        
        public override void SetDefaults() {
            Item.width = 28;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 25);
            Item.UseSound = SoundID.Item29;
            Item.maxStack = 99;
        }

        public override bool? UseItem(Player player) {
            player.GetModPlayer<ExileTreePlayer>().RespecAll();
            return true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.Amethyst, 5)
                .AddIngredient(ItemID.Bottle, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
