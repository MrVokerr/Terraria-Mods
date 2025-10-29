# Mr. Game and Watch Boss Mod for Terraria

A tModLoader mod that adds Mr. Game and Watch from Super Smash Bros. as a boss fight in Terraria!

## 🎮 Features

### Boss: Mr. Game and Watch
- **Health:** 8,000 HP
- **Damage:** 35 (varies by attack)
- **Defense:** 12

### Attack Patterns (Based on Super Smash Bros. Moveset)

1. **Chef** - Tosses multiple sausages that bounce and create chaos
2. **Judge** - Dashes at the player and swings a hammer with random damage (1-9 multiplier)
   - Getting hit by a 9 is devastating!
3. **Oil Panic** - Defensive stance with the bucket, then releases oil blast
   - Applies Slimed debuff (slippery movement)
4. **Fire Attack** - Shoots fire torches that home in on players
   - Applies On Fire debuff
5. **Parachute** - Jumps high and floats down while moving toward the player

## 📦 Items

### Summoning Item: Flat Beep
**Recipe:**
- 10x Wood
- 5x Wire
- 3x Cog
- Crafted at Work Bench

### Drops
- **Treasure Bag** (Expert Mode)
- **Mr. Game and Watch Trophy** (10% drop rate)
- Various weapons and accessories (add your own!)

## 🛠️ Installation

1. Subscribe to this mod on Steam Workshop, OR
2. Download and place in: `Documents/My Games/Terraria/tModLoader/Mods/`
3. Enable the mod in tModLoader's Mod menu
4. Reload and enjoy!

## 🎨 Sprites Required

**IMPORTANT:** You need to create sprite files for the mod to work properly!

See `SPRITE_REQUIREMENTS.txt` for a complete list of required sprites and their dimensions.

All sprites should maintain Mr. Game and Watch's iconic flat, black silhouette style.

### Quick Sprite List:
- Boss sprite sheet (48x384)
- Boss head icon (36x36)
- 4 projectile sprites (various sizes)
- 3 item sprites
- Trophy tile sprite (54x54)

## 🔧 Development

Built for:
- **tModLoader:** v2025.8.3.1
- **Terraria:** v1.4.4.9

### Code Structure
```
Content/
├── NPCs/
│   └── Bosses/
│       └── MrGameAndWatch/
│           └── MrGameAndWatch.cs (Main boss AI)
├── Projectiles/
│   └── Bosses/
│       └── MrGameAndWatch/
│           ├── ChefSausage.cs
│           ├── JudgeHammer.cs
│           ├── OilBlast.cs
│           └── FireTorch.cs
├── Items/
│   ├── SummonItems/
│   │   └── FlatBeep.cs
│   ├── BossBags/
│   │   └── MrGameAndWatchBag.cs
│   └── Placeables/
│       └── MrGameAndWatchTrophy.cs
└── Tiles/
    └── MrGameAndWatchTrophy.cs
```

## 🎯 Future Enhancements

Potential additions:
- Custom music track
- More attack patterns (Down Air, Up Special, etc.)
- Unique weapons as boss drops
- Expert mode-only attacks
- Master mode relic
- Custom sound effects

## ⚔️ Strategy Tips

1. **Watch for the Judge attack** - The damage is random, but a 9 can one-shot you!
2. **During Oil Panic**, keep your distance - the boss has increased defense
3. **Fire attacks home in** - Keep moving!
4. **Build an arena** with platforms for better mobility
5. **Bring buff potions** - Ironskin, Regeneration, and Swiftness recommended

## 📝 Notes

- This mod uses modern tModLoader v2025.8 API
- All deprecated methods have been removed
- Boss follows modern best practices for NPC AI
- Projectiles use proper hostile/friendly flags
- Localization support included

## 🤝 Credits

- Created by: Vokerr
- Mr. Game and Watch is © Nintendo
- Super Smash Bros. is © Nintendo
- Terraria is © Re-Logic
- tModLoader by the tML team

## 📄 License

This is a fan-made mod for educational and entertainment purposes.
All Nintendo characters and properties remain property of Nintendo.

---

**Enjoy fighting this retro legend in Terraria!** 🎮
