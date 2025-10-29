# ⚔️ BOSS STATS UPDATE - DUKE FISHRON TIER

## 📊 New Boss Stats

Mr. Game & Watch has been upgraded to **Duke Fishron difficulty tier**!

### Base Stats:
| Stat | Old Value | New Value | Notes |
|------|-----------|-----------|-------|
| **Health** | 8,000 | **50,000** | Duke Fishron tier (hardmode) |
| **Defense** | 12 | **50** | Matches Duke Fishron exactly |
| **Contact Damage** | 35 | **100** | Hardmode-level threat |
| **Coin Value** | 5 gold | **15 gold** | Increased reward |
| **NPC Slots** | 10 | **15** | Higher threat level |
| **Music** | Boss1 (pre-HM) | **Boss3** (hardmode) |

### Defense During Oil Panic:
- **Old:** 20
- **New:** 70 (even tankier than base!)

---

## 💥 Projectile Damage Updates

### Chef Sausages:
- **Old:** 20 damage
- **New:** 60 damage
- Bounces 3 times before despawning

### Judge Hammer (Normal):
- **Old:** 5-45 damage (1-9 multiplier × 5)
- **New:** 15-135 damage (1-9 multiplier × 15)
- Number 9 = 135 damage!

### Judge Hammer (Last Judge):
- **Old:** 10-90 damage (1-9 multiplier × 10)
- **New:** 30-270 damage (1-9 multiplier × 30)
- Number 9 = **270 damage + INSTANT KILL**

### Oil Blast:
- **Old:** 25 damage
- **New:** 80 damage
- Applies Slimed debuff

### Fire Torch:
- **Old:** 18 damage
- **New:** 55 damage
- Homing after 20 frames
- Applies OnFire debuff

---

## 🎯 Difficulty Tier

### Progression Placement:
**Post-Mechanical Bosses, Pre-Golem**

Similar to Duke Fishron in difficulty, making this a hardmode challenge boss.

### Recommended Gear:
- Chlorophyte/Hallowed armor
- Post-mech boss weapons (Megashark, Daedalus Stormbow, etc.)
- Wings (required for mobility)
- Hardmode accessories
- Good arena setup

### Why This Tier?
- 50,000 HP is comparable to Duke Fishron (50,000 HP)
- 50 Defense matches Duke Fishron exactly
- Projectile damage scales appropriately for post-mech gear
- RNG-based Last Judge mechanic adds unique challenge
- Fast-paced attack patterns require hardmode mobility

---

## ⚠️ Challenge Comparison

### vs Duke Fishron:
| Feature | Mr. Game & Watch | Duke Fishron |
|---------|------------------|--------------|
| HP | 50,000 | 50,000 |
| Defense | 50 | 50 |
| Damage | 100 | 210 (enraged) |
| Special Mechanic | Last Judge (RNG) | Enrage (time-based) |
| Difficulty | **Hard** | **Very Hard** |

Mr. Game & Watch is slightly easier than Duke due to:
- Lower contact damage (100 vs 210)
- No enrage mechanic
- More predictable attack patterns
- BUT: Last Judge RNG adds unique risk!

---

## 🎲 Last Judge Impact

At **1 HP**, the boss becomes invulnerable and rolls for Last Judge:

### The 9-Hammer:
- **270 base damage**
- **Instant kill** regardless of defense
- **Boss escapes** - no loot
- **11% chance** to happen

This means even with endgame gear, you're never safe from the RNG!

### Strategy Against Last Judge:
1. **Burst damage** before he reaches 1 HP
2. **Dodge perfectly** if he does
3. **Pray** he doesn't roll a 9
4. **89% chance** you win with loot!

---

## 📈 Damage Breakdown

### Total Damage Per Attack:
| Attack | Projectiles | Damage Each | Total Potential |
|--------|-------------|-------------|-----------------|
| Chef | 6 sausages | 60 | 360 |
| Judge | 1 hammer | 15-135 | 15-135 |
| Oil Panic | 1 blast | 80 | 80 |
| Fire | 6 torches | 55 | 330 |
| Parachute | Contact | 100 | 100 |
| Last Judge | 1 hammer | 30-270 | 30-270 (or death) |

### DPS Calculation:
With proper dodging, the boss can output **significant sustained damage** through rapid Chef/Fire attacks.

---

## 🛡️ Survivability Tips

### For Players:
1. **Defense matters** - 50 boss defense means you need strong weapons
2. **Wings are mandatory** - mobility is key
3. **Build an arena** - platforms for dodging
4. **Health pool** - 400+ HP recommended
5. **Buffs** - Use Ironskin, Endurance, Lifeforce
6. **Accessories** - Cross Necklace helps with Judge hits

### Tank Test:
With **full Hallowed armor** (49 defense):
- Chef sausage: ~40 damage taken
- Judge 9: ~106 damage taken
- Oil blast: ~61 damage taken
- Fire torch: ~36 damage taken
- Contact: ~81 damage taken

Survivable but you'll need healing!

---

## ✅ Balance Status

The boss is now properly scaled for **hardmode endgame** content:

- ✅ Challenging but fair
- ✅ Requires good gear to beat
- ✅ Rewards skill with dodging
- ✅ RNG mechanic adds excitement
- ✅ Comparable to Duke Fishron tier

---

## 🔧 Technical Changes Made

### Files Modified:
1. `MrGameAndWatch.cs`:
   - NPC.lifeMax: 8000 → 50000
   - NPC.damage: 35 → 100
   - NPC.defense: 12 → 50
   - NPC.value: 5g → 15g
   - Music: Boss1 → Boss3
   - Oil Panic defense: 20 → 70
   - Chef damage: 20 → 60
   - Judge damage: 5× → 15×
   - Oil damage: 25 → 80
   - Fire damage: 18 → 55
   - Last Judge: 10× → 30×

2. `JudgeHammer.cs`:
   - Instant kill threshold: 90 → 200 damage

### Compilation Status: ✅ No Errors

---

**Mr. Game & Watch is now a true hardmode challenge! May RNG be with you!** 🎮
