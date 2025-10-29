# QUICK START GUIDE

## What You Need to Do Next

### 1. Create Sprites ⚠️ REQUIRED
The mod will NOT compile without sprite files! You need to create PNG images for:

**Minimum Required Sprites:**
- `Content/NPCs/Bosses/MrGameAndWatch/MrGameAndWatch.png` (48x384 pixels)
- `Content/NPCs/Bosses/MrGameAndWatch/MrGameAndWatch_Head_Boss.png` (36x36 pixels)
- `Content/Projectiles/Bosses/MrGameAndWatch/ChefSausage.png` (16x16 pixels)
- `Content/Projectiles/Bosses/MrGameAndWatch/JudgeHammer.png` (48x48 pixels)
- `Content/Projectiles/Bosses/MrGameAndWatch/OilBlast.png` (32x32 pixels)
- `Content/Projectiles/Bosses/MrGameAndWatch/FireTorch.png` (20x20 pixels)
- `Content/Items/SummonItems/FlatBeep.png` (32x32 pixels)
- `Content/Items/BossBags/MrGameAndWatchBag.png` (36x32 pixels)
- `Content/Items/Placeables/MrGameAndWatchTrophy.png` (32x32 pixels)
- `Content/Tiles/MrGameAndWatchTrophy.png` (54x54 pixels)

**TIP:** You can create simple placeholder sprites (colored rectangles) to test the mod, then replace them with proper art later.

### 2. Build the Mod
Once sprites are in place:
1. Open tModLoader
2. Go to Workshop -> Develop Mods
3. Click "Build + Reload" on VokerrsBosses
4. Fix any errors that appear
5. Enable the mod if it's not auto-enabled

### 3. Test in Game
1. Create a new world or load existing
2. Craft "Flat Beep" at a Work Bench (10 Wood + 5 Wire + 3 Cogs)
3. Use it to summon Mr. Game and Watch
4. Fight and defeat him!

### 4. Customize (Optional)
You can customize:
- Boss stats in `MrGameAndWatch.cs` (line 59-72)
- Attack patterns and timing (lines 145-350)
- Projectile behavior in each projectile .cs file
- Loot drops in `MrGameAndWatch.cs` (line 91-102)
- Recipe in `FlatBeep.cs` (line 52-57)

## Common Issues

### "Type or namespace X could not be found"
- Make sure all files are in the correct folders
- Check that namespace declarations match folder structure

### "Texture not found"
- Create the sprite files! See SPRITE_REQUIREMENTS.txt

### Boss doesn't appear after using item
- Make sure you're not already fighting the boss
- Check console for errors (F10 in game)

### Boss behaves strangely
- Check the AI code in MrGameAndWatch.cs
- Verify attack timers are reasonable

## Need More Help?

1. Check the tModLoader wiki: https://github.com/tModLoader/tModLoader/wiki
2. Join the tModLoader Discord
3. Review the example mod source code
4. Check compilation errors carefully - they usually point to the exact issue

---

**Good luck with your mod! 🎮**
