using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MoonSharp.Interpreter;

public class LuaInventoryOW {
    public ScriptWrapper appliedScript;

    [MoonSharpHidden] public LuaInventoryOW() { }

    public delegate void LoadedAction(string coroName, object args, string evName);
    [MoonSharpHidden] public static event LoadedAction StCoroutine;

    [MoonSharpHidden] public void SetEquip(string itemName) {
        if (!Inventory.ItemExists(itemName))
            throw new CYFException("The item \"" + itemName + "\" doesn't exist in the item list.");
        if (Inventory.InventoryNumber(itemName) == -1)
            throw new CYFException("You can't equip an item that isn't in the inventory.");
        Inventory.ChangeEquipment(Inventory.InventoryNumber(itemName));
        appliedScript.Call("CYFEventNextCommand");
    }

    [CYFEventFunction] public void SetWeapon(string weapon) { SetEquip(weapon); }
    [CYFEventFunction] public void SetArmor(string armor)   { SetEquip(armor); }

    [CYFEventFunction] public bool AddItem(string Name) {
        try { return Inventory.AddItem(Name); }
        finally { appliedScript.Call("CYFEventNextCommand"); }
    }

    [CYFEventFunction] public void RemoveItem(int ID) { Inventory.RemoveItem(ID - 1); appliedScript.Call("CYFEventNextCommand"); }

    [CYFEventFunction] public bool IsItemInTheInventory(string name) { try { return Inventory.InventoryNumber(name) != -1; } finally { appliedScript.Call("CYFEventNextCommand"); } }

    [CYFEventFunction] public bool ItemExists(string name) { try { return Inventory.ItemExists(name); } finally { appliedScript.Call("CYFEventNextCommand"); } }

    [CYFEventFunction] public int GetItemID(string name) { try { return Inventory.InventoryNumber(name); } finally { appliedScript.Call("CYFEventNextCommand"); } }

    [CYFEventFunction] public int GetItemCount() { try { return Inventory.inventory.Count; } finally { appliedScript.Call("CYFEventNextCommand"); } }

    [CYFEventFunction] public void SpawnBoxMenu() { StCoroutine("ISpawnBoxMenu", null, appliedScript.GetVar("_internalScriptName").String); }
}