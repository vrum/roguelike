﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Creatures
{
    /// <summary>
    /// Slow but tough
    /// </summary>
    public class OgreUnique : MonsterFightAndRunAI
    {
        const int classDeltaHitpoints = 15;
        const int classMinHitpoints = 35;

        public string UniqueName { get; set; }

        public OgreUnique()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
            Unique = true;

            UniqueName = "Grug the ogre";

        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        public override int BaseSpeed()
        {
            return 80;
        }
        
        protected override int ClassMaxHitpoints()
        {
            return classMinHitpoints + Game.Random.Next(classDeltaHitpoints) + 1;
        }

        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return 8;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return 6;
        }

        /// <summary>
        /// Creature damage modifier.  Set by type of creature.
        /// </summary>
        public override int DamageModifier()
        {
            return 1;
        }

        public override int HitModifier()
        {
            return 3;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "ogre"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription { get { return "ogres"; } }

        protected override char GetRepresentation()
        {
            return 'G';
        }

        protected override int GetChanceToRecover()
        {
            return 10;
        }

        protected override int GetChanceToFlee()
        {
            return 0;
        }

        protected override int GetMaxHPWillFlee()
        {
            return Hitpoints;
        }

        public override int CreatureCost()
        {
            return 40;
        }

        public override int CreatureLevel()
        {
            return 3;
        }

        public  override  Monster NewCreatureOfThisType()
        {
            return new Ogre();
        }

        public override Color RepresentationColor()
        {
            return ColorPresets.Brown;
        }

        public override int GetCombatXP()
        {
            return 60;
        }

        public override int GetMagicXP()
        {
            return 60;
        }

        public override int GetMagicRes()
        {
            return 0;
        }

        public override int GetCharmRes()
        {
            return 30;
        }

        public override bool CanBeCharmed()
        {
            return false;
        }
    }
}
