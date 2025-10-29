using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Vokerropweapons.Content.Items.Weapons.Ranged
{
    public class DuhCannon : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Uses localization from en-US.hjson
        }

        public override void SetDefaults()
        {
            Item.damage = 6000;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 80;
            Item.height = 36;
            Item.useTime = 120;
            Item.useAnimation = 120;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 12f;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item14; // BOOM!
            Item.autoReuse = true;

            Item.shoot = ModContent.ProjectileType<Projectiles.CannonballProjectile>();
            Item.shootSpeed = 1f; // Very slow
            // Optional: Item.useAmmo = AmmoID.Cannonball;
        }

        public override void AddRecipes()
        {
            // Simple test recipe for ease of testing: only requires a Work Bench
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 15)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn the cannonball
            Projectile.NewProjectile(
                source,
                position,
                velocity,
                ModContent.ProjectileType<Projectiles.CannonballProjectile>(),
                damage,
                knockback,
                player.whoAmI
            );

            // Recoil effect: knock player back
            float recoilStrength = 20f; // tweak as needed
            player.velocity -= velocity.SafeNormalize(Vector2.Zero) * recoilStrength;

            return false; // we manually spawn the projectile
        }
    }
}