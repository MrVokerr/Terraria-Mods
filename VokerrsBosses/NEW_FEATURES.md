# 🎮 NEW FEATURES ADDED!

## 🔨 The Last Judge - Desperation Attack

### What is it?
When Mr. Game & Watch reaches **1 HP**, instead of dying, he:
1. **Becomes invulnerable** (can't take damage)
2. Performs a **dramatic buildup** with golden particle effects
3. Executes **ONE FINAL JUDGE ATTACK**
4. The hammer rolls a random number from **1 to 9**

### Outcomes:

#### 🏆 If he rolls a 9 (The Legendary 9):
- **INSTANT KILL** on any player hit
- Boss **escapes and flees upward** 
- **NO LOOT DROPS** - he got away!
- You must resummon and fight him again
- Message: *"Mr. Game & Watch's 9-Hammer connected! He beeps triumphantly and escapes!"*

#### 💀 If he rolls 1-8:
- Boss **dies immediately** and drops loot normally
- No more fighting needed!
- Message: *"The hammer missed the legendary 9... Mr. Game & Watch accepts defeat!"*

### Anti-Overkill Protection:
- Boss **CANNOT be overkilled** past 1 HP
- Damage is automatically capped to leave him at exactly 1 HP
- **Guarantees** the Last Judge will always trigger
- No way to accidentally skip the dramatic finale!

### Visual Effects:
- **Golden dust pulses** during 60-frame buildup
- **Epic particle effects** every 10 frames
- Judge number appears above the hammer in **color-coded text**
- If 9 lands: **100 golden dust particles** as he escapes

---

## 🎲 Enhanced Judge Hammer Display

### Number Visibility:
Every Judge attack now **displays the number** that was rolled!

### Color Coding:
- **9** = Gold (legendary!)
- **7-8** = Orange (very strong)
- **5-6** = Yellow (decent)
- **3-4** = White (average)
- **1-2** = Gray (weak)

### Damage Scale:
- Number **1** = 15 damage
- Number **2** = 30 damage
- Number **3** = 45 damage
- ...
- Number **9** = 135 damage (270 in Last Judge = instant kill)

### Knockback Effects:
- **7-9**: Massive knockback (10 units horizontal, 8 units up)
- **4-6**: Medium knockback (5 units horizontal, 4 units up)
- **1-3**: Normal knockback

### Visual Effects by Number:
- **9**: Golden flames constantly
- **7-8**: Orange torch sparks
- **1-6**: Standard hammer appearance

---

## 😂 Funny Summoning Messages

When you use the **Vintage Game & Watch** item, you get a random funny message:

1. *"The ancient Game & Watch beeps to life! BEEP BEEP BEEP!"*
2. *"You pressed the wrong button sequence! Mr. Game & Watch appears!"*
3. *"The LCD screen flickers... something 2D approaches!"*
4. *"BEEP BEEP BEEP - Someone's high score has been challenged!"*
5. *"A wild 2D fighter appears from the digital realm!"*
6. *"ERROR 404: Dimension Not Found. Mr. Game & Watch has arrived!"*
7. *"You've activated a retro gaming curse! Mr. Game & Watch materializes!"*

Plus dramatic smoke particles spawn around you!

---

## 🎯 Strategy Tips

### Fighting the Last Judge:
1. **Keep your distance** when he hits 1 HP
2. Watch for the **golden pulse effects** - that's your warning
3. The attack happens at **frame 80** after going invulnerable
4. **11% chance** he rolls a 9 and escapes (you lose)
5. **89% chance** he rolls 1-8 and dies (you win with loot!)

### Managing the Risk:
- **Cannot overkill** - boss will always survive to 1 HP for Last Judge
- If you're low on health, **dodge the hammer** or you might die before he does
- Judge has a **dash attack** before the swing
- The hammer has **enhanced damage** in Last Judge mode (10x multiplier)
- If hit by the 9, you'll see **epic golden explosion** before dying

### Maximizing Success:
- You have an **89% chance** to win (rolls 1-8)
- Only **11% chance** he escapes with a 9
- Either way, the fight ends at 1 HP - no prolonged combat
- Consider this your **final dice roll** - may RNG be with you!

---

## 📊 Statistics

### Last Judge Mechanics:
- Invulnerability Duration: 60 frames (1 second)
- Dash Speed: 15 units/frame (vs normal 10)
- Hammer Damage Multiplier: 30x instead of 15x
- Escape Speed: -20 velocity upward
- Number Roll Range: 1-9 (equal probability)

### Judge Number Probabilities:
- Each number: **11.11%** chance
- Numbers 7-9 (strong): **33.33%** combined
- Numbers 1-3 (weak): **33.33%** combined
- The legendary 9: **11.11%** (1 in 9 chance)

---

## 🐛 Technical Details

### New AI State:
- Added `AIState.LastJudge` (state 6)
- Tracked with `hasUsedLastJudge` boolean flag
- Only triggers once per boss fight

### Multiplayer Support:
- Messages broadcast to all players
- Works in both singleplayer and multiplayer
- Proper net sync for invulnerability state

### Death Prevention:
When 9 lands:
- `NPC.life = 0`
- `NPC.checkDead()`
- `NPC.active = false`
- No loot drop triggered
- Boss properly despawns

When 1-8 lands:
- `NPC.dontTakeDamage = false`
- `NPC.life = 0`
- `NPC.checkDead()`
- Normal loot drops
- Boss dies with honor

### Overkill Prevention:
- `ModifyIncomingHit` hook caps damage
- If damage would kill boss before 1 HP: `modifiers.SetMaxDamage(NPC.life - 1)`
- Ensures Last Judge **always triggers**
- No accidental skipping of finale

---

## 🎨 Item Rename

**Old Name:** Flat Beep
**New Name:** Vintage Game & Watch

**New Tooltip:**
```
Summons Mr. Game & Watch
Press all the buttons at once to summon him!
'BEEP BEEP BEEP!'
```

More thematic and funny!

---

## ✅ What's Been Updated

### Files Modified:
1. `MrGameAndWatch.cs` - Added Last Judge AI state and logic
2. `JudgeHammer.cs` - Added number display and enhanced effects
3. `FlatBeep.cs` - Added random funny summoning messages
4. `en-US_Mods.VokerrsBosses.hjson` - Updated item name/tooltip
5. `description.txt` - Added Last Judge feature description

### Lines of Code Added: ~150+
### New Features: 3 major features
### Compilation Errors: 0 ✅

---

## 🎉 The Result

Mr. Game & Watch is now **even more unpredictable and exciting**! 

The Last Judge mechanic adds:
- ✅ **Dramatic tension** at low HP
- ✅ **Risk vs reward** gameplay
- ✅ **True RNG madness** just like Smash Bros
- ✅ **Memorable boss moments**
- ✅ **Replayability** (if he escapes!)

Every fight can end differently - will you get the loot or will he escape with the legendary 9? 🎲

---

**Good luck, and may the RNG be ever in your favor!** 🍀
