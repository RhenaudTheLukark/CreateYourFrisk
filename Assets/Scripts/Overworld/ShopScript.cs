using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopScript : MonoBehaviour {
    public static string scriptName;
    [HideInInspector] int numberOfChoices = 4;
    [HideInInspector] int selection = 0;
    [HideInInspector] bool waitForSelection = false;
    bool infoActive = false;
    bool interrupted = false;
    string[] mainName;
    DynValue[] mainInfo;
    int[] mainPrice;
    int currentItemIndex = 0;
    int sellItem = -1;
    State[] canSelect = new State[] { State.MENU, State.BUY, State.BUYCONFIRM, State.SELL, State.SELLCONFIRM, State.TALK };
    State[] bigTexts = new State[] { State.SELL, State.SELLCONFIRM, State.TALKINPROGRESS, State.INTERRUPT, State.EXIT };

    enum State { MENU, BUY, BUYCONFIRM, SELL, SELLCONFIRM, TALK, TALKINPROGRESS, EXIT, INTERRUPT };
    State currentState = State.MENU;
    State interruptState = State.MENU;
    TPHandler tp = null;
    public TextManager tmMain, tmChoice, tmInfo, tmBigTalk, tmGold, tmItem;
    public GameObject tmInfoParent, utHeart;
    public ScriptWrapper script;

	// Use this for initialization
	void Start () {
        FindObjectOfType<Fading>().BeginFade(-1);

        tmBigTalk.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice]", false, true) });
        EnableBigText(false);

        if (scriptName == null)
            throw new CYFException("You must give a valid script name to the function General.EnterShop()");

        script = new ScriptWrapper() {
            scriptname = scriptName
        };
        string scriptText = ScriptRegistry.Get(ScriptRegistry.SHOP_PREFIX + scriptName);

        if (scriptText == null)
            throw new CYFException("You must give a valid script name to the function General.EnterShop()");

        try {
            script.DoString(scriptText);
            script.SetVar("background", UserData.Create(new LuaSpriteController(GameObject.Find("Background").GetComponent<Image>())));
            script.script.Globals["Interrupt"] = ((Action<DynValue, string>) Interrupt);
            script.script.Globals["CreateSprite"] = (Func<string, string, int, DynValue>) SpriteUtil.MakeIngameSprite;
            script.script.Globals["CreateLayer"] = (Func<string, string, bool, bool>) SpriteUtil.CreateLayer;
            script.script.Globals["CreateText"] = (Func<Script, DynValue, DynValue, int, string, int, LuaTextManager>) LuaScriptBinder.CreateText;
            TryCall("Start");

            tmMain.SetCaller(script);
            tmChoice.SetCaller(script);
            tmInfo.SetCaller(script);
            tmBigTalk.SetCaller(script);

            tmMain.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][linespacing:11]" + script.GetVar("maintalk").String, true, false) });
            tmChoice.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice][font:uidialoglilspace][linespacing:9]    Buy\n    Sell\n    Talk\n    Exit", false, true) });
            tmGold.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice]" + PlayerCharacter.instance.Gold + "G", false, true) });
            tmItem.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice]" + Inventory.inventory.Count + "/8", false, true) });
            tmInfo.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice]", false, true) });

            Camera.main.GetComponent<AudioSource>().clip = AudioClipRegistry.GetMusic(script.GetVar("music").String);
            Camera.main.GetComponent<AudioSource>().time = 0;
            Camera.main.GetComponent<AudioSource>().Play();

            SetPlayerOnSelection();
        }
        catch (InterpreterException ex) { UnitaleUtil.DisplayLuaError(scriptName, ex.DecoratedMessage != null ? UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message : ex.Message); }
        catch (Exception ex)            { UnitaleUtil.DisplayLuaError(scriptName, "Unknown error. Usually means you're missing a sprite.\nSee documentation for details.\nStacktrace below in case you wanna notify a dev.\n\nError: " + ex.Message + "\n\n" + ex.StackTrace); }

    }

    private delegate TResult Func<T1, T2, T3, T4, T5, T6, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg, T6 arg6);

    // void CreateLayer(string name, string relatedTag = "BelowUI", bool before = false) { SpriteUtil.CreateLayer(name, relatedTag, before); }
    // DynValue CreateSprite(string filename, string tag = "BelowUI", int childNumber = -1) { return SpriteUtil.MakeIngameSprite(filename, tag, childNumber); }

    TextMessage[] BuildTextFromTable(DynValue text, string beforeText) {
        TextMessage[] msgs = new TextMessage[text.Type == DataType.Table ? text.Table.Length : 1];
        for (int i = 0; i < msgs.Length; i++)
            if (text.Type == DataType.Table) msgs[i] = new RegularMessage(beforeText + text.Table.Get(i + 1).String);
            else                             msgs[i] = new RegularMessage(beforeText + text.String);
        return msgs;
    }

    void Interrupt(DynValue text, string nextState = "MENU") {
        if (currentState != State.INTERRUPT) {
            TryCall("OnInterrupt", DynValue.NewString(nextState));
            try { interruptState = (State)Enum.Parse(typeof(State), nextState, true); }
            catch { throw new CYFException("\"" + nextState + "\" is not a valid shop state."); }
            ChangeState(State.INTERRUPT, -1, text);
        }
    }

    void EnableBigText(bool enable) {
        if (enable) {
            if (!tmMain.IsFinished())   tmMain.SkipLine();
            if (!tmChoice.IsFinished()) tmChoice.SkipLine();
        }
        tmBigTalk.transform.parent.parent.gameObject.SetActive(enable);
    }

    void ChangeState(State state, int select = -1, object arg = null) {
        EnableBigText(bigTexts.Contains(state));
        if (currentState == State.INTERRUPT)
            interrupted = false;
        currentState = state;
        //Switch comparison states
        string text;
        switch (state) {
            case State.MENU:
                TryCall("EnterMenu");
                if (!interrupted) {
                    sellItem = -1;
                    if (select != -1)
                        selection = select;
                    numberOfChoices = 4;
                    tmChoice.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice][font:uidialoglilspace][linespacing:9]    Buy\n    Sell\n    Talk\n    Exit", false, true) });
                    tmMain.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][linespacing:11]" + script.GetVar("maintalk").String, true, false) });
                    tmGold.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice]" + PlayerCharacter.instance.Gold + "G", false, true) });
                    tmItem.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice]" + Inventory.inventory.Count + "/8", false, true) });
                    infoActive = false;
                    tmInfoParent.transform.position = new Vector3(tmInfoParent.transform.position.x, 70, tmInfoParent.transform.position.z);
                }
                break;
            case State.BUYCONFIRM:
                currentItemIndex = selection;
                selection = 1;
                numberOfChoices = 2;
                tmChoice.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][font:uidialoglilspace][linespacing:0][novoice]Buy for\n" + mainPrice[currentItemIndex] + "G?\n\n    Yes\n    No", false, true) });
                break;
            case State.SELLCONFIRM:
                currentItemIndex = selection;
                selection = 1;
                numberOfChoices = 2;
                tmBigTalk.SetTextQueue(new TextMessage[] { new TextMessage("\n[linespacing:11][noskipatall][font:uidialoglilspace][novoice]          Sell the " + Inventory.inventory[currentItemIndex].Name + " for " +
                                                                           Inventory.NametoPrice[Inventory.inventory[currentItemIndex].Name] / 5 + "G?\n\n              Yes\tNo" +
                                                                           "\n\n\t   [color:ffff00](" + PlayerCharacter.instance.Gold + "G)", false, true) });
                break;
            case State.BUY:
                TryCall("EnterBuy");
                if (!interrupted) {
                    if (select != -1)
                        selection = select;
                    text = BuildBuyString().Replace("\n", " \n").Replace("\r", " \r").Replace("\t", " \t");
                    numberOfChoices = text.Split(new char[] { '\n', '\r', '\t' }).Length;
                    tmChoice.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][linespacing:9][font:uidialoglilspace]" + script.GetVar("buytalk").String, false, false) });
                    tmMain.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice][linespacing:11][font:uidialoglilspace]" + text, false, true) });
                    tmGold.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice]" + PlayerCharacter.instance.Gold + "G", false, true) });
                    tmItem.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice]" + Inventory.inventory.Count + "/8", false, true) });
                }
                break;
            case State.SELL:
                TryCall("EnterSell");
                if (!interrupted) {
                    if (select != -1)
                        selection = select;
                    if (sellItem == -1)
                        sellItem = Inventory.inventory.Count;
                    text = BuildSellString();
                    numberOfChoices = Inventory.inventory.Count + 1;
                    tmBigTalk.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice][font:uidialoglilspace][linespacing:11]" + text, false, true) });
                    if (Inventory.inventory.Count == 0) {
                        TryCall("FailSell", DynValue.NewString("empty"));
                        if (!interrupted)
                            HandleCancel();
                    }
                }
                break;
            case State.TALK:
                TryCall("EnterTalk");
                if (!interrupted) {
                    if (select != -1)
                        selection = select;
                    text = BuildTalkString();
                    numberOfChoices = mainName.Length + 1;
                    tmMain.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][linespacing:11]" + text, false, true) });
                    tmChoice.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][font:uidialoglilspace][linespacing:9]" + script.GetVar("talktalk").String, false, false) });
                }
                break;
            case State.TALKINPROGRESS:
                TryCall("SuccessTalk", DynValue.NewString(mainName[selection]));
                if (!interrupted) {
                    TextMessage[] texts = BuildTalkResultStrings();
                    tmBigTalk.SetTextQueue(texts);
                    utHeart.GetComponent<Image>().enabled = false;
                }
                break;
            case State.EXIT:
                TryCall("EnterExit");
                if (!interrupted) {
                    TextMessage[] texts2 = BuildTextFromTable(script.GetVar("exittalk"), "[linespacing:11]");
                    tmBigTalk.SetTextQueue(texts2);
                    utHeart.GetComponent<Image>().enabled = false;
                }
                break;
            case State.INTERRUPT:
                tmBigTalk.SetTextQueue(BuildTextFromTable((DynValue) arg, "[linespacing:11]"));
                utHeart.GetComponent<Image>().enabled = false;
                interrupted = true;
                break;
        }
        if (canSelect.Contains(state))
            waitForSelection = true;
    }

    void SetPlayerOnSelection() {
        if (Array.IndexOf(canSelect, currentState) == -1)
            return;
        TextManager tm = null;
        switch (currentState) {
            case State.MENU:
            case State.BUYCONFIRM:
                tm = tmChoice;
                break;
            case State.BUY:
            case State.TALK:
                tm = tmMain;
                break;
            case State.SELL:
            case State.SELLCONFIRM:
                tm = tmBigTalk;
                break;
            default:
                break;
        }
        string[] text = tm.textQueue[0].Text.Split(new char[] { '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        int selectionTemp = selection;
        if (currentState == State.SELL && selection == numberOfChoices - 1)
            selection = 8;
        else if (currentState == State.BUYCONFIRM)
            selection = selection + 2;
        else if (currentState == State.SELLCONFIRM)
            selection = selection + 1;
        int beginLine = GetIndexFirstCharOfLineFromChild(tm);
        Vector3 v = tm.transform.GetChild(GetIndexFirstCharOfLineFromText(text[selection]) + beginLine).position;
        utHeart.transform.position = new Vector3(v.x - 16, v.y + 8, v.z);
        selection = selectionTemp;
        if (currentState == State.BUY) {
            if (selection == numberOfChoices - 1)
                infoActive = false;
            else {
                infoActive = true;
                string info = mainPrice[selection] == 0 ? "SOLD OUT" : mainInfo[selection].String;
                tmInfo.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice][font:uidialoglilspace]" + info, false, true) });
            }
        }
    }

    int GetIndexFirstCharOfLineFromChild(TextManager tm) {
        int beginLine = 0;
        if (selection != 0) {
            float y = tm.transform.GetChild(0).position.y;
            int count = 0;
            for (int i = 0; i < tm.transform.childCount && count < selection; i++)
                if (tm.transform.GetChild(i).position.y <= y - 8 || tm.transform.GetChild(i).position.y >= y + 8 || Mathf.Round(tm.transform.GetChild(i).position.x) == 356) {
                    count++;
                    y = tm.transform.GetChild(i).position.y;
                    if (count == selection) {
                        beginLine = i;
                        break;
                    }
                }
        }
        return beginLine;
    }

    int GetIndexFirstCharOfLineFromText(string text) {
        int count = 0, commandLess = 0;
        for (int i = 0; i < text.Length; i++) {
            if (text[i] == ' ' || text[i] == '*') continue;
            if (text[i] == '[')                   count++;
            if (count == 0)                       return i - commandLess;
            else                                  commandLess++;
            if (text[i] == ']')                   count--;
        }
        return 0;
    }

    string BuildBuyString() {
        string result = "[font:uidialoglilspace][linespacing:11]";
        DynValue[] itemName, itemInfo, itemPrice;
        try {
            itemName = UnitaleUtil.TableToDynValueArray(script.GetVar("buylist").Table.Get(1).Table);
            itemInfo = UnitaleUtil.TableToDynValueArray(script.GetVar("buylist").Table.Get(2).Table);
            itemPrice = UnitaleUtil.TableToDynValueArray(script.GetVar("buylist").Table.Get(3).Table);
        } catch { throw new CYFException("The variable \"buylist\" must contain a table!"); }

        mainName = new string[itemName.Length];
        mainInfo = new DynValue[itemName.Length];
        mainPrice = new int[itemName.Length];

        for (int i = 0; i < itemName.Length; i ++) {
            if (i < itemPrice.Length && itemPrice[i].Type != DataType.Number && itemPrice[i].Number % 1 != 0) throw new CYFException("The price table must contain integers.");
            if (!Inventory.NametoDesc.Keys.Contains(itemName[i].String))
                throw new CYFException("The item \"" + itemName[i].String + "\" doesn't exist in the inventory database.");
            mainName[i] = itemName[i].String;
            mainInfo[i] = i >= itemInfo.Length ? DynValue.NewString(Inventory.NametoDesc[itemName[i].String]) : itemInfo[i];
            mainPrice[i] = i >= itemPrice.Length ? Inventory.NametoPrice[mainName[i]] : (int)itemPrice[i].Number;
            try {
                if (mainPrice[i] == -1) mainPrice[i] = Inventory.NametoPrice[mainName[i]];
                if (mainPrice[i] == 0)  result += "  [color:808080]--- SOLD OUT ---[color:ffffff]\n";
                else                    result += "  " + mainPrice[i] + "G - " + mainName[i] + "\n";
            } catch { throw new CYFException("The item \"" + mainName[i] + "\" doesn't have a price in the database."); }
        }
        return result + "  Exit";
    }

    string BuildTalkString() {
        DynValue talks, talkResults;
        try {
            talks = script.GetVar("talklist").Table.Get(1);
            talkResults = script.GetVar("talklist").Table.Get(2);
        } catch { throw new CYFException("The variable talklist must be an array which contains two other arrays."); }

        string result = "[font:uidialoglilspace][linespacing:11]";
        if (talks.Type == DataType.Table) {
            mainName = new string[talks.Table.Length];
            mainInfo = new DynValue[talks.Table.Length];
            for (int i = 0; i < talks.Table.Length; i ++) {
                mainName[i] = talks.Table.Get(i + 1).String;
                result += "  " + mainName[i] + "\n";
                mainInfo[i] = talkResults.Table.Get(i + 1);
            }
        } else
            throw new CYFException("The variable talklist must be an array which contains two other arrays.");
        return result + "  Exit";
    }

    TextMessage[] BuildTalkResultStrings() { return BuildTextFromTable(mainInfo[selection], "[linespacing:11]"); }

    string BuildSellString() {
        string result = "";
        mainName = new string[Inventory.inventory.Count];
        for (int i = 0; i < 8; i ++) {
            if (i < Inventory.inventory.Count) {
                int price = Inventory.NametoPrice[Inventory.inventory[i].Name];
                result += "  " + (i % 2 == 0 ? "" : "  ") + (price == 0 ? "NO!" : price / 5 + "G") + " - " + Inventory.inventory[i].ShortName;
            } else if (i < sellItem) result += "  [color:888888](thanks PURCHASE)";
            else                     result += " ";
            result += (i % 2 == 0 ? "\t" : "\n");
        }
        return result + "  [color:ffffff]Exit\t                [color:ffff00](" + PlayerCharacter.instance.Gold + "G)";
    }

    void HandleAction() {
        switch (currentState) {
            case State.EXIT:
                break;
            case State.MENU:
                switch (selection) {
                    case 0: ChangeState(State.BUY, 0);  break;
                    case 1: ChangeState(State.SELL, 0); break;
                    case 2: ChangeState(State.TALK, 0); break;
                    case 3: HandleCancel();             break;
                }
                break;
            case State.BUY:
                if (selection == numberOfChoices - 1) HandleCancel();
                else {
                    ChangeState(State.BUYCONFIRM, 0);
                    if (mainPrice[currentItemIndex] == 0) {
                        TryCall("FailBuy", DynValue.NewString("soldout"));
                        HandleCancel();
                    }
                }
                break;
            case State.SELL:
                if (selection == numberOfChoices - 1) HandleCancel();
                else {
                    ChangeState(State.SELLCONFIRM, 0);
                    if (Inventory.NametoPrice[Inventory.inventory[currentItemIndex].Name] == 0) {
                        UnitaleUtil.PlaySound("SeparateSound", "ShopFail");
                        TryCall("FailSell", DynValue.NewString("cantsell"));
                        HandleCancel();
                    }
                }
                break;
            case State.TALK:
                if (selection == numberOfChoices - 1) HandleCancel();
                else                                  ChangeState(State.TALKINPROGRESS);
                break;
            case State.BUYCONFIRM:
                if (selection == numberOfChoices - 1) TryCall("ReturnBuy");
                else {
                    if (PlayerCharacter.instance.Gold < mainPrice[currentItemIndex]) TryCall("FailBuy", DynValue.NewString("gold"));
                    else if (Inventory.inventory.Count == Inventory.inventorySize)   TryCall("FailBuy", DynValue.NewString("full"));
                    else if (mainPrice[currentItemIndex] == 0)                       TryCall("FailBuy", DynValue.NewString("soldout"));
                    else {
                        TryCall("SuccessBuy", DynValue.NewString(mainName[currentItemIndex]));
                        UnitaleUtil.PlaySound("SeparateSound", "ShopSuccess");
                        PlayerCharacter.instance.SetGold(PlayerCharacter.instance.Gold - mainPrice[currentItemIndex]);
                        Inventory.AddItem(mainName[currentItemIndex]);
                        tmGold.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice]" + PlayerCharacter.instance.Gold + "G", false, true) });
                        tmItem.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall][novoice]" + Inventory.inventory.Count + "/8", false, true) });
                    }
                }
                if (!interrupted) {
                    selection = currentItemIndex;
                    HandleCancel();
                }
                break;
            case State.SELLCONFIRM:
                if (selection == numberOfChoices - 1) TryCall("ReturnSell");
                else {
                    TryCall("SuccessSell", DynValue.NewString(mainName[currentItemIndex]));
                    UnitaleUtil.PlaySound("SeparateSound", "ShopSuccess");
                    PlayerCharacter.instance.SetGold(PlayerCharacter.instance.Gold + Inventory.NametoPrice[Inventory.inventory[currentItemIndex].Name] / 5);
                    Inventory.RemoveItem(currentItemIndex);
                    if (currentItemIndex == Inventory.inventory.Count && Inventory.inventory.Count != 1)
                        currentItemIndex--;
                }
                if (!interrupted) {
                    selection = currentItemIndex;
                    HandleCancel();
                }
                break;
        }
    }

    void HandleCancel(bool fromKey = false) {
        switch (currentState) {
            case State.MENU:        if (!fromKey) ChangeState(State.EXIT);                 break;
            case State.BUY:
            case State.SELL:
            case State.TALK:        ChangeState(State.MENU, (int)(currentState - 1) / 2 ); break;
            case State.BUYCONFIRM:  ChangeState(State.BUY, currentItemIndex);              break;
            case State.SELLCONFIRM: ChangeState(State.SELL, currentItemIndex);             break;
        }
    }

    void SelectionInputManager() {
        utHeart.GetComponent<Image>().enabled = true;
        if (GlobalControls.input.Down == UndertaleInput.ButtonState.PRESSED) {
            if (currentState == State.SELL) {
                if (selection == numberOfChoices - 2)                                  selection = numberOfChoices - 1;
                else if (selection == numberOfChoices - 1)                             selection = 0;
                else if (selection == numberOfChoices - 3 && numberOfChoices % 2 == 0) selection = (selection + 1) % numberOfChoices;
                else                                                                   selection = (selection + 2) % numberOfChoices;
            } else                                                                     selection = (selection + 1) % numberOfChoices;
            SetPlayerOnSelection();
        } else if (GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED) {
            if (currentState == State.SELL) {
                if (selection == numberOfChoices - 1)                                                          { }
                else if (selection % 2 == 1 || (selection == numberOfChoices - 2 && numberOfChoices % 2 == 0)) selection = (selection + numberOfChoices - 1) % numberOfChoices;
                else                                                                                           selection = (selection + 1) % numberOfChoices;
            } else                                                                                             selection = (selection + 1) % numberOfChoices;
            SetPlayerOnSelection();
        } else if (GlobalControls.input.Up == UndertaleInput.ButtonState.PRESSED) {
            if (currentState == State.SELL) {
                if (selection == 0)                                                    selection = numberOfChoices - 1;
                else if (selection == numberOfChoices - 1 && numberOfChoices % 2 == 0) selection = (selection + numberOfChoices - 1) % numberOfChoices;
                else                                                                   selection = (selection + numberOfChoices - 2) % numberOfChoices;
            } else                                                                     selection = (selection + numberOfChoices - 1) % numberOfChoices;
            SetPlayerOnSelection();
        } else if (GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED) {
            if (currentState == State.SELL) {
                if (selection == numberOfChoices - 1)                                                          { }
                else if (selection % 2 == 1 || (selection == numberOfChoices - 2 && numberOfChoices % 2 == 0)) selection = (selection + numberOfChoices - 1) % numberOfChoices;
                else                                                                                           selection = (selection + 1) % numberOfChoices;
            } else                                                                                             selection = (selection + numberOfChoices - 1) % numberOfChoices;
            SetPlayerOnSelection();
        } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED)
            HandleAction();
        else if (GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED)
            HandleCancel(true);
}

    void TextInputManager() {
        if (GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED && !tmBigTalk.blockSkip && !tmBigTalk.LineComplete() && tmBigTalk.CanSkip()) {
            if (script.GetVar("playerskipdocommand").Boolean)
                tmBigTalk.DoSkipFromPlayer();
            else
                tmBigTalk.SkipLine();
        } else if ((GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED || tmBigTalk.CanAutoSkipAll()) && tmBigTalk.LineComplete() && !tmBigTalk.AllLinesComplete())
            tmBigTalk.NextLineText();
        else if ((GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED || tmBigTalk.CanAutoSkipAll()) && tmBigTalk.AllLinesComplete()) {
            switch (currentState) {
                case State.TALKINPROGRESS:
                    ChangeState(State.TALK);
                    utHeart.GetComponent<Image>().enabled = true;
                    EnableBigText(false);
                    break;
                case State.EXIT:
                    if (tp != null)
                        break;
                    if (script.GetVar("returnscene").Type != DataType.String)
                        throw new CYFException("The variable \"returnscene\" must be a string.");
                    if (script.GetVar("returnpos").Type != DataType.Table)
                        throw new CYFException("The variable \"returnpos\" must be a table.");
                    else if (script.GetVar("returnpos").Table.Length < 2)
                        throw new CYFException("The variable \"returnpos\" must be a table with two numbers.");
                    else if (script.GetVar("returnpos").Table.Get(1).Type != DataType.Number || script.GetVar("returnpos").Table.Get(2).Type != DataType.Number)
                        throw new CYFException("The variable \"returnpos\" must be a table with two numbers.");

                    if (script.GetVar("returndir").Type != DataType.Number)
                        throw new CYFException("The variable \"returndir\" must be a number.");
                    else if (script.GetVar("returndir").Number > 8 || script.GetVar("returndir").Number < 2 || script.GetVar("returndir").Number % 2 == 1)
                        throw new CYFException("The variable \"returndir\" must be either 2 (Down), 4 (Left), 6 (Right) or 8 (Up).");

                    tmBigTalk.DestroyChars();

                    tp = Instantiate(Resources.Load<TPHandler>("Prefabs/TP On-the-fly"));
                    tp.sceneName = script.GetVar("returnscene").String;
                    tp.position = new Vector2((float) script.GetVar("returnpos").Table.Get(1).Number, (float) script.GetVar("returnpos").Table.Get(2).Number);
                    tp.direction = (int) script.GetVar("returndir").Number;
                    GameObject.DontDestroyOnLoad(tp);
                    StartCoroutine(tp.LaunchTP());
                    break;
                case State.INTERRUPT:
                    utHeart.GetComponent<Image>().enabled = true;
                    EnableBigText(false);
                    ChangeState(interruptState);
                    break;
            }
        }
    }

    public bool TryCall(string func, DynValue param) { return TryCall(func, new DynValue[] { param }); }
    public bool TryCall(string func, DynValue[] param = null) {
        try {
            DynValue sval = script.GetVar(func);
            if (sval == null || sval.Type == DataType.Nil) return false;
            if (param != null) script.Call(func, param);
            else script.Call(func);
            return true;
        } catch (InterpreterException ex) { UnitaleUtil.DisplayLuaError(scriptName, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message); }
        return true;
    }

    // Update is called once per frame
    void Update () {
        if (script.GetVar("Update") != null)
            TryCall("Update");
        if (waitForSelection) {
            SetPlayerOnSelection();
            waitForSelection = false;
        }
        if (canSelect.Contains(currentState)) SelectionInputManager();
        else                                  TextInputManager();
        if (infoActive && tmInfoParent.transform.position.y != 234) {
            tmInfoParent.transform.position = new Vector3(tmInfoParent.transform.position.x,
                                                          tmInfoParent.transform.position.y + 6 <= 234 ? tmInfoParent.transform.position.y + 6 : 234,
                                                          tmInfoParent.transform.position.z);
        } else if (!infoActive && tmInfoParent.transform.position.y != 70)
            tmInfoParent.transform.position = new Vector3(tmInfoParent.transform.position.x,
                                                          tmInfoParent.transform.position.y - 6 >= 70 ? tmInfoParent.transform.position.y - 6 : 70,
                                                          tmInfoParent.transform.position.z);
    }
}
