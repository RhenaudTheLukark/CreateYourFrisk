using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MoonSharp.Interpreter;

public class LuaInventoryOW {

    [MoonSharpHidden]
    public LuaInventoryOW() { }
    
    public delegate void LoadedAction(string name, object args);
    [MoonSharpHidden]
    public static event LoadedAction StCoroutine;

    [MoonSharpHidden]
    public void setEquip(string itemName)                   { Inventory.ChangeEquipment(itemName); }
    [CYFEventFunction] public void SetWeapon(string weapon) { setEquip(weapon); }
    [CYFEventFunction] public void SetArmor(string armor)   { setEquip(armor); }

    [CYFEventFunction]
    public bool AddItem(string Name) {
        try { return Inventory.AddItem(Name); } 
        finally { EventManager.instance.script.Call("CYFEventNextCommand"); }
    }

    [CYFEventFunction]
    public void RemoveItem(int ID) { Inventory.RemoveItem(ID - 1); EventManager.instance.script.Call("CYFEventNextCommand"); }

    public bool IsItemInTheInventory(string name) { return Inventory.isInInventory(name); }

    public bool ItemExists(string name) { return Inventory.itemExists(name); }

    public int GetItemID(string name) {
        if (!Inventory.itemExists(name))     return -1;
        if (!Inventory.isInInventory(name))  return -1;
        return Inventory.container.IndexOf(new UnderItem(name, Inventory.NametoType[name]));
    }
}
