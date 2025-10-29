using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;

namespace Vokerropweapons.Content.Items.Weapons.Summon
{
    public class HumanoidBook : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Localization via .hjson is recommended. Tooltips are added in the localization file.
        }

        public override void SetDefaults()
        {
            Item.damage = 90; // Comparable to Terraprisma
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.width = 40;
            Item.height = 40;

            Item.useTime = 36;
            Item.useAnimation = 36;
            // Make the item behave like a book (hold up to cast)
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;

            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Yellow;
            // Use a book-like cast sound (using vanilla sound for now; custom sound at Content/Sounds/Item/HumanoidBookCast.wav)
            Item.UseSound = SoundID.Item8; // Book/spell cast sound

            Item.shoot = ModContent.ProjectileType<Projectiles.Minions.HumanoidMinion>();
            Item.shootSpeed = 8f;

            Item.buffType = ModContent.BuffType<Buffs.HumanoidMinionBuff>();
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply buff so the minion persists
            player.AddBuff(Item.buffType, 2);

            if (player.whoAmI == Main.myPlayer)
            {
                Vector2 spawnPos = Main.MouseWorld;
                if (velocity == Vector2.Zero)
                    velocity = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitY) * Item.shootSpeed;

                Projectile.NewProjectile(source, spawnPos, velocity, type, damage, knockback, player.whoAmI);
            }

            return false;
        }

        public override void AddRecipes()
        {
            // Simple recipe for testing
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 10)
                .AddIngredient(ItemID.FallenStar, 3)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
