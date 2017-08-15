public class LuaInventory {
    public string GetItem(int index) {
        if (index > Inventory.inventory.Count) {
            UnitaleUtil.DisplayLuaError("Getting an item", "Out of bounds. You tried to access the item n°" + index + 1 + " of your inventory, but you only have " + Inventory.inventory.Count + " items.");
            return "";
        }
        return Inventory.inventory[index-1].Name;
    }

    public void SetItem(int index, string Name) { Inventory.SetItem(index-1, Name); }

    public bool AddItem(string Name) { return Inventory.AddItem(Name); }

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