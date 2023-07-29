using System;
using System.Linq;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

public class ShopScript : MonoBehaviour {
    public static string scriptName;
    private int numberOfChoices = 4;
    private int selection;
    private bool waitForSelection;
    private bool infoActive;
    private bool interrupted;
    private string[] mainName;
    private DynValue[] mainInfo;
    private int[] mainPrice;
    private int currentItemIndex;
    private int sellItem = -1;
    private readonly State[] canSelect = { State.MENU, State.BUY, State.BUYCONFIRM, State.SELL, State.SELLCONFIRM, State.TALK };
    private readonly State[] bigTexts = { State.SELL, State.SELLCONFIRM, State.TALKINPROGRESS, State.INTERRUPT, State.EXIT };

    private enum State { MENU, BUY, BUYCONFIRM, SELL, SELLCONFIRM, TALK, TALKINPROGRESS, EXIT, INTERRUPT }
    private State currentState = State.MENU;
    private State interruptState = State.MENU;

    private TPHandler tp;
    public TextManager tmMain, tmChoice, tmInfo, tmBigTalk, tmGold, tmItem;
    public GameObject tmInfoParent, utHeart;
    public ScriptWrapper script;

    // Use this for initialization
    private void Start() {
        FindObjectOfType<Fading>().BeginFade(-1);

        tmBigTalk.SetTextQueue(new[] { new TextMessage("[novoice]", false, true) });
        EnableBigText(false);

        if (scriptName == null)
            throw new CYFException("You must give a valid script name to the function General.EnterShop()");

        script = new ScriptWrapper {
            scriptname = scriptName
        };
        string scriptText = FileLoader.GetScript("Shops/" + scriptName, "Loading a shop", "event");

        try {
            script.DoString(scriptText);
            script.SetVar("background", UserData.Create(LuaSpriteController.GetOrCreate(GameObject.Find("Background"))));
            script.script.Globals["Interrupt"] = ((Action<DynValue, string>) Interrupt);
            script.script.Globals["CreateSprite"] = (Func<string, string, int, DynValue>) SpriteUtil.MakeIngameSprite;
            script.script.Globals["CreateLayer"] = (Func<string, string, bool, bool>) SpriteUtil.CreateLayer;
            script.script.Globals["CreateText"] = (Func<Script, DynValue, DynValue, int, string, int, LuaTextManager>) LuaScriptBinder.CreateText;
            UnitaleUtil.TryCall(script, "Start");

            tmMain.SetCaller(script);
            tmChoice.SetCaller(script);
            tmInfo.SetCaller(script);
            tmBigTalk.SetCaller(script);

            tmMain.SetTextQueue(new[] { new TextMessage("[linespacing:11]" + script.GetVar("maintalk").String, true, false) });
            tmChoice.SetTextQueue(new[] { new TextMessage("[novoice][font:uidialoglilspace][linespacing:9]    Buy\n    Sell\n    Talk\n    Exit", false, true) });
            tmGold.SetTextQueue(new[] { new TextMessage("[novoice]" + PlayerCharacter.instance.Gold + "G", false, true) });
            tmItem.SetTextQueue(new[] { new TextMessage("[novoice]" + Inventory.inventory.Count + "/8", false, true) });
            tmInfo.SetTextQueue(new[] { new TextMessage("[novoice]", false, true) });

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

    private static TextMessage[] BuildTextFromTable(DynValue text, string beforeText) {
        TextMessage[] msgs = new TextMessage[text.Type == DataType.Table ? text.Table.Length : 1];
        for (int i = 0; i < msgs.Length; i++)
            if (text.Type == DataType.Table) msgs[i] = new RegularMessage(beforeText + text.Table.Get(i + 1).String);
            else                             msgs[i] = new RegularMessage(beforeText + text.String);
        return msgs;
    }

    private void Interrupt(DynValue text, string nextState = "MENU") {
        if (currentState == State.INTERRUPT) return;
        UnitaleUtil.TryCall(script, "OnInterrupt", DynValue.NewString(nextState));
        try { interruptState = (State)Enum.Parse(typeof(State), nextState, true); }
        catch { throw new CYFException("\"" + nextState + "\" is not a valid shop state."); }
        ChangeState(State.INTERRUPT, -1, text);
    }

    private void EnableBigText(bool enable) {
        if (enable) {
            if (!tmMain.IsFinished())   tmMain.SkipLine();
            if (!tmChoice.IsFinished()) tmChoice.SkipLine();
        }
        tmBigTalk.transform.parent.parent.gameObject.SetActive(enable);
    }

    private void ChangeState(State state, int select = -1, object arg = null) {
        EnableBigText(bigTexts.Contains(state));
        if (currentState == State.INTERRUPT)
            interrupted = false;
        currentState = state;
        //Switch comparison states
        string text;
        switch (state) {
            case State.MENU:
                UnitaleUtil.TryCall(script, "EnterMenu");
                if (!interrupted) {
                    sellItem = -1;
                    if (select != -1)
                        selection = select;
                    numberOfChoices = 4;
                    tmChoice.SetTextQueue(new[] { new TextMessage("[novoice][font:uidialoglilspace][linespacing:9]    Buy\n    Sell\n    Talk\n    Exit", false, true) });
                    tmMain.SetTextQueue(new[] { new TextMessage("[linespacing:11]" + script.GetVar("maintalk").String, true, false) });
                    tmGold.SetTextQueue(new[] { new TextMessage("[novoice]" + PlayerCharacter.instance.Gold + "G", false, true) });
                    tmItem.SetTextQueue(new[] { new TextMessage("[novoice]" + Inventory.inventory.Count + "/8", false, true) });
                    infoActive = false;
                    tmInfoParent.transform.position = new Vector3(tmInfoParent.transform.position.x, 70, tmInfoParent.transform.position.z);
                }
                break;
            case State.BUYCONFIRM:
                currentItemIndex = selection;
                selection = 1;
                numberOfChoices = 2;
                tmChoice.SetTextQueue(new[] { new TextMessage("[font:uidialoglilspace][linespacing:0][novoice]Buy for\n" + mainPrice[currentItemIndex] + "G?\n\n    Yes\n    No", false, true) });
                break;
            case State.SELLCONFIRM:
                currentItemIndex = selection;
                selection = 1;
                numberOfChoices = 2;
                tmBigTalk.SetTextQueue(new[] { new TextMessage("\n[linespacing:11][font:uidialoglilspace][novoice]          Sell the " + Inventory.inventory[currentItemIndex].Name + " for " +
                                                               Inventory.NametoPrice[Inventory.inventory[currentItemIndex].Name] / 5 + "G?\n\n              Yes\tNo" +
                                                               "\n\n\t   [color:ffff00](" + PlayerCharacter.instance.Gold + "G)", false, true) });
                break;
            case State.BUY:
                UnitaleUtil.TryCall(script, "EnterBuy");
                if (!interrupted) {
                    if (select != -1)
                        selection = select;
                    text = BuildBuyString().Replace("\n", " \n").Replace("\r", " \r").Replace("\t", " \t");
                    numberOfChoices = text.Split('\n', '\r', '\t').Length;
                    tmChoice.SetTextQueue(new[] { new TextMessage("[linespacing:9][font:uidialoglilspace]" + script.GetVar("buytalk").String, false, false) });
                    tmMain.SetTextQueue(new[] { new TextMessage("[novoice][linespacing:11][font:uidialoglilspace]" + text, false, true) });
                    tmGold.SetTextQueue(new[] { new TextMessage("[novoice]" + PlayerCharacter.instance.Gold + "G", false, true) });
                    tmItem.SetTextQueue(new[] { new TextMessage("[novoice]" + Inventory.inventory.Count + "/8", false, true) });
                }
                break;
            case State.SELL:
                UnitaleUtil.TryCall(script, "EnterSell");
                if (!interrupted) {
                    if (select != -1)
                        selection = select;
                    if (sellItem == -1)
                        sellItem = Inventory.inventory.Count;
                    text = BuildSellString();
                    numberOfChoices = Inventory.inventory.Count + 1;
                    tmBigTalk.SetTextQueue(new[] { new TextMessage("[novoice][font:uidialoglilspace][linespacing:11]" + text, false, true) });
                    if (Inventory.inventory.Count == 0) {
                        UnitaleUtil.TryCall(script, "FailSell", DynValue.NewString("empty"));
                        if (!interrupted)
                            HandleCancel();
                    }
                }
                break;
            case State.TALK:
                UnitaleUtil.TryCall(script, "EnterTalk");
                if (!interrupted) {
                    if (select != -1)
                        selection = select;
                    text = BuildTalkString();
                    numberOfChoices = mainName.Length + 1;
                    tmMain.SetTextQueue(new[] { new TextMessage("[linespacing:11]" + text, false, true) });
                    tmChoice.SetTextQueue(new[] { new TextMessage("[font:uidialoglilspace][linespacing:9]" + script.GetVar("talktalk").String, false, false) });
                }
                break;
            case State.TALKINPROGRESS:
                UnitaleUtil.TryCall(script, "SuccessTalk", DynValue.NewString(mainName[selection]));
                if (!interrupted) {
                    TextMessage[] texts = BuildTalkResultStrings();
                    tmBigTalk.SetTextQueue(texts);
                    utHeart.GetComponent<Image>().enabled = false;
                }
                break;
            case State.EXIT:
                UnitaleUtil.TryCall(script, "EnterExit");
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

    private void SetPlayerOnSelection() {
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
        }

        if (tm == null) return;

        int usedSelection = selection;
        if (currentState == State.BUYCONFIRM)  usedSelection += 3;
        if (currentState == State.SELLCONFIRM) usedSelection += 6;

        Vector3 v = tm.letters[GetIndexFirstCharOfGivenLine(tm, usedSelection)].image.transform.position;
        utHeart.transform.position = new Vector3(v.x - 16, v.y + 8, v.z);
        if (currentState != State.BUY) return;
        infoActive = selection != numberOfChoices - 1;
        if (!infoActive) return;
        string info = mainPrice[selection] == 0 ? "SOLD OUT" : mainInfo[selection].String;
        tmInfo.SetTextQueue(new[] { new TextMessage("[novoice][font:uidialoglilspace]" + info, false, true) });
    }

    private static int GetIndexFirstCharOfGivenLine(TextManager tm, int choiceIndex = 0) {
        string text = tm.textQueue[0].Text;
        int count = -1, columnsUsed = 0, bracketCount = 0, bracketBegin = 0;
        for (int i = 0; i < text.Length; i++) {
            if (tm.letters.Any(data => data.index == i))
                count++;

            switch (text[i]) {
                case ' ':
                case '*': continue;
                case '[':
                    if (bracketCount == 0)
                        bracketBegin = i;
                    bracketCount++;
                    break;
                case '\n':
                case '\r':
                    choiceIndex -= tm.columnNumber - columnsUsed;
                    columnsUsed = 0;
                    continue;
                case '\t':
                    choiceIndex--;
                    columnsUsed++;
                    continue;
            }

            if (bracketCount == 0 && choiceIndex <= 0) return count;

            if (text[i] == ']' && bracketCount > 0)
                bracketCount--;

            // Unmatched open bracket at end of text
            if (bracketCount > 0 && i == text.Length - 1) {
                bracketCount = 0;
                i = bracketBegin;
            }
        }
        return 0;
    }

    private string BuildBuyString() {
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

    private string BuildTalkString() {
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

    private TextMessage[] BuildTalkResultStrings() { return BuildTextFromTable(mainInfo[selection], "[linespacing:11]"); }

    private string BuildSellString() {
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

    private void HandleAction() {
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
                        UnitaleUtil.TryCall(script, "FailBuy", DynValue.NewString("soldout"));
                        HandleCancel();
                    }
                }
                break;
            case State.SELL:
                if (selection == 8) HandleCancel();
                else {
                    ChangeState(State.SELLCONFIRM, 0);
                    if (Inventory.NametoPrice[Inventory.inventory[currentItemIndex].Name] == 0) {
                        UnitaleUtil.PlaySound("SeparateSound", "ShopFail");
                        UnitaleUtil.TryCall(script, "FailSell", DynValue.NewString("cantsell"));
                        HandleCancel();
                    }
                }
                break;
            case State.TALK:
                if (selection == numberOfChoices - 1) HandleCancel();
                else                                  ChangeState(State.TALKINPROGRESS);
                break;
            case State.BUYCONFIRM:
                if (selection == numberOfChoices - 1) UnitaleUtil.TryCall(script, "ReturnBuy");
                else {
                    if (PlayerCharacter.instance.Gold < mainPrice[currentItemIndex]) UnitaleUtil.TryCall(script, "FailBuy", DynValue.NewString("gold"));
                    else if (Inventory.inventory.Count == Inventory.inventorySize)   UnitaleUtil.TryCall(script, "FailBuy", DynValue.NewString("full"));
                    else if (mainPrice[currentItemIndex] == 0)                       UnitaleUtil.TryCall(script, "FailBuy", DynValue.NewString("soldout"));
                    else {
                        UnitaleUtil.TryCall(script, "SuccessBuy", DynValue.NewString(mainName[currentItemIndex]));
                        UnitaleUtil.PlaySound("SeparateSound", "ShopSuccess");
                        PlayerCharacter.instance.SetGold(PlayerCharacter.instance.Gold - mainPrice[currentItemIndex]);
                        Inventory.AddItem(mainName[currentItemIndex]);
                        tmGold.SetTextQueue(new[] { new TextMessage("[novoice]" + PlayerCharacter.instance.Gold + "G", false, true) });
                        tmItem.SetTextQueue(new[] { new TextMessage("[novoice]" + Inventory.inventory.Count + "/8", false, true) });
                    }
                }
                if (!interrupted) {
                    selection = currentItemIndex;
                    HandleCancel();
                }
                break;
            case State.SELLCONFIRM:
                if (selection == numberOfChoices - 1) UnitaleUtil.TryCall(script, "ReturnSell");
                else {
                    UnitaleUtil.TryCall(script, "SuccessSell", DynValue.NewString(mainName[currentItemIndex]));
                    UnitaleUtil.PlaySound("SeparateSound", "ShopSuccess");
                    PlayerCharacter.instance.SetGold(PlayerCharacter.instance.Gold + Inventory.NametoPrice[Inventory.inventory[currentItemIndex].Name] / 5);
                    Inventory.RemoveItem(currentItemIndex);
                    if (currentItemIndex == Inventory.inventory.Count && Inventory.inventory.Count != 1)
                        currentItemIndex--;
                }
                if (!interrupted) {
                    currentItemIndex = 0;
                    selection = currentItemIndex;
                    HandleCancel();
                }
                break;
        }
    }

    private void HandleCancel(bool fromKey = false) {
        switch (currentState) {
            case State.MENU:        if (!fromKey) ChangeState(State.EXIT);                 break;
            case State.BUY:
            case State.SELL:
            case State.TALK:        ChangeState(State.MENU, (int)(currentState - 1) / 2 ); break;
            case State.BUYCONFIRM:  ChangeState(State.BUY, currentItemIndex);              break;
            case State.SELLCONFIRM: ChangeState(State.SELL, currentItemIndex);             break;
        }
    }

    private void SelectionInputManager() {
        utHeart.GetComponent<Image>().enabled = true;
        int xMov = GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED ? -1 : GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED ? 1 : 0;
        int yMov = GlobalControls.input.Up   == UndertaleInput.ButtonState.PRESSED ? -1 : GlobalControls.input.Down  == UndertaleInput.ButtonState.PRESSED ? 1 : 0;
        if (xMov != 0 || yMov != 0) {
            switch (currentState) {
                case State.MENU:        selection = UnitaleUtil.SelectionChoice(4, selection, xMov, yMov, 4, 1); break;
                case State.TALK:
                case State.BUY:         selection = UnitaleUtil.SelectionChoice(numberOfChoices, selection, xMov, yMov, numberOfChoices, 1); break;
                case State.BUYCONFIRM:  selection = UnitaleUtil.SelectionChoice(2, selection, xMov, yMov, 2, 1); break;
                case State.SELLCONFIRM: selection = UnitaleUtil.SelectionChoice(2, selection, xMov, yMov, 1, 2, false); break;
                case State.SELL:
                    if (selection == 8 && yMov == -1)
                        selection = numberOfChoices - 2 - (numberOfChoices - 2) % 2;
                    else
                        selection = UnitaleUtil.SelectionChoice(selection < 8 && (xMov != 0 || selection % 2 == 1) ? numberOfChoices - 1 : 9, selection, xMov, yMov, 5, 2);

                    if (currentState == State.SELL && selection >= numberOfChoices - 1)
                        selection = 8;
                    break;
            }
            SetPlayerOnSelection();
        } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED)
            HandleAction();
        else if (GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED)
            HandleCancel(true);
    }

    private void TextInputManager() {
        if (GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED && !tmBigTalk.LineComplete() && tmBigTalk.CanSkip()) {
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
                    script.Remove();
                    DontDestroyOnLoad(tp);
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

    // Update is called once per frame
    private void Update() {
        if (script.GetVar("Update") != null)
            UnitaleUtil.TryCall(script, "Update");
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
