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
        }

        public override void SetDefaults()
        {
            Item.damage = 30;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<Projectiles.Minions.BeanMinion>();
            Item.shootSpeed = 10f;
            Item.buffType = ModContent.BuffType<Buffs.BeanMinionBuff>();
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply the buff so the minion persists
            player.AddBuff(Item.buffType, 2);
            Vector2 spawnPosition = Main.MouseWorld;
            Projectile.NewProjectile(source, spawnPosition, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Vine, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
