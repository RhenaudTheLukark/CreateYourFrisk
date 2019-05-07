using System.Collections.Generic;

public static class ItemBox {
    public static int capacity = 10;
    public static List<UnderItem> items = new List<UnderItem>();

    public static void AddToBox(string name) {
        if (!Inventory.ItemExists(name)) {
            UnitaleUtil.WriteInLogAndDebugger("The item " + name + "doesn't exist in CYF's item database.");
            return;
        }
        if (items.Count == capacity) {
            UnitaleUtil.WriteInLogAndDebugger("The box is already full! You can't add another item to it!");
            return;
        }
        items.Add(new UnderItem(name));
    }

    public static void RemoveFromBox(int index) {
        if (index < 0 || index >= items.Count) {
            UnitaleUtil.WriteInLogAndDebugger("Tried to remove the item #" + index + " of the box, however it only has " + items.Count + " items in it, starting from the index 0.");
            return;
        }
        items.RemoveAt(index);
    }
}