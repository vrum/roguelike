﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Base class for Creatures.
    /// </summary>
    public abstract class Creature
    {
        /// <summary>
        /// Level the creature is on
        /// </summary>
        int locationLevel;

        /// <summary>
        /// Point on the map on this level that the creature is on
        /// </summary>
        Point locationMap;

        /// <summary>
        /// ASCII character
        /// </summary>
        char representation;

        /// <summary>
        /// Is the creature still alive?
        /// </summary>
        bool alive;

        /// <summary>
        /// Increment each game turn for the creature's internal clock. Turn at turnClockLimit
        /// </summary>
        protected int speed = 10;

        /// <summary>
        /// Current turn clock value for the creature. When 1000 the creature takes a turn
        /// </summary>
        protected int turnClock = 0;

        /// <summary>
        /// How much the turn clock has to reach to process
        /// </summary>
        protected const int turnClockLimit = 1000;

        public int LocationLevel
        {
            get
            {
                return locationLevel;
            }
            set
            {
                locationLevel = value;
            }
        }

        public Point LocationMap
        {
            get
            {
                return locationMap;
            }
            set
            {
                locationMap = value;
            }
        }

        public char Representation
        {
            get
            {
                return representation;
            }
            set
            {
                representation = value;
            }
        }

        public int Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }

        /// <summary>
        /// Set to false to kill the creature
        /// </summary>
        public bool Alive
        {
            get
            {
                return alive;
            }
            set
            {
                alive = value;
            }
        }

        public Creature()
        {
            alive = true;
        }

        /// <summary>
        /// Increment the internal turn timer and resets if over boundary. Return true if a turn should be had.
        /// </summary>
        internal virtual bool IncrementTurnTime()
        {
            turnClock += speed;

            if (turnClock >= turnClockLimit)
            {
                turnClock -= turnClockLimit;

                return true;
            }
            else return false;
        }
    }
}