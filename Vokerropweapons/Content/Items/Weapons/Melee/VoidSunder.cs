using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Vokerropweapons.Content.Items.Weapons.Melee
{
    public class VoidSunder : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Name/tooltip handled by Localization/en-US_Mods.Vokerropweapons.hjson
        }

        public override void SetDefaults()
        {
            Item.damage = 120;
            Item.DamageType = DamageClass.Melee;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Purple;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            // Fire the custom VoidSunder projectile (flies out to screen edge then returns)
            Item.shoot = ModContent.ProjectileType<Projectiles.VoidSunderProjectile>();
            Item.shootSpeed = 18f; // used if other code relies on shootSpeed
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HellstoneBar, 10);
            recipe.AddIngredient(ItemID.SoulofNight, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }

        // Spawn a fast projectile toward the cursor which will travel to the edge of the screen then return
        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 direction = Main.MouseWorld - player.Center;
            if (direction == Vector2.Zero)
            {
                direction = new Vector2(player.direction, 0);
            }
            direction.Normalize();

            float speed = 18f; // fairly fast as requested

            Projectile.NewProjectile(
                source,
                player.Center,
                direction * speed,
                type,
                damage,
                knockback,
                player.whoAmI
            );

            // Prevent default shooting
            return false;
        }
    }
}
