﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    class MapGeneratorCave
    {
        Map baseMap;

        /// <summary>
        /// Chance of digging surrounding squares
        /// </summary>
        public int DiggingChance { get; set; }
        /// <summary>
        /// When connecting stairs, the chance to expand the corridor
        /// </summary>
        public int MineChance { get; set; }
        /// <summary>
        /// How much of the level must be open (not necessarily connected)
        /// </summary>
        public double PercOpenRequired { get; set; }
        /// <summary>
        /// How far away the stairs are guaranteed to be
        /// </summary>
        public double RequiredStairDistance { get; set; }

        public int Width {get; set;}
        public int Height {get; set;}

        Point upStaircase;
        Point downStaircase;

        public MapGeneratorCave()
        {

        }

        public Map GenerateMap() {

            if (Width < 1 || Height < 1)
            {
                LogFile.Log.LogEntry("Can't make 0 dimension map");
                throw new ApplicationException("Can't make with 0 dimension");
            }

            DiggingChance = 20;
            MineChance = 15;
            PercOpenRequired = 0.4;
            RequiredStairDistance = 40;

            baseMap = new Map(Width, Height);

            //Fill map with walls
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    SetSquareClosed(i, j);
                }
            }

            //Start digging from a random point
            int noDiggingPoints = 4 + Game.Random.Next(4);

            for (int i = 0; i < noDiggingPoints; i++)
            {
                int x = Game.Random.Next(Width);
                int y = Game.Random.Next(Height);

                //Don't dig right to the edge
                if (x == 0)
                    x = 1;
                if (x == Width - 1)
                    x = Width - 2;
                if (y == 0)
                    y = 1;
                if (y == Height - 1)
                    y = Height - 2;

                Dig(x, y);
            }

            //Check if we are too small, and add more digs
            while (CalculatePercentageOpen() < PercOpenRequired)
            {
                int x = Game.Random.Next(Width);
                int y = Game.Random.Next(Height);

                //Don't dig right to the edge
                if (x == 0)
                    x = 1;
                if (x == Width - 1)
                    x = Width - 2;
                if (y == 0)
                    y = 1;
                if (y == Height - 1)
                    y = Height - 2;

                Dig(x, y);
            }

            //Find places for the stairs

            double stairDistance;

            do
            {
                upStaircase = RandomPoint();
                downStaircase = RandomPoint();

                stairDistance = Math.Sqrt(Math.Pow(upStaircase.x - downStaircase.x, 2) + Math.Pow(upStaircase.y - downStaircase.y, 2));

            } while (stairDistance < RequiredStairDistance);
            //Ensure the stairs are connected
            ConnectPoints(upStaircase, downStaircase);

            //Screen.Instance.DrawMapDebug(baseMap);

            return baseMap;
        }

        private void SetSquareClosed(int i, int j)
        {
            baseMap.mapSquares[i, j].Terrain = MapTerrain.Wall;
            baseMap.mapSquares[i, j].BlocksLight = true;
            baseMap.mapSquares[i, j].Walkable = false;
        }

        private void SetSquareOpen(int i, int j)
        {
            baseMap.mapSquares[i, j].Terrain = MapTerrain.Empty;
            baseMap.mapSquares[i, j].BlocksLight = false;
            baseMap.mapSquares[i, j].Walkable = true;
        }

        private double CalculatePercentageOpen()
        {
            int totalOpen = 0;

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (baseMap.mapSquares[i, j].Terrain == MapTerrain.Empty)
                        totalOpen++;
                }
            }

            double percOpen = totalOpen / (double)(Width * Height);

            return percOpen;
        }

        private void ConnectPoints(Point upStairsPoint, Point downStairsPoint)
        {
            //First check if the stairs are connected... 

            //Build tcodmap
            TCODFov tcodMap = new TCODFov(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    tcodMap.SetCell(i, j, !baseMap.mapSquares[i, j].BlocksLight, baseMap.mapSquares[i, j].Walkable);
                }
            }

            //Try to walk the path between the 2 staircases
            TCODPathFinding path = new TCODPathFinding(tcodMap, 1.0);
            path.ComputePath(upStairsPoint.x, upStairsPoint.y, downStairsPoint.x, downStairsPoint.y);

            //Find the first step. We need to load x and y with the origin of the path
            int x = upStaircase.x;
            int y = upStaircase.y;

            bool obstacleHit = false;

            //If there's no routeable path
            if (path.IsPathEmpty())
            {
                obstacleHit = true;
            }

            //We are done with tcod
            path.Dispose();
            tcodMap.Dispose();

            //If we managed to get there OK, return
            if (obstacleHit == false)
                return;

            //If not, open a path between the staircases

            TCODLineDrawing.InitLine(upStairsPoint.x, upStairsPoint.y, downStairsPoint.x, downStairsPoint.y);

            int nextX = upStairsPoint.x;
            int nextY = upStairsPoint.y;

            Random rand = Game.Random;

            do
            {
                SetSquareOpen(nextX, nextY);

                //Chance surrounding squares also get done
                if (nextX - 1 > 0 && nextY - 1 > 0)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX - 1, nextY - 1);
                    }
                }

                if (nextY - 1 > 0)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX, nextY - 1);
                    }
                }

                if (nextX + 1 < Width && nextY - 1 > 0)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX + 1, nextY - 1);
                    }
                }


                if (nextX - 1 > 0)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX - 1, nextY);
                    }
                }

                if (nextX + 1 < Width)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX + 1, nextY);
                    }
                }

                if (nextX - 1 > 0 && nextY + 1 < Height)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX - 1, nextY + 1);
                    }
                }

                if (nextY + 1 < Height)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX, nextY + 1);
                    }
                }

                if (nextX + 1 < Width && nextY + 1 < Height)
                {
                    if (rand.Next(100) < MineChance)
                    {
                        SetSquareOpen(nextX + 1, nextY + 1);
                    }
                }

            } while (!TCODLineDrawing.StepLine(ref nextX, ref nextY));

        }
    
        private Point RandomPoint()
        {
            do
            {
                int x = Game.Random.Next(Width);
                int y = Game.Random.Next(Height);

                if (baseMap.mapSquares[x, y].Terrain == MapTerrain.Empty)
                {
                    return new Point(x, y);
                }
            }
            while (true);
        }

        public void Dig(int x, int y)
        {
            //Check this is a valid square to dig
            if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                return;

            //Already dug
            if (baseMap.mapSquares[x, y].Terrain == MapTerrain.Empty)
                return;

            //Set this as open
            SetSquareOpen(x, y);

            //Did in all the directions

            Random rand = Game.Random;

            //TL
            if (rand.Next(100) < DiggingChance)
                Dig(x - 1, y - 1);
            //T
            if (rand.Next(100) < DiggingChance)
                Dig(x, y - 1);
            //TR
            if (rand.Next(100) < DiggingChance)
                Dig(x + 1, y - 1);
            //CL
            if (rand.Next(100) < DiggingChance)
                Dig(x - 1, y);
            //CR
            if (rand.Next(100) < DiggingChance)
                Dig(x + 1, y);
            //BL
            if (rand.Next(100) < DiggingChance)
                Dig(x - 1, y + 1);
            //B
            if (rand.Next(100) < DiggingChance)
                Dig(x, y + 1);
            //BR
            if (rand.Next(100) < DiggingChance)
                Dig(x + 1, y + 1);
        }

        /// <summary>
        /// Add staircases to the map once it has been added to dungeon
        /// </summary>
        /// <param name="levelNo"></param>
        public void AddStaircases(int levelNo) {

            Game.Dungeon.AddFeature(new Features.StaircaseUp(), levelNo, upStaircase);
            Game.Dungeon.AddFeature(new Features.StaircaseDown(), levelNo, downStaircase);
        }
    }
}