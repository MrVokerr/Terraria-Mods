using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Vokerropweapons.Players;

namespace Vokerropweapons.Content.Items.Weapons.Melee
{
    public class Voidreaver : ModItem
    {
        public override void SetStaticDefaults()
        {
            // No manual SetDefault calls — using Localization/en-US.hjson instead
        }

        public override void SetDefaults()
        {
            Item.damage = 4000;
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8;
            Item.value = Item.sellPrice(platinum: 1);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;

            Item.shoot = ModContent.ProjectileType<Projectiles.VoidSlash>();
            Item.shootSpeed = 20f;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.DirtBlock, 10); // Simple test recipe
            recipe.AddIngredient(ItemID.Wood, 5);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<VoidreaverPlayer>();

            if (player.altFunctionUse == 2) // Right-click
            {
                if (modPlayer.CanUseTimeRip())
                {
                    modPlayer.TriggerTimeRip();
                    return true;
                }
                return false;
            }

            return base.CanUseItem(player);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2))
            {
                Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.Shadowflame);
            }
        }
    }
}
