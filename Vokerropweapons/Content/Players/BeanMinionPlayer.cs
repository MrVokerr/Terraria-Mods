using Terraria.ModLoader;

namespace Vokerropweapons.Players
{
    public class BeanMinionPlayer : ModPlayer
    {
        public bool beanMinion;

        public override void ResetEffects()
        {
            beanMinion = false;
        }

        public override void UpdateDead()
        {
            beanMinion = false;
        }
    }
}
