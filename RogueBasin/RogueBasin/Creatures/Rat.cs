﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Creatures
{
    public class Rat : MonsterSimpleAI
    {
        const int classMaxHitpoints = 10;

        public Rat()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
        }

        protected override int ClassMaxHitpoints()
        {
            return classMaxHitpoints;
        }

        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return 10;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return 2;
        }

        /// <summary>
        /// Creature damage modifier.  Set by type of creature.
        /// </summary>
        public override int DamageModifier()
        {
            return 0;
        }

        public override int HitModifier()
        {
            return 0;
        }
    }
}
