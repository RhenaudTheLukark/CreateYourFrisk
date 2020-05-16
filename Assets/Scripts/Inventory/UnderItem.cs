/// <summary>
/// Class for ingame items. Used to create TestDog# items. But now...
/// </summary>
public class UnderItem {
    //private static int dogNumber = 1;

    public UnderItem(string Name) {
        //Let's end this dog tyranny!
        //ID = "DOGTEST" + dogNumber;
        //ShortName = "TestDog" + dogNumber;
        //dogNumber++;
        foreach (string str in Inventory.addedItems) {
            if (str.ToLower() == Name.ToLower()) {
                this.Name = Name;
                string Short = "";
                if (!Inventory.NametoShortName.TryGetValue(Name, out Short))
                    ShortName = Name;
                else
                    ShortName = Short;
                Type = Inventory.GetItemType(Name);
                return;
            }
        }

        if (Inventory.NametoDesc.Keys.Count == 0) {
            Inventory.luaInventory = new LuaInventory();
            Inventory.AddItemsToDictionaries();
        }

        this.Name = Name; string Sn = "", Desc = ""; int Ty = Type;
        if (!Inventory.NametoDesc.TryGetValue(Name, out Desc))     UnitaleUtil.DisplayLuaError("Creating an item", "Tried to create the item \"" + Name + "\", but a set description for it was not found.");
        if (!Inventory.NametoShortName.TryGetValue(Name, out Sn))  Sn = Name;
        if (Type == 0)                                             Inventory.NametoType.TryGetValue(Name, out Ty);

        ShortName = Sn; Description = Desc; Type = Ty;
    }

    public string Name { get; private set; }
    public string ShortName { get; private set; }
    public string Description { get; private set; }
    public int Type { get; private set; } //0 = normal, 1 = equipATK, 2 = equipDEF, 3 = special
}