﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using System.Linq;

namespace RogueBasin
{
    public class Player : Creature
    {
        /// <summary>
        /// Effects that are active on the player
        /// </summary>
        public List<PlayerEffect> effects { get; private set; }

        public List<Monster> Kills { get; set;}

        public int KillCount = 0;

        public string Name { get; set; }

        public bool ItemHelpMovieSeen = false;
        public bool TempItemHelpMovieSeen = false;

        /// <summary>
        /// How many worldClock ticks do we have to rescue our friend?
        /// </summary>
        public long TimeToRescueFriend { get; set; }


        public int TotalPlotItems { get; set; }
        public int PlotItemsFound { get; set; }

        /// <summary>
        /// Play movies and give plot exerpts for items
        /// </summary>
        public bool PlayItemMovies { get; set; }

        /// <summary>
        /// Current hitpoints
        /// </summary>
        int hitpoints;

        /// <summary>
        /// Maximum hitpoints
        /// </summary>
        int maxHitpoints;

        public int Shield { get; set; }

        public int MaxShield { get; set; }
        
        public int Energy { get; set; }

        public int MaxEnergy { get; set; }

        public bool ShieldWasDamagedThisTurn { get; set; }

        public bool HitpointsWasDamagedThisTurn { get; set; }

        public bool EnergyWasDamagedThisTurn { get; set; }

        public bool ShieldIsDisabled { get; private set; }

        public bool EnergyRechargeIsDisabled { get; private set; }

        public bool DoesShieldRecharge { get; private set; }

        private int TurnsSinceShieldDisabled { get; set; }

        private int TurnsSinceEnergyRechargeDisabled { get; set; }

        private const int TurnsForShieldToTurnBackOn = 20;

        private const int TurnsForEnergyRechargeToTurnBackOn = 5;

        private const int TurnsToRegenerateShield = 20;

        private const int TurnsToRegenerateHP = 20;

        private const int TurnsToRegenerateEnergy = 10;

        private Dictionary<Item, int> wetwareDisabledTurns = new Dictionary<Item, int>();

        //4 ticks per turn currently
        private const int turnsToDisableStealthWareAfterAttack = 80;
        private const int turnsToDisableBoostWareAfterAttack = 80;

        private const int turnsToDisableStealthWareAfterUnequip = 20;
        private const int turnsToDisableBoostWareAfterUnequip = 20;

        /// <summary>
        /// Player level
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Which princess RL dungeon are we in?
        /// </summary>
        public int CurrentDungeon { get; set; }

        /// <summary>
        /// Player armour class. Auto-calculated so not serialized
        /// </summary>
        int armourClass;

        /// <summary>
        /// Player damage base. Auto-calculated so not serialized
        /// </summary>
        int damageBase;

        /// <summary>
        /// Player damage modifier. Auto-calculated so not serialized
        /// </summary>
        int damageModifier;

        /// <summary>
        /// Player damage modifier. Auto-calculated so not serialized
        /// </summary>
        int hitModifier;

        /// <summary>
        /// Magic casting points
        /// </summary>
        int magicPoints;

        /// <summary>
        /// Maximum magic points
        /// </summary>
        int maxMagicPoints;


        /// <summary>
        /// Number of times we get knocked out
        /// </summary>
        public int NumDeaths { get; set; }

        public int MaxCharmedCreatures { get; set; }

        public int CurrentCharmedCreatures { get; set; }

        public bool CombatUse { get; set; }
        public bool MagicUse { get; set; }
        public bool CharmUse { get; set; }

        /// <summary>
        /// Combat stat calculated from training stat and items
        /// </summary>
        public int CharmPoints { get; set; }

        /// <summary>
        /// PrincessRL has a maximum no of equipped items when using EquipItemNoSlots
        /// </summary>
        public int MaximumEquippedItems { get; set; }

        public int CurrentEquippedItems { get; set; }

        //XP in dungeons
        public int CombatXP { get; set; }
        public int MagicXP { get; set; }
        public int CharmXP { get; set; }

        //Training stats

        public int MaxHitpointsStat { get; set; }
        public int HitpointsStat { get; set; }
        public int AttackStat { get; set; }
        public int SpeedStat { get; set; }
        public int CharmStat { get; set; }
        public int MagicStat { get; set; }

        public int ArmourClassAccess { get { return armourClass; }  set { armourClass = value; } }
        public int DamageBaseAccess { get { return damageBase; } set { damageBase = value; } }
        public int DamageModifierAccess { get { return damageModifier; } set { damageModifier = value; } }
        public int HitModifierAccess { get { return hitModifier; } set { hitModifier = value; } }


        public Player()
        {
            //Set unique ID to 0 (player)
            UniqueID = 0;

            effects = new List<PlayerEffect>();
            Kills = new List<Monster>();

            Level = 1;

            //Representation = '\xd7';

            MaxCharmedCreatures = 0;
            CurrentCharmedCreatures = 0;
            MaximumEquippedItems = 3;

            NumDeaths = 0;
            CombatUse = false;
            MagicUse = false;
            CharmUse = false;

            CurrentDungeon = -1;

            //Add default equipment slots
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Utility));
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Weapon));
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.Wetware));

            //Set initial HP
            SetupInitialStats();

            SightRadius = 0;

            TurnCount = 0;
        }

        private void SetupInitialStats()
        {
            //CalculateCombatStats();
            maxHitpoints = 50;
            hitpoints = maxHitpoints;

            MaxShield = 100;
            Shield = MaxShield;

            MaxEnergy = 200;
            Energy = MaxEnergy;

            DoesShieldRecharge = false;
        }

        internal bool IsWeaponTypeAvailable(Type weaponType)
        {
            return IsInventoryTypeAvailable(weaponType);
        }

        internal bool IsWetwareTypeAvailable(Type wetWareType)
        {

            return IsInventoryTypeAvailable(wetWareType);
        }

        internal bool IsInventoryTypeAvailable(Type wetWareType)
        {
            var wetwareInInventory = Inventory.GetItemsOfType(wetWareType);

            if (wetwareInInventory.Count() == 0)
            {
                return false;
            }

            return true;
        }

        internal int InventoryQuantityAvailable(Type wetWareType)
        {
            return Inventory.GetItemsOfType(wetWareType).Count();
        }

        internal bool IsWetwareTypeDisabled(Type wetwareType)
        {
            var wetwareInInventory = Inventory.GetItemsOfType(wetwareType);

            if (wetwareInInventory.Count() == 0)
            {
                return false;
            }

            var thisWetware = wetwareInInventory.First();

            if (wetwareDisabledTurns.ContainsKey(thisWetware) && wetwareDisabledTurns[thisWetware] > 0)
            {
                return true;
            }
            return false;
        }


        public void DisableWetware(Type wetwareToDisable, int turnsToDisable)
        {
            var wetwareInInventory = Inventory.GetItemsOfType(wetwareToDisable);

            if (wetwareInInventory.Count() == 0)
            {
                LogFile.Log.LogEntryDebug("Can't disable wetware " + wetwareToDisable + " not in inventory", LogDebugLevel.Medium);
                return;
            }

            var thisWetware = wetwareInInventory.First();

            var currentlyDisableTurns = 0;

            if (wetwareDisabledTurns.ContainsKey(thisWetware))
                currentlyDisableTurns = wetwareDisabledTurns[thisWetware];

            if (currentlyDisableTurns > turnsToDisable)
                return;

            wetwareDisabledTurns[thisWetware] = turnsToDisable;
        }

        /// <summary>
        /// Remove all our items and reset our equipped items count
        /// </summary>
        public void RemoveAllItems()
        {
            Inventory.RemoveAllItems();
            CurrentEquippedItems = 0;
        }

        public int MagicPoints
        {
            get
            {
                return magicPoints;
            }
            set
            {
                magicPoints = value;
            }
        }

        public int MaxMagicPoints
        {
            get
            {
                return maxMagicPoints;
            }
            set
            {
                maxMagicPoints = value;
            }
        }

        /// <summary>
        /// Function called after an effect is applied or a new item is equipped.
        /// Calculates all derived statistics from bases with modifications from equipment and effects.
        /// </summary>
        public void CalculateCombatStats()
        {

            Inventory inv = Inventory;

            //Armour class
            ArmourClassAccess = 12;

            //Charm points
            CharmPoints = CharmStat;

            //Max charmed creatures
            /*
            if (inv.ContainsItem(new Items.SparklingEarrings()))
            {
                MaxCharmedCreatures = 2;
            }
            else
                MaxCharmedCreatures = 1;
            */
            //Sight

            NormalSightRadius = 0;

            /*
            if(inv.ContainsItem(new Items.Lantern()))
                NormalSightRadius = 7;
            */
            //Speed

            //int speedDelta = SpeedStat - 10;

            //speedDelta = speedDelta * 2;

            Speed = 100;// +speedDelta;

            //To Hit

            int toHit;

            if (AttackStat > 60)
            {
                toHit = (int)Math.Round((AttackStat - 60) / 30.0) + 3;
            }
            else
            {
                toHit = AttackStat / 20;
            }

            HitModifierAccess = toHit;

            //Damage base

            int damageBase;
            if (AttackStat > 120)
            {
                damageBase = 12;
            }
            if (AttackStat > 80)
            {
                damageBase = 10;
            }
            else if (AttackStat > 50)
            {
                damageBase = 8;
            }
            else if (AttackStat > 20)
            {
                damageBase = 6;
            }
            else
                damageBase = 4;

            DamageBaseAccess = damageBase;

            //Armour

            Screen.Instance.PCColor = ColorPresets.White;
            /*
            if (inv.ContainsItem(new Items.MetalArmour()) && AttackStat > 50)
            {
                ArmourClassAccess += 6;
                Screen.Instance.PCColor = ColorPresets.SteelBlue;
            }
            else if (inv.ContainsItem(new Items.LeatherArmour()) && AttackStat > 25)
            {
                ArmourClassAccess += 3;
                Screen.Instance.PCColor = ColorPresets.BurlyWood;
            }
            else if (inv.ContainsItem(new Items.KnockoutDress()))
            {
                CharmPoints += 40;
                ArmourClassAccess += 3;
                Screen.Instance.PCColor = ColorPresets.Yellow;
            }
            else if (inv.ContainsItem(new Items.PrettyDress()))
            {
                CharmPoints += 20;
                ArmourClassAccess += 1;
                Screen.Instance.PCColor = ColorPresets.BlueViolet;
            }*/

            //Consider equipped weapons (only 1 will work)
            DamageModifierAccess = 0;

            /*
            if (inv.ContainsItem(new Items.GodSword()))
            {
                DamageModifierAccess += 8;
            }
            else if (inv.ContainsItem(new Items.LongSword()))
            {
                DamageModifierAccess += 4;
            }
            else if (inv.ContainsItem(new Items.ShortSword()))
            {
                DamageModifierAccess += 2;
            }
            else if (inv.ContainsItem(new Items.Dagger()))
            {
                DamageModifierAccess += 1;
            }*/

            //Check and apply effects

            ApplyEffects();

            //Calculate sight radius (depends on dungeon light level)

            //CalculateSightRadius();
        }

        /// <summary>
        /// Calculate the derived (used by other functions) sight radius based on the player's NormalSightRadius and the light level of the dungeon level the player is on
        /// Note that 0 is infinite
        /// </summary>
        public void CalculateSightRadius()
        {
            //Set vision
            double sightRatio = NormalSightRadius / 5.0;
            SightRadius = (int)Math.Ceiling(Game.Dungeon.Levels[LocationLevel].LightLevel * sightRatio);
        }

        /// <summary>
        /// As part of CalculateCombatStats, go through the current effects
        /// find the most-good and most-bad stat modifiers and apply them (only)
        /// </summary>
        private void ApplyEffects()
        {
            int maxDamage = 0;
            int minDamage = 0;

            int maxHit = 0;
            int minHit = 0;

            int maxAC = 0;
            int minAC = 0;

            int maxSpeed = 0;
            int minSpeed = 0;

            int maxSight = 0;
            int minSight = 0;

            //Only the greatest magnitude (+ or -) effects have an effect
            foreach (PlayerEffect effect in effects)
            {
                if(effect.ArmourClassModifier() > maxAC)
                    maxAC = effect.ArmourClassModifier();

                if(effect.ArmourClassModifier() < minAC)
                    minAC = effect.ArmourClassModifier();

                if(effect.HitModifier() > maxHit)
                    maxHit = effect.HitModifier();

                if(effect.HitModifier() < minHit)
                    minHit = effect.HitModifier();

                if(effect.SpeedModifier() > maxSpeed)
                    maxSpeed = effect.SpeedModifier();

                if(effect.SpeedModifier() < minSpeed)
                    minSpeed = effect.SpeedModifier();

                if(effect.DamageModifier() > maxDamage)
                    maxDamage = effect.DamageModifier();

                if(effect.DamageModifier() < minDamage)
                    minDamage = effect.DamageModifier();

                if (effect.SightModifier() < minSight)
                    minSight = effect.SightModifier();

                if (effect.SightModifier() > maxSight)
                    maxSight = effect.SightModifier();
            }

            damageModifier += maxDamage;
            damageModifier += minDamage;

            Speed += maxSpeed;
            Speed += minSpeed;

            hitModifier += maxHit;
            hitModifier += minHit;

            armourClass += maxAC;
            armourClass += minAC;

            NormalSightRadius += maxSight;
            NormalSightRadius += minSight;
        }

        /// <summary>
        /// Current HP
        /// </summary>
        public int Hitpoints
        {
            get
            {
                return hitpoints;
            }
            set
            {
                hitpoints = value;
            }
        }

        /// <summary>
        /// Normal maximum hp
        /// </summary>
        public int MaxHitpoints
        {
            get
            {
                return maxHitpoints;
            }
            set
            {
                maxHitpoints = value;
            }
        }

        /// <summary>
        /// Player can overdrive to 50% of normal hp. This is the max possible with this fact.
        /// </summary>
        public int OverdriveHitpoints { get; set; }

        /// <summary>
        /// Used as accessors only for Player
        /// </summary>
        /// <returns></returns>
        public override int BaseSpeed()
        {
            return Speed;
        }

        /// <summary>
        /// Used as accessors only for Player
        /// </summary>
        public override int ArmourClass()
        {
            return armourClass;
        }

        /// <summary>
        /// Used as accessors only for Player
        /// </summary>
        public override int DamageBase()
        {
            return damageBase;
        }

        /// <summary>
        /// Used as accessors only for Player
        /// </summary>
        public override int DamageModifier()
        {
            return damageModifier;
        }

        public override int HitModifier()
        {
            return hitModifier;
        }

        /// <summary>
        /// Will we have a turn if we IncrementTurnTime()
        /// </summary>
        /// <returns></returns>
        public bool CheckReadyForTurn()
        {
            if(turnClock + speed >= turnClockLimit)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Work out the damage from an attack with the specified one-time modifiers (could be from a special attack etc.)
        /// Note ACmod positive is a bonus to the monster AC
        /// </summary>
        /// <param name="hitMod"></param>
        /// <param name="damBase"></param>
        /// <param name="damMod"></param>
        /// <param name="ACmod"></param>
        /// <returns></returns>


        //int toHitRoll; //just so we can use it in debug

        private int AttackWithModifiers(Monster monster, int hitMod, int damBase, int damMod, int ACmod)
        {
            //Flatline has a rather simple combat system
            IEquippableItem item = GetEquippedWeapon();

            int baseDamage = 2;

            if (item.HasMeleeAction())
            {
                baseDamage = item.MeleeDamage();
            }

            string combatResultsMsg = "PvM " + monster.Representation + " = " + baseDamage;

            return baseDamage;

            /*
            int attackToHit = hitModifier + hitMod;
            int attackDamageMod = damageModifier + damMod;
            
            int attackDamageBase;

            if(damBase > damageBase)
                attackDamageBase = damBase;
            else
                attackDamageBase = damageBase;

            int monsterAC = monster.ArmourClass() + ACmod;
            int toHitRoll = Utility.d20() + attackToHit;

            if (toHitRoll >= monsterAC)
            {
                //Hit - calculate damage
                int totalDamage = Utility.DamageRoll(attackDamageBase) + attackDamageMod;
                string combatResultsMsg = "PvM " + monster.Representation + " ToHit: " + toHitRoll + "[+" + hitModifier + "+" + hitMod + "] AC: " + monsterAC + "(" + monster.ArmourClass() + "+" + ACmod + ") " + " Dam: 1d" + attackDamageBase + "+" + damageModifier + "+" + damMod + " = " + totalDamage;

                //            string combatResultsMsg = "PvM Attack ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monster.Hitpoints + " miss";
                LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                return totalDamage;
            }*/

            //Miss
            //return 0;
        }

        public bool CastSpell(Spell toCast, Point target)
        {
            //Check MP
            if (toCast.MPCost() > MagicPoints)
            {
                Game.MessageQueue.AddMessage("Not enough MP! " + toCast.MPCost().ToString() + " required.");
                LogFile.Log.LogEntryDebug("Not enough MP to cast " + toCast.SpellName(), LogDebugLevel.Medium);

                return false;
            }

            //Check we are in target
            int range = toCast.GetRange();
            /*
            if (Inventory.ContainsItem(new Items.ExtendOrb()))
            {
                range += 1;
            }*/

            if (toCast.NeedsTarget() && Utility.GetDistanceBetween(LocationMap, target) > range)
            {
                Game.MessageQueue.AddMessage("Out of range!");
                LogFile.Log.LogEntryDebug("Out of range for " + toCast.SpellName(), LogDebugLevel.Medium);

                return false;
            }


            //Actually cast the spell
            bool success = toCast.DoSpell(target);

            //Remove MP if successful
            if (success)
            {
                MagicPoints -= toCast.MPCost();
                if (MagicPoints < 0)
                    MagicPoints = 0;

                //Using magic is an instrinsic
                MagicUse = true;
            }

            return success;
        }

        public double CalculateAimBonus()
        {
            var aimBonus = 0.1;

            var aimEffect = GetActiveEffects(typeof(PlayerEffects.AimEnhance));

            if(aimEffect.Count() > 0)
            {
                aimBonus = ((PlayerEffects.AimEnhance)aimEffect.First()).aimEnhanceAmount * 0.3;
            }

            var stationaryBonus = Math.Min(TurnsInactive, 3) * aimBonus;

            var nonFireBonus = Math.Min(TurnsSinceAction, 3) * aimBonus / 2;

            return stationaryBonus + nonFireBonus;
        }

        public double CalculateRangedAttackModifiersOnMonster(Monster target)
        {
            var damageModifier = 1.0;

            //Aiming
            damageModifier += CalculateAimBonus();

            //Enemy moving
            /*if (target != null && target.TurnsMoving > 0)
            {
                damageModifier -= 0.2;
            }*/

            return damageModifier;
        }

        public double CalculateMeleeAttackModifiersOnMonster(Monster target)
        {
            var meleeEffect = GetActiveEffects(typeof(PlayerEffects.SpeedBoost));

            var meleeMultiplier = 1.0;

            if (meleeEffect.Count() > 0)
            {
                meleeMultiplier = ((PlayerEffects.SpeedBoost)meleeEffect.First()).Level * 0.5 + 1;
            }

            return meleeMultiplier;
        }

        public double CalculateDamageModifierForAttacksOnPlayer(Monster target)
        {
            var damageModifier = 1.0;

            var speedEffect = GetActiveEffects(typeof(PlayerEffects.SpeedBoost));

            var speedModifier = 1.0;

            if (speedEffect.Count() > 0)
            {
                speedModifier += ((PlayerEffects.SpeedBoost)speedEffect.First()).Level;
            }

            if(TurnsMoving > 0)
            {
                damageModifier -= 0.2 * speedModifier;
            }

            if (target != null)
            {
                //Test cover
                var coverItems = GetPlayerCover(target);
                var hardCover = coverItems.Item1;
                var softCover = coverItems.Item2;

                if (hardCover > 0)
                    damageModifier -= 0.5;
                if (softCover > 0)
                    damageModifier -= 0.1;
            }

            return Math.Max(0.0, damageModifier);
        }

        public Tuple<int, int> GetPlayerCover()
        {
            var nearestMonster = Game.Dungeon.FindClosestHostileCreatureInFOV(this) as Monster;
            if (nearestMonster == null)
                return new Tuple<int, int>(0, 0);

            return GetPlayerCover(nearestMonster);
        }

        public Tuple<int, int> GetPlayerCover(Monster target)
        {
            if (target == null)
                return new Tuple<int, int>(0, 0);

            var coverItems = Game.Dungeon.GetNumberOfCoverItemsBetweenPoints(target.LocationLevel, target.LocationMap, LocationLevel, LocationMap);
            return coverItems;
        }

        /// <summary>
        /// Normal attack on a monster. Takes care of killing them off if required.
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        public CombatResults AttackMonster(Monster monster)
        {
            return AttackMonsterWithModifiers(monster, 0, 0, 0, 0, false);
        }

        /// <summary>
        /// Attack a monster with modifiers. Takes care of killing them off if required.
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        public CombatResults AttackMonsterWithModifiers(Monster monster, int hitModifierMod, int damageBaseMod, int damageModifierMod, int enemyACMod, bool specialMoveUsed)
        {
            //Do we need to recalculate combat stats?
            if (this.RecalculateCombatStatsRequired)
                this.CalculateCombatStats();

            if (monster.RecalculateCombatStatsRequired)
                monster.CalculateCombatStats();

            //Attacking a monster with hand to hand give an instrinsic
            CombatUse = true;

            //Calculate damage from a normal attack

            int damage = AttackWithModifiers(monster, hitModifierMod, damageBaseMod, damageModifierMod, enemyACMod);

            return ApplyDamageToMonster(monster, damage, false, specialMoveUsed);
        }

        public CombatResults AttackMonsterRanged(Monster monster, int damage)
        {
            var modifiedDamage = (int)Math.Ceiling(CalculateRangedAttackModifiersOnMonster(monster) * damage);
            string combatResultsMsg = "PvM (ranged) " + monster.Representation + "base " + damage + " modified " + modifiedDamage;
            LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

            CancelStealthDueToAttack();
            CancelBoostDueToAttack();

            return ApplyDamageToMonster(monster, modifiedDamage, false, false);
        }

        public CombatResults AttackMonsterThrown(Monster monster, int damage)
        {
            string combatResultsMsg = "PvM (thrown) " + monster.Representation + " = " + damage;
            LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

            CancelStealthDueToAttack();
            CancelBoostDueToAttack();

            return ApplyDamageToMonster(monster, damage, false, false);
        }

        public CombatResults AttackMonsterMelee(Monster monster)
        {

            //Flatline has a rather simple combat system
            IEquippableItem item = GetBestMeleeWeapon();

            int baseDamage = 2;

            if (item != null && item.HasMeleeAction())
            {
                baseDamage = item.MeleeDamage();
            }

            var modifiedDamage = (int)Math.Ceiling(CalculateMeleeAttackModifiersOnMonster(monster) * baseDamage);

            string combatResultsMsg = "PvM (melee) " + monster.Representation + " base " + baseDamage + " mod " + modifiedDamage;
            LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

            CancelStealthDueToAttack();
            ResetTurnsMoving();

            return ApplyDamageToMonster(monster, modifiedDamage, false, false);
        }

        public void CancelStealthDueToAttack()
        {
            if (Game.Dungeon.PlayerCheating)
                return;

            //Forceably unequip any StealthWare and disable for some time
            if (IsWetwareTypeEquipped(typeof(Items.StealthWare)))
            {
                UnequipWetware();
                DisableWetware(typeof(Items.StealthWare), turnsToDisableStealthWareAfterAttack);
            }
        }

        public void CancelBoostDueToAttack()
        {
            if (Game.Dungeon.PlayerCheating)
                return;

            //Forceably unequip any SpeedWare and disable for some time
            if (IsWetwareTypeEquipped(typeof(Items.BoostWare)))
            {
                UnequipWetware();
                DisableWetware(typeof(Items.BoostWare), turnsToDisableBoostWareAfterAttack);
            }
        }

        public void CancelStealthDueToUnequip()
        {

            DisableWetware(typeof(Items.StealthWare), turnsToDisableStealthWareAfterUnequip);
        }

        public void CancelBoostDueToUnequip()
        {

            DisableWetware(typeof(Items.BoostWare), turnsToDisableBoostWareAfterUnequip);

        }

        /// <summary>
        /// Apply stun damage (miss n-turns) to monster. All stun attacks are routed through here
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="stunTurns"></param>
        /// <returns></returns>
        public CombatResults ApplyStunDamageToMonster(Monster monster, int stunTurns)
        {
            //Wake monster up etc.
            AIForMonsterIsAttacked(monster);

            int monsterOrigStunTurns = monster.StunnedTurns;

            //Do we hit the monster?
            if (stunTurns > 0)
            {
                monster.StunnedTurns += stunTurns;

                //Notify the creature that it has taken damage
                //It may activate a special ability or stop running away etc.
                monster.NotifyHitByCreature(this, 0);

                //Message string
                string playerMsg2 = "";
                if (!monster.Unique)
                    playerMsg2 += "The ";
                playerMsg2 += monster.SingleDescription + " is stunned!";
                Game.MessageQueue.AddMessage(playerMsg2);

                string debugMsg2 = "MStun: " + monsterOrigStunTurns + "->" + monster.StunnedTurns;
                LogFile.Log.LogEntryDebug(debugMsg2, LogDebugLevel.Medium);

                return CombatResults.NeitherDied;
            }

            //Miss

            string playerMsg3 = "";
            if (!monster.Unique)
                playerMsg3 += "The ";
            playerMsg3 += monster.SingleDescription + " shrugs off the attack.";
            Game.MessageQueue.AddMessage(playerMsg3);
            string debugMsg3 = "MStun: " + monsterOrigStunTurns + "->" + monster.StunnedTurns;
            LogFile.Log.LogEntryDebug(debugMsg3, LogDebugLevel.Medium);

            return CombatResults.NeitherDied;

        }

        /// <summary>
        /// Apply damage to monster and deal with death. All player attacks are routed through here.
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        public CombatResults ApplyDamageToMonster(Monster monster, int damage, bool magicUse, bool specialMove)
        {
            //Wake monster up etc.
            AIForMonsterIsAttacked(monster);

            //Do we hit the monster?
            if (damage > 0)
            {
                int monsterOrigHP = monster.Hitpoints;

                monster.Hitpoints -= damage;

                bool monsterDead = monster.Hitpoints <= 0;

                //Add HP from the glove if wielded
                SpecialCombatEffectsOnMonster(monster, damage, monsterDead, specialMove);

                //Notify the creature that it has taken damage
                //It may activate a special ability or stop running away etc.
                monster.NotifyHitByCreature(this, damage);

                //Is the monster dead, if so kill it?
                if (monsterDead)
                {
                    //Add it to our list of kills (simply adding the whole object here)
                    KillCount++;
                    Kills.Add(monster);

                    //Message string
                    string playerMsg = "You destroyed ";
                    playerMsg += "the ";
                    playerMsg += monster.SingleDescription + ".";
                    Game.MessageQueue.AddMessage(playerMsg);

                    string debugMsg = "MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " killed";
                    LogFile.Log.LogEntryDebug(debugMsg, LogDebugLevel.Medium);

                    Game.Dungeon.KillMonster(monster, false);

                    //No XP in flatline
                    
                    return CombatResults.DefenderDied;
                }

                //Message string
                string playerMsg2 = "You hit ";
                playerMsg2 += "the ";
                playerMsg2 += monster.SingleDescription + ".";
                Game.MessageQueue.AddMessage(playerMsg2);
                
                string debugMsg2 = "MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " injured";
                LogFile.Log.LogEntryDebug(debugMsg2, LogDebugLevel.Medium);

                return CombatResults.DefenderDamaged;
            }

            //Miss

            string playerMsg3 = "You missed the " + monster.SingleDescription + ".";
            Game.MessageQueue.AddMessage(playerMsg3);
            string debugMsg3 = "MHP: " + monster.Hitpoints + "->" + monster.Hitpoints + " missed";
            LogFile.Log.LogEntryDebug(debugMsg3, LogDebugLevel.Medium);

            return CombatResults.NeitherDied;
        }

        /// <summary>
        /// Monster has been attacked. Wake it up etc.
        /// </summary>
        /// <param name="monster"></param>
        private void AIForMonsterIsAttacked(Monster monster)
        {
            //Set the attacked by marker
            monster.LastAttackedBy = this;
            monster.LastAttackedByID = this.UniqueID;

            //Was this a passive creature? It loses that flag
            if (monster.Passive)
                monster.UnpassifyCreature();

            //Was this a sleeping creature? It loses that flag
            if (monster.Sleeping)
            {
                monster.WakeCreature();

                //All wake on sight creatures should be awake at this point. If it's a non-wake-on-sight tell the player it wakes
                Game.MessageQueue.AddMessage("The " + monster.SingleDescription + " wakes up!");
                LogFile.Log.LogEntryDebug(monster.Representation + " wakes on attack by player", LogDebugLevel.Low);
            }

            //Notify the creature that it has been hit
            monster.NotifyAttackByCreature(this);
        }

        /// <summary>
        /// A monster has been killed by magic or combat. Add XP
        /// </summary>
        /// <param name="magicUse"></param>
        private void AddXPPlayerAttack(Monster monster, bool magicUse)
        {
            //No XP for summonded creatures
            if (monster.WasSummoned)
            {
                LogFile.Log.LogEntryDebug("No XP for summounded creatures.", LogDebugLevel.Medium);
                return;
            }

            //Magic case
            if (magicUse)
            {
                int monsterXP = monster.GetMagicXP();
                double diffDelta = (MagicStat - monsterXP) / (double)MagicStat;
                if (diffDelta < 0)
                    diffDelta = 0;

                double xpUpChance = 1 - diffDelta;
                int xpUpRoll = (int)Math.Floor(xpUpChance * 100.0);
                int xpUpRollActual = Game.Random.Next(100);
                LogFile.Log.LogEntryDebug("MagicXP up. Chance: " + xpUpRoll + " roll: " + xpUpRollActual, LogDebugLevel.Medium);

                if (xpUpRollActual < xpUpRoll)
                {
                    MagicXP++;
                    LogFile.Log.LogEntryDebug("MagicXP up!", LogDebugLevel.Medium);
                    Game.MessageQueue.AddMessage("You feel your magic grow stronger.");
                }
            }
            //Combat use
            else
            {
                int monsterXP = monster.GetCombatXP();
                double diffDelta = (AttackStat - monsterXP) / (double)AttackStat;
                if (diffDelta < 0)
                    diffDelta = 0;

                double xpUpChance = 1 - diffDelta;
                int xpUpRoll = (int)Math.Floor(xpUpChance * 100.0);
                int xpUpRollActual = Game.Random.Next(100);
                LogFile.Log.LogEntryDebug("CombatXP up roll. Chance: " + xpUpRoll + " roll: " + xpUpRollActual, LogDebugLevel.Medium);

                if (xpUpRollActual < xpUpRoll)
                {
                    CombatXP++;
                    LogFile.Log.LogEntryDebug("CombatXP up!", LogDebugLevel.Medium);
                    Game.MessageQueue.AddMessage("You feel your combat skill increase.");
                }
            }
        }

        /// <summary>
        /// List of special combat effects that might happen to a HIT monster
        /// </summary>
        /// <param name="monster"></param>
        private void SpecialCombatEffectsOnMonster(Monster monster, int damage, bool isDead, bool specialMove)
        {
            //If short sword is equipped, do a slow down effect (EXAMPLE)
            /*
            Item shortSword = null;
            foreach (Item item in Inventory.Items)
            {
                if (item as Items.ShortSword != null)
                {
                    shortSword = item as Items.ShortSword;
                    break;
                }
            }

            //If we are using the short sword apply the slow effect
            if (shortSword != null)
            {
                monster.AddEffect(new MonsterEffects.SlowDown(monster, 500, 50));
            }*/

            //If glove is equipped we leech some of the monster HP

            Player player = Game.Dungeon.Player;
            
            Item glove = null;
            foreach (Item item in Inventory.Items)
            {
                /*
                if (item as Items.Glove != null)
                {
                    glove = item as Items.Glove;
                    break;
                }*/
            }

            if (glove != null && specialMove)
            {
                //The glove in PrincessRL only works on special moves

                double hpGain;

              //  if (player.AttackStat < 50)
                    hpGain = damage / 10.0;
              //  else
              //      hpGain = damage / 5.0;

                GainHPFromLeech((int)Math.Ceiling(hpGain));
                


                /*
                //If the monster isn't dead we get 1/5th of the HP done
                if (!isDead)
                {
                    double hpGain = damage / 5.0;

                    if (hpGain > 0.9999)
                    {
                        GainHPFromLeech((int)Math.Ceiling(hpGain));
                    }

                    //If we're become 1 there's only a chance that we gain an HP
                    else
                    {
                        int hpChance = (int) (hpGain * 100.0);
                        if (Game.Random.Next(100) < hpChance)
                            GainHPFromLeech(1);
                    }
                }

                //If monster is dead we get 1/5 of the total HP
                else
                {
                    double hpGain = monster.MaxHitpoints / 10.0;

                    if (hpGain > 0.9999)
                    {
                        GainHPFromLeech((int)Math.Ceiling(hpGain));
                    }

                    //If we're become 1 there's only a chance that we gain an HP
                    else
                    {
                        int hpChance = (int) (hpGain * 100.0);
                        if (Game.Random.Next(100) < hpChance)
                            GainHPFromLeech(1);
                    }
                }*/
            }
        }

        /// <summary>
        /// Increase HP from leech attack up to overdrive limit
        /// </summary>
        /// <param name="numHP"></param>
        internal void GainHPFromLeech(int numHP)
        {
            hitpoints += numHP;

            if (hitpoints > maxHitpoints)
                hitpoints = maxHitpoints;

            LogFile.Log.LogEntryDebug("Gain " + numHP + " hp from leech.", LogDebugLevel.Medium);
        }

        /// <summary>
        /// Increment time on all player events. Events that expire will run their onExit() routines and then delete themselves from the list
        /// </summary>
        internal void IncrementEventTime()
        {
            //Increment time on events and remove finished ones
            List<PlayerEffect> finishedEffects = new List<PlayerEffect>();

            bool eventEnded = false;

            foreach (PlayerEffect effect in effects)
            {
                effect.IncrementTime(this);

                if (effect.HasEnded())
                {
                    finishedEffects.Add(effect);
                    eventEnded = true;
                }
            }

            //Remove finished effects
            foreach (PlayerEffect effect in finishedEffects)
            {
                effects.Remove(effect);
            }

            if(eventEnded)
                CalculateCombatStats();
        }

        /// <summary>
        /// Remove all effects on player
        /// </summary>
        internal void RemoveAllEffects()
        {
            //Increment time on events and remove finished ones
            List<PlayerEffect> finishedEffects = new List<PlayerEffect>();

            foreach (PlayerEffect effect in effects)
            {
                if(!effect.HasEnded())
                    effect.OnEnd(this);

                finishedEffects.Add(effect);
            }

            //Remove finished effects
            foreach (PlayerEffect effect in finishedEffects)
            {
                effects.Remove(effect);
            }

            //Check the effect on our stats
            CalculateCombatStats();
        }

        /// <summary>
        /// Increment time on all player events then use the base class to increment time on the player's turn counter
        /// </summary>
        /// <returns></returns>
        internal override bool IncrementTurnTime()
        {
            IncrementEventTime();

            //Work around for bizarre problem - shouldn't happen any more
            if (speed < 30)
            {
                LogFile.Log.LogEntryDebug("ERROR! Player's speed reduced to <30", LogDebugLevel.High);
                speed = 100;
            }

            OverdriveHitpointDecay();

            DecreaseWetwareDisabledCounts();

            return base.IncrementTurnTime();
        }

        private void DecreaseWetwareDisabledCounts()
        {
            //yuck
            var allKeys = wetwareDisabledTurns.Keys.ToList();
            for (int i = 0; i < allKeys.Count; i++)
            {
                wetwareDisabledTurns[allKeys[i]] = Math.Max(wetwareDisabledTurns[allKeys[i]] - 1, 0);
            }
        }


        int overDriveDecayCounter = 0;

        /// <summary>
        /// If we're over our max hitpoint, they decay slowly
        /// This function is typically called 100 times per turn for a normal speed character
        /// </summary>
        private void OverdriveHitpointDecay()
        {
            overDriveDecayCounter++;

            if (hitpoints <= maxHitpoints)
                return;

            //Lose 1% of overdrive HP rounded up per turn
            if (overDriveDecayCounter > 1000)
            {
                overDriveDecayCounter = 0;

                //Proportional decay
                double hpToLose = (hitpoints - maxHitpoints) / 100.0;
                int hpLoss = (int)Math.Ceiling(hpToLose);

                hitpoints -= hpLoss;

                if (hitpoints < maxHitpoints)
                    hitpoints = maxHitpoints;
            }
        }

        /// <summary>
        /// Run an effect on the player. Calls the effect's onStart and adds it to the current effects queue
        /// </summary>
        /// <param name="effect"></param>
        internal void AddEffect(PlayerEffect effect)
        {
            effects.Add(effect);

            effect.OnStart(this);

            //Check if it altered our combat stats
            CalculateCombatStats();
            
            //Should be done in effect itself or optionally each time we attack
            //I prefer it done here, less to remember
        }

        /// <summary>
        /// Is this class of effect currently active?
        /// Refactor to take a Type not an object
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public bool IsEffectActive(Type effectType)
        {
            PlayerEffect activeEffect = effects.Find(x => x.GetType() == effectType);

            if (activeEffect != null)
                return true;
            
            return false;
        }

        /// <summary>
        /// Is this class of effect currently active?
        /// Refactor to take a Type not an object
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public IEnumerable<PlayerEffect> GetActiveEffects(Type effectType)
        {
            return effects.FindAll(x => x.GetType() == effectType);
        }

        protected override char GetRepresentation()
        {
            var weapon = GetEquippedWeapon();

            if (weapon != null)
            {
                if (weapon.GetType() == typeof(Items.Fists))
                    return (char)257;

                if (weapon.GetType() == typeof(Items.Pistol))
                    return (char)513;

                if (weapon.GetType() == typeof(Items.HeavyPistol))
                    return (char)512;

                if (weapon.GetType() == typeof(Items.Shotgun))
                    return (char)514;

                if (weapon.GetType() == typeof(Items.AssaultRifle))
                    return (char)515;

                if (weapon.GetType() == typeof(Items.HeavyShotgun))
                    return (char)516;

                if (weapon.GetType() == typeof(Items.Laser))
                    return (char)517;

                if (weapon.GetType() == typeof(Items.HeavyLaser))
                    return (char)517;

                if (weapon.GetType() == typeof(Items.Vibroblade))
                    return (char)518;

                if (weapon.GetType() == typeof(Items.FragGrenade))
                    return (char)521;

                if (weapon.GetType() == typeof(Items.StunGrenade))
                    return (char)522;

                if (weapon.GetType() == typeof(Items.SoundGrenade))
                    return (char)520;
            }
            return (char)257;
        }

        /// <summary>
        /// Equip an item. Item is removed from the main inventory.
        /// Returns true if item was used successfully.
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        public bool EquipItem(InventoryListing selectedGroup)
        {
            //Select the first item in the stack
            int itemIndex = selectedGroup.ItemIndex[0];
            Item itemToUse = Inventory.Items[itemIndex];

            //Check if this item is equippable
            IEquippableItem equippableItem = itemToUse as IEquippableItem;

            if (equippableItem == null)
            {
                LogFile.Log.LogEntryDebug("Can't equip item, not equippable: " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Can't equip " + itemToUse.SingleItemDescription);
                return false;
            }

            //Find all matching slots available on the player

            List<EquipmentSlot> itemPossibleSlots = equippableItem.EquipmentSlots;
            List<EquipmentSlotInfo> matchingEquipSlots = new List<EquipmentSlotInfo>();

            foreach (EquipmentSlot slotType in itemPossibleSlots)
            {
                matchingEquipSlots.AddRange(this.EquipmentSlots.FindAll(x => x.slotType == slotType));
            }

            //No suitable slots
            if (matchingEquipSlots.Count == 0)
            {
                LogFile.Log.LogEntryDebug("Can't equip item, no valid slots: " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Can't equip " + itemToUse.SingleItemDescription);

                return false;
            }

            //Look for first empty slot

            EquipmentSlotInfo freeSlot = matchingEquipSlots.Find(x => x.equippedItem == null);

            if (freeSlot == null)
            {
                //Not slots free, unequip first slot
                Item oldItem = matchingEquipSlots[0].equippedItem;
                IEquippableItem oldItemEquippable = oldItem as IEquippableItem;

                //Sanity check
                if (oldItemEquippable == null)
                {
                    LogFile.Log.LogEntry("Currently equipped item is not equippable!: " + oldItem.SingleItemDescription);
                    return false;
                }

                //Run unequip routine
                oldItemEquippable.UnEquip(this);
                oldItem.IsEquipped = false;
                
                //Can't do this right now, since not in inventory items appear on the floor

                //This slot is now free
                freeSlot = matchingEquipSlots[0];
            }

            //We now have a free slot to equip in

            //Put new item in first relevant slot and run equipping routine
            matchingEquipSlots[0].equippedItem = itemToUse;
            equippableItem.Equip(this);
            itemToUse.IsEquipped = true;

            //Update the inventory listing since equipping an item changes its stackability
            Inventory.RefreshInventoryListing();

            //Message the user
            LogFile.Log.LogEntryDebug("Item equipped: " + itemToUse.SingleItemDescription, LogDebugLevel.Low);
            Game.MessageQueue.AddMessage(itemToUse.SingleItemDescription + " equipped.");

            return true;
        }

        public bool ToggleEquipWetware(Type wetwareTypeToEquip)
        {
            if(!IsWetwareTypeAvailable(wetwareTypeToEquip)) {
                LogFile.Log.LogEntryDebug("Do not have wetware of type: " + wetwareTypeToEquip.ToString(), LogDebugLevel.Medium);
                return false;
            }

            var justUnequip = false;
            var currentlyEquippedWetware = GetEquippedWetware();
            if (currentlyEquippedWetware != null && currentlyEquippedWetware.GetType() == wetwareTypeToEquip)
            {
                justUnequip = true;
            }
            
            UnequipWetware();

            if (currentlyEquippedWetware is Items.StealthWare)
                CancelStealthDueToUnequip();

            if (currentlyEquippedWetware is Items.BoostWare)
                CancelBoostDueToUnequip();

            if (justUnequip)
                return true;
                //return false;

            var equipTime = EquipWetware(wetwareTypeToEquip);
            return equipTime;
        }

        internal bool EquipWetware(Type wetwareTypeToEquip)
        {
            //Check if we have this item
            var wetwareOfTypeInInventory = Inventory.GetItemsOfType(wetwareTypeToEquip);

            if (wetwareOfTypeInInventory.Count == 0)
            {
                LogFile.Log.LogEntryDebug("Do not have wetware of type: " + wetwareTypeToEquip.ToString(), LogDebugLevel.Medium);
                return false;
            }

            Item wetwareToEquip = wetwareOfTypeInInventory[0];
            IEnumerable<Item> wetwareToFind;

            if (wetwareTypeToEquip == typeof(Items.ShieldWare))
            {
                wetwareToFind = wetwareOfTypeInInventory.Cast<Items.ShieldWare>().Where(s => s.level == 3);
                if (!wetwareToFind.Any())
                    wetwareToFind = wetwareOfTypeInInventory.Cast<Items.ShieldWare>().Where(s => s.level == 2);
                if (!wetwareToFind.Any())
                    wetwareToFind = wetwareOfTypeInInventory.Cast<Items.ShieldWare>().Where(s => s.level == 1);

                wetwareToEquip = wetwareToFind.First();
            }

            if (wetwareTypeToEquip == typeof(Items.BoostWare))
            {
                wetwareToFind = wetwareOfTypeInInventory.Cast<Items.BoostWare>().Where(s => s.level == 3);
                if (!wetwareToFind.Any())
                    wetwareToFind = wetwareOfTypeInInventory.Cast<Items.BoostWare>().Where(s => s.level == 2);
                if (!wetwareToFind.Any())
                    wetwareToFind = wetwareOfTypeInInventory.Cast<Items.BoostWare>().Where(s => s.level == 1);

                wetwareToEquip = wetwareToFind.First();
            }

            if (wetwareTypeToEquip == typeof(Items.AimWare))
            {
                wetwareToFind = wetwareOfTypeInInventory.Cast<Items.AimWare>().Where(s => s.level == 3);
                if (!wetwareToFind.Any())
                    wetwareToFind = wetwareOfTypeInInventory.Cast<Items.AimWare>().Where(s => s.level == 2);
                if (!wetwareToFind.Any())
                    wetwareToFind = wetwareOfTypeInInventory.Cast<Items.AimWare>().Where(s => s.level == 1);

                wetwareToEquip = wetwareToFind.First();
            }

            //Check if it is disabled

            if (wetwareDisabledTurns.ContainsKey(wetwareToEquip))
            {
                if (wetwareDisabledTurns[wetwareToEquip] > 0)
                {
                    LogFile.Log.LogEntryDebug("Can't enable wetware, is disabled for " + wetwareDisabledTurns[wetwareToEquip] + "turns", LogDebugLevel.Medium);
                    return false;
                }
            }

            //Equip the new wetware
            EquipmentSlotInfo wetwareSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Wetware);

            if (wetwareSlot == null)
            {
                LogFile.Log.LogEntryDebug("Can't find wetware slot - bug ", LogDebugLevel.High);
                return false;
            }
            
            var wetwareToEquipAsEquippable = wetwareToEquip as IEquippableItem;
            wetwareToEquip.IsEquipped = true;
            wetwareSlot.equippedItem = wetwareToEquip;
            wetwareToEquipAsEquippable.Equip(this);

            return true;
        }

        private void UnequipWetware()
        {
            var currentlyEquippedWetware = GetEquippedWetware();
            if (currentlyEquippedWetware != null)
            {
                var currentlyEquippedWetwareItem = currentlyEquippedWetware as Item;
                currentlyEquippedWetware.UnEquip(this);
                currentlyEquippedWetwareItem.IsEquipped = false;

                EquipmentSlotInfo wetwareSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Wetware);

                if (wetwareSlot == null)
                {
                    LogFile.Log.LogEntryDebug("Can't find wetware slot - bug ", LogDebugLevel.High);
                    return;
                }

                wetwareSlot.equippedItem = null;

            }

            CalculateCombatStats();
        }


        /// <summary>
        /// Drop an item at a specific point. Equippable items never exist in the inventory in FlatlineRL
        /// </summary>
        /// <param name="itemToDrop"></param>
        /// <returns></returns>
        public bool DropEquippableItem(Item itemToDrop, int levelToDropAt, Point locToDropAt)
        {
            //Remove from inventory
            Inventory.RemoveItem(itemToDrop);

            itemToDrop.LocationLevel = levelToDropAt;
            itemToDrop.LocationMap = locToDropAt;
            itemToDrop.InInventory = false;

            return true;
        }

        /// <summary>
        /// Drop an item at current location. Equippable items never exist in the inventory in FlatlineRL
        /// </summary>
        /// <param name="itemToDrop"></param>
        /// <returns></returns>
        public bool DropEquippableItem(Item itemToDrop)
        {
            return DropEquippableItem(itemToDrop, this.LocationLevel, this.LocationMap);
        }



        internal Type HeavyWeaponTranslation(Type itemType)
        {

            //Do weapon translations
            if (itemType == typeof(Items.Pistol) && IsInventoryTypeAvailable(typeof(Items.HeavyPistol)))
                return typeof(Items.HeavyPistol);

            if (itemType == typeof(Items.Shotgun) && IsInventoryTypeAvailable(typeof(Items.HeavyShotgun)))
                return typeof(Items.HeavyShotgun);

            if (itemType == typeof(Items.Laser) && IsInventoryTypeAvailable(typeof(Items.HeavyLaser)))
                return typeof(Items.HeavyLaser);

            if (itemType == typeof(Items.Fists) && IsInventoryTypeAvailable(typeof(Items.Vibroblade)))
                return typeof(Items.Vibroblade);

            return itemType;
        }

        internal bool EquipInventoryItemType(Type itemType, bool reequip=false)
        {
            itemType = HeavyWeaponTranslation(itemType);

            var invAvailable = IsInventoryTypeAvailable(itemType);
            if (!invAvailable)
            {
                LogFile.Log.LogEntryDebug("Can't equip inventory type " + itemType + " - not in inventory", LogDebugLevel.Medium);

            }

            var equipSuccess = false;
            if(invAvailable)
                equipSuccess = EquipAndReplaceItem(Inventory.GetItemsOfType(itemType).First());

            if (equipSuccess == false && reequip == false)
            {
                //Try to reequip melee
                EquipBestMeleeWeapon();
            }
            return false;
        }
        
        public virtual bool PickUpItem(Item itemToPickUp)
        {
            base.PickUpItem(itemToPickUp);

            if (AutoequipItem(itemToPickUp))
            {
                EquipAndReplaceItem(itemToPickUp);
            }

            return true;
        }

        private bool AutoequipItem(Item itemToPickUp)
        {
            if (itemToPickUp is Items.Fists)
                return true;

            if (itemToPickUp is Items.Vibroblade)
                return true;

            if (itemToPickUp is Items.Pistol)
                return true;

            if (itemToPickUp is Items.Shotgun)
                return true;

            if (itemToPickUp is Items.AssaultRifle)
                return true;

            if (itemToPickUp is Items.Laser)
                return true;

            if (itemToPickUp is Items.HeavyPistol)
                return true;

            if (itemToPickUp is Items.HeavyShotgun)
                return true;

            if (itemToPickUp is Items.HeavyLaser)
                return true;

            return false;
        }

        /// <summary>
        /// Equip an item into a relevant slot.
        /// Will unequip and drop an item in the same slot.
        /// Returns true if operation successful
        /// Should be called after picking item up
        /// </summary>
        /// <param name="selectedGroup"></param>
        /// <returns></returns>
        public bool EquipAndReplaceItem(Item itemToUse)
        {
            //Check if this item is equippable
            IEquippableItem equippableItem = itemToUse as IEquippableItem;

            if (equippableItem == null)
            {
                LogFile.Log.LogEntryDebug("Can't equip item, not equippable: " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Can't equip " + itemToUse.SingleItemDescription);
                return false;
            }
            
            //Check item is in inventory
            if (!Inventory.ContainsItem(itemToUse))
            {
                LogFile.Log.LogEntryDebug("Can't equip item, not in inventory: " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);
                return false;
            };

            //Find all matching slots available on the player

            List<EquipmentSlot> itemPossibleSlots = equippableItem.EquipmentSlots;
            //Is always only 1 slot in FlatlineRL

            EquipmentSlot itemSlot = itemPossibleSlots[0];
            
            //We always have 2 equipment slots, 1 of each type on a player in FlatlineRL
            //So we should match exactly on 1 free slot

            List<EquipmentSlotInfo> matchingEquipSlots = new List<EquipmentSlotInfo>();

            foreach (EquipmentSlot slotType in itemPossibleSlots)
            {
                matchingEquipSlots.AddRange(this.EquipmentSlots.FindAll(x => x.slotType == slotType));
            }

            //No suitable slots
            if (matchingEquipSlots.Count == 0)
            {
                LogFile.Log.LogEntryDebug("Can't equip item, no valid slots: " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Can't equip " + itemToUse.SingleItemDescription);

                return false;
            }

            //Look for first empty slot

            EquipmentSlotInfo freeSlot = matchingEquipSlots.Find(x => x.equippedItem == null);

            if (freeSlot == null)
            {
                //Not slots free, unequip first slot
                Item oldItem = matchingEquipSlots[0].equippedItem;
                IEquippableItem oldItemEquippable = oldItem as IEquippableItem;

                //Sanity check
                if (oldItemEquippable == null)
                {
                    LogFile.Log.LogEntry("Old item did not equip: " + oldItem.SingleItemDescription);
                    return false;
                }

                //We destroy obselete ware
                if (IsObselete(oldItem))
                {
                    LogFile.Log.LogEntryDebug("Item discarded: " + oldItem.SingleItemDescription, LogDebugLevel.Low);
                    Game.MessageQueue.AddMessage("Discarding obselete " + oldItem.SingleItemDescription + ".");

                    UnequipAndDestroyItem(oldItem);
                }
                else
                    UnequipItem(oldItem);

                //This slot is now free
                freeSlot = matchingEquipSlots[0];
            }

            //We now have a free slot to equip in

            //Put new item in first relevant slot and run equipping routine
            matchingEquipSlots[0].equippedItem = itemToUse;
            equippableItem.Equip(this);
            itemToUse.IsEquipped = true;

            LogFile.Log.LogEntryDebug("Equipping new item " + itemToUse.SingleItemDescription, LogDebugLevel.Medium);

            //Message the user
            LogFile.Log.LogEntryDebug("Item equipped: " + itemToUse.SingleItemDescription, LogDebugLevel.Low);
            Game.MessageQueue.AddMessage(itemToUse.SingleItemDescription + " equipped.");

            return true;
        }

        private bool IsObselete(Item oldItem)
        {
            if (oldItem is Items.Pistol && IsWeaponTypeAvailable(typeof(Items.HeavyPistol)))
                return true;

            if (oldItem is Items.Shotgun && IsWeaponTypeAvailable(typeof(Items.HeavyShotgun)))
                return true;

            if (oldItem is Items.Laser && IsWeaponTypeAvailable(typeof(Items.HeavyLaser)))
                return true;
            return false;
        }

        /// <summary>
        /// FlatlineRL - unequip and item at drop at player loc
        /// </summary>
        public void UnequipAndDropItem(Item item)
        {
            UnequipAndDropItem(item, this.LocationLevel, this.LocationMap);
        }

        /// <summary>
        /// FlatlineRL - unequip and item at drop at the coords given
        /// </summary>
        public void UnequipAndDropItem(Item item, int levelToDrop, Point toDropLoc)
        {
            UnequipItem(item);

            //Drop the old item
            DropEquippableItem(item, levelToDrop, toDropLoc);
        }

        private void UnequipItem(Item item)
        {
            //Run unequip routine
            IEquippableItem equipItem = item as IEquippableItem;

            if (equipItem == null)
            {
                LogFile.Log.LogEntryDebug("UnequipItem - item not equippable " + item.SingleItemDescription, LogDebugLevel.High);
                return;
            }

            equipItem.UnEquip(this);
            item.IsEquipped = false;

            //Locate the slot it was in and empty it
            EquipmentSlotInfo oldSlot = EquipmentSlots.Find(x => x.equippedItem == item);
            if (oldSlot == null)
            {
                LogFile.Log.LogEntryDebug("Error - can't find equipment slot for item " + item.SingleItemDescription, LogDebugLevel.High);
            }
            else
            {
                oldSlot.equippedItem = null;
            }
        }

        public void UnequipAndDestoryAllItems()
        {
            foreach (EquipmentSlotInfo es in EquipmentSlots)
            {
                UnequipAndDestroyItem(es.equippedItem);
            }
        }

        /// <summary>
        /// FlatlineRL - unequip item and remove it from the game
        /// </summary>
        public void UnequipAndDestroyItem(Item item)
        {
            if (item == null)
                return;

            //Run unequip routine
            IEquippableItem equipItem = item as IEquippableItem;
            equipItem.UnEquip(this);
            item.IsEquipped = false;

            //Locate the slot it was in and empty it
            EquipmentSlotInfo oldSlot = EquipmentSlots.Find(x => x.equippedItem == item);
            if (oldSlot == null)
            {
                LogFile.Log.LogEntryDebug("Error - can't find equipment slot for item " + item.SingleItemDescription, LogDebugLevel.High);
            }
            else
            {
                oldSlot.equippedItem = null;
            }

            //Delete the old item
            Inventory.RemoveItemAndDestroy(item);
            //There should now be no references to it

        }

        /// <summary>
        /// TraumaRL - return equipped wetware or null
        /// </summary>
        /// <returns></returns>
        public IEquippableItem GetEquippedWetware()
        {
            EquipmentSlotInfo weaponSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Wetware);

            if (weaponSlot == null)
            {
                LogFile.Log.LogEntryDebug("Can't find wetware slot - bug ", LogDebugLevel.High);
                return null;
            }

            return weaponSlot.equippedItem as IEquippableItem;
        }

        public bool IsWetwareTypeEquipped(Type wetwareType)
        {
            var equippedWetware = GetEquippedWetware();

            if (equippedWetware != null && equippedWetware.GetType() == wetwareType)
                return true;

            return false;
        }

        /// <summary>
        /// FlatlineRL - return equipped weapon or null
        /// </summary>
        /// <returns></returns>
        public IEquippableItem GetEquippedWeapon() 
        {
            EquipmentSlotInfo weaponSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Weapon);

            if(weaponSlot == null) {
                LogFile.Log.LogEntryDebug("Can't find weapon slot - bug ", LogDebugLevel.High);
                return null;
            }

            return weaponSlot.equippedItem as IEquippableItem;
        }

        public bool HasMeleeWeaponEquipped()
        {
            var currentWeapon = GetEquippedWeapon();

            if (currentWeapon == null)
                return false;

            if (currentWeapon.GetType() == typeof(Items.Fists) || currentWeapon.GetType() == typeof(Items.Vibroblade))
                return true;

            return false;
        }

        public bool HasThrownWeaponEquipped()
        {
            var currentWeapon = GetEquippedWeapon();

            if (currentWeapon == null)
                return false;

            if (currentWeapon.GetType() == typeof(Items.FragGrenade) || currentWeapon.GetType() == typeof(Items.StunGrenade) || currentWeapon.GetType() == typeof(Items.SoundGrenade))
                return true;

            return false;
        }



        /// <summary>
        /// FlatlineRL - return equipped weapon as item reference (always works)
        /// </summary>
        /// <returns></returns>
        public Item GetEquippedWeaponAsItem()
        {
            EquipmentSlotInfo weaponSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Weapon);

            if(weaponSlot == null) {
                LogFile.Log.LogEntryDebug("Can't find weapon slot - bug ", LogDebugLevel.High);
                return null;
            }

            return weaponSlot.equippedItem;
        }

        /// <summary>
        /// FlatlineRL - return equipped utility or null
        /// </summary>
        /// <returns></returns>
        public IEquippableItem GetEquippedUtility()
        {
            EquipmentSlotInfo weaponSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Utility);

            if(weaponSlot == null) {
                LogFile.Log.LogEntryDebug("Can't find utility slot - bug ", LogDebugLevel.High);
                return null;
            }

            return weaponSlot.equippedItem as IEquippableItem;
        }

        /// <summary>
        /// FlatlineRL - return equipped utility or null
        /// </summary>
        /// <returns></returns>
        public Item GetEquippedUtilityAsItem()
        {
            EquipmentSlotInfo weaponSlot = this.EquipmentSlots.Find(x => x.slotType == EquipmentSlot.Utility);

            if(weaponSlot == null) {
                LogFile.Log.LogEntryDebug("Can't find utility slot - bug ", LogDebugLevel.High);
                return null;
            }

            return weaponSlot.equippedItem;
        }

        public void GiveAllWetware(int level)
        {
            if (level == 3)
            {
                Inventory.AddItemNotFromDungeon(new Items.StealthWare());
                Inventory.AddItemNotFromDungeon(new Items.ShieldWare(3));
                Inventory.AddItemNotFromDungeon(new Items.AimWare(3));
                Inventory.AddItemNotFromDungeon(new Items.BoostWare(3));
            }

            if (level == 2)
            {
                Inventory.AddItemNotFromDungeon(new Items.StealthWare());
                Inventory.AddItemNotFromDungeon(new Items.ShieldWare(2));
                Inventory.AddItemNotFromDungeon(new Items.AimWare(2));
                Inventory.AddItemNotFromDungeon(new Items.BoostWare(2));
            }

            if (level == 1)
            {
                Inventory.AddItemNotFromDungeon(new Items.StealthWare());
                Inventory.AddItemNotFromDungeon(new Items.ShieldWare(1));
                Inventory.AddItemNotFromDungeon(new Items.AimWare(1));
                Inventory.AddItemNotFromDungeon(new Items.BoostWare(1));
            }

            Inventory.AddItemNotFromDungeon(new Items.BioWare());
        }

        public void GiveItemNotFromDungeon(Item item)
        {
            Inventory.AddItemNotFromDungeon(item);
        }

        public void GiveAllWeapons(int level)
        {
            if (level == 1)
            {
                Inventory.AddItemNotFromDungeon(new Items.Vibroblade());
                Inventory.AddItemNotFromDungeon(new Items.AssaultRifle());
                Inventory.AddItemNotFromDungeon(new Items.Pistol());
                Inventory.AddItemNotFromDungeon(new Items.Shotgun());
                Inventory.AddItemNotFromDungeon(new Items.Laser());
            
                
            }

            if (level == 2)
            {

                Inventory.AddItemNotFromDungeon(new Items.HeavyPistol());
                Inventory.AddItemNotFromDungeon(new Items.HeavyShotgun());
                Inventory.AddItemNotFromDungeon(new Items.HeavyLaser());
            }

            for (int i = 0; i < 5; i++)
            {
                Inventory.AddItemNotFromDungeon(new Items.FragGrenade());
                Inventory.AddItemNotFromDungeon(new Items.StunGrenade());
                Inventory.AddItemNotFromDungeon(new Items.SoundGrenade());
                Inventory.AddItemNotFromDungeon(new Items.NanoRepair());
            }

            //Inventory.AddItemNotFromDungeon(new Items.Pistol());
            //Inventory.AddItemNotFromDungeon(new Items.Shotgun());
            //Inventory.AddItemNotFromDungeon(new Items.Laser());

            
        }
        public void EquipStartupWeapons() {
            //Non debug from here
            //Start with fists equipped
            Inventory.AddItemNotFromDungeon(new Items.Fists());
            Game.Dungeon.Player.EquipInventoryItemType(ItemMapping.WeaponMapping[1]);
        }

        /// <summary>
        /// Predicate for matching equipment slot of EquipmentSlot type
        /// </summary>
        private static bool EquipmentSlotMatchesType(EquipmentSlotInfo equipSlot, EquipmentSlot type)
        {
            return (equipSlot.slotType == type);
        }

        /// <summary>
        /// Use the item group. Function is responsible for deleting the item if used up etc. Return true if item was used successfully and time should be advanced.
        /// </summary>
        /// <param name="selectedGroup"></param>
        internal bool UseItem(InventoryListing selectedGroup)
        {
            //For now, we use the first item in any stack only
            int itemIndex = selectedGroup.ItemIndex[0];
            Item itemToUse = Inventory.Items[itemIndex];

            //Check if this is a useable item
            IUseableItem useableItem = itemToUse as IUseableItem;

            if (useableItem == null)
            {
                Game.MessageQueue.AddMessage("Cannot use this type of item!");
                LogFile.Log.LogEntry("Tried to use non-useable item: " + itemToUse.SingleItemDescription);
                return false;
            }

            bool usedSuccessfully = useableItem.Use(this);

            if (useableItem.UsedUp)
            {
                //Remove item from inventory and don't drop on floor
                //Goes back into the global list and will be respawned at town
                //Inventory.RemoveItem(itemToUse);
                
                //This permanently deletes it from the game
                //Game.Dungeon.RemoveItem(itemToUse);

                //If the line above is commented, the item will be returned to town. Will want to un-use it in this case
                
                //Only ditch the non-equippable items
                IEquippableItem equipItem = useableItem as IEquippableItem;
                if (equipItem == null)
                {
                    Inventory.RemoveItem(itemToUse);
                }

                

                //useableItem.UsedUp = false;
                
            }

            return usedSuccessfully;
        }



        /// <summary>
        /// Simpler version of equip item, doesn't care about slots
        /// </summary>
        /// <param name="equipItem"></param>
        internal bool EquipItemNoSlots(IEquippableItem equipItem)
        {
            Item item = equipItem as Item;

            if (item == null)
            {
                //Should never happen
                LogFile.Log.LogEntry("Problem with item equip");
                Game.MessageQueue.AddMessage("You can't equip this item (bug)");
                return false;
            }

            //Play help movie
            if (Game.Dungeon.Player.PlayItemMovies && ItemHelpMovieSeen == false)
            {
                //Screen.Instance.PlayMovie("helpitems", true);
                ItemHelpMovieSeen = true;
            }

            //Set the item as found
            item.IsFound = true;

            //If we have room in our equipped slots, equip and add the item to the inventory
            if (CurrentEquippedItems < MaximumEquippedItems)
            {
                //Add the item to our inventory
                item.IsEquipped = true;
                Inventory.AddItem(item);

                CurrentEquippedItems++;

                //Let the item do its equip action
                //This can happen multiple times in PrincessRL since items can be dropped
                //Probably just play a video
                equipItem.Equip(this);

                //Update the player's combat stats which may have been affected

                CalculateCombatStats();

                //Update the inventory listing since equipping an item changes its stackability
                //No longer necessary since no equippable items get displayed in inventory
                //Inventory.RefreshInventoryListing();

                //Message the user
                LogFile.Log.LogEntryDebug("Item equipped: " + item.SingleItemDescription, LogDebugLevel.Medium);
                //Game.MessageQueue.AddMessage(item.SingleItemDescription + " found.");

                return true;
            }
            else if (LocationLevel == 0)
            {
                //If not, and we're in town, don't pick it up
                Game.MessageQueue.AddMessage("You can't carry any more items. Press 'd' to drop your current items.");
                LogFile.Log.LogEntryDebug("Max number of items reached", LogDebugLevel.Medium);

                return false;
            }
            else
            {
                //If not, and we're not in town, set it as inInventory so it won't be drawn. It'll get returned to town on when go back
                item.InInventory = true;

                //Play the video
                equipItem.Equip(this);

                Game.MessageQueue.AddMessage("You place the " + item.SingleItemDescription + " in your backpack.");
                LogFile.Log.LogEntryDebug("Max number of items reached. Item returns to town.", LogDebugLevel.Medium);

                return true;
            }          
        }

        /// <summary>
        /// Level up the player!
        /// </summary>
        internal void LevelUp()
        {
            //Level up!
            Level++;

            int lastMaxHP = maxHitpoints;

            //Recalculate combat stats
            CalculateCombatStats();

            hitpoints += maxHitpoints - lastMaxHP;

            //Calculate HP etc
            //HPOnLevelUP();

            LogFile.Log.LogEntry("Player levels up to: " + Level);
        }

        /// <summary>
        /// Apply level up effect to current hitpoints
        /// </summary>
        private void HPOnLevelUP()
        {
            hitpoints += 10;
            maxHitpoints += 10;
        }

        /// <summary>
        /// Try to add another charmed creature. Will return false if already at max.
        /// </summary>
        internal bool AddCharmCreatureIfPossible()
        {
            if (CurrentCharmedCreatures < MaxCharmedCreatures)
            {
                CurrentCharmedCreatures++;
                return true;
            }

            return false;
        }

        internal bool MoreCharmedCreaturesPossible()
        {
            if (CurrentCharmedCreatures < MaxCharmedCreatures)
                return true;
            return false;
        }

        internal void RemoveCharmedCreature()
        {
            CurrentCharmedCreatures--;

            if (CurrentCharmedCreatures < 0)
            {
                LogFile.Log.LogEntryDebug("tried to remove a charmed creature when there were 0", LogDebugLevel.High);
                CurrentCharmedCreatures = 0;
            }
        }

        /// <summary>
        /// This happens when a charmed creature attacks another or a non-charmed creature fights back
        /// </summary>
        /// <param name="attackingMonster"></param>
        /// <param name="targetMonster"></param>
        internal void AddXPMonsterAttack(Monster attackingMonster, Monster targetMonster)
        {
            //Check this monster was charmed
            if (!attackingMonster.Charmed)
            {
                LogFile.Log.LogEntryDebug("Attacking monster was not charmed, no XP.", LogDebugLevel.Medium);
                return;
            }

          //Add charm XP. Use the target's combat XP against the player's charm stat
            //This also has the advantage that every creature in the game has a combat XP
            int monsterXP = targetMonster.GetCombatXP();
            double diffDelta = (CharmStat - monsterXP) / (double)CharmStat;
            if (diffDelta < 0)
                 diffDelta = 0;

                double xpUpChance = 1 - diffDelta;
                int xpUpRoll = (int)Math.Floor(xpUpChance * 100.0);
                int xpUpRollActual = Game.Random.Next(100);
                LogFile.Log.LogEntryDebug("CharmXP up. Chance: " + xpUpRoll + " roll: " + xpUpRollActual, LogDebugLevel.Medium);

                if (xpUpRollActual < xpUpRoll)
                {
                    CharmXP++;
                    LogFile.Log.LogEntryDebug("CharmXP up!", LogDebugLevel.Medium);
                    Game.MessageQueue.AddMessage("You feel more charming.");
                }
            
        }

        internal void ResetTemporaryPlayerStats()
        {
            CurrentCharmedCreatures = 0;
        }

        /// <summary>
        /// Do setup just before the game starts. Dungeons etc. all ready to go.
        /// </summary>
        public void StartGameSetup()
        {
            CalculateCombatStats();

            //keep this
            EquipStartupWeapons();
        }

        /// <summary>
        /// Important to keep this the only place where the player injures themselves
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual CombatResults AttackPlayer(int damage)
        {
            //Do we hit the player?
            if (damage > 0)
            {
                int monsterOrigHP = Hitpoints;

                //bypasses cover etc.
                //var modifiedDamaged = (int)Math.Floor(CalculateDamageModifierForAttacksOnPlayer(this) * damage);

                ApplyDamageToPlayer(damage);

                //Hitpoints -= damage;

                //Is the player dead, if so kill it?
                if (Hitpoints <= 0)
                {

                    //Message queue string
                    string combatResultsMsg = "PvP Dam: " + damage + " HP: " + monsterOrigHP + "->" + Hitpoints + " killed";

                    //string playerMsg = "The " + this.SingleDescription + " hits you. You die.";
                    string playerMsg = "You knock yourself out!";
                    Game.MessageQueue.AddMessage(playerMsg);
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    Game.Dungeon.SetPlayerDeath("was knocked out by themselves");

                    return CombatResults.DefenderDied;
                }

                //Debug string
                string combatResultsMsg3 = "PvP Dam: " + damage + " HP: " + monsterOrigHP + "->" + Hitpoints + " injured";
                //string playerMsg3 = "The " + this.SingleDescription + " hits you.";
                Game.MessageQueue.AddMessage("You damage yourself.");
                LogFile.Log.LogEntryDebug(combatResultsMsg3, LogDebugLevel.Medium);

                return CombatResults.DefenderDamaged;
            }

            //Miss
            string combatResultsMsg2 = "PvP Dam: " + damage + " HP: " + Hitpoints + "->" + Hitpoints + " miss";
            //string playerMsg2 = "The " + this.SingleDescription + " misses you.";
            string playerMsg2 = "You don't damage yourself.";
            Game.MessageQueue.AddMessage(playerMsg2);
            LogFile.Log.LogEntryDebug(combatResultsMsg2, LogDebugLevel.Medium);

            return CombatResults.DefenderUnhurt;
        }

        public IEquippableItem GetBestMeleeWeapon()
        {
            Type bestMeleeType = HeavyWeaponTranslation(typeof(Items.Fists));

            var meleeWeapon = Inventory.GetItemsOfType(bestMeleeType).First() as IEquippableItem;

            return meleeWeapon;
        }

        public void EquipBestMeleeWeapon()
        {
            EquipInventoryItemType(typeof(Items.Fists), true);
        }

        /// <summary>
        /// Heal the player by a quantity. Won't exceed max HP.
        /// </summary>
        /// <param name="healingQuantity"></param>
        public void HealPlayer(int healingQuantity)
        {
            Hitpoints += healingQuantity;

            if (Hitpoints > MaxHitpoints)
                Hitpoints = MaxHitpoints;

        }

        internal bool isStealthed()
        {
            if (IsEffectActive(typeof(PlayerEffects.StealthBoost)) || IsEffectActive(typeof(PlayerEffects.StealthField)))
                return true;

            return false;
        }

        internal void RemoveEffect(Type effectType)
        {

            //Increment time on events and remove finished ones
            List<PlayerEffect> finishedEffects = effects.FindAll(x => x.GetType() == effectType);

            //Remove these effects
            
            foreach (PlayerEffect effect in finishedEffects)
            {
                if(!effect.HasEnded())
                    effect.OnEnd(this);

                effects.Remove(effect);
            }

        }

        /// <summary>
        /// Generic throw method for most normal items
        /// </summary>
        /// <param name="item"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public Point ThrowItemGeneric(IEquippableItem item, Point target, int damageOrStunTurns, bool stunDamage)
        {
            Item itemAsItem = item as Item;

            LogFile.Log.LogEntryDebug("Throwing " + itemAsItem.SingleItemDescription, LogDebugLevel.Medium);

            //Find target

            List<Point> targetSquares = Game.Dungeon.CalculateTrajectory(target);
            Monster monster = Game.Dungeon.FirstMonsterInTrajectory(targetSquares);

            //Find where it landed

            //Destination will be the last square in trajectory
            Point destination;
            if (targetSquares.Count > 0)
                destination = targetSquares[targetSquares.Count - 1];
            else
                //Threw it on themselves!
                destination = LocationMap;

            //Stopped by a monster
            if (monster != null)
            {
                destination = monster.LocationMap;
            }

            //Make throwing sound AT target location
            Game.Dungeon.AddSoundEffect(item.ThrowSoundMagnitude(), LocationLevel, destination);

            //Draw throw
            Screen.Instance.DrawAreaAttackAnimation(targetSquares, ColorPresets.Gray);

            if (stunDamage)
            {
                if (monster != null && damageOrStunTurns > 0)
                {
                    ApplyStunDamageToMonster(monster, damageOrStunTurns);
                }
            }
            else
            {
                if (monster != null && damageOrStunTurns > 0)
                {
                    AttackMonsterThrown(monster, damageOrStunTurns);
                }
            }

            return destination;
        }

        public void ApplyDamageToPlayer(int damage)
        {
            var remainingDamage = damage;
            int shieldAbsorbs = 0;
            int hpAbsorbs = 0;

            //Shield absorbs damage first
            if (Shield > 0)
            {
                var shieldEffect = GetActiveEffects(typeof(PlayerEffects.ShieldEnhance));

                int shieldEnhance = 1;
                if (shieldEffect.Count() > 0)
                {
                     shieldEnhance += (shieldEffect.First() as PlayerEffects.ShieldEnhance).shieldEnhanceAmount;
                }

                shieldAbsorbs = Math.Min(Shield * shieldEnhance, remainingDamage);
                Shield -= shieldAbsorbs / shieldEnhance;
                remainingDamage -= shieldAbsorbs;

                ShieldWasDamagedThisTurn = true;
            }

            if (Shield == 0)
            {
                ShieldIsDisabled = true;
            }

            //Through to health
            //Unless we lost our shield this turn, in which case we get a 1 turn grace
            if (!ShieldWasDamagedThisTurn)
            {
                hpAbsorbs = ApplyDamageToPlayerHitpoints(remainingDamage);
            }

            LogFile.Log.LogEntryDebug("Player takes " + shieldAbsorbs + " damage " + hpAbsorbs + " hitpoint damage.", LogDebugLevel.Medium);
        }

        public int ApplyDamageToPlayerHitpoints(int damage)
        {
            int hpAbsorbs = 0;

            if (damage > 0)
            {
                hpAbsorbs = Math.Min(Hitpoints, damage);
                Hitpoints -= damage;

                HitpointsWasDamagedThisTurn = true;
            }

            if (Hitpoints <= 0)
            {
                //Player died
                Game.Dungeon.SetPlayerDeath("Took damage");

                LogFile.Log.LogEntry("Player takes " + damage + " damage and dies.");
            }
            return hpAbsorbs;
        }

        /// <summary>
        /// Carry out all pre-turn checks and sets
        /// </summary>
        internal void PreTurnActions()
        {
            UseEnergyForWetware();
            RegenerateStatsPerTurn();

            ShieldWasDamagedThisTurn = false;
            HitpointsWasDamagedThisTurn = false;
            EnergyWasDamagedThisTurn = false;
            
            if (Game.Dungeon.Player.RecalculateCombatStatsRequired)
                Game.Dungeon.Player.CalculateCombatStats();

        }

        private void UseEnergyForWetware()
        {
            var equippedWetware = GetEquippedWetware();

            if (equippedWetware != null)
            {
                var energyDrain = equippedWetware.GetEnergyDrain();
                Energy -= Math.Min(energyDrain, Energy);
                if (energyDrain > 0)
                    EnergyWasDamagedThisTurn = true;

                if (Energy == 0)
                {
                    UnequipWetware();
                    DisableEnergyRecharge();
                }
            }
        }

        public void DisableEnergyRecharge() {
            EnergyRechargeIsDisabled = true;
        }

        public void HealCompletely()
        {
            Hitpoints = MaxHitpoints;
            Shield = MaxShield;
            Energy = MaxEnergy;
        }

        public bool NeedsHealing()
        {
            if (Hitpoints < MaxHitpoints)
                return true;
            if (Shield < MaxShield)
                return true;
            if(Energy < MaxEnergy)
                return true;
            return false;
        }

        private void RegenerateStatsPerTurn()
        {
            if (ShieldIsDisabled)
            {
                TurnsSinceShieldDisabled++;

                if (TurnsSinceShieldDisabled >= TurnsForShieldToTurnBackOn)
                {
                    ShieldIsDisabled = false;
                    TurnsSinceShieldDisabled = 0;
                }
            }

            if (!ShieldIsDisabled && !ShieldWasDamagedThisTurn && DoesShieldRecharge)
            {
                double shieldRegenRate = MaxShield / (double)TurnsToRegenerateShield;
                AddShield((int)Math.Ceiling(shieldRegenRate));

                if (Shield > MaxShield)
                    Shield = MaxShield;
            }

            if (!HitpointsWasDamagedThisTurn)
            {
                double hpRegenRate = MaxHitpoints / (double)TurnsToRegenerateHP;
                Hitpoints += (int)Math.Ceiling(hpRegenRate);
                if (Hitpoints > MaxHitpoints)
                    Hitpoints = MaxHitpoints;
            }

            if (EnergyRechargeIsDisabled)
            {
                TurnsSinceEnergyRechargeDisabled++;

                if (TurnsSinceEnergyRechargeDisabled >= TurnsForEnergyRechargeToTurnBackOn)
                {
                    EnergyRechargeIsDisabled = false;
                    TurnsSinceEnergyRechargeDisabled = 0;
                }
            }

            if (!EnergyRechargeIsDisabled && !EnergyWasDamagedThisTurn)
            {
                double energyRegenRate = MaxEnergy / (double)TurnsToRegenerateEnergy;
                Energy += (int)Math.Ceiling(energyRegenRate);
                if (Energy > MaxEnergy)
                    Energy = MaxEnergy;
            }
        }

        internal void ResetAfterDeath()
        {
            SetupInitialStats();
        }



        internal void AddShield(int shieldBonus)
        {
            Shield += shieldBonus;

            if (Shield > MaxShield)
                Shield = MaxShield;
        }

        internal void FullAmmo()
        {
            foreach (var i in Inventory.Items)
            {
                var item = i as RangedWeapon;

                if (item != null)
                    item.Ammo = item.MaxAmmo();

            }
        }

        internal void AddAmmoToCurrentWeapon()
        {
            var equippedWeapon = GetEquippedWeaponAsItem() as RangedWeapon;

            if (equippedWeapon != null && equippedWeapon.RemainingAmmo() < equippedWeapon.MaxAmmo())
            {
                equippedWeapon.Ammo = equippedWeapon.MaxAmmo();
                Game.MessageQueue.AddMessage(equippedWeapon.SingleItemDescription + " reloaded.");
                LogFile.Log.LogEntryDebug("Giving ammo to " + equippedWeapon.SingleItemDescription, LogDebugLevel.Medium);
            }
            else
            {
                //Apply to a random weapon item
                foreach (var i in Inventory.Items)
                {
                    var item = i as RangedWeapon;

                    if (item != null && item.RemainingAmmo() < item.MaxAmmo())
                    {
                        item.Ammo = item.MaxAmmo();
                        Game.MessageQueue.AddMessage(item.SingleItemDescription + " reloaded.");
                        LogFile.Log.LogEntryDebug("Giving ammo to " + item.SingleItemDescription, LogDebugLevel.Medium);
                        break;
                    }
                }
            }
        }


        internal void NotifyAttack(Monster monster)
        {
            if (!Screen.Instance.TargetSelected())
            {
                Screen.Instance.CreatureToView = monster;
            }
        }

        internal void RefillWeapons()
        {
            foreach (var i in Inventory.Items)
            {
                var item = i as RangedWeapon;

                if (item != null && item.RemainingAmmo() < item.MaxAmmo())
                {
                    item.Ammo = item.MaxAmmo();
                    Game.MessageQueue.AddMessage(item.SingleItemDescription + " reloaded.");
                    LogFile.Log.LogEntryDebug("Giving ammo to " + item.SingleItemDescription, LogDebugLevel.Medium);
                }
            }
        }
    }
}
