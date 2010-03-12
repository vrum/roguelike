﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.IO.Compression;


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

        /// <summary>
        /// Set if no monster or player
        /// </summary>
        public bool empty = false;

        public bool offMap = false;

        public SquareContents()
        {

        }
    }

    /// <summary>
    /// Keeps or links to all the state in the game
    /// </summary>
    public class Dungeon
    {
        List<Map> levels;
        List<TCODFov> levelTCODMaps;
        //List<TCODFov> levelTCODMapsIgnoringClosedDoors; //used for adding monsters and items
        List<Monster> monsters;
        List<Item> items;
        List<Feature> features;
        public List<HiddenNameInfo> HiddenNameInfo {get; set;} //for serialization
        public List<DungeonSquareTrigger> Triggers { get; set; }

        List<SpecialMove> specialMoves;

        List<Spell> spells;

        Player player;

        public bool SaveScumming { get; set; }

        public GameDifficulty Difficulty { get; set; }

        public bool PlayerImmortal { get; set; }

        private List<Monster> summonedMonsters; //no need to serialize

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

        long worldClock = 0;

        /// <summary>
        /// Count the days in the year
        /// </summary>
        int dateCounter = 0;

        /// <summary>
        /// Set to false to end the game
        /// </summary>
        public bool RunMainLoop { get; set;}

        /// <summary>
        /// List of global events
        /// </summary>
        List<DungeonEffect> effects;

        Color defaultPCColor = ColorPresets.White;

        public Dungeon()
        {
            levels = new List<Map>();
            monsters = new List<Monster>();
            items = new List<Item>();
            features = new List<Feature>();
            levelTCODMaps = new List<TCODFov>();
            //levelTCODMapsIgnoringClosedDoors = new List<TCODFov>();
            effects = new List<DungeonEffect>();
            specialMoves = new List<SpecialMove>();
            spells = new List<Spell>();
            HiddenNameInfo = new List<HiddenNameInfo>();
            Triggers = new List<DungeonSquareTrigger>();

            PlayerImmortal = false;

            SetupSpecialMoves();

            SetupSpells();

            SetupHiddenNameMappings();

            player = new Player();

            RunMainLoop = true;

            summonedMonsters = new List<Monster>();

            SaveScumming = false;
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

        /// <summary>
        /// Return the calendar month, 1-12
        /// </summary>
        /// <returns></returns>
        public int GetDateMonth()
        {
            return (int)Math.Floor(dateCounter / 28.0) + 1;
        }

        /// <summary>
        /// Return the calendar day, 1-28
        /// </summary>
        /// <returns></returns>
        public int GetDateDay()
        {
            int day = dateCounter % 28;
            return day + 1;
        }

        /// <summary>
        /// Are we at the start of the working week
        /// </summary>
        /// <returns></returns>
        public bool IsWeekday()
        {
            return (dateCounter % 7 == 0);
        }

        /// <summary>
        /// Are we at the start of the working week
        /// </summary>
        /// <returns></returns>
        public bool IsNormalWeekend()
        {
            if (dateCounter % 7 != 5)
                return false;

            if (dateCounter % 28 == 26)
                return false;

            return true;
        }

        /// <summary>
        /// An adventure weekend
        /// </summary>
        /// <returns></returns>
        public bool IsAdventureWeekend()
        {
            if (dateCounter == 26)
                return true;

            return false;

        }

        /// <summary>
        /// Move to the next date event, be it weekend, or end of month adventure
        /// </summary>
        public void MoveToNextDate()
        {
            //Calendar

            //1-5 Weekday
            //6-7 Weekend
            //8-12 Weekday
            //13-14 Weekend
            //15-19 Weekday
            //20-21 Weekend
            //22-26 Weekday
            //27-28 Special Weekend

            if (dateCounter % 7 == 0)
            {
                dateCounter += 5;
                return;
            }

            if (dateCounter % 7 == 5)
            {
                dateCounter += 2;
                return;
            }

            //Shouldn't get here
            LogFile.Log.LogEntryDebug("Impossible date reached: " + dateCounter.ToString(), LogDebugLevel.High);

            return;
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
        /// Return the distance between 2 objects on the map
        /// -1 means they are on different levels
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        public double GetDistanceBetween(MapObject obj1, MapObject obj2) {

            if (obj1.LocationLevel != obj2.LocationLevel)
            {
                return -1.0;
            }

            double distance = Math.Sqrt(Math.Pow(obj1.LocationMap.x - obj2.LocationMap.x, 2.0) + Math.Pow(obj1.LocationMap.y - obj2.LocationMap.y, 2.0));
            return distance;
        }

        /// <summary>
        /// Return the distance between an objects and a point on the same level
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        public double GetDistanceBetween(MapObject obj1, Point p2)
        {
            double distance = Math.Sqrt(Math.Pow(obj1.LocationMap.x - p2.x, 2.0) + Math.Pow(obj1.LocationMap.y - p2.y, 2.0));
            return distance;
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
                distance = GetDistanceBetween(origin, creature);

                if (distance > 0 && distance < closestDistance && origin != creature)
                {
                    closestDistance = distance;
                    closestCreature = creature;
                }
            }

            //And check for player

            distance = GetDistanceBetween(origin, Game.Dungeon.Player);

            if (distance > 0 && distance < closestDistance && origin != Game.Dungeon.Player)
            {
                closestDistance = distance;
                closestCreature = Game.Dungeon.Player;
            }

            return closestCreature;
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

                distance = GetDistanceBetween(origin, creature);

                if (distance > 0 && distance < closestDistance && origin != creature)
                {
                    closestDistance = distance;
                    closestCreature = creature;
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

            if(thisInfo == null) {
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

            if(hiddenName == null) {
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
            specialMoves.Add(new SpecialMoves.WallVault());
            specialMoves.Add(new SpecialMoves.VaultBackstab());
            specialMoves.Add(new SpecialMoves.OpenSpaceAttack());
            //specialMoves.Add(new SpecialMoves.Evade());
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

                    if(trigger == null) {
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
                //saveGameInfo.levels = this.levels;
                //saveGameInfo.levelTCODMaps = this.levelTCODMaps; //If this doens't work, we could easily recalculate them
                saveGameInfo.monsters = this.monsters;
                saveGameInfo.player = this.player;
                saveGameInfo.specialMoves = this.specialMoves;
                saveGameInfo.spells = this.spells;
                saveGameInfo.hiddenNameInfo = this.HiddenNameInfo;
                saveGameInfo.worldClock = this.worldClock;
                saveGameInfo.triggers = this.Triggers;
                saveGameInfo.difficulty = this.Difficulty;

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

                XmlTextWriter writer = new XmlTextWriter(compStream, System.Text.Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, saveGameInfo);

                Game.MessageQueue.AddMessage("Game saved successfully.");
                LogFile.Log.LogEntry("Game saved successfully: " + filename);
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

            //Add TCOD version
            levelTCODMaps.Add(new TCODFov(mapToAdd.width, mapToAdd.height));

            return levels.Count - 1;
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

                //Check connectivity if required
                if(!CheckInConnectedPartOfMap(level, location)) {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Position not connected to stairs", LogDebugLevel.Medium);
                    return false;
                }

                //Otherwise OK
                creature.LocationLevel = level;
                creature.LocationMap = location;

                creature.SightRadius = (int)Math.Ceiling(creature.NormalSightRadius * levels[level].LightLevel);

                monsters.Add(creature);
                return true;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry(String.Format("AddCreature: ") + ex.Message);
                return false;
            }

        }
        /// <summary>
        /// A creature does something that creates a new creature, e.g. raising summoning
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

                //Check connectivity if required
                if (!CheckInConnectedPartOfMap(level, location))
                {
                    LogFile.Log.LogEntryDebug("AddMonster failure: Position not connected to stairs", LogDebugLevel.Medium);
                    return false;
                }

                //Otherwise OK
                creature.LocationLevel = level;
                creature.LocationMap = location;

                creature.SightRadius = (int)Math.Ceiling(creature.NormalSightRadius * levels[level].LightLevel);

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
        /// Checks if location is in the connected part of the dungeon. Checked by routing a path from the down stairs
        /// </summary>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private bool CheckInConnectedPartOfMap(int level, Point location)
        {
            //Level nature
            if (levels[level].GuaranteedConnected)
                return true;

            //Find downstairs
            Features.StaircaseDown downStairs = null;
            Point stairlocation = new Point(0, 0);

            foreach (Feature feature in features)
            {
                if (feature.LocationLevel == level &&
                    feature is Features.StaircaseDown)
                {
                    downStairs = feature as Features.StaircaseDown;
                    stairlocation = feature.LocationMap;
                    break;
                }
            }

            //We don't have downstairs, warn but return true
            if (downStairs == null)
            {
                LogFile.Log.LogEntryDebug("CheckInConnectedPartOfMap called on level with no downstairs", LogDebugLevel.Medium);
                return true;
            }

            return ArePointsConnected(level, location, stairlocation);
        }

        public bool ArePointsConnected(int level, Point firstPoint, Point secondPoint)
        {

            //Build tcodmap
            int Width = levels[level].width;
            int Height = levels[level].height;

            TCODFov tcodMap = levelTCODMaps[level];

            //Try to walk the path between the 2 staircases
            TCODPathFinding path = new TCODPathFinding(tcodMap, 1.0);
            path.ComputePath(firstPoint.x, firstPoint.y, secondPoint.x, secondPoint.y);

            //Find the first step. We need to load x and y with the origin of the path
            int x = firstPoint.x;
            int y = firstPoint.y;

            bool obstacleHit = false;

            //If there's no routeable path
            if (path.IsPathEmpty())
            {
                obstacleHit = true;
            }

            path.Dispose();

            return (!obstacleHit);
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

                //Check connectivity if required
                if(!CheckInConnectedPartOfMap(level, location)) {
                    LogFile.Log.LogEntryDebug("AddItem failure: Position not connected to stairs", LogDebugLevel.Medium);
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
        /// Add feature to the dungeon. Check it can be reached by the player. Not suitable for adding staircases.
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool AddFeatureCheckConnectivity(Feature feature, int level, Point location)
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

                //Check connectivity if required
                if (!CheckInConnectedPartOfMap(level, location))
                {
                    LogFile.Log.LogEntryDebug("AddFeature failure: Position not connected to stairs", LogDebugLevel.Medium);
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

            //Check for PC blocking
            if (player.LocationLevel == level && player.LocationMap.x == location.x && player.LocationMap.y == location.y)
            {
                contents.player = player;
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
                LogFile.Log.LogEntryDebug("MapSquareCanBeEntered failure: Not Walkable", LogDebugLevel.Low);
                return false;
            }

            //These are duplicates that use different code, so should be obsoleted
            
            //A wall - should be caught above
            if (!Dungeon.IsTerrainWalkable(levels[level].mapSquares[location.x, location.y].Terrain))
            {
                LogFile.Log.LogEntryDebug("MapSquareCanBeEntered failure: not walkable by terrain type", LogDebugLevel.High);
                return false;
            }

            //Void (outside of map) - should be caught above
            if (levels[level].mapSquares[location.x, location.y].Terrain == MapTerrain.Void)
            {
                LogFile.Log.LogEntryDebug("MapSquareCanBeEntered failure: Void", LogDebugLevel.High);
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

        public List<TCODFov> FOVs
        {
            get
            {
                return levelTCODMaps;
            }
        }

        /// <summary>
        /// For serialization only
        /// </summary>
        public List<DungeonEffect> Effects
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


        public Player Player {
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
            player.LocationLevel = level;
            player.LocationMap = new Point(x,y);

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
        internal bool MovePCAbsoluteSameLevel(int x, int y) {

            MovePCAbsolute(player.LocationLevel, x, y);

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
        /// Process a relative PC move, from a keypress
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool PCMove(int x, int y)
        {
            Point newPCLocation = new Point(Player.LocationMap.x + x, Player.LocationMap.y + y);

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

            foreach (SpecialMove move in specialMoves)
            {
                if (move.Known)
                {
                    //Test the move twice
                    //The first check may cause a long chain to fail but the move could be a valid new start move
                    //The second check picks this up

                    bool moveSuccess = move.CheckAction(true, newPCLocation);

                    if (!moveSuccess)
                    {
                        move.CheckAction(true, newPCLocation);
                    }
                }
            }

            //Are any moves ready, if so carry the first one out.
            //Try allow multiple moves on one turn. Have to be careful to make sure there 

            SpecialMove moveToDo = null;

            foreach (SpecialMove move in specialMoves)
            {
                if (move.Known && move.MoveComplete())
                {
                    moveToDo = move;
                    move.DoMove(newPCLocation);
                }
            }

            //If there's no special move, do a conventional move
            if (moveToDo == null)
            {
                //Moving into void not allowed (but should never happen)
                if (!MapSquareIsWalkable(player.LocationLevel, newPCLocation))
                {
                    //This now costs time since it could be part of a special move
                    return true;
                }

                //Check for monsters in the square
                SquareContents contents = MapSquareContents(player.LocationLevel, newPCLocation);
                bool okToMoveIntoSquare = false;

                //If it's empty, it's OK
                if (contents.monster == null)
                {
                    okToMoveIntoSquare = true;
                }

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
                        CombatResults results = player.AttackMonster(contents.monster);
                        if (results == CombatResults.DefenderDied)
                        {
                            okToMoveIntoSquare = true;
                        }

                    }
                    else
                    {
                        //Monster hostile 

                        CombatResults results = player.AttackMonster(contents.monster);
                        if (results == CombatResults.DefenderDied)
                        {
                            okToMoveIntoSquare = true;
                        }
                    }
                }

                //If not OK to move, return here
                if (!okToMoveIntoSquare)
                    return true;

                MovePCAbsoluteSameLevel(newPCLocation.x, newPCLocation.y);
            }

            //Run any entering square messages
            //Happens for both normal and special moves

            //Tell the player if there are multiple items in the square
            if (MultipleItemAtSpace(player.LocationLevel, player.LocationMap))
            {
                Game.MessageQueue.AddMessage("There are multiple items here.");
            }

            //If there is a feature and an item (feature will be hidden)
            if (FeatureAtSpace(player.LocationLevel, player.LocationMap) != null &&
                ItemAtSpace(player.LocationLevel, player.LocationMap) != null)
            {
                Game.MessageQueue.AddMessage("There is a staircase here.");
            }

            return true;
        }

        

        /// <summary>
        /// Kill a monster. This monster won't get any further turns.
        /// </summary>
        /// <param name="monster"></param>
        public void KillMonster(Monster monster)
        {
            //We can't take the monster out of the collection directly since we might still be iterating through them
            //Instead set a flag on the monster and remove it after all turns are complete
            monster.Alive = false;

            //Drop its inventory (including plot items we gave it)
            monster.DropAllItems();

            //Drop any insta-create treasure
            monster.InventoryDrop();

            //If the creature was charmed, delete 1 charmed creature from the player total
            if(monster.Charmed)
                Game.Dungeon.Player.RemoveCharmedCreature();

            //Leave a corpse
            AddDecorationFeature(new Features.Corpse(), monster.LocationLevel, monster.LocationMap);

            //Deal with special monsters (bit rubbish programming)
            Creatures.Lich lich = monster as Creatures.Lich;


            if (lich != null)
            {

                //Kill all other monsters on the level

                foreach (Monster m in monsters)
                {
                    if (m.LocationLevel == lich.LocationLevel && m != lich)
                        KillMonster(m);
                }

                //OK, we've killed the end baddy have a moral decision
                Screen.Instance.PlayMovie("lichGem", true);

                bool takeGem = Screen.Instance.YesNoQuestion("Take the gem?");

                if (takeGem)
                {
                    Screen.Instance.PlayMovie("becomeLich", true);
                    EndGame("became a powerful lich and begun his reign of terror.");
                }
                else
                {
                    Screen.Instance.PlayMovie("crushLichGem", true);
                }
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
        /// Check and set the walkable parameter on each map square
        /// At the moment done for all levels
        /// </summary>
        internal void RecalculateWalkable()
        {
            //Terrain

            for (int i = 0; i < levels.Count; i++)
            {
                {
                    Map level = levels[i];

                    for (int j = 0; j < level.width; j++)
                    {
                        for (int k = 0; k < level.height; k++)
                        {

                            //Terrain

                            bool walkable = true;

                            //Use new function

                            if (!Dungeon.IsTerrainWalkable(level.mapSquares[j, k].Terrain))
                                walkable = false;

                            /*
                            //Walls
                            if (level.mapSquares[j, k].Terrain == MapTerrain.Wall)
                            {
                                walkable = false;
                            }

                            //Void
                            if (level.mapSquares[j, k].Terrain == MapTerrain.Void)
                            {
                                walkable = false;
                            }

                            //Closed door
                            if (level.mapSquares[j, k].Terrain == MapTerrain.ClosedDoor)
                            {
                                walkable = false;
                            }

                            if (level.mapSquares[j, k].Terrain == MapTerrain.Mountains)
                            {
                                walkable = false;
                            }

                            if (level.mapSquares[j, k].Terrain == MapTerrain.Trees)
                            {
                                walkable = false;
                            }

                            if (level.mapSquares[j, k].Terrain == MapTerrain.River)
                            {
                                walkable = false;
                            }
                            */
                            level.mapSquares[j, k].Walkable = walkable;
                        }
                    }
                }
            }

            //Creatures
            
            //Set each monster's square to non-walkable
            //Don't do this anymore
            /*foreach (Monster monster in monsters)
            {
                levels[monster.LocationLevel].mapSquares[monster.LocationMap.x, monster.LocationMap.y].Walkable = false;
            }*/
        }

        /// <summary>
        /// Find best path between 2 points. No reason really to restrict this to one level only but that would require extending TCOD
        /// </summary>
        /// <param name="level"></param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public bool CalculatePath(int level, Point startPoint, Point endPoint)
        {
            return true;
        }

        /// <summary>
        /// Refresh the TCOD maps used for FOV and pathfinding
        /// Unoptimised at present
        /// </summary>
        internal void RefreshTCODMaps()
        {
            //Set the properties on the TCODMaps from our Maps
            for (int i = 0; i < levels.Count; i++)
            {
                RefreshTCODMap(i);
            }
        }

        /// <summary>
        /// Refresh the TCOD maps used for FOV and pathfinding
        /// Unoptimised at present
        /// </summary>
        internal void RefreshTCODMap(int levelToRefresh)
        {
            //Fail if we have been asked for an invalid level
            if (levelToRefresh < 0 || levelToRefresh > levels.Count)
            {
                LogFile.Log.LogEntry("RefreshTCODMap: Level " + levelToRefresh + " does not exist");
                return;
            }

            Map level = levels[levelToRefresh];
            TCODFov tcodLevel = levelTCODMaps[levelToRefresh];

            for (int j = 0; j < level.width; j++)
            {
                for (int k = 0; k < level.height; k++)
                {
                    tcodLevel.SetCell(j, k, !level.mapSquares[j, k].BlocksLight, level.mapSquares[j, k].Walkable);
                }
            }

            /*
            //Ignoring closed doors

            tcodLevel = levelTCODMapsIgnoringClosedDoors[levelToRefresh];
            for (int j = 0; j < level.width; j++)
            {
                for (int k = 0; k < level.height; k++)
                {
                    MapTerrain terrainHere = level.mapSquares[j, k].Terrain;

                    tcodLevel.SetCell(j, k, !level.mapSquares[j, k].BlocksLight, level.mapSquares[j, k].Walkable || terrainHere == MapTerrain.ClosedDoor);
                }
            }*/

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

        /// <summary>
        /// Calculates the FOV for a creature
        /// </summary>
        /// <param name="creature"></param>
        public TCODFov CalculateCreatureFOV(Creature creature)
        {
            Map currentMap = levels[creature.LocationLevel];
            TCODFov tcodFOV = levelTCODMaps[creature.LocationLevel];

            //Update FOV
            tcodFOV.CalculateFOV(creature.LocationMap.x, creature.LocationMap.y, creature.SightRadius);

            return tcodFOV;

        }

        /// <summary>
        /// Displays the creature FOV on the map. Note that this clobbers the FOV map
        /// </summary>
        /// <param name="creature"></param>
        public void ShowCreatureFOVOnMap(Creature creature) {

            //Only do this if the creature is on a visible level
            if(creature.LocationLevel != Player.LocationLevel)
                return;

            Map currentMap = levels[creature.LocationLevel];
            TCODFov tcodFOV = levelTCODMaps[creature.LocationLevel];
           
            //Calculate FOV
            tcodFOV.CalculateFOV(creature.LocationMap.x, creature.LocationMap.y, creature.SightRadius);

            //Only check sightRadius around the creature

            int xl = creature.LocationMap.x - creature.SightRadius;
            int xr = creature.LocationMap.x + creature.SightRadius;

            int yt = creature.LocationMap.y - creature.SightRadius;
            int yb = creature.LocationMap.y + creature.SightRadius;

            //If sight is infinite, check all the map
            if (creature.SightRadius == 0)
            {
                xl = 0;
                xr = currentMap.width;
                yt = 0;
                yb = currentMap.height;
            }

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
                    bool inFOV = tcodFOV.CheckTileFOV(i, j);
                    if(inFOV)
                        thisSquare.InMonsterFOV = true;
                }
            }
        }

        /// <summary>
        /// Recalculate the players FOV. Subsequent accesses to the TCODMap of the player's level will have his FOV
        /// Note that the maps may get hijacked by other creatures
        /// </summary>
        internal void CalculatePlayerFOV()
        {
            //Get TCOD to calculate the player's FOV
            Map currentMap = levels[Player.LocationLevel];

            TCODFov tcodFOV = levelTCODMaps[Player.LocationLevel];
            
            tcodFOV.CalculateFOV(Player.LocationMap.x, Player.LocationMap.y, Player.SightRadius);

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
        }

        /// <summary>
        /// Returns the direction to go in (+-xy) for the next step towards the target
        /// If there's no route at all, return -1, -1. Right now we throw an exception for this, since it shouldn't happen in a connected dungeon
        /// If there's a route but its blocked by a creature return the originCreature's coords
        /// </summary>
        /// <param name="originCreature"></param>
        /// <param name="destCreature"></param>
        /// <returns></returns>
        internal Point GetPathTo(Creature originCreature, Creature destCreature)
        {
            //If on different levels it's an error
            if (originCreature.LocationLevel != destCreature.LocationLevel)
            {
                string msg = originCreature.Representation + " not on the same level as " + destCreature.Representation;
                LogFile.Log.LogEntry(msg);
                throw new ApplicationException(msg);
            }


            //Destination square needs to be walkable for the path finding algorithm. However it isn't walkable at the moment since there is the target creature on it
            //Temporarily make it walkable, keeping transparency the same
            //levelTCODMaps[destCreature.LocationLevel].SetCell(destCreature.LocationMap.x, destCreature.LocationMap.y,
              //  !levels[destCreature.LocationLevel].mapSquares[destCreature.LocationMap.x, destCreature.LocationMap.y].BlocksLight, true);

            

            //Try to walk the path
            //If we fail, check if this square occupied by a creature
            //If so, make that square temporarily unwalkable and try to re-route

            List<Point> blockedSquares = new List<Point>();
            bool goodPath = false;
            bool pathBlockedByCreature = false;
            Point nextStep = new Point(-1, -1);

            do
            {
                //Generate path object
                TCODPathFinding path = new TCODPathFinding(levelTCODMaps[originCreature.LocationLevel], 1.0);
                path.ComputePath(originCreature.LocationMap.x, originCreature.LocationMap.y, destCreature.LocationMap.x, destCreature.LocationMap.y);

                //Find the first step. We need to load x and y with the origin of the path
                int x, y;
                int xOrigin, yOrigin;
               
                path.GetPathOrigin(out x, out y);
                xOrigin = x; yOrigin = y;

                path.WalkPath(ref x, ref y, false);

                //If the x and y of the next step it means the path is blocked

                if (x == xOrigin && y == yOrigin)
                {
                    //If there was no blocking creature then there is no possible route (hopefully impossible in a fully connected dungeon)
                    if (!pathBlockedByCreature)
                    {
                        //This gets thrown a lot mainly when you cheat
                        LogFile.Log.LogEntry("Path blocked in connected dungeon!");
                        return originCreature.LocationMap;
                        //throw new ApplicationException("Path blocked in connected dungeon!");
                        
                        /*
                        nextStep = new Point(x, y);
                        bool trans;
                        bool walkable;
                        levelTCODMaps[0].GetCell(originCreature.LocationMap.x, originCreature.LocationMap.y, out trans, out walkable);
                        levelTCODMaps[0].GetCell(destCreature.LocationMap.x, destCreature.LocationMap.y, out trans, out walkable);
                        */

                        //Uncomment this if you want to return -1, -1
                        
                        //nextStep = new Point(-1, -1);
                        //goodPath = true;
                        //continue;
                    }
                    else
                    {
                        //Blocking creature but no path
                        nextStep = new Point(x, y);
                        goodPath = true;
                        continue;
                    }
                }


                //Check if that square is occupied
                Creature blockingCreature = null;

                foreach (Monster creature in monsters)
                {
                    if (creature.LocationLevel != originCreature.LocationLevel)
                        continue;

                    //Is it the source creature itself?
                    if (creature == originCreature)
                        continue;

                    //Is it the target creature?
                    if (creature == destCreature)
                        continue;

                    //Another creature is blocking
                    if (creature.LocationMap.x == x && creature.LocationMap.y == y)
                    {
                        blockingCreature = creature;
                    }
                }
                //Do the same for the player (if the creature is chasing another creature around the player)

                if (destCreature != Player)
                {
                    if (Player.LocationLevel == originCreature.LocationLevel &&
                        Player.LocationMap.x == x && Player.LocationMap.y == y)
                    {
                        blockingCreature = Player;
                    }
                }

                //If no blocking creature, the path is good
                if (blockingCreature == null)
                {
                    goodPath = true;
                    nextStep = new Point(x, y);
                    path.Dispose();
                }
                else
                {
                    //Otherwise, there's a blocking creature. Make his square unwalkable temporarily and try to reroute
                    pathBlockedByCreature = true;
                    
                    int blockingLevel = blockingCreature.LocationLevel;
                    int blockingX = blockingCreature.LocationMap.x;
                    int blockingY = blockingCreature.LocationMap.y;
                    
                    levelTCODMaps[blockingLevel].SetCell(blockingX, blockingY, !levels[blockingLevel].mapSquares[blockingX, blockingY].BlocksLight, false);

                    //Add this square to a list of squares to put back
                    blockedSquares.Add(new Point(blockingX, blockingY));

                    //Dispose the old path
                    path.Dispose();

                    //We will try again
                }
            } while (!goodPath);

            //Put back any squares we made unwalkable
            foreach (Point sq in blockedSquares)
            {
                levelTCODMaps[originCreature.LocationLevel].SetCell(sq.x, sq.y, !levels[originCreature.LocationLevel].mapSquares[sq.x, sq.y].BlocksLight, true);
            }

            //path.WalkPath(ref x, ref y, false);

            //path.GetPointOnPath(0, out x, out y); //crashes for some reason

            //Dispose of path (bit wasteful seeming!)
            //path.Dispose();

            //Set the destination square as unwalkable again
            //levelTCODMaps[destCreature.LocationLevel].SetCell(destCreature.LocationMap.x, destCreature.LocationMap.y,
              //  !levels[destCreature.LocationLevel].mapSquares[destCreature.LocationMap.x, destCreature.LocationMap.y].BlocksLight, false);

            //Point nextStep = new Point(x, y);

            return nextStep;
        }

        /// <summary>
        /// Returns the direction to go in (+-xy) for the next step towards the target
        /// If there's no route at all, return -1, -1
        /// If there's a route but its blocked by a creature return the originCreature's coords
        /// </summary>
        /// <param name="originCreature"></param>
        /// <param name="destCreature"></param>
        /// <returns></returns>
        internal Point GetPathFromCreatureToPoint(int level, Monster originCreature, Point destCreature)
        {
            //If on different levels it's an error
            
            //Destination square needs to be walkable for the path finding algorithm. However it isn't walkable at the moment since there is the target creature on it
            //Temporarily make it walkable, keeping transparency the same
            //levelTCODMaps[destCreature.LocationLevel].SetCell(destCreature.LocationMap.x, destCreature.LocationMap.y,
            //  !levels[destCreature.LocationLevel].mapSquares[destCreature.LocationMap.x, destCreature.LocationMap.y].BlocksLight, true);



            //Try to walk the path
            //If we fail, check if this square occupied by a creature
            //If so, make that square temporarily unwalkable and try to re-route

            List<Point> blockedSquares = new List<Point>();
            bool goodPath = false;
            bool pathBlockedByCreature = false;
            Point nextStep = new Point(-1, -1);

            do
            {
                //Generate path object
                TCODPathFinding path = new TCODPathFinding(levelTCODMaps[level], 1.0);
                path.ComputePath(originCreature.LocationMap.x, originCreature.LocationMap.y, destCreature.x, destCreature.y);

                //Find the first step. We need to load x and y with the origin of the path
                int x, y;
                int xOrigin, yOrigin;

                path.GetPathOrigin(out x, out y);
                xOrigin = x; yOrigin = y;

                path.WalkPath(ref x, ref y, false);

                //If the x and y of the next step it means the path is blocked

                if (x == xOrigin && y == yOrigin)
                {
                    //If there was no blocking creature then there is no possible route
                    if (!pathBlockedByCreature)
                    {
                        return new Point(-1, -1);

                        /*
                        nextStep = new Point(x, y);
                        bool trans;
                        bool walkable;
                        levelTCODMaps[0].GetCell(originCreature.LocationMap.x, originCreature.LocationMap.y, out trans, out walkable);
                        levelTCODMaps[0].GetCell(destCreature.LocationMap.x, destCreature.LocationMap.y, out trans, out walkable);
                        */

                        //Uncomment this if you want to return -1, -1

                        //nextStep = new Point(-1, -1);
                        //goodPath = true;
                        //continue;
                    }
                    else
                    {
                        //Blocking creature but no path
                        nextStep = new Point(x, y);
                        goodPath = true;
                        continue;
                    }
                }


                //Check if that square is occupied
                Creature blockingCreature = null;

                foreach (Monster creature in monsters)
                {
                    if (creature.LocationLevel != level)
                        continue;

                    //Is it the source creature itself?
                    if (creature.LocationMap.x == originCreature.LocationMap.x &&
                        creature.LocationMap.y == originCreature.LocationMap.y)
                        continue;

                    //Another creature is blocking
                    if (creature.LocationMap.x == x && creature.LocationMap.y == y)
                    {
                        blockingCreature = creature;
                    }
                }
                //Do the same for the player (if the creature is chasing another creature around the player)
                    if (Player.LocationLevel == originCreature.LocationLevel &&
                        Player.LocationMap.x == x && Player.LocationMap.y == y)
                    {
                        blockingCreature = Player;
                    }


                //If no blocking creature, the path is good
                if (blockingCreature == null)
                {
                    goodPath = true;
                    nextStep = new Point(x, y);
                    path.Dispose();
                }
                else
                {
                    //Otherwise, there's a blocking creature. Make his square unwalkable temporarily and try to reroute
                    pathBlockedByCreature = true;

                    int blockingLevel = blockingCreature.LocationLevel;
                    int blockingX = blockingCreature.LocationMap.x;
                    int blockingY = blockingCreature.LocationMap.y;

                    levelTCODMaps[blockingLevel].SetCell(blockingX, blockingY, !levels[blockingLevel].mapSquares[blockingX, blockingY].BlocksLight, false);

                    //Add this square to a list of squares to put back
                    blockedSquares.Add(new Point(blockingX, blockingY));

                    //Dispose the old path
                    path.Dispose();

                    //We will try again
                }
            } while (!goodPath);

            //Put back any squares we made unwalkable
            foreach (Point sq in blockedSquares)
            {
                levelTCODMaps[originCreature.LocationLevel].SetCell(sq.x, sq.y, !levels[originCreature.LocationLevel].mapSquares[sq.x, sq.y].BlocksLight, true);
            }

            //path.WalkPath(ref x, ref y, false);

            //path.GetPointOnPath(0, out x, out y); //crashes for some reason

            //Dispose of path (bit wasteful seeming!)
            //path.Dispose();

            //Set the destination square as unwalkable again
            //levelTCODMaps[destCreature.LocationLevel].SetCell(destCreature.LocationMap.x, destCreature.LocationMap.y,
            //  !levels[destCreature.LocationLevel].mapSquares[destCreature.LocationMap.x, destCreature.LocationMap.y].BlocksLight, false);

            //Point nextStep = new Point(x, y);

            return nextStep;
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
        /// Increment time on all dungeon (global) events. Events that expire will run their onExit() routines and then delete themselves from the list
        /// </summary>
        internal void IncrementEventTime()
        {
            //Increment time on events and remove finished ones
            List<DungeonEffect> finishedEffects = new List<DungeonEffect>();

            foreach (DungeonEffect effect in effects)
            {
                effect.IncrementTime();

                if (effect.HasEnded())
                {
                    finishedEffects.Add(effect);
                }
            }

            //Remove finished effects
            foreach (DungeonEffect effect in finishedEffects)
            {
                effects.Remove(effect);
            }
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
                if(feature.IsLocatedAt(locationLevel, locationMap) && feature is UseableFeature) {
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
        /// Master is terrain walkable from MapTerrain type (not universally used yet)
        /// </summary>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static bool IsTerrainWalkable(MapTerrain terrain)
        {
            if (terrain == MapTerrain.Empty || terrain == MapTerrain.Flooded || terrain == MapTerrain.OpenDoor || terrain == MapTerrain.Corridor || terrain == MapTerrain.Grass || terrain == MapTerrain.Road || terrain == MapTerrain.Gravestone || terrain == MapTerrain.Trees || terrain == MapTerrain.Rubble)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes an item from the master list.
        /// </summary>
        /// <param name="itemToUse"></param>
        internal void RemoveItem(Item itemToUse)
        {
            items.Remove(itemToUse);
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

                //Open the door
                levels[level].mapSquares[doorLocation.x, doorLocation.y].Terrain = MapTerrain.OpenDoor;
                levels[level].mapSquares[doorLocation.x, doorLocation.y].SetOpen();

                //This is very inefficient since it resets the whole level. Could just do the door
                //RefreshTCODMap(level);

                //More efficient version
                levelTCODMaps[level].SetCell(doorLocation.x, doorLocation.y, !levels[level].mapSquares[doorLocation.x, doorLocation.y].BlocksLight, levels[level].mapSquares[doorLocation.x, doorLocation.y].Walkable);


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
            //Check special moves.

            foreach (SpecialMove move in specialMoves)
            {
                if(move.Known)
                    move.CheckAction(false, new Point(0, 0));
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
                moveToDo.DoMove(new Point(-1,-1));

                //Clear all moves
                foreach (SpecialMove move in specialMoves)
                {
                    move.ClearMove();
                }
            }
        }

        /// <summary>
        /// It's all gone wrong!
        /// </summary>
        internal void PlayerDeath(string verb)
        {
            if (PlayerImmortal && !verb.Contains("quit"))
                return;

            //In PrincessRL death is not permanent, but quitting is!

            //Knocked out, go back to school
            if(!verb.Contains("quit")) {

                LogFile.Log.LogEntryDebug("Player knocked out", LogDebugLevel.Medium);

                Screen.Instance.PlayMovie("knockedout", false);

                //Up date counter
                player.NumDeaths++;

                //Game.MessageQueue.ClearList(); //If want to lose the last message, fit it in calling function
                Game.MessageQueue.AddMessage("You get taken back to school by the guards.");

                PlayerLeavesDungeon();

                return;
            }

            //Right now, only seen on a quit (will be changed too)

            //Set up the death screen

            //Death preamble

            List<string> deathPreamble = new List<string>();

            deathPreamble.Add(Game.Dungeon.player.Name + " the assassin " + verb + " on level " + (player.LocationLevel + 1).ToString() + " of the dungeon.");
            deathPreamble.Add("He lasted " + Game.Dungeon.player.TurnCount + " turns.");
            deathPreamble.Add("Difficulty: " + StringEquivalent.GameDifficultyString[Game.Dungeon.Difficulty]);
            deathPreamble.Add("");
            deathPreamble.Add("He found " + Game.Dungeon.Player.PlotItemsFound + " of " + Game.Dungeon.Player.TotalPlotItems + " plot items.");

            //Total kills
            
            //Make killCount list

            List<Monster> kills = player.Kills;
            List<KillCount> killCount = new List<KillCount>();

            int totalKills = 0;

            foreach (Monster kill in kills)
            {
                totalKills++;

                //Check that we are the same type (and therefore sort of item)
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
                //Look only at the first item in the group (stored by index). All the items in this group must have the same type
                

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

            deathPreamble.Add("");
            deathPreamble.Add("He killed " + totalKills + " creatures.");

            //Load up screen and display
            Screen.Instance.TotalKills = killRecord;
            Screen.Instance.DeathPreamble = deathPreamble;

            Screen.Instance.DrawDeathScreen();
            Screen.Instance.FlushConsole();

            SaveObituary(deathPreamble, killRecord);

            if (!Game.Dungeon.SaveScumming)
            {
                DeleteSaveFile();
            }

            //Wait for a keypress
            KeyPress userKey = Keyboard.WaitForKeyPress(true);

            //Stop the main loop
            RunMainLoop = false;
            
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
            foreach (DungeonSquareTrigger trigger in Triggers)
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
        /// Victory!
        /// </summary>
        /// <param name="p"></param>
        internal void EndGame(string endPhrase)
        {
            //Set up the death screen

            //Death preamble

            List<string> deathPreamble = new List<string>();

            string playerName = Game.Dungeon.player.Name;

            deathPreamble.Add(playerName + " the assassin " + endPhrase);
            deathPreamble.Add("He lasted " + Game.Dungeon.player.TurnCount + " turns.");
            deathPreamble.Add("Difficulty: " + StringEquivalent.GameDifficultyString[Game.Dungeon.Difficulty]);
            deathPreamble.Add("");
            deathPreamble.Add("He found " + Game.Dungeon.Player.PlotItemsFound + " of " + Game.Dungeon.Player.TotalPlotItems + " plot items.");

            //Total kills

            //Make killCount list

            List<Monster> kills = player.Kills;
            List<KillCount> killCount = new List<KillCount>();

            int totalKills = 0;

            foreach (Monster kill in kills)
            {
                totalKills++;

                //Check that we are the same type (and therefore sort of item)
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
                //Look only at the first item in the group (stored by index). All the items in this group must have the same type


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

            deathPreamble.Add("");
            deathPreamble.Add("He killed " + totalKills + " creatures");


            SaveObituary(deathPreamble, killRecord);

            if (!Game.Dungeon.SaveScumming)
            {
                DeleteSaveFile();
            }

            //Load up screen and display
            Screen.Instance.TotalKills = killRecord;
            Screen.Instance.DeathPreamble = deathPreamble;

            Screen.Instance.DrawVictoryScreen();
            Screen.Instance.FlushConsole();

            //Wait for a keypress
            KeyPress userKey = Keyboard.WaitForKeyPress(true);

            //Stop the main loop
            RunMainLoop = false;
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
            foreach(Monster monster in summonedMonsters) {
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
                    if (!monster.CanBeCharmed() && !monster.CanBePassified())
                    {
                        Game.MessageQueue.AddMessage("The " + monster.SingleDescription + " laughs at your feeble attempt.");
                        return true;
                    }

                    bool canCharm = true;

                    if (!monster.CanBeCharmed())
                    {
                        //On for passify only
                        canCharm = false;
                    }

                    //Try to charm, may fail if the player has no more charms
                    
                    bool playerOK = false;
                    if (canCharm)
                    {
                        //Check if the player has any more charms
                        playerOK = player.AddCharmCreatureIfPossible();
                    }

                    if (!playerOK)
                    {
                        canCharm = false;
                        //Game.MessageQueue.AddMessage("Too many charmed creatures.");
                        //return true;
                    }

                    //All OK do the charm
                    if (canCharm)
                    {
                        //Test against statistic here

                        string msg = "The " + monster.SingleDescription + " looks at you lovingly.";

                        Game.MessageQueue.AddMessage(msg);
                        contents.monster.CharmCreature();

                        return true;
                    }

                    //Only a passify
                    else
                    {
                        //Test against statistic here

                        string msg = "The " + monster.SingleDescription + " sighs and turns away.";

                        Game.MessageQueue.AddMessage(msg);
                        contents.monster.PassifyCreature();

                        return true;
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
        /// Teleport the user back to town
        /// </summary>
        internal void PlayerBackToTown()
        {
            //Move to town
            Player.LocationLevel = 0;
            Player.LocationMap = levels[0].PCStartLocation;

            //Drop all the player's equipped items
            PutItemsInStore();
        }

        /// <summary>
        /// Exit a dungeon and go back to town
        /// </summary>

        public void PlayerLeavesDungeon()
        {
            LogFile.Log.LogEntryDebug("Player back to town. Date moved on.", LogDebugLevel.Medium);
            Game.Dungeon.MoveToNextDate();
            Game.Dungeon.PlayerBackToTown();
        }

        Point storeTL = new Point(33, 2);
        Point storeBR = new Point(39, 3);

        /// <summary>
        /// Put all the user's items in the store
        /// </summary>
        public void PutItemsInStore()
        {
            //Drop all the items from the player.
            //This returns them to the master list in Dungeon
            Game.Dungeon.player.RemoveAllItems();
                
            //Reset the player's appearance
            Screen.Instance.PCColor = defaultPCColor;

            //Place all the found objects in the store room
            int xLoc = storeTL.x;
            int yLoc = storeTL.y;
            
            foreach (Item item in items)
            {
                if (item.IsFound)
                {
                    item.InInventory = false;
                    item.LocationLevel = 0;
                    item.LocationMap = new Point(xLoc, yLoc);

                    xLoc++;

                    if (xLoc > storeBR.x)
                    {
                        yLoc++;
                    }

                    if (yLoc > storeBR.y)
                    {
                        //Run out of room - shouldn't happen
                        LogFile.Log.LogEntryDebug("Run out of room in store for items!", LogDebugLevel.High);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Set the player's real stats as determined by their training stats.
        /// Done before adventuring.
        /// </summary>
        internal void SyncStatsWithTraining()
        {
            Player player = Game.Dungeon.player;
            Inventory inv = player.Inventory;

            //Set all the stats which can only be set when leaving the town

            //Hitpoints
            player.Hitpoints = player.HitpointsStat;
            player.MaxHitpoints = player.HitpointsStat;

            //Magic points
            player.MaxMagicPoints = player.MagicStat * 2;
            player.MagicPoints = player.MagicStat * 2;

            //Set all the stats that can be set at any time
            player.CalculateCombatStats();

            /*            
                        armourClass = 12;
                                        damageBase = 4;
                                        damageModifier = 0;
                                        hitModifier = 0;
                                        maxHitpoints = 15;
                                        MaxCharmedCreatures = 1;
 

                        //Armour class
                        player.ArmourClassAccess = 12;

                        //Charm points
                        player.CharmPoints = player.CharmStat;

                        //Max charmed creatures
                        if (inv.ContainsItem(new Items.SparklingEarrings()))
                        {
                            player.MaxCharmedCreatures = 2;
                        }
                        else
                            player.MaxCharmedCreatures = 1;

                        //To Hit

                        int toHit;

                        if(player.AttackStat > 60) {
                            toHit = (int)Math.Round((player.AttackStat - 60)/30.0) + 3;
                        }
                        else {
                            toHit = player.AttackStat / 20;
                        }

                        player.HitModifierAccess = toHit;

                        //Damage base

                        int damageBase;
                        if(player.AttackStat > 100) {
                            damageBase = 10;
                        }
                        else if(player.AttackStat > 60) {
                            damageBase = 8;
                        }
                        else if(player.AttackStat > 30) {
                            damageBase = 6;
                        }
                        else
                            damageBase = 4;

                        player.DamageBaseAccess = damageBase;
                        Screen.Instance.PCColor = ColorPresets.White;

                        //Consider equipped clothing items (only 1 will work)
                        if (inv.ContainsItem(new Items.MetalArmour()))
                        {
                            player.ArmourClassAccess += 4;
                            Screen.Instance.PCColor = ColorPresets.SteelBlue;
                        }
                        else if(inv.ContainsItem(new Items.LeatherArmour())) {
                            player.ArmourClassAccess += 2;
                            Screen.Instance.PCColor = ColorPresets.BurlyWood;
                        }
                        else if (inv.ContainsItem(new Items.PrettyDress()))
                        {
                            player.CharmPoints += 20;
                            Screen.Instance.PCColor = ColorPresets.BlueViolet;
                        }
            
                        //Consider equipped weapons (only 1 will work)
                        if (inv.ContainsItem(new Items.GodSword()))
                        {
                            player.DamageModifierAccess += 4;
                        }
                        else if (inv.ContainsItem(new Items.LongSword()))
                        {
                            player.DamageModifierAccess += 2;
                        }
                        else if (inv.ContainsItem(new Items.ShortSword()))
                        {
                            player.DamageModifierAccess += 1;
                        }
            
                        */


            
            //etc
        }
    }
}
