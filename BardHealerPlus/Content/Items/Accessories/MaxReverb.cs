using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;

namespace BardHealerPlus.Content.Items.Accessories
{
    public class MaxReverb : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<ThoriumPlayer>().bardRangeBoost += 1500;
            player.GetModPlayer<ThoriumPlayer>().bardBuffDuration += 1800; // 30s
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Silk, 10)
                .AddIngredient(ItemID.Amethyst, 5)
                .AddTile(TileID.Loom)
                .Register();
        }

        public override void ModifyTooltips(System.Collections.Generic.List<Terraria.ModLoader.TooltipLine> tooltips)
        {
            tooltips.Add(new Terraria.ModLoader.TooltipLine(Mod, "Stat1", "300% increased empowerment range"));
            tooltips.Add(new Terraria.ModLoader.TooltipLine(Mod, "Stat2", "Increases empowerment duration by 30 seconds."));
        }
    }
}
