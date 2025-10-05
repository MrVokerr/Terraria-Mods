using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Vokerropweapons.Content.Items.Weapons.Summon
{
    public class BeanFlicker : ModItem
    {
        public override void SetStaticDefaults()
        {
            // (Localization via .hjson recommended)
        }

        public override void SetDefaults()
        {
            Item.damage = 40;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.width = 40;
            Item.height = 40;

            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing; // keep swing; HoldUp also works for staves
            Item.noMelee = true;

            Item.knockBack = 2f;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item44;

            Item.shoot = ModContent.ProjectileType<Projectiles.Minions.BeanMinion>();
            Item.shootSpeed = 10f;

            Item.buffType = ModContent.BuffType<Buffs.BeanMinionBuff>(); // keeps minion alive
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply the buff so the minion persists (AI sets timeLeft = 2 every tick)
            player.AddBuff(Item.buffType, 2);

            // Spawn at cursor on the owning client to avoid multiplayer double-spawns
            if (player.whoAmI == Main.myPlayer)
            {
                Vector2 spawnPosition = Main.MouseWorld;
                // velocity will be normalized and scaled by Item.shootSpeed by the engine,
                // but providing it here makes initial movement snappier toward cursor direction.
                if (velocity == Vector2.Zero)
                    velocity = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitY) * Item.shootSpeed;

                Projectile.NewProjectile(source, spawnPosition, velocity, type, damage, knockback, player.whoAmI);
            }
            return false; // we handled the spawn
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Vine, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
