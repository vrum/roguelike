﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.IO.Compression;
using System.Linq;


namespace RogueBasin
{
    /// <summary>
    /// Store the mapping between a hidden name and the actual name of a potion. Much nicer OO ways to do it but I don't have time!
    /// </summary>
    public class HiddenNameInfo
    {
        public string ActualName { get; set; } //SingleItemDescription
        public string HiddenName { get; set; } //random each time
        public string UserName { get; set; } //name the user has given it

        public HiddenNameInfo() { } //for serialization

        public HiddenNameInfo(string actual, string hidden, string user) { ActualName = actual; HiddenName = hidden; UserName = user; }
    }

    public class KillCount
    {
        public Monster type;
        public int count = 0;
    }

    /// <summary>
    /// The contents of a map square: Creatures & Items
    /// </summary>
    public class SquareContents
    {
        /// <summary>
        /// Reference to monster in the square
        /// </summary>
        public Monster monster = null;

        /// <summary>
        /// Reference to player in the square
        /// </summary>
        public Player player = null;

        public Feature feature = null;

        public List<Item> items = new List<Item>();

        /// <summary>
        /// Set if no monster or player
        /// </summary>
        public bool empty = false;

        public bool offMap = false;

        public SquareContents()
        {

        }
    }

    public class DungeonProfile {
        public int dungeonStartLevel;
        public int dungeonEndLevel;

        public bool subUniqueDefeated = false;
        public bool masterUniqueDefeated = false;

        public bool visited = false;
        public bool open = false;

        public bool PlayerLeftDock { get; set; }

        public bool LevelObjectiveComplete { get; set; }

        public bool LevelObjectiveKillAllMonstersComplete { get; set; }

        public DungeonProfile()
        {
            PlayerLeftDock = false;
            LevelObjectiveComplete = false;
            LevelObjectiveKillAllMonstersComplete = false;
        }
    }

    /// <summary>
    /// Information about the state of dungeons in princessRL
    /// </summary>
    public class DungeonInfo
    {
        List<DungeonProfile> dungeons;

        public bool LastMission { get; set; }

        public bool DragonDead { get; set; }

        /// <summary>
        /// No of times the player has died
        /// </summary>
        public int NoDeaths { get; set; }

        /// <summary>
        /// No of times the player has aborted a mission (this mission)
        /// </summary>
        public int NoAborts { get; set; }

        /// <summary>
        /// No of times the player has aborted a mission (in all time)
        /// </summary>
        public int TotalAborts { get; set; }

        /// <summary>
        /// Max deaths before the real end
        /// </summary>
        public int MaxDeaths { get; set; }

        /// <summary>
        /// Max aborts until the real end
        /// </summary>
        public int MaxAborts { get; set; }

        Dictionary<int, string> levelNaming = new Dictionary<int, string>();


        //public List<bool> level3UniqueStatus;
        //public List<bool> level4UniqueStatus;

        //public int CurrentDungeon { get; set; }

        public void SetL3UniqueDead(int dungeonID)
        {
            dungeons[dungeonID].subUniqueDefeated = true;

            if (dungeons[dungeonID].masterUniqueDefeated)
            {
                Game.MessageQueue.AddMessage("The two toughest creatures have been defeated! Well done!");
                LogFile.Log.LogEntryDebug("Master and sub unique defeated, dungeon " + dungeonID, LogDebugLevel.Medium);
            }
        }

        public void SetL4UniqueDead(int dungeonID)
        {
            dungeons[dungeonID].masterUniqueDefeated = true;

            if (dungeons[dungeonID].subUniqueDefeated)
            {
                Game.MessageQueue.AddMessage("The two toughest creatures have been defeated! Well done!");
                LogFile.Log.LogEntryDebug("Master and sub unique defeated, dungeon " + dungeonID, LogDebugLevel.Medium);
            }
        }

        public DungeonInfo()
        {
            dungeons = new List<DungeonProfile>();
            //false = unique alive
            //level3UniqueStatus = new List<bool>();
            //false = unique alive
            //level4UniqueStatus = new List<bool>();

            LastMission = false;
            //CurrentDungeon = -1;
            DragonDead = false;

            //Per mission respawns
            MaxAborts = 2;
            //Deaths in the game
            MaxDeaths = 5;
        }

        /// <summary>
        /// Flatline - each level has its own info.
        /// This add a new profile so must be called each level
        /// </summary>
        public void SetupLevelInfo()
        {
            DungeonProfile thisDung = new DungeonProfile();
            dungeons.Add(thisDung);
        }

        /// <summary>
        /// Setup the dungeon level starts and unique status
        /// </summary>
        public void SetupDungeonStartAndEnd()
        {
            //In Princess RL there are 7 dungeons. This probably should be done in DungeonMaker
            for (int i = 0; i < 7; i++)
            {
                DungeonProfile thisDung = new DungeonProfile();

                thisDung.dungeonStartLevel = 2 + i * 4;
                thisDung.dungeonEndLevel = 5 + i * 4;

                dungeons.Add(thisDung);
            }

            /*
            for (int i = 0; i < 6; i++)
            {
                level3UniqueStatus.Add(false);
                level4UniqueStatus.Add(false);
            }*/

            //Setup the original open dungeons
            dungeons[0].open = true;
            dungeons[1].open = true;
        }

        /// <summary>
        /// Setup the dungeon level starts and unique status
        /// </summary>
        public void SetupDungeonStartAndEndDebug()
        {
                DungeonProfile thisDung = new DungeonProfile();

                thisDung.dungeonStartLevel = 1;
                thisDung.dungeonEndLevel = 1;

                dungeons.Add(thisDung);
    

            /*
            for (int i = 0; i < 6; i++)
            {
                level3UniqueStatus.Add(false);
                level4UniqueStatus.Add(false);
            }*/

            //Setup the original open dungeons
            dungeons[0].open = true;
        
        }

        public List<DungeonProfile> Dungeons
        {
            get
            {
                return dungeons;
            }
        }

        /// <summary>
        /// All dungeons are 4 levels deep
        /// </summary>
        /// <param name="levelNo"></param>
        public int DungeonNumberFromLevelNo(int levelNo)
        {

            if (levelNo < 2)
            {
                LogFile.Log.LogEntryDebug("Asked for a dungeon with a non-dungeon level", LogDebugLevel.High);
                return -1;
            }

            int dungeonNo = (int)Math.Floor((levelNo - 2) / 4.0);

            return dungeonNo;
        }


        public int GetDungeonStartLevel(int dungeonNo)
        {
            try
            {
                return dungeons[dungeonNo].dungeonStartLevel;
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Asked for an out of range dungeon " + dungeonNo, LogDebugLevel.High);
                return 0;
            }
        }

        public void VisitedDungeon(int dungeonNo)
        {
            try
            {
                dungeons[dungeonNo].visited = true;
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Asked for an out of range dungeon " + dungeonNo, LogDebugLevel.High);
            }
        }

        public void OpenDungeon(int dungeonNo)
        {
            try
            {
                dungeons[dungeonNo].open = true;
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Asked for an out of range dungeon " + dungeonNo, LogDebugLevel.High);
            }
        }

        public bool IsDungeonVisited(int dungeonNo)
        {
            try
            {
                return dungeons[dungeonNo].visited;
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Asked for an out of range dungeon " + dungeonNo, LogDebugLevel.High);
                return false;
            }
        }

        public bool IsDungeonOpen(int dungeonNo)
        {
            try
            {
                return dungeons[dungeonNo].open;
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Asked for an out of range dungeon " + dungeonNo, LogDebugLevel.High);
                return false;
            }
        }


        public int GetDungeonEndLevel(int dungeonNo)
        {
            try
            {
                return dungeons[dungeonNo].dungeonEndLevel;
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Asked for an out of range dungeon " + dungeonNo, LogDebugLevel.High);
                return 0;
            }
        }

        public Dictionary<int, string> LevelNaming { get { return levelNaming; } set { levelNaming = value; }}
  
        /// <summary>
        /// Names for the areas
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        internal string LookupMissionName(int level)
        {
            if(LevelNaming.ContainsKey(level))
                return LevelNaming[level];
            return "";
        }
    }


    /// <summary>
    /// Keeps or links to all the state in the game
    /// </summary>
    public class Dungeon
    {
        List<Map> levels;

        Pathing pathFinding;
        LibTCOD.TCODFovWrapper fov;

        List<Monster> monsters;
        List<Item> items;
        List<Feature> features;
        Dictionary<Location, List<Lock>> locks;
        public List<HiddenNameInfo> HiddenNameInfo { get; set; } //for serialization
        public List<DungeonSquareTrigger> Triggers { get; set; }

        List<SpecialMove> specialMoves;

        List<Spell> spells;

        Player player;

        public bool SaveScumming { get; set; }

        public GameDifficulty Difficulty { get; set; }

        public bool PlayerImmortal { get; set; }
        public bool PlayerCheating { get; set; }

        private List<Monster> summonedMonsters; //no need to serialize

        DungeonInfo dungeonInfo;

        public bool Profiling { get; set; }

        /*
        public int Dungeon1StartLevel { get; set;}
        public int Dungeon1EndLevel { get; set; }

        public int Dungeon2StartLevel { get; set; }
        public int Dungeon2EndLevel { get; set; }

        public int Dungeon3StartLevel { get; set; }
        public int Dungeon3EndLevel { get; set; }

        public int Dungeon4StartLevel { get; set; }
        public int Dungeon4EndLevel { get; set; }

        public int Dungeon5StartLevel { get; set; }
        public int Dungeon5EndLevel { get; set; }

        public int Dungeon6StartLevel { get; set; }
        public int Dungeon6EndLevel { get; set; }

        public int Dungeon7StartLevel { get; set; }
        public int Dungeon7EndLevel { get; set; }
        */
        long worldClock = 0;

        /// <summary>
        /// Count the days in the year
        /// </summary>
        public int dateCounter = 0;

        /// <summary>
        /// Monster have a unique ID. This stores the next free ID. The player is 0.
        /// </summary>
        public int nextUniqueID = 1;

        /// <summary>
        /// Sounds have a unique ID. This stores the next free ID.
        /// </summary>
        public int nextUniqueSoundID = 0;

        /// <summary>
        /// Set to false to end the game
        /// </summary>
        public bool RunMainLoop { get; set; }

        /// <summary>
        /// Give the player a bonus turn on next loop
        /// </summary>
        bool playerBonusTurn;

        /// <summary>
        /// Set after bonus turn complete
        /// </summary>
        public bool PlayerHadBonusTurn { get; set; }

        /// <summary>
        /// List of global events, indexed by the time they occur
        /// </summary>
        List<SoundEffect> effects;


        Color defaultPCColor = ColorPresets.White;

        public DungeonMaker DungeonMaker { get; set; }

        public Dungeon()
        {
            SetupDungeon();
        }

        private void SetupDungeon()
        {
            levels = new List<Map>();
            monsters = new List<Monster>();
            items = new List<Item>();
            features = new List<Feature>();
            locks = new Dictionary<Location, List<Lock>>();

            pathFinding = new Pathing(this, new LibTCOD.TCODPathFindingWrapper());
            fov = new LibTCOD.TCODFovWrapper();

            ///DungeonEffects are indexed by the time that they occur
            effects = new List<SoundEffect>();

            specialMoves = new List<SpecialMove>();
            spells = new List<Spell>();
            HiddenNameInfo = new List<HiddenNameInfo>();
            Triggers = new List<DungeonSquareTrigger>();

            dungeonInfo = new DungeonInfo();

            //Should pull this out as an interface, and get TraumaRL to set it
            MonsterPlacement = new MonsterPlacement();

            PlayerImmortal = false;

            playerBonusTurn = false;
            PlayerHadBonusTurn = false;

            SetupSpecialMoves();

            SetupSpells();

            SetupHiddenNameMappings();

            player = new Player();

            RunMainLoop = true;

            summonedMonsters = new List<Monster>();

            SaveScumming = true;

            Profiling = true;
        }

        public Dungeon(DungeonInfo info)
        {
            SetupDungeon();
            dungeonInfo = info;
        }

        /// <summary>
        /// Give the player a bonus turn before the monsters
        /// </summary>
        public bool PlayerBonusTurn
        {
            get
            {
                return playerBonusTurn;
            }
            set
            {
                PlayerHadBonusTurn = false;
                playerBonusTurn = true;
            }
        }

        public int DateCounter
        {
            get
            {
                return dateCounter;
            }
            set
            {
                dateCounter = value;
            }
        }

        public DungeonInfo DungeonInfo
        {
            get
            {
                return dungeonInfo;
            }
            set
            {
                dungeonInfo = value;
            }
        }

        public Algorithms.IFieldOfView FOV
        {
            get
            {
                return fov;
            }
        }

        /// <summary>
        /// How much of your previous life do you remember?
        /// </summary>
        /// <returns></returns>
        public int PercentRemembered()
        {
            double total = player.PlotItemsFound / (double)player.TotalPlotItems * 100.0;
            return (int)Math.Ceiling(total);
        }

        public bool FireShotgunWeapon(Point target, Items.ShotgunTypeWeapon item, int damageBase, int damageDropWithRange, int damageDropWithInterveningMonster)
        {
            //The shotgun fires towards its target and does less damage with range

            //Get all squares in range and within FOV (shotgun needs a straight line route to fire)

            IEquippableItem equipItem = item as IEquippableItem;

            if (equipItem == null)
            {
                LogFile.Log.LogEntryDebug("Shotgun is not equippable", LogDebugLevel.High);
                return false;
            }

            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(player);
            List<Point> targetSquares = currentFOV.GetPointsForTriangularTargetInFOV(player.LocationMap, target, PCMap, equipItem.RangeFire(), item.ShotgunSpreadAngle());

            //Draw attack
            Screen.Instance.DrawAreaAttackAnimation(targetSquares, ColorPresets.Chocolate);

            //Make firing sound
            Game.Dungeon.AddSoundEffect(item.FireSoundMagnitude(), player.LocationLevel, player.LocationMap);

            //Attack all monsters in the area

            foreach (Point sq in targetSquares)
            {
                SquareContents squareContents = MapSquareContents(player.LocationLevel, sq);

                Monster m = squareContents.monster;

                //Hit the monster if it's there
                if (m != null)
                {
                    //Calculate range
                    int rangeToMonster = (int)Math.Floor(Utility.GetDistanceBetween(player.LocationMap, m.LocationMap));

                    //How many monsters between monster and player
                    var pointsFromPlayerToMonster = Utility.GetPointsOnLine(player.LocationMap, m.LocationMap);
                    //(exclude player and monster itself)
                    var numInterveningMonsters = pointsFromPlayerToMonster.Skip(1).Where(p => MapSquareContents(player.LocationLevel, p).monster != null).Count() - 1;

                    LogFile.Log.LogEntryDebug("Shotgun. Monster at " + m.LocationMap + " intervening monsters: " + numInterveningMonsters, LogDebugLevel.Medium);

                    int damage = damageBase - rangeToMonster * damageDropWithRange;
                    damage = damage - numInterveningMonsters * damageDropWithInterveningMonster;

                    string combatResultsMsg = "PvM (" + m.Representation + ") Shotgun: Dam: " + damage;
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    //Apply damage
                    if(damage > 0)
                        player.AttackMonsterRanged(squareContents.monster, damage);
                }
            }

            return true;
        }

        public bool FirePistolLineWeapon(Point target, RangedWeapon item, int damageBase)
        {
            Player player = Player;

            //Remove 1 ammo
            item.Ammo--;

            //Make firing sound
            AddSoundEffect(item.FireSoundMagnitude(), player.LocationLevel, player.LocationMap);

            //Find monster target

            var targetSquares = CalculateTrajectory(target);
            Monster monster = FirstMonsterInTrajectory(targetSquares);

            if (monster == null)
            {
                LogFile.Log.LogEntryDebug("No monster in target for pistol-like weapon. Ammo used anyway.", LogDebugLevel.Medium);
                return true;
            }

            var targetSquaresToDraw = targetSquares.Count() > 1 ? targetSquares.GetRange(1, targetSquares.Count - 1) :
                targetSquares;

            //Draw attack

            Screen.Instance.DrawAreaAttackAnimation(targetSquaresToDraw, ColorPresets.Gray);

            //Apply damage
            player.AttackMonsterRanged(monster, damageBase);

            return true;
        }

        /// <summary>
        /// Create obfuscated names for the potions etc.
        /// </summary>
        private void SetupHiddenNameMappings()
        {
            //Add all potions here
            List<Item> allPotions = new List<Item>() { new Items.Potion(), new Items.PotionDamUp(), new Items.PotionMajDamUp(), new Items.PotionMajHealing(), new Items.PotionMajSightUp(),
                new Items.PotionMajSpeedUp(),    new Items.PotionMajToHitUp(), new Items.PotionSightUp(), new Items.PotionSpeedUp(), new Items.PotionSuperDamUp(),
                new Items.PotionSuperHealing(), new Items.PotionSuperSpeedUp(), new Items.PotionSuperToHitUp(), new Items.PotionToHitUp()};

            List<string> descsUsed = new List<string>();

            foreach (Item potion in allPotions)
            {
                string hiddenDesc;
                do
                {
                    hiddenDesc = Utility.RandomHiddenDescription();
                } while (descsUsed.Contains(hiddenDesc));

                HiddenNameInfo info = new HiddenNameInfo(potion.SingleItemDescription, hiddenDesc + " " + potion.HiddenSuffix, null);
                HiddenNameInfo.Add(info);
                descsUsed.Add(hiddenDesc);
            }
        }

        /// <summary>
        /// Finds a currently alive monster by type. Returns first creature found, or null if none.
        /// </summary>
        /// <param name="monsterType"></param>
        /// <returns></returns>
        public Monster FindMonsterByType(Type monsterType)
        {
            Monster foundMonster = monsters.Find(m => m.GetType() == monsterType);
            return foundMonster;
        }

        /// <summary>
        /// Find the closest creature to the map object
        /// </summary>
        /// <param name="originCreature"></param>
        /// <returns></returns>
        public Creature FindClosestCreature(MapObject origin)
        {
            //Find the closest creature
            Creature closestCreature = null;
            double closestDistance = Double.MaxValue; //a long way

            double distance;

            foreach (Monster creature in monsters)
            {
                distance = Utility.GetDistanceBetween(origin, creature);

                if (distance > 0 && distance < closestDistance && origin != creature)
                {
                    closestDistance = distance;
                    closestCreature = creature;
                }
            }

            //And check for player

            distance = Utility.GetDistanceBetween(origin, Game.Dungeon.Player);

            if (distance > 0 && distance < closestDistance && origin != Game.Dungeon.Player)
            {
                closestDistance = distance;
                closestCreature = Game.Dungeon.Player;
            }

            return closestCreature;
        }

        public int CalculateLevelSecurity(int level)
        {
            var camerasOnThisLevel = monsters.Where(m => m.LocationLevel == level && m.GetType() == typeof(Creatures.Camera));
            var aliveCamerasOnThisLevel = camerasOnThisLevel.Where(m => m.Alive);

            int levelSecurity = (int)Math.Ceiling((aliveCamerasOnThisLevel.Count() / (double)camerasOnThisLevel.Count()) * 100);

            return levelSecurity;
        }

        /// <summary>
        /// Find the hostile creature to the map object
        /// </summary>
        /// <param name="originCreature"></param>
        /// <returns></returns>
        public Creature FindClosestHostileCreature(MapObject origin)
        {
            //Find the closest creature
            Creature closestCreature = null;
            double closestDistance = Double.MaxValue; //a long way

            double distance;

            foreach (Monster creature in monsters)
            {
                if (creature.Charmed || creature.Passive)
                    continue;

                distance = Utility.GetDistanceBetween(origin, creature);

                if (distance > 0 && distance < closestDistance && origin != creature)
                {
                    closestDistance = distance;
                    closestCreature = creature;
                }
            }

            return closestCreature;
        }

        /// <summary>
        /// Public access to path finding function
        /// </summary>
        public Pathing Pathing
        {
            get
            {
                return pathFinding;
            }
        }

        public IEnumerable<Tuple<double, Monster>> GetNearbyCreaturesInOrderOfRange(double range, CreatureFOV currentFOV, int level, Point start)
        {
            var rangeRoundedUp = (int)Math.Ceiling(range);

            var candidates = new List<Tuple<double, Monster>>();

            for (int i = start.x - rangeRoundedUp; i < start.x + rangeRoundedUp; i++)
            {
                for (int j = start.y - rangeRoundedUp; j < start.y + rangeRoundedUp; j++)
                {

                    if (i > 0 && j > 0 && i < Game.Dungeon.Levels[level].width && j < Game.Dungeon.Levels[level].height)
                    {
                        foreach (var c in Game.Dungeon.Monsters)
                        {
                            if (c.LocationLevel != level)
                                continue;

                            if (currentFOV.CheckTileFOV(i, j))
                            {
                                candidates.Add(new Tuple<double, Monster>(Utility.GetDistanceBetween(start, c.LocationMap), c));
                            }
                        }
                    }
                }
            }
            return candidates.OrderBy(c => c.Item1);
        }

        public IEnumerable<Monster> FindClosestCreaturesInPlayerFOV()
        {
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);
            var creatures = GetNearbyCreaturesInOrderOfRange(10.0, currentFOV, player.LocationLevel, player.LocationMap).Select(c => c.Item2);
            return creatures.Where(c => currentFOV.CheckTileFOV(c.LocationMap));
        }

        /// <summary>
        /// Find the hostile creature to the map object
        /// </summary>
        /// <param name="originCreature"></param>
        /// <returns></returns>
        public Creature FindClosestHostileCreatureInFOV(MapObject origin)
        {
            //Find the closest creature
            Creature closestCreature = null;
            double closestDistance = Double.MaxValue; //a long way

            double distance;

            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

            foreach (Monster creature in monsters)
            {
                if (creature.Charmed || creature.Passive)
                    continue;

                distance = Utility.GetDistanceBetween(origin, creature);

                if (distance > 0 && distance < closestDistance && origin != creature)
                {
                    //Check in FOV

                    if (currentFOV.CheckTileFOV(creature.LocationMap.x, creature.LocationMap.y))
                    {

                        closestDistance = distance;
                        closestCreature = creature;
                    }
                }
            }

            return closestCreature;
        }


        /// <summary>
        /// Link a potion with a user-provided string
        /// </summary>
        /// <param name="item"></param>
        /// <param name="newName"></param>
        public void AssociateNameWithItem(Item item, string newName)
        {
            HiddenNameInfo thisInfo = HiddenNameInfo.Find(x => x.ActualName == item.SingleItemDescription);

            if (thisInfo == null)
            {
                LogFile.Log.LogEntryDebug("Couldn't find an item to associate with this name", LogDebugLevel.High);
                return;
            }
            LogFile.Log.LogEntryDebug("Renaming " + GetHiddenName(item) + " to " + newName, LogDebugLevel.Medium);
            thisInfo.UserName = newName;
        }

        /// <summary>
        /// Get the hidden name of an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal string GetHiddenName(Item item)
        {
            //Not a hidden item
            if (!item.UseHiddenName)
            {
                LogFile.Log.LogEntryDebug("GetHiddenName called on non-hidden item", LogDebugLevel.High);
                return item.SingleItemDescription;
            }

            HiddenNameInfo hiddenName = HiddenNameInfo.Find(x => x.ActualName == item.SingleItemDescription);

            if (hiddenName == null)
            {
                LogFile.Log.LogEntryDebug("Couldn't find hidden name for item", LogDebugLevel.High);
                return item.SingleItemDescription;
            }

            if (hiddenName.UserName != null)
            {
                return hiddenName.UserName;
            }
            else
                return hiddenName.HiddenName;
        }

        /// <summary>
        /// Add to the special moves list
        /// </summary>
        private void SetupSpecialMoves()
        {
            specialMoves.Add(new SpecialMoves.ChargeAttack());
            //specialMoves.Add(new SpecialMoves.StunBox());
            //specialMoves.Add(new SpecialMoves.WallPush());
            specialMoves.Add(new SpecialMoves.WallLeap());
            specialMoves.Add(new SpecialMoves.WallVault());
            specialMoves.Add(new SpecialMoves.VaultBackstab());
            specialMoves.Add(new SpecialMoves.OpenSpaceAttack());
            specialMoves.Add(new SpecialMoves.Evade());
            specialMoves.Add(new SpecialMoves.MultiAttack());
            specialMoves.Add(new SpecialMoves.BurstOfSpeed());
            specialMoves.Add(new SpecialMoves.CloseQuarters());


            foreach (SpecialMove move in specialMoves)
            {
                move.Known = false;
            }
        }

        /// <summary>
        /// Add to the spells list
        /// </summary>
        private void SetupSpells()
        {
            spells.Add(new Spells.MagicMissile());
            spells.Add(new Spells.MageArmour());
            spells.Add(new Spells.Blink());
            spells.Add(new Spells.SlowMonster());
            spells.Add(new Spells.FireLance());
            spells.Add(new Spells.FireBall());
            spells.Add(new Spells.EnergyBlast());
            spells.Add(new Spells.Exit());
            spells.Add(new Spells.Light());
            spells.Add(new Spells.ShowItems());


            foreach (Spell move in spells)
            {
                move.Known = false;
            }
        }

        /// <summary>
        /// Triggers which flip terrain into different type
        /// Yeah, ok, I was tired when I wrote this. I think it has no redeeming features!
        /// </summary>
        /// <param name="triggerIDToFlip"></param>
        public void FlipTerrain(string triggerIDToFlip)
        {
            foreach (DungeonSquareTrigger trigger in Triggers)
            {
                if (trigger.GetType() == typeof(Triggers.TerrainFlipTrigger))
                {
                    Triggers.TerrainFlipTrigger flipTrig = trigger as Triggers.TerrainFlipTrigger;

                    if (trigger == null)
                    {
                        LogFile.Log.LogEntryDebug("Trigger is not terrain flip - problem", LogDebugLevel.High);
                        continue;
                    }

                    if (triggerIDToFlip == flipTrig.triggerID)
                    {
                        flipTrig.FlipTerrain();
                    }
                }
            }

        }

        /// <summary>
        /// Save the game to disk. Throws exceptions
        /// </summary>
        /// <param name="saveGameName"></param>
        public void SaveGame()
        {
            FileStream stream = null;
            GZipStream compStream = null;

            try
            {

                //Copy across the data we need to save from dungeon

                SaveGameInfo saveGameInfo = new SaveGameInfo();

                saveGameInfo.effects = this.effects;
                saveGameInfo.features = this.features;
                saveGameInfo.items = this.items;
                saveGameInfo.monsters = this.monsters;
                saveGameInfo.player = this.player;
                saveGameInfo.specialMoves = this.specialMoves;
                saveGameInfo.spells = this.spells;
                saveGameInfo.hiddenNameInfo = this.HiddenNameInfo;
                saveGameInfo.worldClock = this.worldClock;
                saveGameInfo.dateCounter = this.dateCounter;
                saveGameInfo.triggers = this.Triggers;
                saveGameInfo.difficulty = this.Difficulty;
                saveGameInfo.dungeonInfo = this.dungeonInfo;
                saveGameInfo.nextUniqueID = this.nextUniqueID;
                saveGameInfo.nextUniqueSoundID = this.nextUniqueSoundID;
                saveGameInfo.messageLog = Game.MessageQueue.GetMessageHistoryAsList();
                saveGameInfo.effects = this.Effects;
                saveGameInfo.dungeonMaker = this.DungeonMaker;

                //Make maps into serializablemaps and store
                List<SerializableMap> serializedLevels = new List<SerializableMap>();
                foreach (Map level in levels)
                {
                    serializedLevels.Add(new SerializableMap(level));
                }

                saveGameInfo.levels = serializedLevels;

                //Construct save game filename
                string filename = player.Name + ".sav";

                XmlSerializer serializer = new XmlSerializer(typeof(SaveGameInfo));
                stream = File.Open(filename, FileMode.Create);
                compStream = new GZipStream(stream, CompressionMode.Compress, true);

                //XmlTextWriter writer = new XmlTextWriter(compStream, System.Text.Encoding.UTF8);
                XmlTextWriter writer = new XmlTextWriter(stream, System.Text.Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, saveGameInfo);

                Game.MessageQueue.AddMessage("Game " + player.Name + " saved successfully.");
                LogFile.Log.LogEntry("Game " + player.Name + " saved successfully: " + filename);
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Save game failed. Name: " + player.Name + ".sav" + " Reason: " + ex.Message);
                throw new ApplicationException("Save game failed. Name: " + player.Name + ".sav" + " Reason: " + ex.Message);
            }
            finally
            {
                if (compStream != null)
                {
                    compStream.Close();
                }

                if (stream != null)
                {
                    stream.Close();
                }
            }

        }

        /// <summary>
        /// Add map and return its level index
        /// </summary>
        /// <param name="mapToAdd"></param>
        /// <returns></returns>
        public int AddMap(Map mapToAdd)
        {
            levels.Add(mapToAdd);

            //Add to dungeoninfo
            dungeonInfo.SetupLevelInfo();

            //Add TCOD version
            //Do this at the end for speed
            //fov.updateFovMap(levels.Count - 1, mapToAdd.FovRepresentaton);

            return levels.Count - 1;
        }

        /// <summary>
        /// Replace a map in memory. Not that much stuff actually caches map so this is probably OK
        /// </summary>
        /// <param name="mapToAdd"></param>
        /// <returns></returns>
        public int ReplaceMap(int level, Map newMap)
        {
            levels[level] = newMap;

            //Add TCOD version
            fov.updateFovMap(level, newMap.FovRepresentaton);

            return level;
        }

        /// <summary>
        /// Player learns a random move. Play all movies?.
        /// </summary>
        public void PlayerLearnsRandomMove()
        {
            //OK, this needs to be fixed so you don't keep learning the same moves, but I'm leaving it like this for now for debug

            int moveToLearn = Game.Random.Next(specialMoves.Count);

            specialMoves[moveToLearn].Known = true;

            //Play movie
            foreach (SpecialMove m1 in specialMoves)
            {
                Screen.Instance.PlayMovie(m1.MovieRoot(), false);
            }
        }

        /// <summary>
        /// Player learns all move. Debug. Movies not played.
        /// </summary>
        public void PlayerLearnsAllMoves()
        {
            //Play movie
            foreach (SpecialMove m1 in specialMoves)
            {
                m1.Known = true;
            }
        }

        /// <summary>
        /// Player learns all spells. Debug. Movies not played.
        /// </summary>
        public void PlayerLearnsAllSpells()
        {
            //Play movie
            foreach (Spell m1 in spells)
            {
                m1.Known = true;
            }
        }

        /// <summary>
        /// Add monster. In addition to normal checks, check connectivity between monster and down stairs. This will ensure the monster is not placed in an unaccessible place
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>

        public bool AddMonster(Monster creature, int level, Point location)
        {
            //Try to add a creature at the requested location
            //This may fail due to something else being there or being non-walkable
            try
            {
                if (creature == null)
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Tried to add null", LogDebugLevel.High);
                    return false;
                }

                if (creature.UniqueID != 0)
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Tried to add monster which already had ID", LogDebugLevel.High);
                    return false;
                }


                Map creatureLevel = levels[level];

                //Check square is accessable
                if (!MapSquareIsWalkable(level, location))
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Square not enterable", LogDebugLevel.Low);
                    return false;
                }

                //Check square has nothing else on it
                SquareContents contents = MapSquareContents(level, location);

                if (contents.monster != null)
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Monster at this square", LogDebugLevel.Low);
                    return false;
                }

                if (contents.player != null)
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Player at this square", LogDebugLevel.Low);
                    return false;
                }

                //Otherwise OK
                creature.LocationLevel = level;
                creature.LocationMap = location;

                creature.CalculateSightRadius();

                AddMonsterToList(creature);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddCreature: ") + ex.Message);
                return false;
            }

        }

        /// <summary>
        /// Adds a monster to the monsters list. It gets a unique ID. This is used when saving targets
        /// </summary>
        /// <param name="creature"></param>
        private void AddMonsterToList(Monster monster)
        {

            monster.UniqueID = nextUniqueID;
            nextUniqueID++;

            monsters.Add(monster);
        }

        /// <summary>
        /// Return a creature (player or monster) reference from a unique ID. Used to check targets are valid after reload
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Creature GetCreatureByUniqueID(int id)
        {
            if (id == 0)
            {
                return player;
            }

            Creature foundCreature = monsters.Find(x => x.UniqueID == id);

            if (foundCreature == null)
            {
                LogFile.Log.LogEntryDebug("Couldn't find monster from ID " + id + " (may have been reaped)", LogDebugLevel.Medium);
            }

            return foundCreature;
        }

        /// <summary>
        /// A creature does something that creates a new creature, e.g. raising summoning.
        /// Adds to the summoning queue which is processed at the end
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool AddMonsterDynamic(Monster creature, int level, Point location)
        {
            //Try to add a creature at the requested location
            //This may fail due to something else being there or being non-walkable
            try
            {
                Map creatureLevel = levels[level];

                //Check square is accessable
                if (!MapSquareIsWalkable(level, location))
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Square not enterable", LogDebugLevel.Low);
                    return false;
                }

                //Check square has nothing else on it
                SquareContents contents = MapSquareContents(level, location);

                if (contents.monster != null)
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Monster at this square", LogDebugLevel.Low);
                    return false;
                }

                if (contents.player != null)
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Player at this square", LogDebugLevel.Low);
                    return false;
                }

                //Otherwise OK
                creature.LocationLevel = level;
                creature.LocationMap = location;

                creature.CalculateSightRadius();

                summonedMonsters.Add(creature);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddCreatureDynamic: ") + ex.Message);
                return false;
            }

        }


        /// <summary>
        /// Add an item to the dungeon. May fail if location is invalid or unwalkable
        /// </summary>
        /// <param name="item"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool AddItem(Item item, int level, Point location)
        {
            //Try to add a item at the requested location
            //This may fail due to the square being inaccessable
            try
            {
                Map creatureLevel = levels[level];

                //Check square is accessable
                if (!MapSquareIsWalkable(level, location))
                {
                    return false;
                }

                //DON'T PLACE UNDER MONSTER FOR FLATLINE

                //Check square has nothing else on it
                SquareContents contents = MapSquareContents(level, location);

                if (contents.monster != null)
                {
                    LogFile.Log.LogEntryDebug("AddItem failure: Monster at this square", LogDebugLevel.Low);
                    return false;
                }

                //Otherwise OK
                item.LocationLevel = level;
                item.LocationMap = location;

                items.Add(item);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddItem: ") + ex.Message);
                return false;
            }

        }

        /// <summary>
        /// Debug. Add an item to the dungeon. May fail if location is invalid or unwalkable
        /// </summary>
        /// <param name="item"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool AddItemNoChecks(Item item, int level, Point location)
        {
            //Try to add a item at the requested location
            //This may fail due to the square being inaccessable
            try
            {
                Map creatureLevel = levels[level];

                //Check square is accessable
                if (!MapSquareIsWalkable(level, location))
                {
                    return false;
                }

                //Otherwise OK
                item.LocationLevel = level;
                item.LocationMap = location;

                items.Add(item);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddItem: ") + ex.Message);
                return false;
            }

        }

        /// <summary>
        /// Add a feature that blocks (sets the square unwalkable)
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool AddFeatureBlocking(Feature feature, int level, Point location, bool blocksLight)
        {
            var addFeatureSuccess = AddFeature(feature, level, location);
            if (!addFeatureSuccess)
                return false;

            feature.IsBlocking = true;

            SetTerrainAtPoint(level, location, blocksLight ? MapTerrain.NonWalkableFeatureLightBlocking : MapTerrain.NonWalkableFeature);

            return true;
        }

        public bool BlockingFeatureAtLocation(int level, Point location)
        {
            var terrain = GetTerrainAtPoint(level, location);
            if (terrain == MapTerrain.NonWalkableFeature || terrain == MapTerrain.NonWalkableFeatureLightBlocking)
                return true;
            return false;
        }

        /// <summary>
        /// Add feature to the dungeon
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool AddFeature(Feature feature, int level, Point location)
        {
            //Try to add a feature at the requested location
            //This may fail due to something else being there or being non-walkable
            try
            {
                Map featureLevel = levels[level];

                //Check square is accessable
                if (!MapSquareIsWalkable(level, location))
                {
                    LogFile.Log.LogEntry("AddFeature: map square can't be entered");
                    return false;
                }

                //Check another feature isn't there
                foreach (Feature otherFeature in features)
                {
                    if (otherFeature.LocationLevel == level &&
                        otherFeature.LocationMap == location)
                    {
                        LogFile.Log.LogEntry("AddFeature: other feature already there");
                        return false;
                    }
                }

                //DON'T PLACE UNDER MONSTERS OR ITEMS - may make the square unroutable

                //Check square has nothing else on it
                SquareContents contents = MapSquareContents(level, location);

                if (contents.monster != null)
                {
                    LogFile.Log.LogEntryDebug("AddFeature failure: Monster at this square", LogDebugLevel.Low);
                    return false;
                }

                if (contents.items.Count() != 0)
                {
                    LogFile.Log.LogEntryDebug("AddFeature failure: Item at this square", LogDebugLevel.Low);
                    return false;
                }

                //Otherwise OK
                feature.LocationLevel = level;
                feature.LocationMap = location;

                features.Add(feature);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddFeature: ") + ex.Message);
                return false;
            }

        }

        /// <summary>
        /// Add a lock to the dungeon. Lock must have level & location specified
        /// </summary>
        public bool AddLock(Lock newLock)
        {
            //Try to add a feature at the requested location
            //This may fail due to something else being there or being non-walkable
            try
            {
                int level = newLock.LocationLevel;
                Point location = newLock.LocationMap;

                SetTerrainAtPoint(level, location, MapTerrain.ClosedLock);

                //Otherwise OK

                if (!locks.ContainsKey(newLock.Location))
                    locks[newLock.Location] = new List<Lock>();

                locks[newLock.Location].Add(newLock);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddLock: ") + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Add decoration feature to the dungeon. Make sure we don't cover up useful non-decoration features
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool AddDecorationFeature(Feature feature, int level, Point location)
        {
            //Try to add a feature at the requested location
            //This may fail due to something else being there or being non-walkable
            try
            {
                Map featureLevel = levels[level];

                //Check another non-decoration feature isn't there
                foreach (Feature otherFeature in features)
                {
                    if (otherFeature.LocationLevel == level &&
                        otherFeature.LocationMap == location)
                    {
                        if (otherFeature as UseableFeature != null)
                        {
                            LogFile.Log.LogEntry("AddDecorationFeature: non-decoration feature already there");
                            return false;
                        }
                    }
                }

                feature.LocationLevel = level;
                feature.LocationMap = location;

                features.Add(feature);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddDecorationFeature: ") + ex.Message);
                return false;
            }

        }

        /// <summary>
        /// Does the square contain a player or creature?
        /// </summary>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public SquareContents MapSquareContents(int level, Point location)
        {
            SquareContents contents = new SquareContents();

            //Check if we're off the map
            if (location.x < 0 || location.x >= levels[level].width || location.y < 0 || location.y > levels[level].height)
            {
                contents.offMap = true;
                return contents;
            }

            //Check creature that be blocking
            foreach (Monster creature in monsters)
            {
                if (creature.LocationLevel == level &&
                    creature.LocationMap.x == location.x && creature.LocationMap.y == location.y)
                {
                    contents.monster = creature;
                    break;
                }
            }

            //Check creature that be blocking
            foreach (Item item in items)
            {
                if (item.LocationLevel == level &&
                    item.LocationMap.x == location.x && item.LocationMap.y == location.y && item.InInventory == false)
                {
                    contents.items.Add(item);
                    break;
                }
            }

            //Check features
            foreach (Feature feature in features)
            {
                if (feature.LocationLevel == level &&
                    feature.LocationMap.x == location.x && feature.LocationMap.y == location.y)
                {
                    contents.feature = feature;
                    break;
                }
            }

            //Check for PC blocking
            //Allow this to work before having the player placed (at beginning of game)
            if (player != null && player.LocationMap != null)
            {
                if (player.LocationLevel == level && player.LocationMap.x == location.x && player.LocationMap.y == location.y)
                {
                    contents.player = player;
                }
            }
            if (contents.monster == null && contents.player == null)
                contents.empty = true;

            return contents;
        }

        public MapTerrain GetTerrainAtPoint(int level, Point location)
        {
            //Not a level
            if (level < 0 || level > levels.Count)
            {
                string error = "Level " + level + "does not exist";
                LogFile.Log.LogEntry(error);
                throw new ApplicationException(error);
            }

            //Off the map
            if (location.x < 0 || location.x >= levels[level].width ||
                location.y < 0 || location.y >= levels[level].height)
            {
                string error = "Location " + location.x + ":" + location.y + " does not exist on level " + level;
                LogFile.Log.LogEntry(error);
                throw new ApplicationException(error);
            }

            //Otherwise return terrain
            return levels[level].mapSquares[location.x, location.y].Terrain;
        }

        /// <summary>
        /// Is the requested square moveable into? Only deals with terrain, not creatures or items
        /// </summary>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool MapSquareIsWalkable(int level, Point location)
        {
            //Off the map
            if (location.x < 0 || location.x >= levels[level].width)
            {
                return false;
            }

            if (location.y < 0 || location.y >= levels[level].height)
            {
                return false;
            }

            //Not walkable
            if (!levels[level].mapSquares[location.x, location.y].Walkable)
            {
                //LogFile.Log.LogEntryDebug("MapSquareCanBeEntered failure: Not Walkable", LogDebugLevel.Low);
                return false;
            }

            //These are duplicates that use different code, so should be obsoleted

            //A wall - should be caught above
            if (!Dungeon.IsTerrainWalkable(levels[level].mapSquares[location.x, location.y].Terrain))
            {
                //LogFile.Log.LogEntryDebug("MapSquareCanBeEntered failure: not walkable by terrain type", LogDebugLevel.High);
                return false;
            }

            //Void (outside of map) - should be caught above
            if (levels[level].mapSquares[location.x, location.y].Terrain == MapTerrain.Void)
            {
                //LogFile.Log.LogEntryDebug("MapSquareCanBeEntered failure: Void", LogDebugLevel.High);
                return false;
            }

            //Otherwise OK
            return true;
        }

        /// <summary>
        /// Increments the world clock. May in future check events
        /// </summary>
        public void IncrementWorldClock()
        {
            worldClock++;
        }

        public int CurrentLevel
        {
            set
            {
                player.LocationLevel = value;
            }
        }

        //Get current map the PC is on
        public Map PCMap
        {
            get
            {
                return levels[player.LocationLevel];
            }
        }

        /// <summary>
        /// Get the list of maps
        /// </summary>
        public List<Map> Levels
        {
            get
            {
                return levels;
            }
        }

        /// <summary>
        /// Get the number of levels
        /// </summary>
        public int NoLevels
        {
            get
            {
                return levels.Count;
            }
        }


        /// <summary>
        /// For serialization only
        /// </summary>
        public List<SoundEffect> Effects
        {
            get
            {
                return effects;
            }
            set
            {
                effects = value;
            }
        }

        /// <summary>
        /// For serialization only
        /// </summary>
        public List<SpecialMove> SpecialMoves
        {
            get
            {
                return specialMoves;
            }
            set
            {
                specialMoves = value;
            }
        }

        /// <summary>
        /// For serialization only
        /// </summary>
        public List<Spell> Spells
        {
            get
            {
                return spells;
            }
            set
            {
                spells = value;
            }
        }

        //Get the list of creatures
        public List<Monster> Monsters
        {
            get
            {
                return monsters;
            }
            //For serialization
            set
            {
                monsters = value;
            }
        }

        /// <summary>
        /// List of all the items in the game
        /// </summary>
        public List<Item> Items
        {
            get
            {
                return items;
            }
            //For serialization
            set
            {
                items = value;
            }
        }

        /// <summary>
        /// List of all the features in the game
        /// </summary>
        public List<Feature> Features
        {
            get
            {
                return features;
            }
            //For serialization
            set
            {
                features = value;
            }
        }

        /// <summary>
        /// List of all the locks in the game
        /// </summary>
        public Dictionary<Location, List<Lock>> Locks
        {
            get
            {
                return locks;
            }
            //For serialization
            set
            {
                locks = value;
            }
        }


        public Player Player
        {
            get
            {
                return player;
            }
            //For serialization
            set
            {
                player = value;
            }
        }

        /// <summary>
        /// Move PC to an absolute square (doesn't check the contents). Runs triggers.
        /// Doesn't do any checking at the mo, should return false if there's a problem.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool MovePCAbsolute(int level, int x, int y)
        {
            return MovePCAbsolute(level, x, y, false);
        }

        internal bool MovePCAbsolute(int level, Point location)
        {
            return MovePCAbsolute(level, location.x, location.y, false);
        }

        /// <summary>
        /// Move PC to an absolute square (doesn't check the contents). Runs triggers.
        /// Doesn't do any checking at the mo, should return false if there's a problem.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool MovePCAbsolute(int level, int x, int y, bool runTriggersAlways)
        {
            player.LocationLevel = level;

            //Don't run triggers if we haven't moved

            if (player.LocationMap.x == x && player.LocationMap.y == y && !runTriggersAlways)
            {
                return true;
            }

            player.LocationMap = new Point(x, y);

            //Kill monsters if they are going to be under the PC
            foreach (Monster monster in monsters)
            {
                if (monster.InSameSpace(player))
                {
                    KillMonster(monster, false);
                }
            }

            RunDungeonTriggers(player.LocationLevel, player.LocationMap);

            return true;
        }

        /// <summary>
        /// Move a creature to a location
        /// </summary>
        /// <param name="monsterToMove"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        internal bool MoveMonsterAbsolute(Monster monsterToMove, int level, Point location)
        {
            monsterToMove.LocationLevel = level;
            monsterToMove.LocationMap = location;

            //Do anything needed with the AI, not needed right now

            return true;
        }

        /// <summary>
        /// Move PC to another square on the same level. Doesn't do any checking at the mo
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool MovePCAbsoluteSameLevel(int x, int y)
        {
            return MovePCAbsoluteSameLevel(x, y, false);
        }

        /// <summary>
        /// Move PC to another square on the same level. Doesn't do any checking at the mo
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool MovePCAbsoluteSameLevel(int x, int y, bool runTriggersAlways)
        {

            MovePCAbsolute(player.LocationLevel, x, y, runTriggersAlways);

            return true;
        }
        /// <summary>
        /// Move PC to another square on the same level. Doesn't do any checking at the mo
        /// </summary>
        internal bool MovePCAbsoluteSameLevel(Point location)
        {
            MovePCAbsolute(player.LocationLevel, location.x, location.y);

            return true;
        }

        /// <summary>
        /// Return a random monster on the level, or null if none
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public Monster RandomMonsterOnLevel(int level)
        {
            //Fail if we have been asked for an invalid level
            if (level < 0 || level > levels.Count)
            {
                LogFile.Log.LogEntry("RandomMonsterOnLevel: Level " + level + " does not exist");
                return null;
            }

            List<Monster> monstersOnLevel = new List<Monster>();

            foreach (Monster monster in monsters)
            {
                if (monster.LocationLevel == level)
                {
                    monstersOnLevel.Add(monster);
                }
            }

            if (monstersOnLevel.Count == 0)
            {
                return null;
            }

            return monstersOnLevel[Game.Random.Next(monstersOnLevel.Count)];
        }

        /// <summary>
        /// Return the instance of the special move class
        /// </summary>
        /// <param name="specialMove"></param>
        /// <returns></returns>
        SpecialMove FindSpecialMove(Type specialMove)
        {
            return specialMoves.Find(x => x.GetType() == specialMove);
        }

        private bool InteractWithFeature()
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = dungeon.Player;

            Feature featureAtSpace = dungeon.FeatureAtSpace(player.LocationLevel, player.LocationMap);

            UseableFeature useableFeature = featureAtSpace as UseableFeature;
            if (useableFeature == null)
            {
                //Game.MessageQueue.AddMessage("Nothing to interact with here");
                return false;
            }

            //Interact with feature - these will normally put success / failure messages in queue
            return useableFeature.PlayerInteraction(player);
        }

        /// <summary>
        /// Process a relative PC move, from a keypress
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool PCMove(int x, int y)
        {
            return PCMove(x, y, false);
        }

        /// <summary>
        /// Process a relative PC move, from a keypress
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool PCMove(int x, int y, bool runTriggersAlways)
        {
            Point newPCLocation = new Point(Player.LocationMap.x + x, Player.LocationMap.y + y);
            Point deltaMove = newPCLocation - Player.LocationMap;

            //Moves off the map don't work

            if (newPCLocation.x < 0 || newPCLocation.x >= levels[player.LocationLevel].width)
            {
                return false;
            }

            if (newPCLocation.y < 0 || newPCLocation.y >= levels[player.LocationLevel].height)
            {
                return false;
            }

            //Check special moves. These take precidence over normal moves. Only if no special move is ready do we do normal resolution here
            SpecialMove moveDone = DoSpecialMove(newPCLocation);

            bool okToMoveIntoSquare = true;

            //Apply environmental effects
            if (Game.Dungeon.DungeonInfo.LevelNaming[player.LocationLevel] == "Arcology")
            {
                if (!player.IsEffectActive(typeof(PlayerEffects.BioProtect)))
                {
                    player.ApplyDamageToPlayerHitpoints(5);
                }
            }

            bool stationaryAction = false;

            //If there's no special move, do a conventional move
            if (moveDone == null)
            {
                //If square is not walkable exit, except in special conditions
                if (!MapSquareIsWalkable(player.LocationLevel, newPCLocation))
                {
                    //Is there a closed door? This is a move, so return
                    if (GetTerrainAtPoint(player.LocationLevel, newPCLocation) == MapTerrain.ClosedDoor)
                    {
                        OpenDoor(player.LocationLevel, newPCLocation);
                        stationaryAction = true;
                        okToMoveIntoSquare = false;
                    }
                    else if (GetTerrainAtPoint(player.LocationLevel, newPCLocation) == MapTerrain.ClosedLock)
                    {
                        //Is there a lock at the new location? Interact
                        var locksAtLocation = LocksAtLocation(player.LocationLevel, newPCLocation);

                        //Try to open each lock
                        foreach (var thisLock in locksAtLocation)
                        {
                            bool thisSuccess = true;
                            if (!thisLock.IsOpen())
                            {
                                thisSuccess = thisLock.OpenLock(player);
                                if (thisSuccess)
                                    SetTerrainAtPoint(player.LocationLevel, newPCLocation, MapTerrain.OpenLock);
                            }
                        }

                        stationaryAction = true;
                        okToMoveIntoSquare = false;
                    }
                    okToMoveIntoSquare = false;
                }

                //Check for monsters in the square
                SquareContents contents = MapSquareContents(player.LocationLevel, newPCLocation);

                //Monster - check for charm / passive / normal status
                if (contents.monster != null)
                {
                    Monster monster = contents.monster;

                    if (monster.Charmed)
                    {
                        //Switch monster to PC position
                        monster.LocationMap = new Point(Player.LocationMap.x, Player.LocationMap.y);

                        //PC will move to monster's old location
                        okToMoveIntoSquare = true;

                    }
                    else if (monster.Passive)
                    {
                        //Attack the passive creature.
                        CombatResults results = player.AttackMonsterMelee(contents.monster);
                        Screen.Instance.CreatureToView = contents.monster;

                        Screen.Instance.DrawMeleeAttack(player, contents.monster, results);
                        okToMoveIntoSquare = false;

                        stationaryAction = true;
                    }
                    else
                    {
                        //Monster hostile 

                        CombatResults results = player.AttackMonsterMelee(contents.monster);
                        Screen.Instance.DrawMeleeAttack(player, contents.monster, results);
                        Screen.Instance.CreatureToView = contents.monster;

                        okToMoveIntoSquare = false;

                        stationaryAction = true;
                    }
                }

                //Apply movement effects to counters
                if (stationaryAction)
                {
                    ResetPCTurnCountersOnActionStatonary();
                }
                else
                {
                    player.AddTurnsSinceAction();
                    if (deltaMove == new Point(0, 0))
                    {
                        player.ResetTurnsMoving();
                        player.AddTurnsInactive();
                    }
                    else
                    {
                        player.ResetTurnsInactive();
                        player.AddTurnsMoving();
                    }
                }

                //If not OK to move, return here
                if (!okToMoveIntoSquare)
                    return true;

                MovePCAbsoluteSameLevel(newPCLocation.x, newPCLocation.y, runTriggersAlways);

                //Auto-pick up any items
                if (contents.items.Count > 0)
                {
                    //Pick up first item only
                    //Might help if the player makes a massive pile
                    PickUpItemInSpace();
                }

                //If there is a feature, auto interact
                InteractWithFeature();
            }

            //Run any entering square messages
            //Happens for both normal and special moves

            //Tell the player if there are items here
            //Don't tell the player again if they haven't moved

            Item itemAtSpace = ItemAtSpace(player.LocationLevel, player.LocationMap);
            if (itemAtSpace != null)
            {
                Game.MessageQueue.AddMessage("There is a " + itemAtSpace.SingleItemDescription + " here.");
            }

            //Tell the player if there are multiple items in the square
            if (MultipleItemAtSpace(player.LocationLevel, player.LocationMap))
            {
                Game.MessageQueue.AddMessage("There are multiple items here.");
            }

            //If there is a feature and an item (feature will be hidden)
            if (FeatureAtSpace(player.LocationLevel, player.LocationMap) != null &&
                ItemAtSpace(player.LocationLevel, player.LocationMap) != null)
            {
                Game.MessageQueue.AddMessage("There is something behind it.");
            }

            return true;
        }

        private SpecialMove DoSpecialMove(Point newPCLocation)
        {
            //New version

            //First check moves that have integrated movement

            Point deltaMove = newPCLocation - Player.LocationMap;

            SpecialMove moveDone = null;
            Point overrideRelativeMove = null;
            bool noMoveSubsequently = false;
            bool specialMoveSuccess = false;

            //For moves that have a bonus attack, collect them in bonusAttack list
            List<Point> bonusAttack = new List<Point>();

            foreach (SpecialMove move in specialMoves)
            {
                if (move.CausesMovement() && move.Known)
                {
                    bool moveSuccess = move.CheckAction(true, deltaMove, specialMoveSuccess);

                    if (moveSuccess && move.AddsAttack())
                    {
                        //Save any extra attacks
                        if (move.AttackIsOn())
                            bonusAttack.Add(move.RelativeAttackVector());
                    }

                    if (!moveSuccess)
                    {
                        //Test the move twice on first failure
                        //The first check may cause a long chain to fail but the move could be a valid new start move
                        //The second check picks this up
                        move.CheckAction(true, deltaMove, specialMoveSuccess);

                        if (moveSuccess && move.AddsAttack())
                        {
                            //Save any extra attacks
                            if (move.AttackIsOn())
                                bonusAttack.Add(move.RelativeAttackVector());
                        }

                    }
                }
            }

            //Carry out movement special moves. Only 1 can trigger at a time (because their completions are orthogonal)

            foreach (SpecialMove move in specialMoves)
            {
                if (move.CausesMovement() && move.Known && move.MoveComplete())
                {
                    //Carry out the move. This will update the player's position so the new relative move makes sense
                    move.DoMove(deltaMove, false);
                    moveDone = move;
                    specialMoveSuccess = true;

                    //On success store the relativised move
                    //e.g. for WallLeap, the real move was a move into the wall but the relativised move is an attack in the opposite direction on the monster leaped to
                    overrideRelativeMove = move.RelativeMoveAfterMovement();
                }
            }

            //If we had a success for one of the special movement moves, adopt the new relative move
            if (overrideRelativeMove != null)
            {
                deltaMove = overrideRelativeMove;
                //Tell subsequent moves that we have already had a special move movement. For simultaneous moves like OpenGround/Multi or OpenGround/Close
                //don't move twice
                noMoveSubsequently = true;
            }

            //Now check any remaining moves that have bonus attacks but don't cause movement

            foreach (SpecialMove move in specialMoves)
            {
                if (move.AddsAttack() && !move.CausesMovement() && move.Known)
                {
                    bool moveSuccess = move.CheckAction(true, deltaMove, specialMoveSuccess);

                    if (moveSuccess)
                    {
                        //Save any extra attacks
                        if (move.AttackIsOn())
                            bonusAttack.Add(move.RelativeAttackVector());
                    }
                    else
                    {
                        //Test the move twice on first failure
                        //The first check may cause a long chain to fail but the move could be a valid new start move
                        //The second check picks this up
                        move.CheckAction(true, deltaMove, specialMoveSuccess);

                        if (moveSuccess)
                        {
                            //Save any extra attacks
                            if (move.AttackIsOn())
                                bonusAttack.Add(move.RelativeAttackVector());
                        }
                    }
                }
            }

            //Now check any moves that start with an attack. If they are not already in progress, then give them a chance to start again with the bonus attacks
            //At the mo, bonus attacks only occur on moves which aren't normal attacks, so it's OK to check bonus attacks before checking normal attacks

            foreach (Point attackVector in bonusAttack)
            {
                foreach (SpecialMove move in specialMoves)
                {
                    if (move.StartsWithAttack() && move.Known && move.CurrentStage() == 0)
                    {
                        bool moveSuccess = move.CheckAction(true, attackVector, specialMoveSuccess);
                    }
                }
            }

            //Now check all remaining moves with the normal move

            foreach (SpecialMove move in specialMoves)
            {
                if (!move.CausesMovement() && !move.StartsWithAttack() && !move.AddsAttack() && !move.NotSimultaneous() && move.Known)
                {
                    //Test the move twice
                    //The first check may cause a long chain to fail but the move could be a valid new start move
                    //The second check picks this up

                    bool moveSuccess = move.CheckAction(true, deltaMove, specialMoveSuccess);

                    if (moveSuccess)
                    {
                    }
                    else
                    {
                        moveSuccess = move.CheckAction(true, deltaMove, specialMoveSuccess);

                        if (moveSuccess)
                        {
                        }
                    }
                }
            }

            //Carry out any moves which are ready (movement causing ones have already been done)
            //Need to exclude ones which cause movement, since they have already been carried out (e.g. multi attack which isn't cancelled by an attack, i.e. still complete)

            foreach (SpecialMove move in specialMoves)
            {
                if (move.Known && move.MoveComplete() && !move.CausesMovement())
                {
                    moveDone = move;
                    specialMoveSuccess = true;
                    move.DoMove(deltaMove, noMoveSubsequently);
                }
            }

            //Finally carry out the non-simultaneous ones
            foreach (SpecialMove move in specialMoves)
            {
                if (move.NotSimultaneous() && move.Known)
                {
                    //Test the move twice
                    //The first check may cause a long chain to fail but the move could be a valid new start move
                    //The second check picks this up

                    bool moveSuccess = move.CheckAction(true, deltaMove, specialMoveSuccess);

                    if (!moveSuccess)
                    {
                        moveSuccess = move.CheckAction(true, deltaMove, specialMoveSuccess);
                    }
                }
            }

            foreach (SpecialMove move in specialMoves)
            {
                if (move.Known && move.NotSimultaneous() && move.MoveComplete())
                {
                    moveDone = move;
                    move.DoMove(deltaMove, noMoveSubsequently);
                }
            }
            return moveDone;
        }

        public void ExplodeAllMonsters()
        {
            List<Monster> livingMonstersOnLevel = monsters.FindAll(x => x.Alive && x.LocationLevel == player.LocationLevel);

            foreach (Monster m in livingMonstersOnLevel)
            {
                List<Point> grenadeAffects = Game.Dungeon.GetPointsForGrenadeTemplate(m.LocationMap, Game.Dungeon.Player.LocationLevel, 4 + Game.Random.Next(3));

                Color randColor = ColorPresets.Red;
                int randInt = Game.Random.Next(5);

                switch (randInt)
                {
                    case 0:
                        randColor = ColorPresets.Red;
                        break;
                    case 1:
                        randColor = ColorPresets.Orange;
                        break;
                    case 2:
                        randColor = ColorPresets.Yellow;
                        break;
                    case 3:
                        randColor = ColorPresets.OrangeRed;
                        break;
                    case 4:
                        randColor = ColorPresets.DarkRed;
                        break;
                }

                KillMonster(m, false);

                Screen.Instance.DrawAreaAttackAnimation(grenadeAffects, randColor);
                Screen.Instance.Update();
            }
        }
        /// <summary>
        /// Generic throw method for most grenade items
        /// Should sync with above method
        /// </summary>
        /// <param name="item"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public Point ThrowItemGrenadeLike(IEquippableItem item, int level, Point target, double size, int damage)
        {
            Item itemAsItem = item as Item;

            LogFile.Log.LogEntryDebug("Throwing " + itemAsItem.SingleItemDescription, LogDebugLevel.Medium);

            Player player = Game.Dungeon.Player;

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
                destination = player.LocationMap;


            //Stopped by a monster
            if (monster != null)
            {
                destination = monster.LocationMap;
            }

            //Make throwing sound AT target location
            Game.Dungeon.AddSoundEffect(item.ThrowSoundMagnitude(), Game.Dungeon.Player.LocationLevel, destination);

            if (Player.LocationLevel >= 6)
                damage *= 2;

            //Work out grenade splash and damage
            DoGrenadeExplosion(level, target, size, damage, null);

            return (destination);

        }

        /// <summary>
        /// Do a grenade explosion. If this originated on a monster pass that in
        /// </summary>
        /// <param name="level"></param>
        /// <param name="p"></param>
        /// <param name="size"></param>
        /// <param name="damage"></param>
        /// <param name="originMonster"></param>
        public void DoGrenadeExplosion(int level, Point locationMap, double size, int damage, Monster originMonster)
        {
            //Work out grenade splash and damage
            List<Point> grenadeAffects = GetPointsForGrenadeTemplate(locationMap, level, size);

            //Use FOV from point of explosion (this means grenades don't go round corners or through walls)
            WrappedFOV grenadeFOV = Game.Dungeon.CalculateAbstractFOV(level, locationMap, 0);

            var grenadeAffectsFiltered = grenadeAffects.Where(sq => grenadeFOV.CheckTileFOV(level, sq));

            //Draw attack
            Screen.Instance.DrawAreaAttackAnimation(grenadeAffectsFiltered, ColorPresets.Chocolate);
           
            foreach (Point sq in grenadeAffectsFiltered)
            {
                SquareContents squareContents = Game.Dungeon.MapSquareContents(level, sq);

                Monster m = squareContents.monster;

                //Don't attack ourself -will loop
                if (m == originMonster)
                    continue;

                if (m != null && !m.Alive)
                    continue;

                //Hit the monster if it's there
                if (m != null)
                {
                    string combatResultsMsg = "PvM (" + m.Representation + ") Grenade: Dam: " + damage;
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    //Apply damage
                    //make this a player attack to avoid AI being confused by monster attack that disappears
                    Game.Dungeon.Player.AttackMonsterRanged(squareContents.monster, damage);
                }
            }

            //And the player

            if (grenadeAffects.Find(p => p.x == Game.Dungeon.Player.LocationMap.x && p.y == Game.Dungeon.Player.LocationMap.y) != null)
            {
                //Apply damage (uses damage base)
                if (originMonster != null)
                    originMonster.AttackPlayer(Game.Dungeon.Player, damage);
                else
                    player.AttackPlayer(damage);
            }
        }

        /// <summary>
        /// Kill a monster. This monster won't get any further turns.
        /// If autokill is set to true, this is a dungeon respawn or similar. Don't count towards achievements
        /// </summary>
        /// <param name="monster"></param>
        public void KillMonster(Monster monster, bool autoKill)
        {
            //We can't take the monster out of the collection directly since we might still be iterating through them
            //Instead set a flag on the monster and remove it after all turns are complete
            monster.Alive = false;

            //Remove all existing effects
            monster.RemoveAllEffects();

            //Notify the monster that it has been killed
            monster.NotifyMonsterDeath();

            //Drop its inventory (including plot items we gave it)
            monster.DropAllItems(monster.LocationLevel);

            //Drop any insta-create treasure
            //Not used at present
            if (!autoKill)
                monster.InventoryDrop();

            //If the creature was charmed, delete 1 charmed creature from the player total
            if (monster.Charmed)
                Game.Dungeon.Player.RemoveCharmedCreature();

            //Leave a corpse
            if (!autoKill)
                AddDecorationFeature(new Features.Corpse(monster.GetCorpseRepresentation(), monster.GetCorpseRepresentationColour()), monster.LocationLevel, monster.LocationMap);

            //Deal with special death effects, but not on an autokill
            if (!autoKill)
            {
                monster.OnKilledSpecialEffects();
                /*
                if (monster.Unique)
                {
                    //We killed an objective
                    bool a = monsters[0] is Creatures.ComputerNode;

                    //Check if there are any living computer nodes on the level
                    Monster livingNode = monsters.Find(x => x is Creatures.ComputerNode && x.Alive && x.LocationLevel == player.LocationLevel);
                    if (livingNode == null)
                    {
                        //Set objective complete
                        dungeonInfo.Dungeons[player.LocationLevel].LevelObjectiveComplete = true;

                        if (player.LocationLevel == 0)
                        {
                            Screen.Instance.PlayMovie("mission0done", true);
                        }

                        if (player.LocationLevel == 14)
                        {
                            //Last level

                            //All remaining monsters explode
                            ExplodeAllMonsters();

                            Game.MessageQueue.AddMessage("It's done. The Space Hulk is yours. Fly back home a VICTOR!");
                            dungeonInfo.Dungeons[player.LocationLevel].LevelObjectiveComplete = true;
                            dungeonInfo.Dungeons[player.LocationLevel].LevelObjectiveKillAllMonstersComplete = true;
                        }
                        else
                        {
                            Game.MessageQueue.AddMessage("All computer nodes destroyed. Primary objective complete! Return to docking bay.");
                            LogFile.Log.LogEntryDebug("Level " + player.LocationLevel + " primary objective complete", LogDebugLevel.Medium);
                        }
                        
                    }
                
                }*/

                //No monsters left on level?
                /*
                Monster livingMonster = monsters.Find(x => x.Alive && x.LocationLevel == player.LocationLevel);

                if (livingMonster == null)
                {
                    //Set secondary objective complete
                    dungeonInfo.Dungeons[player.LocationLevel].LevelObjectiveKillAllMonstersComplete = true;

                    Game.MessageQueue.AddMessage("All defenses disabled. Secondary objective complete! Return to docking bay.");
                    LogFile.Log.LogEntryDebug("Level " + player.LocationLevel + " secondary objective complete", LogDebugLevel.Medium);
                }

                if (dungeonInfo.LastMission && monster.Unique)
                {

                    //OK we killed the dragon!

                    //Kill all the monsters on this level
                    foreach (Monster m in monsters)
                    {
                        //But not itself

                        //Creatures.DragonUnique drag = m as Creatures.DragonUnique;

                        if (m.LocationLevel == player.LocationLevel)
                        {
                            Creatures.DragonUnique drag = m as Creatures.DragonUnique;
                            Creatures.Friend friend = m as Creatures.Friend;
                            if (m != drag && m != friend)
                                KillMonster(m, true);
                        }
                    }

                    Screen.Instance.PlayMovie("dragondead", true);

                    dungeonInfo.DragonDead = true;
                }*/
            }
        }

        /// <summary>
        /// Remove all dead creatures from the list so they are not processed again
        /// </summary>
        public void RemoveDeadMonsters()
        {
            //Can use RemoveAll now
            List<Monster> deadMonsters = new List<Monster>();

            foreach (Monster monster in monsters)
            {
                if (monster.Alive == false)
                {
                    deadMonsters.Add(monster);
                }
            }

            foreach (Monster monster in deadMonsters)
            {
                monsters.Remove(monster);
            }
        }

        /// <summary>
        /// Pick up an item if there is one in this square
        /// </summary>
        /// <returns></returns>
        public bool PickUpItemInSpace()
        {
            Player player = Player;

            var itemToPickUp = ItemsAtSpace(player.LocationLevel, player.LocationMap).ToList();

            if (!itemToPickUp.Any())
                return false;

            foreach (var item in itemToPickUp)
            {
                Game.MessageQueue.AddMessage(item.SingleItemDescription + " picked up.");

                item.OnPickup(player);
                player.PickUpItem(item);

            }
            return true;
        }

        /// <summary>
        /// Set all items on the floor in this level to visible
        /// </summary>
        /// <param name="level"></param>
        public void RevealItemsOnLevel(int level)
        {
            //Check level
            if (level < 0 || level > levels.Count)
            {
                LogFile.Log.LogEntryDebug("RevealItemsOnLevel: Asked for non-existant level: " + level, LogDebugLevel.High);
                return;
            }

            //Find all items on level on floor

            List<Item> itemsOnLevel = items.FindAll(x => x.LocationLevel == level);
            List<Item> itemsOnFloor = itemsOnLevel.FindAll(x => !x.InInventory);

            Map map = levels[level];

            foreach (Item item in itemsOnFloor)
            {
                map.mapSquares[item.LocationMap.x, item.LocationMap.y].SeenByPlayerThisRun = true;
            }
        }

        /// <summary>
        /// Refresh the TCOD maps used for FOV and pathfinding
        /// Unoptimised at present
        /// </summary>
        public void RefreshAllLevelPathingAndFOV()
        {
            //Set the walkable flag based on the terrain
            for (int i = 0; i < levels.Count; i++)
            {
                levels[i].RecalculateWalkable();
            }

            //Set the light blocking flag based on the terrain
            for (int i = 0; i < levels.Count; i++)
            {
                levels[i].RecalculateLightBlocking();
            }

            //Set the properties on the TCODMaps from our Maps
            for (int i = 0; i < levels.Count; i++)
            {

                //New fov representation
                fov.updateFovMap(i, levels[i].FovRepresentaton);

                //Notify abstract path finding lib
                pathFinding.PathFindingInternal.updateMap(i, levels[i].PathRepresentation);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void ResetCreatureFOVOnMap()
        {
            Map level = levels[Player.LocationLevel];

            foreach (MapSquare sq in level.mapSquares)
            {
                sq.InMonsterFOV = false;
            }
        }

        public void ResetSoundOnMap()
        {
            Map level = levels[Player.LocationLevel];

            foreach (MapSquare sq in level.mapSquares)
            {
                sq.SoundMag = 0;
            }
        }

        /// <summary>
        /// Calculates the FOV for a creature
        /// </summary>
        /// <param name="creature"></param>
        public CreatureFOV CalculateCreatureFOV(Creature creature)
        {
            Map currentMap = levels[creature.LocationLevel];

            //Update FOV
            fov.CalculateFOV(creature.LocationLevel, creature.LocationMap, creature.SightRadius);

            //Wrapper with game-specific FOV layer
            CreatureFOV wrappedFOV = new CreatureFOV(creature, new WrappedFOV(fov), creature.FOVType());

            return wrappedFOV;
        }


        public Point GetEndOfLine(Point start, Point midPoint, int level)
        {
            int deltaX = midPoint.x - start.x;
            int deltaY = midPoint.y - start.y;

            Vector3 dirVector = new Vector3(deltaX, deltaY, 0);
            dirVector.Normalize();

            bool endNow = false;

            Vector3 startVector = new Vector3(start.x, start.y, 0);

            Vector3 lastVector = startVector;

            do
            {
                startVector += dirVector;

                if (startVector.X >= levels[level].width || startVector.X < 0 ||
                    startVector.Y >= levels[level].height || startVector.Y < 0)
                    endNow = true;
                else
                    lastVector = startVector;

            } while (!endNow);

            return new Point((int)Math.Floor(lastVector.X), (int)Math.Floor(lastVector.Y));

        }

        /// <summary>
        /// Get points on a line in order.
        /// /// Only returns points within FOV. Moral: If you can see it, you can shoot it.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<Point> GetPathLinePointsInFOV(int level, Point start, Point end, WrappedFOV fov)
        {
            List<Point> pointsToRet = new List<Point>();

            foreach (Point p in Utility.GetPointsOnLine(start, end))
            {
                if (fov.CheckTileFOV(level, p))
                {
                    pointsToRet.Add(p);
                }
            }

            return pointsToRet;
        }


        /// <summary>
        /// Calculates the FOV for a creature if it was in the location
        public CreatureFOV CalculateCreatureFOV(Creature creature, Point location)
        {
            Map currentMap = levels[creature.LocationLevel];

            //Update FOV
            fov.CalculateFOV(creature.LocationLevel, location, creature.SightRadius);

            //Wrapper with game-specific FOV layer
            CreatureFOV wrappedFOV = new CreatureFOV(creature, new WrappedFOV(fov), creature.FOVType(), location);

            return wrappedFOV;

        }

        /// <summary>
        /// Calculates the FOV for an abstract point. Uses the old TCODFov without modification
        public WrappedFOV CalculateAbstractFOV(int level, Point mapLocation, int sightRadius)
        {
            Map currentMap = levels[level];

            //Update FOV
            fov.CalculateFOV(level, mapLocation, sightRadius);

            return new WrappedFOV(fov);
        }


        /// <summary>
        /// Show all sounds on map for debug purposes
        /// </summary>
        public void ShowSoundsOnMap()
        {
            //Debug: show all sounds on the map

            foreach (SoundEffect effectPair in Game.Dungeon.Effects)
            {
                SoundEffect sEffect = effectPair;

                Map currentMap = levels[sEffect.LevelLocation];

                //Check if the sound is too old to draw
                double soundDecay = sEffect.DecayedMagnitude(WorldClock);

                if (soundDecay < 0.0001)
                    continue;

                int soundRadius = (int)Math.Ceiling(sEffect.SoundRadius());

                //Draw circle around sound

                int xl = sEffect.MapLocation.x - soundRadius;
                int xr = sEffect.MapLocation.x + soundRadius;

                int yt = sEffect.MapLocation.y - soundRadius;
                int yb = sEffect.MapLocation.y + soundRadius;

                if (xl < 0)
                    xl = 0;
                if (xr >= currentMap.width)
                    xr = currentMap.width - 1;
                if (yt < 0)
                    yt = 0;
                if (yb >= currentMap.height)
                    yb = currentMap.height - 1;

                for (int i = xl; i <= xr; i++)
                {
                    for (int j = yt; j <= yb; j++)
                    {
                        MapSquare thisSquare = currentMap.mapSquares[i, j];
                        thisSquare.SoundMag = sEffect.DecayedMagnitude(WorldClock, sEffect.LevelLocation, new Point(i, j));
                    }
                }

            }

        }

        /// <summary>
        /// Displays the creature FOV on the map. Note that this clobbers the FOV map
        /// </summary>
        /// <param name="creature"></param>
        public void ShowCreatureFOVOnMap(Creature creature)
        {

            //Only do this if the creature is on a visible level
            if (creature.LocationLevel != Player.LocationLevel)
                return;

            Map currentMap = levels[creature.LocationLevel];

            //Calculate FOV
            CreatureFOV creatureFov = Game.Dungeon.CalculateCreatureFOV(creature);

            //Only check sightRadius around the creature

            int sightRangeMax = 20;
            int xl = creature.LocationMap.x - sightRangeMax;
            int xr = creature.LocationMap.x + sightRangeMax;

            int yt = creature.LocationMap.y - sightRangeMax;
            int yb = creature.LocationMap.y + sightRangeMax;

            //If sight is infinite, check all the map
            //if (creature.SightRadius == 0)
            //{
            //}

            //Always check the whole map (now we have strange FOVs)
            // (may not be necessary) [and is certainly slow]

            //According to profiling this is *BY FAR* the slowest thing in the game

            /*
            int xl = 0;
            int xr = currentMap.width;
            int yt = 0;
            int yb = currentMap.height;*/

            if (xl < 0)
                xl = 0;
            if (xr >= currentMap.width)
                xr = currentMap.width - 1;
            if (yt < 0)
                yt = 0;
            if (yb >= currentMap.height)
                yb = currentMap.height - 1;

            for (int i = xl; i <= xr; i++)
            {
                for (int j = yt; j <= yb; j++)
                {
                    MapSquare thisSquare = currentMap.mapSquares[i, j];
                    bool inFOV = creatureFov.CheckTileFOV(i, j);
                    if (inFOV)
                        thisSquare.InMonsterFOV = true;
                }
            }
        }

        /// <summary>
        /// Recalculate the players FOV. Subsequent accesses to the TCODMap of the player's level will have his FOV
        /// Note that the maps may get hijacked by other creatures
        /// </summary>
        internal CreatureFOV CalculatePlayerFOV()
        {
            //Get TCOD to calculate the player's FOV
            Map currentMap = levels[Player.LocationLevel];

            CreatureFOV tcodFOV = Game.Dungeon.CalculateCreatureFOV(player);

            //Set the FOV flags on the map
            //Process the whole level, which effectively resets out-of-FOV areas

            for (int i = 0; i < currentMap.width; i++)
            {
                for (int j = 0; j < currentMap.height; j++)
                {
                    MapSquare thisSquare = currentMap.mapSquares[i, j];
                    thisSquare.InPlayerFOV = tcodFOV.CheckTileFOV(i, j);
                    //Set 'has ever been seen flag' if appropriate
                    if (thisSquare.InPlayerFOV == true)
                    {
                        thisSquare.SeenByPlayer = true;
                    }
                }
            }

            return tcodFOV;
        }



        public long WorldClock
        {
            get
            {
                return worldClock;
            }
            //For serialization
            set
            {
                worldClock = value;
            }
        }

        /// <summary>
        /// Add a dungeon-wide sound effect, occurring now on the WorldClock
        /// mapLevel - dungeonLevel
        /// soundMagnitude - 0 -> 1
        /// mapLocation - mapLocation
        /// </summary>
        internal SoundEffect AddSoundEffect(double soundMagnitude, int mapLevel, Point mapLocation)
        {
            SoundEffect newEffect = new SoundEffect(nextUniqueSoundID, this, WorldClock, soundMagnitude, mapLevel, mapLocation);

            effects.Add(newEffect);
            nextUniqueSoundID++;
            LogFile.Log.LogEntryDebug("Adding new sound mag: " + soundMagnitude.ToString() + " at level: " + mapLevel.ToString() + " loc: " + mapLocation.ToString(), LogDebugLevel.Medium);

            return newEffect;
        }

        /// <summary>
        /// Get Sound Effect by ID. Sound effects are stored by ID in creatures for easier serialization
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal SoundEffect GetSoundByID(int id)
        {
            //Data structure really needs indexing on id
            foreach (SoundEffect ef in effects)
            {
                if (ef.ID == id)
                    return ef;
            }
            string msg = "Can't find sound ID: " + id;
            LogFile.Log.LogEntryDebug(msg, LogDebugLevel.High);
            throw new ApplicationException(msg);
        }

        /// <summary>
        /// Return sounds after (not including) a particular tick. Used to check for new sounds since last decision
        /// </summary>
        /// <param name="soundAfterThisTime"></param>
        /// <returns></returns>
        internal List<SoundEffect> GetSoundsAfterTime(long soundAfterThisTime)
        {
            List<SoundEffect> newSounds = new List<SoundEffect>();

            //Should do a binary search here

            //Could cache the result

            //SortedList doesn't let us do duplicate keys, so I need a filtering solution instead (inefficient)

            foreach (SoundEffect soundPair in effects)
            {
                if (soundPair.SoundTime > soundAfterThisTime)
                    newSounds.Add(soundPair);
            }

            /*
            IList<long> keys = effects.Keys;
            IList<SoundEffect> values = effects.Values;

            int firstIndexGreater = keys.Count;
            
            for (int i = keys.Count - 1; i >= 0; i--)
            {
                if (keys[i] > soundAfterThisTime)
                    firstIndexGreater = i;
            }

            for (int i = firstIndexGreater; i <= keys.Count; i++)
            {
                newSounds.Add(keys[i], values[i]);
            }*/

            return newSounds;
        }


        /// <summary>
        /// Return a (the first) feature at this location or null. Ignores decorativefeatures
        /// </summary>
        /// <param name="locationLevel"></param>
        /// <param name="locationMap"></param>
        /// <returns></returns>
        internal Feature FeatureAtSpace(int locationLevel, Point locationMap)
        {
            foreach (Feature feature in features)
            {
                if (feature.IsLocatedAt(locationLevel, locationMap) && feature is UseableFeature)
                {
                    return feature;
                }
            }

            return null;
        }

        /// <summary>
        /// Return an item if there is one at the requested square, or return null if not
        /// </summary>
        /// <param name="locationLevel"></param>
        /// <param name="locationMap"></param>
        /// <returns></returns>
        internal Item ItemAtSpace(int locationLevel, Point locationMap)
        {
            foreach (Item item in items)
            {
                if (item.IsLocatedAt(locationLevel, locationMap) &&
                    !item.InInventory)
                {
                    return item;
                }
            }

            return null;
        }

        public bool IsItemAtSpace(int locationLevel, Point locationMap)
        {
            return ItemAtSpace(locationLevel, locationMap) != null ? true : false;
        }

        internal IEnumerable<Item> ItemsAtSpace(int locationLevel, Point locationMap)
        {
            return items.Where(i => i.IsLocatedAt(locationLevel, locationMap) && !i.InInventory);


        }

        internal List<Lock> LocksAtLocation(int level, Point mapLocation)
        {
            List<Lock> locksAtLocation;
            locks.TryGetValue(new Location(level, mapLocation), out locksAtLocation);
            if (locksAtLocation == null)
                return new List<Lock>();
            return locksAtLocation;

        }

        internal bool NonOpenLocksAtLocation(int level, Point mapLocation)
        {
            return LocksAtLocation(level, mapLocation).Select(l => !l.IsOpen()).Any();
        }

        /// <summary>
        /// Are there multiple items here?
        /// </summary>
        /// <param name="locationLevel"></param>
        /// <param name="locationMap"></param>
        /// <returns></returns>
        internal bool MultipleItemAtSpace(int locationLevel, Point locationMap)
        {
            int itemCount = 0;

            foreach (Item item in items)
            {
                if (item.IsLocatedAt(locationLevel, locationMap) &&
                    !item.InInventory)
                {
                    itemCount++;
                }
            }

            if (itemCount < 2)
                return false;
            return true;
        }

        public Creature CreatureAtSpaceIncludingPlayer(int locationLevel, Point locationMap)
        {
            var monsterAtSpace = MonsterAtSpace(locationLevel, locationMap);
            bool playerAtSpace = (Player.LocationLevel == locationLevel) && (Player.LocationMap == locationMap);

            if (playerAtSpace)
                return Player;
            return monsterAtSpace;
        }

        /// <summary>
        /// Return an creature if there is one at the requested square, or return null if not
        /// </summary>
        public Monster MonsterAtSpace(int locationLevel, Point locationMap)
        {
            List<Monster> monsters = Monsters;

            foreach (Monster monster in monsters)
            {
                if (monster.LocationLevel == locationLevel && monster.LocationMap == locationMap)
                {
                    return monster;
                }
            }

            return null;
        }

        /// <summary>
        /// Return a random walkable point in map level
        /// </summary>
        /// <param name="levelNo"></param>
        /// <returns></returns>
        public Point RandomWalkablePointInLevel(int level)
        {
            //Not a level
            if (level < 0 || level > levels.Count)
            {
                string error = "Level " + level + "does not exist";
                LogFile.Log.LogEntry(error);
                throw new ApplicationException(error);
            }

            do
            {
                Map map = levels[level];

                int x = Game.Random.Next(map.width);
                int y = Game.Random.Next(map.height);

                if (Dungeon.IsTerrainWalkable(map.mapSquares[x, y].Terrain))
                {
                    return new Point(x, y);
                }
            }
            while (true);
        }

        /// <summary>
        /// Master is terrain walkable from MapTerrain type (not universally used yet) - defaults false
        /// </summary>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static bool IsTerrainWalkable(MapTerrain terrain)
        {
            if (terrain == MapTerrain.Empty ||
                terrain == MapTerrain.Flooded ||
                terrain == MapTerrain.OpenDoor ||
                terrain == MapTerrain.Corridor ||
                terrain == MapTerrain.Grass ||
                terrain == MapTerrain.Road ||
                terrain == MapTerrain.Gravestone ||
                terrain == MapTerrain.Trees ||
                terrain == MapTerrain.Rubble ||
                terrain == MapTerrain.OpenLock)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Master is terrain light blocking from MapTerrain type (not universally used yet) - defaults true
        /// </summary>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static bool IsTerrainLightBlocking(MapTerrain terrain)
        {
            if (terrain == MapTerrain.Empty ||
                terrain == MapTerrain.Flooded ||
                terrain == MapTerrain.OpenDoor ||
                terrain == MapTerrain.Corridor ||
                terrain == MapTerrain.Grass ||
                terrain == MapTerrain.Road ||
                terrain == MapTerrain.Gravestone ||
                terrain == MapTerrain.Trees ||
                terrain == MapTerrain.Rubble ||
                terrain == MapTerrain.OpenLock ||
                terrain == MapTerrain.NonWalkableFeature)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Removes an item from the game entirely.
        /// </summary>
        /// <param name="itemToUse"></param>
        public void RemoveItem(Item itemToUse)
        {
            items.Remove(itemToUse);
        }

        /// <summary>
        /// Hides an item so it can't be interacted with
        /// </summary>
        /// <param name="itemToUse"></param>
        public void HideItem(Item itemToUse)
        {
            itemToUse.InInventory = true;
        }

        /// <summary>
        /// Open the door at the requested location. Returns true if the door was successfully opened
        /// </summary>
        /// <param name="p"></param>
        /// <param name="doorLocation"></param>
        /// <returns></returns>
        internal bool OpenDoor(int level, Point doorLocation)
        {
            try
            {
                //Check there is a door here                
                MapTerrain doorTerrain = GetTerrainAtPoint(player.LocationLevel, doorLocation);

                if (doorTerrain != MapTerrain.ClosedDoor)
                {
                    return false;
                }

                SetTerrainAtPoint(level, doorLocation, MapTerrain.OpenDoor);

                return true;
            }
            catch (ApplicationException)
            {
                //Not a valid location - should not occur
                LogFile.Log.LogEntry("Non-valid location for door requested");
                return false;
            }
        }

        /// <summary>
        /// Set a point to a new type of terrain and update pathing and fov
        /// </summary>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <param name="newTerrain"></param>
        private void SetTerrainAtPoint(int level, Point location, MapTerrain newTerrain)
        {
            //Update map

            levels[level].mapSquares[location.x, location.y].Terrain = newTerrain;

            levels[level].mapSquares[location.x, location.y].Walkable = IsTerrainWalkable(newTerrain) ? true : false;
            levels[level].mapSquares[location.x, location.y].BlocksLight = IsTerrainLightBlocking(newTerrain) ? true : false;

            //Update pathing and fov
            fov.updateFovMap(level, location, IsTerrainLightBlocking(newTerrain) ? FOVTerrain.Blocking : FOVTerrain.NonBlocking);

            PathingTerrain pathingTerrain;
            if (newTerrain == MapTerrain.ClosedDoor)
                pathingTerrain = PathingTerrain.ClosedDoor;
            else if (newTerrain == MapTerrain.ClosedLock)
                pathingTerrain = PathingTerrain.ClosedLock;
            else
                pathingTerrain = IsTerrainWalkable(newTerrain) ? PathingTerrain.Walkable : PathingTerrain.Unwalkable;
            Pathing.PathFindingInternal.updateMap(level, location, pathingTerrain);
        }

        /// <summary>
        /// Close the door at the requested location. Returns true if the door was successfully opened
        /// </summary>
        /// <param name="p"></param>
        /// <param name="doorLocation"></param>
        /// <returns></returns>
        internal bool CloseDoor(int level, Point doorLocation)
        {
            try
            {
                //Check there is a door here                
                MapTerrain doorTerrain = GetTerrainAtPoint(player.LocationLevel, doorLocation);

                if (doorTerrain != MapTerrain.OpenDoor)
                {
                    return false;
                }

                SetTerrainAtPoint(level, doorLocation, MapTerrain.ClosedDoor);

                return true;
            }
            catch (ApplicationException)
            {
                //Not a valid location - should not occur
                LogFile.Log.LogEntry("Non-valid location for door requested");
                return false;
            }
        }



        /// <summary>
        /// Equivalent of PCMove for an action that doesn't have a move.
        /// Tell the special moves that this was a non-move action
        /// Theoretically I should also check to see if any of them fire, but I can't imagine why
        /// </summary>
        internal void PCActionNoMove()
        {
            ResetPCTurnCountersOnActionStatonary();

            //Check special moves.

            foreach (SpecialMove move in specialMoves)
            {
                if (move.Known)
                    move.CheckAction(false, new Point(0, 0), false);
            }

            //Are any moves ready, if so carry the first one out. All other are deleted (otherwise move interactions have to be worried about)

            SpecialMove moveToDo = null;

            foreach (SpecialMove move in specialMoves)
            {
                if (move.Known && move.MoveComplete())
                {
                    moveToDo = move;
                    break;
                }
            }

            //Carry out move, if one is ready
            if (moveToDo != null)
            {
                moveToDo.DoMove(new Point(-1, -1), false);

                //Clear all moves
                foreach (SpecialMove move in specialMoves)
                {
                    move.ClearMove();
                }
            }
        }

        public void ResetPCTurnCountersOnActionStatonary()
        {
            player.ResetTurnsInactive();
            player.ResetTurnsMoving();
            player.ResetTurnsSinceAction();
        }

        public String PlayerDeathString { get; set; }
        public bool PlayerDeathOccured { get; set; }

        /// <summary>
        /// Can't kill the player immediately now have to wait until end of monster loop
        /// </summary>
        /// <param name="deathString"></param>
        internal void SetPlayerDeath(string deathString)
        {
            PlayerDeathOccured = true;
            PlayerDeathString = deathString;
        }

        /// <summary>
        /// It's all gone wrong!
        /// </summary>
        internal void PlayerDeath(string verb)
        {
            if (PlayerImmortal && !verb.Contains("quit"))
            {
                Game.Dungeon.Player.HealCompletely();
                PlayerDeathOccured = false;
                return;
            }

            //In FlatlineRL death is not permanent, but quitting is!

            if (!verb.Contains("quit"))
            {

                //Reset vars
                PlayerDeathString = "";
                PlayerDeathOccured = false;

                LogFile.Log.LogEntryDebug("Player killed", LogDebugLevel.Medium);

                DungeonInfo.NoDeaths++;
                EndOfGame(false, false);

                //For now, we just healup the player
                //Game.Dungeon.Player.HealCompletely();
                //return;
                /*
                if (DungeonInfo.NoDeaths == DungeonInfo.MaxDeaths)
                {
                    //This is true death
                    EndOfGame(false);
                    return;
                }
                else
                {

                    //We get another try
                    
                    //Is level complete? Move onto next
                    if (DungeonInfo.Dungeons[player.LocationLevel].LevelObjectiveComplete)
                    {
                        if(Game.Dungeon.Player.PlayItemMovies && !PlayedMissionFailedDeathButCompleted) {
                            //Screen.Instance.PlayMovie("deadbutnextmission", true);
                            PlayedMissionFailedDeathButCompleted = true;
                        }
                        MoveToNextMission();
                        return;
                    }

                    //If not, reset mission with different seed
                    if (Game.Dungeon.Player.PlayItemMovies && !PlayedMissionFailedDeath)
                    {
                        //Screen.Instance.PlayMovie("deadretrymission", true);
                        PlayedMissionFailedDeath = true;
                    }
                    ResetCurrentMission(true);

                    return;
                }*/

            }
            else
            {
                EndOfGame(false, true);
            }


            /*
            //Right now, only seen on a quit (will be changed too)

            int noDeaths = this.DungeonInfo.NoDeaths;
            int noAborts = this.DungeonInfo.TotalAborts;

            int totalLevels = 1;

            //How many levels completed?
            int secondaryObjectives = 0;
            int primaryObjectives = 0;

            foreach (DungeonProfile profile in this.DungeonInfo.Dungeons)
            {
                if (profile.LevelObjectiveComplete)
                    primaryObjectives++;

                if (profile.LevelObjectiveKillAllMonstersComplete)
                    secondaryObjectives++;
            }

            //testable
            bool wonGame = this.DungeonInfo.Dungeons[totalLevels - 1].LevelObjectiveComplete;

            int primaryObjectiveScore = primaryObjectives * 100;
            int secondaryObjectiveScore = secondaryObjectives * 100;
            int killScore = (GetKillRecord().killScore);
            List<string> finalScreen = new List<string>();

            finalScreen.Add("Private " + Game.Dungeon.player.Name + " turned tail and ran from Space Hulk OE1x1!");
            finalScreen.Add("Woe betide him when the sergeant catches up!");

            finalScreen.Add("");

            finalScreen.Add("Primary objectives " + primaryObjectives + "/" + totalLevels + ": " + primaryObjectiveScore + " pts");
            finalScreen.Add("Secondary objectives " + secondaryObjectives + "/" + totalLevels + ": " + secondaryObjectiveScore + " pts");

            //Total kills
            KillRecord killRecord = GetKillRecord();

            finalScreen.Add("");

            finalScreen.Add("Robots destroyed " + killRecord.killCount + ": " + killScore + " pts");
            finalScreen.Add("");

            finalScreen.Add("Total: " + (primaryObjectiveScore + secondaryObjectiveScore + killScore).ToString("0000") + " pts");

            finalScreen.Add("");

            finalScreen.Add("Aborted Missions: " + noAborts);
            finalScreen.Add("");

            finalScreen.Add("R. E. E. D.s lost: " + noDeaths);

            finalScreen.Add("");

            finalScreen.Add("Thanks for playing! -flend");

            Screen.Instance.DrawEndOfGameInfo(finalScreen);

            //Compose the obituary

            List<string> obString = new List<string>();

            obString.AddRange(finalScreen);
            obString.Add("");
            obString.Add("Robots destroyed: " + killRecord.killCount);
            obString.Add("");
            obString.Add("Creatures defeated:");
            obString.Add("");

            SaveObituary(obString, killRecord.killStrings);

            if (!Game.Dungeon.SaveScumming)
            {
                DeleteSaveFile();
            }

            //Wait for a keypress
            //KeyPress userKey = Keyboard.WaitForKeyPress(true);

            //Stop the main loop
            RunMainLoop = false;
            */

        }

        public struct KillRecord
        {
            public int killCount;
            public List<string> killStrings;
            public int killScore;
        }
        /// <summary>
        /// Generate the grouped kill record for the player
        /// </summary>
        /// <returns></returns>
        public KillRecord GetKillRecord()
        {
            //fake
            /*
            KillRecord fakeKillRec = new KillRecord();
            fakeKillRec.killStrings = new List<string>();
            fakeKillRec.killCount = Game.Dungeon.player.KillCount;

            return fakeKillRec;*/

            //Make killCount list

            List<Monster> kills = player.Kills;
            List<KillCount> killCount = new List<KillCount>();

            int totalKills = 0;
            int totalScore = 0;

            foreach (Monster kill in kills)
            {
                totalKills++;
                totalScore += kill.CreatureCost();

                Type monsterType = kill.GetType();
                bool foundGroup = false;

                foreach (KillCount record in killCount)
                {
                    if (record.type.GetType() == monsterType)
                    {
                        record.count++;
                        foundGroup = true;
                        break;
                    }
                }

                //If there is no group, create a new one
                if (!foundGroup)
                {
                    KillCount newGroup = new KillCount();
                    newGroup.type = kill;
                    newGroup.count = 1;
                    killCount.Add(newGroup);
                }
            }

            List<string> killRecord = new List<string>();

            //Turn list into strings to be displayed
            foreach (KillCount record in killCount)
            {

                string killStr = "";

                if (record.count == 1)
                {
                    killStr += "1 " + record.type.SingleDescription;
                }
                else
                {
                    killStr += record.count.ToString() + " " + record.type.GroupDescription;
                }

                //Add to string list
                killRecord.Add(killStr);
            }

            KillRecord recordS = new KillRecord();
            recordS.killStrings = killRecord;
            recordS.killCount = totalKills;
            recordS.killScore = totalScore;
            return recordS;
        }

        /// <summary>
        /// The player learns a new move. Right now doesn't use the parameter (except as a reference) and just updates the Known parameter
        /// </summary>
        internal void LearnMove(SpecialMove moveToLearn)
        {
            LogFile.Log.LogEntryDebug("Player learnt move: " + moveToLearn.MoveName(), LogDebugLevel.Medium);

            foreach (SpecialMove move in specialMoves)
            {
                if (move.GetType() == moveToLearn.GetType())
                {
                    move.Known = true;
                }
            }
        }

        /// <summary>
        /// Returns points in grenade template. Includes walls and everything.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public List<Point> GetPointsForGrenadeTemplate(Point location, int level, double size)
        {
            List<Point> splashSquares = new List<Point>();

            int sizeI = (int)Math.Ceiling(size) + 1;

            for (int i = location.x - sizeI; i < location.x + sizeI; i++)
            {
                for (int j = location.y - sizeI; j < location.y + sizeI; j++)
                {
                    if (i >= 0 && i < levels[level].width && j >= 0 && j < levels[level].height)
                    {
                        if (Math.Pow(i - location.x, 2) + Math.Pow(j - location.y, 2) < Math.Pow(size, 2) + 0.1)
                        {
                            splashSquares.Add(new Point(i, j));
                        }
                    }
                }
            }

            return splashSquares;
        }



        /// <summary>
        /// The player learns a new spell. Right now doesn't use the parameter (except as a reference) and just updates the Known parameter
        /// </summary>
        internal void LearnSpell(Spell moveToLearn)
        {
            foreach (Spell spell in spells)
            {
                if (spell.GetType() == moveToLearn.GetType())
                {
                    spell.Known = true;
                }
            }
        }

        public void RunDungeonTriggers(int level, Point mapLocation)
        {
            List<DungeonSquareTrigger> triggersSnapshot = new List<DungeonSquareTrigger>();

            //Make a copy in case the trigger adds more triggers to the global collection

            foreach (DungeonSquareTrigger trigger in Triggers)
            {
                triggersSnapshot.Add(trigger);
            }

            foreach (DungeonSquareTrigger trigger in triggersSnapshot)
            {
                trigger.CheckTrigger(level, mapLocation);
            }


        }

        internal void AddTrigger(int level, Point point, DungeonSquareTrigger trigger)
        {
            //Set the trigger position
            trigger.Level = level;
            trigger.mapPosition = point;

            Triggers.Add(trigger);
        }



        /// <summary>
        /// Delete save file on player death
        /// </summary>
        private void DeleteSaveFile()
        {
            try
            {
                string filename = player.Name + ".sav";

                File.Delete(filename);
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Couldn't delete save file: " + ex.Message);
            }
        }

        private void SaveObituary(List<string> deathPreamble, List<string> killRecord)
        {
            try
            {

                //Date stamp
                DateTime dateTime = DateTime.Now;
                string timeStamp = dateTime.Year.ToString("0000") + "-" + dateTime.Month.ToString("00") + "-" + dateTime.Day.ToString("00") + "_" + dateTime.Hour.ToString("00") + "-" + dateTime.Minute.ToString("00") + "-" + dateTime.Second.ToString("00");


                Directory.CreateDirectory("obituary");
                string obFilename = "obituary/" + Game.Dungeon.player.Name + " epilogue " + timeStamp + ".txt";

                StreamWriter obFile = new StreamWriter(obFilename);

                foreach (string s in deathPreamble)
                {
                    obFile.WriteLine(s);
                }

                foreach (string s in killRecord)
                {
                    obFile.WriteLine(s);
                }
                obFile.Close();
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Couldn't write obituary file " + ex.Message);
            }

        }

        /// <summary>
        /// Add monsters from the summoning queue to the actual dungeon. Clear at the end. Some monsters may not add if things have moved
        /// </summary>
        internal void AddDynamicMonsters()
        {
            foreach (Monster monster in summonedMonsters)
            {
                Game.Dungeon.AddMonster(monster, monster.LocationLevel, monster.LocationMap);
            }

            summonedMonsters.Clear();
        }


        /// <summary>
        /// Attempt to charm a monster in a target direction.
        /// Returns whether time passes (not if there is a successful charm)
        /// </summary>
        /// <param name="direction"></param>
        internal bool AttemptCharmMonsterByPlayer(Point direction)
        {
            //Work out the monster's location

            Point targetLocation = new Point(Game.Dungeon.Player.LocationMap.x + direction.x, Game.Dungeon.Player.LocationMap.y + direction.y);

            Player player = Game.Dungeon.Player;

            //Is there a monster here?

            if (!Game.Dungeon.MapSquareIsWalkable(player.LocationLevel, targetLocation))
            {
                //No monster
                Game.MessageQueue.AddMessage("No target.");
                return false;
            }
            else
            {
                //Check for monsters in the square
                SquareContents contents = MapSquareContents(player.LocationLevel, targetLocation);

                //Monster - try to charm it
                if (contents.monster != null)
                {
                    Monster monster = contents.monster;

                    //Is the creature already charmed?
                    if (monster.Charmed)
                    {
                        Game.MessageQueue.AddMessage("The creature is already charmed.");
                        return false;
                    }

                    //Check if this class of creature can be charmed or passified
                    if (!monster.CanBeCharmed() || monster.GetCharmRes() >= player.CharmPoints)// && !monster.CanBePassified())
                    {
                        Game.MessageQueue.AddMessage("The " + monster.SingleDescription + " laughs at your feeble attempt.");
                        return true;
                    }

                    //   bool canCharm = true;
                    /*
                    if (!monster.CanBeCharmed())
                    {
                        //On for passify only
                        canCharm = false;
                    }*/

                    //Try to charm, may fail if the player has no more charms


                    bool playerOK = false;
                    //  if (canCharm)
                    //  {
                    //Check if the player has any more charms
                    playerOK = player.MoreCharmedCreaturesPossible();
                    //}

                    if (!playerOK)
                    {
                        //canCharm = false;
                        Game.MessageQueue.AddMessage("Too many charmed creatures.");
                        return true;
                        //return true;
                    }


                    //Test against statistic here
                    int monsterRes = monster.GetCharmRes();
                    int charmRoll = Game.Random.Next(player.CharmPoints);

                    LogFile.Log.LogEntryDebug("Charm attempt. Res: " + monsterRes + " Roll: " + charmRoll, LogDebugLevel.Medium);

                    if (charmRoll < monsterRes)
                    {
                        //Charm not successful
                        string msg;

                        int chance = Game.Random.Next(100);

                        if (chance < 50)
                            msg = "The " + monster.SingleDescription + " does not look convinced by your overtures.";
                        else
                            msg = "The " + monster.SingleDescription + " ignores you and attacks.";

                        Game.MessageQueue.AddMessage(msg);
                        return true;
                    }


                    //All OK do the charm
                    //   if (canCharm)
                    //    {
                    //Should be possible at this point
                    player.AddCharmCreatureIfPossible();

                    int chance2 = Game.Random.Next(100);

                    string msg2;
                    if (chance2 < 30)
                        msg2 = "The " + monster.SingleDescription + " looks at you lovingly.";
                    else if (chance2 < 65)
                        msg2 = "The " + monster.SingleDescription + " can't resist your charms.";
                    else
                        msg2 = "The " + monster.SingleDescription + " is all over you.";
                    Game.MessageQueue.AddMessage(msg2);
                    contents.monster.CharmCreature();

                    //Add XP
                    double diffDelta = (player.CharmStat - monsterRes) / (double)player.CharmStat;
                    if (diffDelta < 0)
                        diffDelta = 0;

                    double xpUpChance = 1 - diffDelta;
                    int xpUpRoll = (int)Math.Floor(xpUpChance * 100.0);
                    int xpUpRollActual = Game.Random.Next(100);
                    LogFile.Log.LogEntryDebug("CharmXP up. Chance: " + xpUpRoll + " roll: " + xpUpRollActual, LogDebugLevel.Medium);

                    if (xpUpRollActual < xpUpRoll)
                    {
                        player.CharmXP++;
                        Game.MessageQueue.AddMessage("You feel more charming.");
                        LogFile.Log.LogEntryDebug("CharmXP up!", LogDebugLevel.Medium);
                    }

                    //Set intrinsic on the player
                    player.CharmUse = true;

                    return true;
                    //    }

                    /*
                //Only a passify
                else
                {
                    //Test against statistic here
                        
                    string msg = "The " + monster.SingleDescription + " sighs and turns away.";

                    Game.MessageQueue.AddMessage(msg);
                    contents.monster.PassifyCreature();

                    return true;
                }*/

                }
                else
                {
                    //No monster
                    Game.MessageQueue.AddMessage("No target.");
                    return false;
                }
            }
            //return false;
        }

        /// <summary>
        /// Remove all active effects on monsters. Used when we leave a dungeon
        /// to ensure events are cancelled before we meet the monster again
        /// </summary>
        void RemoveAllMonsterEffects()
        {
            //Increment time on events and remove finished ones
            List<PlayerEffect> finishedEffects = new List<PlayerEffect>();

            foreach (Monster monster in monsters)
            {
                monster.RemoveAllEffects();
            }
        }

        /// <summary>
        /// Attempt to uncharm a monster in a target direction.
        /// Returns whether time passes (not if there is a successful charm)
        /// </summary>
        /// <param name="direction"></param>
        internal bool UnCharmMonsterByPlayer(Point direction)
        {
            //Work out the monster's location

            Point targetLocation = new Point(Game.Dungeon.Player.LocationMap.x + direction.x, Game.Dungeon.Player.LocationMap.y + direction.y);

            Player player = Game.Dungeon.Player;

            //Is there a monster here?

            if (!Game.Dungeon.MapSquareIsWalkable(player.LocationLevel, targetLocation))
            {
                //No monster
                Game.MessageQueue.AddMessage("No target.");
                return false;
            }
            else
            {
                //Check for monsters in the square
                SquareContents contents = MapSquareContents(player.LocationLevel, targetLocation);

                //Monster - is it already charmed
                if (contents.monster != null)
                {
                    Monster monster = contents.monster;

                    //Is the creature already charmed?
                    if (monster.Charmed)
                    {
                        Game.MessageQueue.AddMessage("The creature looks wistful and then goes about its business.");
                        monster.UncharmCreature();
                        monster.PassifyCreature();

                        player.RemoveCharmedCreature();

                        return true;
                    }
                    else
                    {
                        //Not charmed

                        Game.MessageQueue.AddMessage("The creature is not charmed.");
                        return false;
                    }
                }
                else
                {
                    //No monster
                    Game.MessageQueue.AddMessage("No target.");
                    return false;
                }
            }
        }

        /// <summary>
        /// Respawn the current dungeon. New seed on abort. Same seed on death
        /// </summary>
        /// <param name="respawnWithSameSeed"></param>
        public void ResetCurrentMission(bool respawnWithSameSeed)
        {

            //Leave seed for another time

            //Reset starting location

            player.LocationMap = Game.Dungeon.Levels[player.LocationLevel].PCStartLocation;

            //Always do new seed for now
            //Respawn the last dungeon the player was in
            RespawnDungeon(Player.LocationLevel, respawnWithSameSeed);

            //Reset starting location (in case the level changed)

            player.LocationMap = Game.Dungeon.Levels[player.LocationLevel].PCStartLocation;


            //Reset dungeon level state
            Game.Dungeon.DungeonInfo.Dungeons[Player.LocationLevel].PlayerLeftDock = false;
            Game.Dungeon.DungeonInfo.Dungeons[Player.LocationLevel].LevelObjectiveComplete = false;

            //End any events on any remaining monsters
            RemoveAllMonsterEffects();

            PlayerActionsBetweenMissions();
            DungeonActionsBetweenMissions();

            string fmt = "00";
            Game.MessageQueue.AddMessage("Re-entering ZONE " + (Player.LocationLevel + 1).ToString(fmt) + " : " + Game.Dungeon.DungeonInfo.LookupMissionName(Player.LocationLevel) + ".");

            //Run a normal turn to set off any triggers
            Game.Dungeon.PCMove(0, 0);

        }

        /// <summary>
        /// Exit a dungeon and go back to town
        /// Do all cleanup here
        /// </summary>

        public void PlayerLeavesDungeon()
        {

            //TODO

            //Check if this is the end of the game
            if (!DungeonInfo.LastMission)
            {
                //Respawn the last dungeon the player was in
                RespawnDungeon(Player.CurrentDungeon, false);

                //End any events on any remaining monsters
                RemoveAllMonsterEffects();

                //Wipe the player's FOV of the last dungeon
                WipeThisRunFOV(Player.CurrentDungeon);

                //Cancel any effect
                player.RemoveAllEffects();

                //Recharge all items
                RechargeEquippableItems();

                //Put found items that were too much for inventory in store
                //PutItemsNotInInventoryInStore();

                //Remove all inventory items
                RemoveInventoryItems();

                LogFile.Log.LogEntryDebug("Player back to town. Date moved on.", LogDebugLevel.Medium);
                //Game.Dungeon.MoveToNextDate();
                //Game.Dungeon.PlayerBackToTown();
                //SyncStatsWithTraining();

                Player.CurrentDungeon = 0;
            }
            else
            {
                //OK, it's the end, they're back from the prince mission one way or the other
                EndOfGame(false, false);
            }
        }

        /// <summary>
        /// Wipe the this-adventure fov for the dungeon
        /// </summary>
        /// <param name="dungeonID"></param>
        private void WipeThisRunFOV(int dungeonID)
        {
            //Kill all the creatures currently in there, except for the uniques
            int dungeonStartLevel = Game.Dungeon.DungeonInfo.GetDungeonStartLevel(dungeonID);
            int dungeonEndLevel = Game.Dungeon.DungeonInfo.GetDungeonEndLevel(dungeonID);

            for (int i = dungeonStartLevel; i < dungeonEndLevel; i++)
            {
                int width = levels[i].width;
                int height = levels[i].height;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        levels[i].mapSquares[x, y].SeenByPlayerThisRun = false;
                    }
                }
            }
        }

        /// <summary>
        /// Because we can't serialize statics, this function will check all the instances of a particular trigger type
        /// and return true if any one of them has been triggered
        /// </summary>
        /// <returns></returns>
        public bool CheckGlobalTrigger(Type triggerType)
        {
            List<DungeonSquareTrigger> triggersOfType = Triggers.FindAll(t => t.GetType() == triggerType);
            if (triggersOfType.Exists(t => t.IsTriggered()))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Remove all temporary items
        /// </summary>
        private void RemoveInventoryItems()
        {
            Inventory inv = player.Inventory;

            List<Item> itemsToRemove = new List<Item>();

            foreach (Item item in inv.Items)
            {
                IEquippableItem itemE = item as IEquippableItem;

                if (itemE == null)
                {
                    itemsToRemove.Add(item);
                }

            }

            LogFile.Log.LogEntryDebug("Removing " + itemsToRemove.Count + " items from inventory.", LogDebugLevel.Medium);
            if (itemsToRemove.Count > 0)
            {
                Game.MessageQueue.AddMessage("The berries you found fade away.");
            }


            foreach (Item item in itemsToRemove)
            {
                inv.RemoveItem(item);
            }
        }

        /// <summary>
        /// Recharge all items in the game
        /// </summary>
        private void RechargeEquippableItems()
        {
            Inventory inv = player.Inventory;

            foreach (Item item in inv.Items)
            {
                IUseableItem itemU = item as IUseableItem;
                if (itemU != null)
                    itemU.UsedUp = false;
            }
        }

        /// <summary>
        /// Run the end of game. Produce and save the obituary.
        /// </summary>
        public void EndOfGame(bool playerWon, bool playerQuit)
        {
            //Work out which ending the player gets

            if (playerWon)
                Screen.Instance.PlayMovie("traumawin", true);
            else
            {
                if (!playerQuit)
                    Screen.Instance.PlayMovie("traumalose", true);
                else
                    Screen.Instance.PlayMovie("traumaquit", true);
            }

            //RunMainLoop = false;
            EndOfGameMechanics(playerWon, playerQuit);
        }

        private void EndOfGameMechanics(bool wonGame, bool quit)
        {
            //Check intrinsics

            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;
            
            //The last screen to display
            List<string> finalScreen = new List<string>();
            
            //A long list of stuff to put in the obituary
            List<string> fullObit = new List<string>();

            //Final stats

            List<string> finalStats = new List<string>();

            if (wonGame)
            {
                finalScreen.Add("Private " + Game.Dungeon.player.Name + " finished what he started and defeated the invasion ");
                finalScreen.Add("of the machines from Space Hulk OE1x1!");
            }
            else
            {
                if (quit)
                    finalScreen.Add("Private " + Game.Dungeon.player.Name + " had pressing business elsewhere.");
                else
                    finalScreen.Add("Private " + Game.Dungeon.player.Name + " fought bravely but was finally overcome.");
            }
            finalScreen.Add("");

            //Total kills
            KillRecord killRecord = GetKillRecord();

            finalScreen.Add("");

            finalScreen.Add("Robots destroyed: " + killRecord.killCount + " (" + killRecord.killScore + " pts)");
            finalScreen.Add("");

            //finalScreen.Add("Total: " + (primaryObjectiveScore + secondaryObjectiveScore + killScore).ToString("0000") +" pts");

            var itemInfo = ItemsUsedSummary();

            finalScreen.AddRange(itemInfo.Item2);

            finalScreen.Add("");

            //finalScreen.Add("Aborted Missions: " + noAborts);
            finalScreen.Add("");

            //finalScreen.Add("R. E. E. D.s lost: " + noDeaths);

            finalScreen.Add("");
            finalScreen.Add("Thanks for playing! -flend & shroomarts");

            Screen.Instance.DrawEndOfGameInfo(finalScreen);

            //Compose the obituary

            List<string> obString = new List<string>();

            obString.AddRange(finalScreen);
            obString.Add("");
            obString.Add("Robots destroyed:");
            obString.Add("");

            SaveObituary(obString, killRecord.killStrings);

            if (!Game.Dungeon.SaveScumming)
            {
                DeleteSaveFile();
            }

            //Wait for a keypress
            //KeyPress userKey = Keyboard.WaitForKeyPress(true);

            //Stop the main loop
            RunMainLoop = false;
        }

        private Tuple<int, List<string>> ItemsUsedSummary()
        {
            var shieldsUsed = Player.Inventory.GetItemsOfType<Items.ShieldPack>().Count();
            var ammoUsed = Player.Inventory.GetItemsOfType<Items.AmmoPack>().Count();

            var itemText = new List<string>();
            itemText.Add(shieldsUsed + " " + new Items.ShieldPack().GroupItemDescription + " used.");

            itemText.Add(ammoUsed + " " + new Items.AmmoPack().GroupItemDescription + " used.");

            return new Tuple<int, List<string>>(0, itemText);
        }


        /// <summary>
        /// Total number of dungeons where we killed both uniques
        /// </summary>
        /// <returns></returns>
        private int GetTotalDungeonsCleared()
        {
            List<DungeonProfile> Cleared = DungeonInfo.Dungeons.FindAll(x => x.masterUniqueDefeated && x.subUniqueDefeated);

            return Cleared.Count;
        }

        /// <summary>
        /// Total number of dungeons known
        /// </summary>
        /// <returns></returns>
        private int GetTotalDungeonsExplored()
        {
            List<DungeonProfile> Cleared = DungeonInfo.Dungeons.FindAll(x => x.visited);

            return Cleared.Count;
        }

        public void MissionComplete()
        {
            if (DungeonInfo.Dungeons[player.LocationLevel].LevelObjectiveKillAllMonstersComplete)
            {
                //With secondary
                if (Game.Dungeon.Player.PlayItemMovies && !PlayedMissionCompleteWithSecondary)
                {
                    //Screen.Instance.PlayMovie("missioncompletewithsecondary", true);
                    PlayedMissionCompleteWithSecondary = true;
                }
                Game.MessageQueue.AddMessage("Mission COMPLETE (primary + secondary objectives)!");
            }
            else
            {
                //Primary only
                if (Game.Dungeon.Player.PlayItemMovies && !PlayedMissionComplete)
                {
                    //Screen.Instance.PlayMovie("missioncomplete", true);
                    PlayedMissionComplete = true;
                }
                Game.MessageQueue.AddMessage("Mission COMPLETE (primary objectives)!");
            }
            MoveToNextMission();
        }

        public bool PlayedMissionAborted { get; set; }
        public bool PlayedMissionNoMoreAborts { get; set; }
        public bool PlayedMissionDeath { get; set; }
        public bool PlayedMissionComplete { get; set; }
        public bool PlayedMissionCompleteWithSecondary { get; set; }
        public bool PlayedMissionFailedDeath { get; set; }
        public bool PlayedMissionFailedDeathButCompleted { get; set; }


        public bool MissionAborted()
        {
            if (DungeonInfo.NoAborts == DungeonInfo.MaxAborts)
            {
                //No more aborts allowed
                if (Game.Dungeon.Player.PlayItemMovies && !PlayedMissionNoMoreAborts)
                {
                    //Screen.Instance.PlayMovie("nomoreaborts", true);
                    PlayedMissionNoMoreAborts = true;
                }
                Game.MessageQueue.AddMessage("No more aborts permitted.");
                return false;

            }

            //Otherwise OK

            DungeonInfo.NoAborts++;
            DungeonInfo.TotalAborts++;

            if (Game.Dungeon.Player.PlayItemMovies && !PlayedMissionAborted)
            {
                // Screen.Instance.PlayMovie("missionaborted", true);
                PlayedMissionAborted = true;
            }
            Game.MessageQueue.AddMessage("Mission ABORTED.");
            ResetCurrentMission(false);

            return true;
        }

        /// <summary>
        /// Move player to next mission
        /// </summary>
        public void MoveToNextMission()
        {
            int newMissionLevel = Game.Dungeon.player.LocationLevel + 1;

            if (newMissionLevel == Levels.Count)
            {
                //Completed the game
                EndOfGame(false, false);
                return;
            }

            //Reset no of aborts
            DungeonInfo.NoAborts = 0;

            PlayerActionsBetweenMissions();
            DungeonActionsBetweenMissions();

            SelectTilesetForMission(newMissionLevel);

            //Specials
            if (newMissionLevel == 11)
            {
                //Bonus units
                DungeonInfo.MaxDeaths += 5;
            }

            //Move player to new level

            player.LocationLevel = newMissionLevel;
            player.LocationMap = Game.Dungeon.Levels[player.LocationLevel].PCStartLocation;

            string fmt = "00";
            Game.MessageQueue.AddMessage("Entering ZONE " + (newMissionLevel + 1).ToString(fmt) + " : " + Game.Dungeon.DungeonInfo.LookupMissionName(newMissionLevel) + ".");

            //Run a normal turn to set off any triggers
            Game.Dungeon.PCMove(0, 0, true);
        }

        /// <summary>
        /// Move player to next mission
        /// </summary>
        public void MoveToLevel(int levelNo)
        {
            if (levelNo < 0 || levelNo >= levels.Count)
                return;

            //Find any elevator that goes here
            var elevator = features.Where(f => f.GetType() == typeof(Features.Elevator) && (f as Features.Elevator).DestLevel == levelNo);
            if (elevator.Count() == 0)
            {
                LogFile.Log.LogEntryDebug("Failed to find elevator to get to on level " + levelNo, LogDebugLevel.Medium);
                return;
            }

            var preferredElevator = elevator.Where(f => f.GetType() == typeof(Features.Elevator) && (f as Features.Elevator).LocationLevel == player.LocationLevel);

            if (preferredElevator.Count() > 0)
            {
                var elevatorToUse = preferredElevator.ElementAt(0) as Features.Elevator;
                elevatorToUse.PlayerInteraction(player);
            }
            else
            {
                var elevatorToUse = elevator.ElementAt(0) as Features.Elevator;
                elevatorToUse.PlayerInteraction(player);
            }
        }

        /// <summary>
        /// Bit of a hack, override the tileset per level
        /// </summary>
        private void SelectTilesetForMission(int level)
        {
            //Tutorial levels

            if (level < 6)
            {
                StringEquivalent.OverrideTerrainChar(MapTerrain.Wall, '#');
            }

            else if (level < 11)
            {
                StringEquivalent.OverrideTerrainChar(MapTerrain.Wall, '\xb0');
            }

            else if (level == 14)
            {
                StringEquivalent.OverrideTerrainChar(MapTerrain.Wall, '\xdb');
            }

            else
            {
                StringEquivalent.OverrideTerrainChar(MapTerrain.Wall, '\xb2');
            }


        }

        public void PlayerActionsBetweenMissions()
        {

            //Heal the player
            player.ResetAfterDeath();

            //Remove all effects
            player.RemoveAllEffects();

            //Remove items
            player.UnequipAndDestoryAllItems();
        }

        private void ResetSounds()
        {
            effects.Clear();
            nextUniqueSoundID = 0;

        }

        public void DungeonActionsBetweenMissions()
        {
            //For going to a new level this is no big deal, but it is important if we are respawning
            ResetSounds();

        }

        /// <summary>
        /// Player starts the game
        /// </summary>
        public void MoveToFirstMission()
        {
            int newMissionLevel = 0;

            //Move player to new level

            player.LocationLevel = newMissionLevel;
            player.LocationMap = Game.Dungeon.Levels[player.LocationLevel].PCStartLocation;

            //Message

            string fmt = "00";
            Game.MessageQueue.AddMessage("Entering ZONE " + (newMissionLevel + 1).ToString(fmt) + " : " + Game.Dungeon.DungeonInfo.LookupMissionName(newMissionLevel) + ".");

            //Run a normal turn to set off any triggers
            Game.Dungeon.PCMove(0, 0, true);
        }

        /// <summary>
        /// Respawn a particular dungeon after the player leaves
        /// </summary>
        /// <param name="dungeonID"></param>
        internal void RespawnDungeon(int dungeonID, bool useOldSeed)
        {
            //A bit wasteful
            DungeonMaker.ReSpawnDungeon(dungeonID, useOldSeed);

            LogFile.Log.LogEntryDebug("Respawning dungeon level " + dungeonID, LogDebugLevel.Medium);
        }

        /// <summary>
        /// Check to see if any special moves which were previously on are now not due to the death or movement of a monster
        /// Not implemented yet
        /// </summary>
        internal void CheckSpecialMoveValidity()
        {
            return;
        }

        public List<Point> CalculateTrajectory(Point target)
        {
            return CalculateTrajectory(player, target);
        }

        public List<Point> CalculateTrajectory(Creature creature, Point target)
        {
            //Get the points along the line of where we are firing
            CreatureFOV currentFOV = CalculateCreatureFOV(creature);
            List<Point> trajPoints = currentFOV.GetPathLinePointsInFOV(creature.LocationMap, target);

            //Also exclude unwalkable points (since we will use this to determine where our item falls
            List<Point> walkableSq = new List<Point>();
            foreach (Point p in trajPoints)
            {
                if (Game.Dungeon.MapSquareIsWalkable(Game.Dungeon.Player.LocationLevel, p))
                    walkableSq.Add(p);
            }

            return walkableSq;
        }

        public Monster FirstMonsterInTrajectory(List<Point> squares)
        {
            //Hit the first monster only
            Monster monster = null;
            foreach (Point p in squares)
            {
                //Check there is a monster at target
                SquareContents squareContents = MapSquareContents(player.LocationLevel, p);

                //Hit the monster if it's there
                if (squareContents.monster != null)
                {
                    monster = squareContents.monster;
                    break;
                }
            }

            return monster;
        }

        public List<Point> GetWalkableAdjacentSquaresFreeOfCreatures(int locationLevel, Point locationMap)
        {
            Map levelMap = levels[locationLevel];

            List<Point> adjacentSqFree = new List<Point>();
            List<Point> adjacentSq = GetAdjacentSquares(locationMap);

            foreach (Point p in adjacentSq)
            {
                if (p.x >= 0 && p.x < levelMap.width
                    && p.y >= 0 && p.y < levelMap.height)
                {
                    if (!MapSquareIsWalkable(locationLevel, p))
                    {
                        continue;
                    }

                    //Check square has nothing else on it
                    SquareContents contents = MapSquareContents(locationLevel, p);

                    if (contents.monster != null)
                    {
                        continue;
                    }

                    if (contents.player != null)
                        continue;

                    //Empty and walkable
                    adjacentSqFree.Add(p);
                }
            }

            return adjacentSqFree;
        }

        public List<Point> GetWalkableAdjacentSquaresFreeOfCreaturesAndItems(int locationLevel, Point locationMap)
        {
            Map levelMap = levels[locationLevel];

            List<Point> adjacentSqFree = new List<Point>();
            List<Point> adjacentSq = GetAdjacentSquares(locationMap);

            foreach (Point p in adjacentSq)
            {
                if (p.x >= 0 && p.x < levelMap.width
                    && p.y >= 0 && p.y < levelMap.height)
                {
                    if (!MapSquareIsWalkable(locationLevel, p))
                    {
                        continue;
                    }

                    //Check square has nothing else on it
                    SquareContents contents = MapSquareContents(locationLevel, p);

                    if (contents.monster != null)
                    {
                        continue;
                    }

                    if (contents.player != null)
                        continue;

                    if (contents.items.Count > 0)
                        continue;

                    //Empty and walkable
                    adjacentSqFree.Add(p);
                }
            }

            return adjacentSqFree;
        }

        private static List<Point> GetAdjacentSquares(Point locationMap)
        {
            List<Point> adjacentSq = new List<Point>();

            adjacentSq.Add(new Point(locationMap.x + 1, locationMap.y - 1));
            adjacentSq.Add(new Point(locationMap.x + 1, locationMap.y));
            adjacentSq.Add(new Point(locationMap.x + 1, locationMap.y + 1));
            adjacentSq.Add(new Point(locationMap.x - 1, locationMap.y - 1));
            adjacentSq.Add(new Point(locationMap.x - 1, locationMap.y));
            adjacentSq.Add(new Point(locationMap.x - 1, locationMap.y + 1));
            adjacentSq.Add(new Point(locationMap.x, locationMap.y + 1));
            adjacentSq.Add(new Point(locationMap.x, locationMap.y - 1));
            return adjacentSq;
        }

        public IEnumerable<Point> GetWalkablePointsFromSet(int level, IEnumerable<Point> allPossiblePoints)
        {
            return allPossiblePoints.SelectMany(p => MapSquareIsWalkable(level, p) ? new List<Point> { p } : new List<Point>());
        }

        /// <summary>
        /// Source is the person firing
        /// </summary>
        /// <param name="sourceLevel"></param>
        /// <param name="source"></param>
        /// <param name="destLevel"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        internal Tuple<int, int> GetNumberOfCoverItemsBetweenPoints(int sourceLevel, Point source, int destLevel, Point dest)
        {
            if (sourceLevel != destLevel)
                return new Tuple<int, int>(0, 0);

            //Any intervening cover. Stuff we're sitting on doesn't count
            var pointsOnLine = Utility.GetPointsOnLine(source, dest).Except(new List<Point> { source, dest });

            var hardCover = 0;
            var softCover = 0;

            foreach (Point p in pointsOnLine)
            {
                var terrain = GetTerrainAtPoint(sourceLevel, p);

                if (!IsTerrainWalkable(terrain))
                    hardCover++;
            }

            //Check for any blocking features
            //Is checked by terrain above
            
            foreach (var f in features)
            {
                if (f.LocationLevel != sourceLevel)
                    continue;

                if (pointsOnLine.Contains(f.LocationMap))
                {
                    if (!f.IsBlocking)
                        softCover += 1;                        
                }
            }

            return new Tuple<int, int>(hardCover, softCover);
        }

        public bool AllLocksOpen { get; set; }

        public MapInfo MapInfo { get; set; }

        public MonsterPlacement MonsterPlacement { get; private set; }
    }
}
