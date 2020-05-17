using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;

public class ItemBoxUI : MonoBehaviour {
    private List<TextManager> inventory = new List<TextManager>();
    private List<LuaSpriteController> inventorySprites = new List<LuaSpriteController>();
    private List<TextManager> boxContents = new List<TextManager>();
    private List<LuaSpriteController> boxContentsSprites = new List<LuaSpriteController>();

    private GameObject player;

    private bool inventoryColumn = true;
    private int lineIndex = 0;

    public static bool active = false;

    void Start() {
        if (Inventory.inventory.Count == 0 && ItemBox.items.Count == 0) {
            System.Random rnd = new System.Random();
            string[] words = new string[] { "effort", "time", "feeling" };

            Table text = new Table(EventManager.instance.luainvow.appliedScript.script);
            text.Set(DynValue.NewNumber(1), DynValue.NewString("You have no items.[w:10]\nYou put a little " + words[rnd.Next(0, 3)] + "\rinto the box."));

            EventManager.instance.luagenow.SetDialog(DynValue.NewTable(text));
            Destroy(this);
            return;
        }

        active = true;

        GetComponent<Image>().color = new Color(1, 1, 1, 1);

        player = GameObject.Find("utHeartMenu");
        Color c = player.GetComponent<Image>().color;
        player.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1);

        for (int i = 0; i < Inventory.inventorySize; i++) {
            GameObject go = new GameObject();
            go.AddComponent<RectTransform>();
            go.AddComponent<TextManager>();
            TextManager tm = go.GetComponent<TextManager>();
            tm.transform.SetParent(transform);
            tm.transform.localPosition = new Vector3(80, 410 - (i * 32));
            inventory.Add(tm);

            LuaSpriteController sprite = (LuaSpriteController) (SpriteUtil.MakeIngameSprite("px", -1).UserData.Object);
            sprite._img.transform.SetParent(transform);
            sprite.SetPivot(0, 0.5f);
            sprite.MoveToAbs(Misc.cameraX + 92, Misc.cameraY + 386 - (i * 32));
            sprite.xscale = 190;
            sprite.color = new float[] { 1f, 0f, 0f };
            inventorySprites.Add(sprite);
        }

        for (int i = 0; i < ItemBox.capacity; i++) {
            GameObject go = new GameObject();
            go.AddComponent<RectTransform>();
            go.AddComponent<TextManager>();
            TextManager tm = go.GetComponent<TextManager>();
            tm.transform.SetParent(transform);
            tm.transform.localPosition = new Vector3(372, 410 - (i * 32));
            boxContents.Add(tm);

            LuaSpriteController sprite = (LuaSpriteController) (SpriteUtil.MakeIngameSprite("px", -1).UserData.Object);
            sprite._img.transform.SetParent(transform);
            sprite.SetPivot(0, 0.5f);
            sprite.MoveToAbs(Misc.cameraX + 384, Misc.cameraY + 386 - (i * 32));
            sprite.xscale = 190;
            sprite.color = new float[] { 1f, 0f, 0f };
            boxContentsSprites.Add(sprite);
        }
        RefreshDisplay();
    }

    // Update is called once per frame
    void Update() {
        if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED) {
            List<UnderItem> selectedInv = inventoryColumn ? Inventory.inventory : ItemBox.items;
            List<UnderItem> otherInv = inventoryColumn ? ItemBox.items : Inventory.inventory;
            int otherInvCapacity = inventoryColumn ? ItemBox.capacity : Inventory.inventorySize;

            if (lineIndex < selectedInv.Count) {
                UnderItem item = selectedInv[lineIndex];
                if (otherInv.Count < otherInvCapacity) {
                    if (inventoryColumn) {
                        ItemBox.AddToBox(item.Name);
                        Inventory.RemoveItem(lineIndex);
                    } else {
                        Inventory.AddItem(item.Name);
                        ItemBox.RemoveFromBox(lineIndex);
                    }
                    UnitaleUtil.PlaySound("SeparateSound", "menuconfirm");
                } else
                    UnitaleUtil.PlaySound("SeparateSound", "menumove");
            } else
                UnitaleUtil.PlaySound("SeparateSound", "menumove");
            RefreshDisplay();

        } else if (GlobalControls.input.Up == UndertaleInput.ButtonState.PRESSED) {
            lineIndex--;
            if (lineIndex < 0)
                lineIndex = (inventoryColumn ? Inventory.inventorySize : ItemBox.capacity) - 1;
            UnitaleUtil.PlaySound("SeparateSound", "menumove");
            RefreshDisplay();

        } else if (GlobalControls.input.Down == UndertaleInput.ButtonState.PRESSED) {
            lineIndex++;
            if (lineIndex >= (inventoryColumn ? Inventory.inventorySize : ItemBox.capacity))
                lineIndex = 0;
            UnitaleUtil.PlaySound("SeparateSound", "menumove");
            RefreshDisplay();

        } else if (GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED || GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED) {
            if (lineIndex < Inventory.inventorySize && lineIndex < ItemBox.capacity) {
                inventoryColumn = !inventoryColumn;
                UnitaleUtil.PlaySound("SeparateSound", "menumove");
                RefreshDisplay();
            }

        } else if (GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED) {
            UnitaleUtil.PlaySound("SeparateSound", "menumove");
            DestroySelf();
        }
    }

    void RefreshDisplay() {
        player.transform.position = new Vector3(Misc.cameraX + (inventoryColumn ? 58 : 350), Misc.cameraY + 390 - (lineIndex * 32), GameObject.Find("utHeartMenu").transform.position.z);

        for (int i = 0; i < Inventory.inventorySize; i++) {
            TextManager tm = inventory[i];

            string text = i < Inventory.inventory.Count ? Inventory.inventory[i].Name : "";
            tm.SetText(new TextMessage("[font:uidialoglilspace]" + text, false, true));

            inventorySprites[i].alpha = i < Inventory.inventory.Count ? 0f : 1f;
        }

        for (int i = 0; i < ItemBox.capacity; i++) {
            TextManager tm = boxContents[i];

            string text = i < ItemBox.items.Count ? ItemBox.items[i].Name : "";
            tm.SetText(new TextMessage("[font:uidialoglilspace]" + text, false, true));

            boxContentsSprites[i].alpha = i < ItemBox.items.Count ? 0f : 1f;
        }
    }

    void DestroySelf() {
        while (inventory.Count > 0) {
            inventory[0].DestroyChars();
            Destroy(inventory[0].gameObject);
            inventory.RemoveAt(0);
            inventorySprites[0].Remove();
            inventorySprites.RemoveAt(0);
        }
        while (boxContents.Count > 0) {
            boxContents[0].DestroyChars();
            Destroy(boxContents[0].gameObject);
            boxContents.RemoveAt(0);
            boxContentsSprites[0].Remove();
            boxContentsSprites.RemoveAt(0);
        }

        Color c = player.GetComponent<Image>().color;
        player.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);

        GetComponent<Image>().color = new Color(1, 1, 1, 0);

        active = false;
        Destroy(this);
    }
}