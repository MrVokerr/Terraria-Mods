using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VokerrsBosses.Content.Items.Placeables
{
	public class MrGameAndWatchTrophy : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Mr. Game and Watch Trophy");
			// Tooltip.SetDefault("'BEEP BEEP BEEP'");
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = Item.buyPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.createTile = ModContent.TileType<Tiles.MrGameAndWatchTrophy>();
			Item.placeStyle = 0;
		}
	}
}
