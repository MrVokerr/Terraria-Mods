using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Vokerropweapons.Content.Items.Weapons.Ranged
{
    public class TKWave : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 36;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 50;
            Item.height = 20;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.TKWaveProjectile>();
            Item.shootSpeed = 6f;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int numberProjectiles = 10;
            float spacing = 8f; // pixels between each wave path

            Vector2 perpendicular = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);

            for (int i = 0; i < numberProjectiles; i++)
            {
                // Offset from center
                float offset = (i - (numberProjectiles - 1) / 2f) * spacing;

                Vector2 spawnPosition = position + perpendicular * offset;

                Projectile.NewProjectile(
                    source,
                    spawnPosition,
                    velocity,
                    ModContent.ProjectileType<Projectiles.TKWaveProjectile>(),
                    damage,
                    knockback,
                    player.whoAmI
                );
            }

            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IllegalGunParts);
            recipe.AddIngredient(ItemID.Wood, 25);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
