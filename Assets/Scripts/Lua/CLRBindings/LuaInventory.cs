using System.Collections.Generic;

public class LuaInventory {
    public string GetItem(int index) {
        if (index > Inventory.inventory.Count) {
            UnitaleUtil.DisplayLuaError("Getting an item", "Out of bounds. You tried to access item number " + index + 1 + " in your inventory, but you only have " + Inventory.inventory.Count + " items.");
            return "";
        }
        return Inventory.inventory[index-1].Name;
    }

    public int GetType(int index) {
        if (index > Inventory.inventory.Count) {
            UnitaleUtil.DisplayLuaError("Getting an item", "Out of bounds. You tried to access item number " + index + 1 + " in your inventory, but you only have " + Inventory.inventory.Count + " items.");
            return -1;
        }
        return Inventory.inventory[index-1].Type;
    }

    public void SetItem(int index, string Name) { Inventory.SetItem(index-1, Name); }

    public bool AddItem(string Name, int index = -1) {
        if (Name == null)
            throw new CYFException("Inventory.AddItem: The first argument (item name) is nil.\n\nSee the documentation for proper usage.");
        if (index == -1)
            return Inventory.AddItem(Name);
        else if (index > 0 && Inventory.inventory.Count < Inventory.inventorySize) {
            if (index > Inventory.inventory.Count + 1)
                index = Inventory.inventory.Count + 1;

            List<UnderItem> inv = new List<UnderItem>();
            bool result = false;
            for (var i = 0; i <= Inventory.inventory.Count; i++) {
                if (i == index - 1) {
                    // Make sure that the item exists before trying to create it
                    string outString = "";
                    int outInt       =  0;
                    if (!Inventory.addedItems.Contains(Name) && !Inventory.NametoDesc.TryGetValue(Name, out outString) &&
                        !Inventory.NametoShortName.TryGetValue(Name, out outString) && !Inventory.NametoType.TryGetValue(Name, out outInt) &&
                        !Inventory.NametoPrice.TryGetValue(Name, out outInt))
                        throw new CYFException("Inventory.AddItem: The item \"" + Name + "\" was not found.\n\nAre you sure you called Inventory.AddCustomItems first?");
                    inv.Add(new UnderItem(Name));
                    result = true;
                }
                if (i == Inventory.inventory.Count)
                    break;
                inv.Add(Inventory.inventory[i]);
            }
            Inventory.inventory = inv;
            return result;
        }
        return false;
    }

    public void RemoveItem(int index) {
        if (Inventory.inventory.Count > 0 && (index < 1 || index > Inventory.inventory.Count))
            UnitaleUtil.DisplayLuaError("Removing an item", "Cannot remove item number " + index + " from an Inventory with " + Inventory.inventory.Count
                + " items.\nRemember that the first item in the inventory is #1.");
        else if (Inventory.inventory.Count == 0)
            UnitaleUtil.DisplayLuaError("Removing an item", "Cannot remove an item when the inventory is empty.");

        Inventory.inventory.RemoveAt(index-1);
    }

    public void AddCustomItems(string[] names, int[] types) {
        if (names == null)
            throw new CYFException("Inventory.AddCustomItems: The first argument (list of item names) is nil.\n\nSee the documentation for proper usage.");
        else if (types == null)
            throw new CYFException("Inventory.AddCustomItems: The second argument (list of item types) is nil.\n\nSee the documentation for proper usage.");
        else if (names.Length != types.Length)
            throw new CYFException("Inventory.AddCustomItems: The second argument (list of item types) is not the same length as the first argument (list of item names).\n\nSee the documentation for proper usage.");
        Inventory.addedItems.AddRange(names);
        Inventory.addedItemsTypes.AddRange(types);
    }

    public void SetInventory(string[] names) {
        if (names == null)
            throw new CYFException("Inventory.SetInventory: Attempt to set the player's inventory to nil.\n\nSee the documentation for proper usage.");
        Inventory.SetItemList(names);
    }

    public int ItemCount {
        get { return Inventory.inventory.Count; }
    }

    public bool NoDelete {
        get { return Inventory.usedItemNoDelete; }
        set { Inventory.usedItemNoDelete = value; }
    }

    public void SetAmount(int amount) {
        Inventory.tempAmount = amount;
    }
}