﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Magic library
    /// </summary>
    public class Mission5Entry : DungeonSquareTrigger
    {

        public Mission5Entry()
        {

        }

        public override bool CheckTrigger(int level, Point mapLocation)
        {
            //Check we are in the right place - should be in the base I think
            if (CheckLocation(level, mapLocation) == false)
            {
                return false;
            }

            //Have we triggered already?

            if (Triggered)
                return false;

            //Mission 2 tutorial
            if (Game.Dungeon.Player.PlayItemMovies)
            {
                Screen.Instance.PlayMovie("mission5", true);
            }

            Triggered = true;

            return true;
        }
    }
}
