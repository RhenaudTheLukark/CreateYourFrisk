public class LuaInventory {
    public string GetItem(int index) {
        if (index > Inventory.inventory.Count) {
            UnitaleUtil.DisplayLuaError("Getting an item", "Out of bounds. You tried to access item #" + index + 1 + " in your inventory, but you only have " + Inventory.inventory.Count + " items.");
            return "";
        }
        return Inventory.inventory[index-1].Name;
    }
    
    public int GetType(int index) {
        if (index > Inventory.inventory.Count) {
            UnitaleUtil.DisplayLuaError("Getting an item", "Out of bounds. You tried to access item #" + index + 1 + " in your inventory, but you only have " + Inventory.inventory.Count + " items.");
            return -1;
        }
        return Inventory.inventory[index-1].Type;
    }

    public void SetItem(int index, string Name) { Inventory.SetItem(index-1, Name); }

    public bool AddItem(string Name) { return Inventory.AddItem(Name); }
    
    public void RemoveItem(int index) {
        if (Inventory.inventory.Count > 0 && (index < 1 || index > Inventory.inventory.Count))
            UnitaleUtil.DisplayLuaError("Removing an item", "Cannot remove item #" + index + " from an Inventory with " + Inventory.inventory.Count
                + " items.\nRemember that the first item in the inventory is #1.");
        else if (Inventory.inventory.Count == 0)
            UnitaleUtil.DisplayLuaError("Removing an item", "Cannot remove an item when the inventory is empty.");
        
        Inventory.inventory.RemoveAt(index-1);
    }

    public void AddCustomItems(string[] names, int[] types) { Inventory.addedItems = names; Inventory.addedItemsTypes = types; }

    public void SetInventory(string[] names) { Inventory.SetItemList(names); }

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