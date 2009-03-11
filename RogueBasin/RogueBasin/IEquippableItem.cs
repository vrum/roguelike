﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// An item which can be equipped. Currently this is inherited off Item which has the Use() method. In future I might made Equippable and Useable interfaces
    /// </summary>
    public interface IEquippableItem
    {
        /// <summary>
        /// Returns true if this object can be equipped in the slot specified
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        bool CanBeEquippedInSlot(EquipmentSlot slot);

        /// <summary>
        /// Returns a list of possible equipment slots that the item can be equipped in
        /// </summary>
        /// <returns></returns>
        List<EquipmentSlot> EquipmentSlots
        {
            get;
        }

        /// <summary>
        /// Apply the equipped effect to the user. Returns true on successfully equipped. May want to consider a hooking interface as well (events).
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
       bool Equip(Creature user);

        /// <summary>
        /// Unequip the object and remove its effect from the user. Returns true on successfully unequipped.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool UnEquip(Creature user);

        /// <summary>
        /// AC modifier +1 -1 etc. 0 if none.
        /// </summary>
        /// <returns></returns>
        int ArmourClassModifier();

        /// <summary>
        /// Damage base 1d(return value). Highest one will be picked of all equipped items. 0 if not a weapon type.
        /// </summary>
        /// <returns></returns>
        int DamageBase();

        /// <summary>
        /// Damage modifier +1 -1 etc. 0 if none.
        /// </summary>
        /// <returns></returns>
        int DamageModifier();

        /// <summary>
        /// Hit modifier +1 -1 etc. 0 if none.
        /// </summary>
        /// <returns></returns>
        int HitModifier();


    }
}
