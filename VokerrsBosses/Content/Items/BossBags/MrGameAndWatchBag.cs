using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace VokerrsBosses.Content.Items.BossBags
{
	public class MrGameAndWatchBag : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Treasure Bag");
			// Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");

			ItemID.Sets.BossBag[Type] = true;
			ItemID.Sets.PreHardmodeLikeBossBag[Type] = true;
		}

		public override void SetDefaults()
		{
			Item.width = 36;
			Item.height = 32;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.rare = ItemRarityID.Purple;
			Item.expert = true;
		}

		public override bool CanRightClick()
		{
			return true;
		}

		public override void ModifyItemLoot(ItemLoot itemLoot)
		{
			// Always drop trophy in expert mode
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Placeables.MrGameAndWatchTrophy>(), 10));
			
			// Add your boss weapon/accessory drops here
			// Example:
			// itemLoot.Add(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<YourWeaponHere>(), 1, 1, 1));
			
			// Coins
			itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<NPCs.Bosses.MrGameAndWatch.MrGameAndWatch>()));
		}
	}
}
