﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{


    /// <summary>
    /// An object store. Can be on a container, creature or player
    /// </summary>
    public class Inventory
    {
        List<Item> items;

        List<InventoryListing> inventoryListing;

        int totalWeight = 0;

        public Inventory()
        {
            items = new List<Item>();

            inventoryListing = new List<InventoryListing>();
        }

        /// <summary>
        /// Add an item to the inventory. The item will be marked as 'in inventory' so not displayed on the world map
        /// </summary>
        /// <param name="itemToAdd"></param>
        public void AddItem(Item itemToAdd) {
            
            itemToAdd.InInventory = true;

            items.Add(itemToAdd);

            totalWeight += itemToAdd.GetWeight();

            //Refresh the listing
            RefreshInventoryListing();
        }

        /// <summary>
        /// Removes an item from the inventory. Does NOT set InInventory = false. This should be done by the object that possesses the inventory (so it can update the position correctly)
        /// </summary>
        /// <param name="itemToRemove"></param>
        public void RemoveItem(Item itemToRemove)
        {
            items.Remove(itemToRemove);

            totalWeight -= itemToRemove.GetWeight();

            //Refresh the listing
            RefreshInventoryListing();
        }

        /// <summary>
        /// Update the listing groups
        /// </summary>
        public void RefreshInventoryListing()
        {
            //List of groups of similar items
            inventoryListing.Clear();

            //Group similar items (based on type) into categories (InventoryListing)
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];

                //Check if we have a similar item group already. If so, add the index of this item to that group
                //Equipped items are not stacked

                bool foundGroup = false;
                
                if (!item.IsEquipped)
                {
                    foreach (InventoryListing group in inventoryListing)
                    {
                        //Check that we are the same type (and therefore sort of item)
                        Type itemType = item.GetType();

                        //Look only at the first item in the group (stored by index). All the items in this group must have the same type
                        if (items[group.ItemIndex[0]].GetType() == item.GetType() && !items[group.ItemIndex[0]].IsEquipped)
                        {
                            group.ItemIndex.Add(i);
                            foundGroup = true;
                            break;
                        }
                    }
                }

                //If there is no group, create a new one
                if (!foundGroup)
                {
                    InventoryListing newGroup = new InventoryListing(this);
                    newGroup.ItemIndex.Add(i);
                    inventoryListing.Add(newGroup);
                }
            }
        }

        /// <summary>
        /// Items in the inventory
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
        /// Listing of the inventory, suitable for the user
        /// </summary>
        public List<InventoryListing> InventoryListing
        {
            get
            {
                return inventoryListing;
            }
            set
            {
                InventoryListing = value;
            }
        }


        public int TotalWeight
        {
            get
            {
                return totalWeight;
            }
            //For serialization
            set
            {
                totalWeight = value;
            }
        }
    }
}
