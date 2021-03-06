﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    /// <summary>
    /// Retreives string equivalents for various enums
    /// </summary>
    static class StringEquivalent
    {
        /// <summary>
        /// Mapping of equipment slots to descriptions
        /// </summary>
        public static Dictionary<EquipmentSlot, string> EquipmentSlots { get; private set; }

        /// <summary>
        /// Mapping of terrain to ASCII characters
        /// </summary>
        public static Dictionary<MapTerrain, char> TerrainChars { get; private set; }

        public static Dictionary<MapTerrain, Color> TerrainColors { get; private set; }

        public static Dictionary<GameDifficulty, string> GameDifficultyString { get; private set; }

        public static char Heading { get { return (char)7; } }

        static StringEquivalent()
        {
            EquipmentSlots = new Dictionary<EquipmentSlot, string>();
            SetupEquipmentSlots();

            TerrainChars = new Dictionary<MapTerrain, char>();
            SetupTerrainChars();

            TerrainColors = new Dictionary<MapTerrain, Color>();
            SetupTerrainColors();

            GameDifficultyString = new Dictionary<GameDifficulty, string>();
            GameDifficultyString.Add(GameDifficulty.Easy, "Easy");
            GameDifficultyString.Add(GameDifficulty.Medium, "Medium");
            GameDifficultyString.Add(GameDifficulty.Hard, "Hard");
        }

        public static void OverrideTerrainChar(MapTerrain terrain, char c)
        {
            TerrainChars[terrain] = c;
        }

        private static void SetupTerrainChars()
        {
            int shroomWallStartRow = 21;
            int shroomWallSkip = 7;
            int rowLength = 16;

            TerrainChars.Add(MapTerrain.Empty, (char)482);
            TerrainChars.Add(MapTerrain.Void, '\xb0');
            TerrainChars.Add(MapTerrain.Wall, (char)((shroomWallStartRow + 7) * rowLength + 0));

            TerrainChars.Add(MapTerrain.NonWalkableFeature, (char)250);
            TerrainChars.Add(MapTerrain.NonWalkableFeatureLightBlocking, (char)250);
            
            TerrainChars.Add(MapTerrain.HellWall, '#');
            TerrainChars.Add(MapTerrain.SkeletonWall, '8');
            TerrainChars.Add(MapTerrain.SkeletonWallWhite, '8');
            TerrainChars.Add(MapTerrain.Corridor, '|');
            
            TerrainChars.Add(MapTerrain.Flooded, '=');
            TerrainChars.Add(MapTerrain.Grass, ',');
            TerrainChars.Add(MapTerrain.River, '=');
            TerrainChars.Add(MapTerrain.Trees, '*');
            TerrainChars.Add(MapTerrain.Rubble, '*');
            TerrainChars.Add(MapTerrain.Road, '-');
            TerrainChars.Add(MapTerrain.Mountains, '^');
            TerrainChars.Add(MapTerrain.Volcano, '^');
            TerrainChars.Add(MapTerrain.Forest, '%');
            TerrainChars.Add(MapTerrain.Gravestone, '+');
            TerrainChars.Add(MapTerrain.BarDoor, '|');
            TerrainChars.Add(MapTerrain.DockWall, (char)368);
            
            TerrainChars.Add(MapTerrain.ClosedDoor, (char)((shroomWallStartRow + 1) * rowLength + 2));
            TerrainChars.Add(MapTerrain.OpenDoor, (char)((shroomWallStartRow + 1) * rowLength + 3));

            TerrainChars.Add(MapTerrain.ClosedLock, (char)((shroomWallStartRow + 1) * rowLength + 2));
            TerrainChars.Add(MapTerrain.OpenLock, (char)((shroomWallStartRow + 1) * rowLength + 3));

            TerrainChars.Add(MapTerrain.BrickWall1, (char)((shroomWallStartRow + 0) * rowLength + 0));
            TerrainChars.Add(MapTerrain.BrickWall2, (char)((shroomWallStartRow + 0) * rowLength + shroomWallSkip + 1));
            TerrainChars.Add(MapTerrain.BrickWall3, (char)((shroomWallStartRow + 0) * rowLength + shroomWallSkip + 2));
            TerrainChars.Add(MapTerrain.BrickWall4, (char)((shroomWallStartRow + 0) * rowLength + shroomWallSkip + 3));
            TerrainChars.Add(MapTerrain.BrickWall5, (char)((shroomWallStartRow + 0) * rowLength + shroomWallSkip + 4));
            TerrainChars.Add(MapTerrain.PanelWall1, (char)((shroomWallStartRow + 1) * rowLength + 0));
            TerrainChars.Add(MapTerrain.PanelWall2, (char)((shroomWallStartRow + 1) * rowLength + shroomWallSkip + 1));
            TerrainChars.Add(MapTerrain.PanelWall3, (char)((shroomWallStartRow + 1) * rowLength + shroomWallSkip + 2));
            TerrainChars.Add(MapTerrain.PanelWall4, (char)((shroomWallStartRow + 1) * rowLength + shroomWallSkip + 3));
            TerrainChars.Add(MapTerrain.PanelWall5, (char)((shroomWallStartRow + 1) * rowLength + shroomWallSkip + 4));
            TerrainChars.Add(MapTerrain.IrisWall1, (char)((shroomWallStartRow + 2) * rowLength + 0));
            TerrainChars.Add(MapTerrain.IrisWall2, (char)((shroomWallStartRow + 2) * rowLength + shroomWallSkip + 1));
            TerrainChars.Add(MapTerrain.IrisWall3, (char)((shroomWallStartRow + 2) * rowLength + shroomWallSkip + 2));
            TerrainChars.Add(MapTerrain.IrisWall4, (char)((shroomWallStartRow + 2) * rowLength + shroomWallSkip + 3));
            TerrainChars.Add(MapTerrain.IrisWall5, (char)((shroomWallStartRow + 2) * rowLength + shroomWallSkip + 4));
            TerrainChars.Add(MapTerrain.LineWall1, (char)((shroomWallStartRow + 3) * rowLength + 0));
            TerrainChars.Add(MapTerrain.LineWall2, (char)((shroomWallStartRow + 3) * rowLength + shroomWallSkip + 1));
            TerrainChars.Add(MapTerrain.LineWall3, (char)((shroomWallStartRow + 3) * rowLength + shroomWallSkip + 2));
            TerrainChars.Add(MapTerrain.LineWall4, (char)((shroomWallStartRow + 3) * rowLength + shroomWallSkip + 3));
            TerrainChars.Add(MapTerrain.LineWall5, (char)((shroomWallStartRow + 3) * rowLength + shroomWallSkip + 4));
            TerrainChars.Add(MapTerrain.SecurityWall1, (char)((shroomWallStartRow + 4) * rowLength + 0));
            TerrainChars.Add(MapTerrain.SecurityWall2, (char)((shroomWallStartRow + 4) * rowLength + shroomWallSkip + 1));
            TerrainChars.Add(MapTerrain.SecurityWall3, (char)((shroomWallStartRow + 4) * rowLength + shroomWallSkip + 2));
            TerrainChars.Add(MapTerrain.SecurityWall4, (char)((shroomWallStartRow + 4) * rowLength + shroomWallSkip + 3));
            TerrainChars.Add(MapTerrain.SecurityWall5, (char)((shroomWallStartRow + 4) * rowLength + shroomWallSkip + 4));
            TerrainChars.Add(MapTerrain.CutWall1, (char)((shroomWallStartRow + 5 ) * rowLength + 0));
            TerrainChars.Add(MapTerrain.CutWall2, (char)((shroomWallStartRow + 5) * rowLength + shroomWallSkip + 1));
            TerrainChars.Add(MapTerrain.CutWall3, (char)((shroomWallStartRow + 5) * rowLength + shroomWallSkip + 2));
            TerrainChars.Add(MapTerrain.CutWall4, (char)((shroomWallStartRow + 5) * rowLength + shroomWallSkip + 3));
            TerrainChars.Add(MapTerrain.CutWall5, (char)((shroomWallStartRow + 5) * rowLength + shroomWallSkip + 4));
            TerrainChars.Add(MapTerrain.DipWall1, (char)((shroomWallStartRow + 6) * rowLength + 0));
            TerrainChars.Add(MapTerrain.DipWall2, (char)((shroomWallStartRow + 6) * rowLength + shroomWallSkip + 1));
            TerrainChars.Add(MapTerrain.DipWall3, (char)((shroomWallStartRow + 6) * rowLength + shroomWallSkip + 2));
            TerrainChars.Add(MapTerrain.DipWall4, (char)((shroomWallStartRow + 6) * rowLength + shroomWallSkip + 3));
            TerrainChars.Add(MapTerrain.DipWall5, (char)((shroomWallStartRow + 6) * rowLength + shroomWallSkip + 4));
            TerrainChars.Add(MapTerrain.BioWall1, (char)((shroomWallStartRow + 7) * rowLength + 0));
            TerrainChars.Add(MapTerrain.BioWall2, (char)((shroomWallStartRow + 7) * rowLength + shroomWallSkip + 1));
            TerrainChars.Add(MapTerrain.BioWall3, (char)((shroomWallStartRow + 7) * rowLength + shroomWallSkip + 2));
            TerrainChars.Add(MapTerrain.BioWall4, (char)((shroomWallStartRow + 7) * rowLength + shroomWallSkip + 3));
            TerrainChars.Add(MapTerrain.BioWall5, (char)((shroomWallStartRow + 7) * rowLength + shroomWallSkip + 4));
        }

        private static void SetupTerrainColors()
        {
            TerrainColors.Add(MapTerrain.Empty, ColorPresets.Gray);
            TerrainColors.Add(MapTerrain.Wall, ColorPresets.DarkSlateGray);

            TerrainColors.Add(MapTerrain.NonWalkableFeature, ColorPresets.White);
            TerrainColors.Add(MapTerrain.NonWalkableFeatureLightBlocking, ColorPresets.White);

            TerrainColors.Add(MapTerrain.HellWall, ColorPresets.Crimson);
            TerrainColors.Add(MapTerrain.Corridor, ColorPresets.Gray);
            TerrainColors.Add(MapTerrain.SkeletonWall, ColorPresets.BlanchedAlmond);
            TerrainColors.Add(MapTerrain.SkeletonWallWhite, ColorPresets.GhostWhite);
            TerrainColors.Add(MapTerrain.ClosedDoor, ColorPresets.DarkGoldenrod);
            TerrainColors.Add(MapTerrain.OpenDoor, ColorPresets.DarkGoldenrod);
            TerrainColors.Add(MapTerrain.ClosedLock, ColorPresets.DarkGoldenrod);
            TerrainColors.Add(MapTerrain.OpenLock, ColorPresets.DarkGoldenrod);
            TerrainColors.Add(MapTerrain.Flooded, ColorPresets.Blue);
            TerrainColors.Add(MapTerrain.Grass, ColorPresets.Green);
            TerrainColors.Add(MapTerrain.River, ColorPresets.Blue);
            TerrainColors.Add(MapTerrain.Trees, ColorPresets.DarkGreen);
            TerrainColors.Add(MapTerrain.Road, ColorPresets.DarkGoldenrod);
            TerrainColors.Add(MapTerrain.Rubble, ColorPresets.Gray);
            TerrainColors.Add(MapTerrain.Mountains, ColorPresets.Gray);
            TerrainColors.Add(MapTerrain.Volcano, ColorPresets.Red);
            TerrainColors.Add(MapTerrain.Forest, ColorPresets.DarkSeaGreen);
            TerrainColors.Add(MapTerrain.Gravestone, ColorPresets.Gray);
            TerrainColors.Add(MapTerrain.BarDoor, ColorPresets.Gray);
            TerrainColors.Add(MapTerrain.DockWall, ColorPresets.DarkSlateGray);
            TerrainColors.Add(MapTerrain.Void, ColorPresets.Gray);

            var brickWallColor = ColorPresets.DarkSlateBlue;
            var panelWallColor = ColorPresets.Teal;
            var irisWallColor = ColorPresets.SlateGray;
            var lineWallColor = ColorPresets.MediumSeaGreen;
            var securityWallColor = ColorPresets.SteelBlue;
            var bioWallColor = ColorPresets.Firebrick;
            var DipWallColor = ColorPresets.DarkOliveGreen;
            var CutWallColor = ColorPresets.Sienna;

            TerrainColors.Add(MapTerrain.BrickWall1, brickWallColor);
            TerrainColors.Add(MapTerrain.BrickWall2, brickWallColor);
            TerrainColors.Add(MapTerrain.BrickWall3, brickWallColor);
            TerrainColors.Add(MapTerrain.BrickWall4, brickWallColor);
            TerrainColors.Add(MapTerrain.BrickWall5, brickWallColor);
            TerrainColors.Add(MapTerrain.PanelWall1, panelWallColor);
            TerrainColors.Add(MapTerrain.PanelWall2, panelWallColor);
            TerrainColors.Add(MapTerrain.PanelWall3, panelWallColor);
            TerrainColors.Add(MapTerrain.PanelWall4, panelWallColor);
            TerrainColors.Add(MapTerrain.PanelWall5, panelWallColor);
            TerrainColors.Add(MapTerrain.IrisWall1, irisWallColor);
            TerrainColors.Add(MapTerrain.IrisWall2, irisWallColor);
            TerrainColors.Add(MapTerrain.IrisWall3, irisWallColor);
            TerrainColors.Add(MapTerrain.IrisWall4, irisWallColor);
            TerrainColors.Add(MapTerrain.IrisWall5, irisWallColor);
            TerrainColors.Add(MapTerrain.LineWall1, lineWallColor);
            TerrainColors.Add(MapTerrain.LineWall2, lineWallColor);
            TerrainColors.Add(MapTerrain.LineWall3, lineWallColor);
            TerrainColors.Add(MapTerrain.LineWall4, lineWallColor);
            TerrainColors.Add(MapTerrain.LineWall5, lineWallColor);
            TerrainColors.Add(MapTerrain.SecurityWall1, securityWallColor);
            TerrainColors.Add(MapTerrain.SecurityWall2, securityWallColor);
            TerrainColors.Add(MapTerrain.SecurityWall3, securityWallColor);
            TerrainColors.Add(MapTerrain.SecurityWall4, securityWallColor);
            TerrainColors.Add(MapTerrain.SecurityWall5, securityWallColor);
            TerrainColors.Add(MapTerrain.BioWall1, bioWallColor);
            TerrainColors.Add(MapTerrain.BioWall2, bioWallColor);
            TerrainColors.Add(MapTerrain.BioWall3, bioWallColor);
            TerrainColors.Add(MapTerrain.BioWall4, bioWallColor);
            TerrainColors.Add(MapTerrain.BioWall5, bioWallColor);
            TerrainColors.Add(MapTerrain.DipWall1, DipWallColor);
            TerrainColors.Add(MapTerrain.DipWall2, DipWallColor);
            TerrainColors.Add(MapTerrain.DipWall3, DipWallColor);
            TerrainColors.Add(MapTerrain.DipWall4, DipWallColor);
            TerrainColors.Add(MapTerrain.DipWall5, DipWallColor);
            TerrainColors.Add(MapTerrain.CutWall1, CutWallColor);
            TerrainColors.Add(MapTerrain.CutWall2, CutWallColor);
            TerrainColors.Add(MapTerrain.CutWall3, CutWallColor);
            TerrainColors.Add(MapTerrain.CutWall4, CutWallColor);
            TerrainColors.Add(MapTerrain.CutWall5, CutWallColor);
        }

        private static void SetupEquipmentSlots()
        {
            EquipmentSlots.Add(EquipmentSlot.Utility, "Body");
            EquipmentSlots.Add(EquipmentSlot.Weapon, "Right Hand");
        }
    }
}
