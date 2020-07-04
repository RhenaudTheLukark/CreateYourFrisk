using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using MoonSharp.Interpreter;

public class LuaTextManager : TextManager {
    private GameObject container;
    private GameObject containerBubble;
    private RectTransform speechThing;
    private RectTransform speechThingShadow;
    private DynValue bubbleLastVar = DynValue.NewNil();
    private bool bubble = true;
    private int framesWait = 60;
    private int countFrames;
    private int _bubbleHeight = -1;
    private BubbleSide bubbleSide = BubbleSide.NONE;
    private ProgressMode progress = ProgressMode.AUTO;
    private Color textColor;
    private float xScale = 1;
    private float yScale = 1;

    private bool autoDestroyed;
    public bool isactive {
        get {
            return container != null && containerBubble != null && speechThing != null && speechThingShadow != null && !autoDestroyed;
        }
    }

    private enum BubbleSide { LEFT = 0, DOWN = 90, RIGHT = 180, UP = 270, NONE = -1 }
    private enum ProgressMode { AUTO, MANUAL, NONE }

    protected override void Awake() {
        base.Awake();
        if (!UnitaleUtil.IsOverworld)
            transform.parent.SetParent(GameObject.Find("TopLayer").transform);
        container = transform.parent.gameObject;
        containerBubble = UnitaleUtil.GetChildPerName(container.transform, "BubbleContainer").gameObject;
        speechThing = UnitaleUtil.GetChildPerName(containerBubble.transform, "SpeechThing", false, true).GetComponent<RectTransform>();
        speechThingShadow = UnitaleUtil.GetChildPerName(containerBubble.transform, "SpeechThingShadow", false, true).GetComponent<RectTransform>();
    }

    protected override void Update() {
        base.Update();

        //Next line/EOF check
        if (!isactive) return;
        switch (progress) {
            case ProgressMode.MANUAL: {
                if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED && LineComplete())
                    Advance();
                break;
            }
            case ProgressMode.AUTO: {
                if (LineComplete())
                    if (countFrames == framesWait) {
                        Advance();
                        countFrames = 0;
                    } else
                        countFrames++;
                break;
            }
        }
        if (CanAutoSkipAll() || CanAutoSkipThis())
            Advance();
        if (CanSkip() && !LineComplete() && GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED)
            DoSkipFromPlayer();
    }

    // Used to test if a text object still exists.
    private void CheckExists() {
        if (!isactive)
            throw new CYFException("Attempt to perform action on removed text object.");
    }

    public void DestroyText() {
        if (!isactive)
            throw new CYFException("Attempt to remove a removed text object.");
        autoDestroyed = true;
        GameObject.Destroy(this.transform.parent.gameObject);
    }

    // Shortcut to `DestroyText()`
    public void Remove() { DestroyText(); }

    private void ResizeBubble() {
        float effectiveBubbleHeight = bubbleHeight != -1 ? bubbleHeight < 16 ? 40 : bubbleHeight + 24 : UnitaleUtil.CalcTextHeight(this) < 16 ? 40 : UnitaleUtil.CalcTextHeight(this) + 24;
        containerBubble.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(textMaxWidth + 20, effectiveBubbleHeight);                                                      //To set the borders
        UnitaleUtil.GetChildPerName(containerBubble.transform, "BackHorz").GetComponent<RectTransform>().sizeDelta = new Vector2(textMaxWidth + 20, effectiveBubbleHeight - 20 * 2);    //BackHorz
        UnitaleUtil.GetChildPerName(containerBubble.transform, "BackVert").GetComponent<RectTransform>().sizeDelta = new Vector2(textMaxWidth - 20, effectiveBubbleHeight);             //BackVert
        UnitaleUtil.GetChildPerName(containerBubble.transform, "CenterHorz").GetComponent<RectTransform>().sizeDelta = new Vector2(textMaxWidth + 16, effectiveBubbleHeight - 16 * 2);  //CenterHorz
        UnitaleUtil.GetChildPerName(containerBubble.transform, "CenterVert").GetComponent<RectTransform>().sizeDelta = new Vector2(textMaxWidth - 16, effectiveBubbleHeight - 4);       //CenterVert
        SetSpeechThingPositionAndSide(bubbleSide.ToString(), bubbleLastVar);
    }

    public string progressmode {
        get {
            CheckExists();
            return progress.ToString();
        }
        set {
            CheckExists();
            try {
                progress = (ProgressMode)Enum.Parse(typeof(ProgressMode), value.ToUpper());
            } catch {
                if (value != null) throw new CYFException("text.progressmode can only have either \"AUTO\", \"MANUAL\" or \"NONE\", but you entered \"" + value.ToUpper() + "\".");
                                   throw new CYFException("text.progressmode can only have either \"AUTO\", \"MANUAL\" or \"NONE\", but you set it to a nil value.");
            }
        }
    }

    public int x {
        get {
            CheckExists();
            return Mathf.RoundToInt(container.transform.localPosition.x);
        }
        set { MoveTo(value, y); }
    }

    public int y {
        get {
            CheckExists();
            return Mathf.RoundToInt(container.transform.localPosition.y);
        }
        set { MoveTo(x, value); }
    }

    public int absx {
        get {
            CheckExists();
            return Mathf.RoundToInt(container.transform.position.x);
        }
        set { MoveToAbs(value, absy); }
    }

    public int absy {
        get {
            CheckExists();
            return Mathf.RoundToInt(container.transform.position.y);
        }
        set { MoveTo(absx, value); }
    }

    public int textMaxWidth {
        get {
            CheckExists();
            return _textMaxWidth;
        }
        set {
            CheckExists();
            _textMaxWidth = value < 16 ? 16 : value;
        }
    }

    public int bubbleHeight {
        get {
            CheckExists();
            return _bubbleHeight;
        }
        set {
            CheckExists();
            _bubbleHeight = value == -1 ? -1 : value < 40 ? 40 : value;
        }
    }

    public float xscale {
        get { return xScale; }
        set {
            xScale = value;
            Scale(xScale, yScale);
        }
    }

    public float yscale {
        get { return yScale; }
        set {
            yScale = value;
            Scale(xScale, yScale);
        }
    }

    public void Scale(float xs, float ys) {
        CheckExists();
        xScale = xs;
        yScale = ys;

        container.gameObject.GetComponent<RectTransform>().localScale = new Vector3(xs, ys, 1.0f);
    }

    public string layer {
        get {
            CheckExists();
            if (!container.transform.parent.name.Contains("Layer"))
                return "spriteObject";
            return container.transform.parent.name.Substring(0, container.transform.parent.name.Length - 5);
        }
        set {
            CheckExists();
            try {
                container.transform.SetParent(GameObject.Find(value + "Layer").transform);
                foreach (Transform child in container.transform) {
                    MaskImage childmask = child.gameObject.GetComponent<MaskImage>();
                    if (childmask != null)
                        childmask.inverted = false;
                }
            }
            catch { throw new CYFException("The layer \"" + value + "\" doesn't exist."); }
        }
    }

    public void MoveBelow(LuaTextManager otherText) {
        CheckExists();
        if (otherText == null || !otherText.isactive)                     throw new CYFException("The text object passed as an argument is nil or inactive.");
        if (transform.parent.parent != otherText.transform.parent.parent) UnitaleUtil.Warn("You can't change the order of two text objects without the same parent.");
        else {
            try { transform.parent.SetSiblingIndex(otherText.transform.parent.GetSiblingIndex()); }
            catch { throw new CYFException("Error while calling text.MoveBelow."); }
        }
    }

    public void MoveAbove(LuaTextManager otherText) {
        CheckExists();
        if (otherText == null || !otherText.isactive)                     throw new CYFException("The text object passed as an argument is nil or inactive.");
        if (transform.parent.parent != otherText.transform.parent.parent) UnitaleUtil.Warn("You can't change the order of two text objects without the same parent.");
        else {
            try { transform.parent.SetSiblingIndex(otherText.transform.parent.GetSiblingIndex() + 1); }
            catch { throw new CYFException("Error while calling text.MoveAbove."); }
        }
    }

    [MoonSharpHidden] public Color _color = Color.white;
    [MoonSharpHidden] public bool hasColorBeenSet;
    [MoonSharpHidden] public bool hasAlphaBeenSet;
    // The color of the text. It uses an array of three floats between 0 and 1
    public float[] color {
        get { return new[] { _color.r, _color.g, _color.b }; }
        set {
            CheckExists();
            if (value == null)
                throw new CYFException("text.color can not be set to a nil value.");
            switch (value.Length) {
                // If we don't have three or four floats, we throw an error
                case 3: _color = new Color(value[0], value[1], value[2], alpha);    break;
                case 4: _color = new Color(value[0], value[1], value[2], value[3]); break;
                default:
                    throw new CYFException("You need 3 or 4 numeric values when setting a text's color.");
            }

            hasColorBeenSet = true;
            hasAlphaBeenSet = value.Length == 4;

            foreach (Image i in letterReferences)
                if (i != null)
                    if (i.color == defaultColor) i.color = _color;
                    else                         break; // Only because we can't go back to the default color

            if (currentColor == defaultColor)
                currentColor = _color;
            defaultColor = _color;
        }
    }

    // The color of the text on a 32 bits format. It uses an array of three or four floats between 0 and 255
    public float[] color32 {
        // We need first to convert the Color into a Color32, and then get the values.
        get { return new float[] { ((Color32)_color).r, ((Color32)_color).g, ((Color32)_color).b }; }
        set {
            CheckExists();
            if (value == null)
                throw new CYFException("text.color32 can not be set to a nil value.");
            switch (value.Length) {
                // If we don't have three or four floats, we throw an error
                case 3: color = new[] { value[0] / 255, value[1] / 255, value[2] / 255, alpha };          break;
                case 4: color = new[] { value[0] / 255, value[1] / 255, value[2] / 255, value[3] / 255 }; break;
                default:
                    throw new CYFException("You need 3 or 4 numeric values when setting a text's color.");
            }
        }
    }

    // The alpha of the text. It is clamped between 0 and 1
    public float alpha {
        get { return _color.a; }
        set {
            CheckExists();
            color = new[] { _color.r, _color.g, _color.b, Mathf.Clamp01(value) };
            hasAlphaBeenSet = true;
        }
    }

    // The alpha of the text in a 32 bits format. It is clamped between 0 and 255
    public float alpha32 {
        get { return ((Color32)_color).a; }
        set {
            CheckExists();
            alpha = value / 255;
        }
    }

    public DynValue GetLetters() {
        Table table = new Table(null);
        int key = 0;
        foreach (Image i in letterReferences)
            if (i != null) {
                key++;
                LuaSpriteController letter = new LuaSpriteController(i) { tag = "letter" };
                table.Set(key, UserData.Create(letter, LuaSpriteController.data));
            }
        return DynValue.NewTable(table);
    }

    public bool lineComplete {
        get {
            CheckExists();
            return LineComplete();
        }
    }

    public bool allLinesComplete {
        get { return AllLinesComplete(); }
    }

    public void SetParent(LuaSpriteController parent) {
        CheckExists();
        if (parent != null && parent.img.transform != null && parent.img.transform.parent.name == "SpritePivot")
            throw new CYFException("text.SetParent(): Can not use SetParent with an Overworld Event's sprite.");
        try {
            if (parent == null) throw new CYFException("text.SetParent(): Can't set a sprite's parent as nil.");
            container.transform.SetParent(parent.img.transform);
            foreach (Transform child in container.transform) {
                MaskImage childmask = child.gameObject.GetComponent<MaskImage>();
                if (childmask != null)
                    childmask.inverted = parent._masked == LuaSpriteController.MaskMode.INVERTEDSPRITE || parent._masked == LuaSpriteController.MaskMode.INVERTEDSTENCIL;
            }
        }
        catch { throw new CYFException("You tried to set a removed sprite/nil sprite as this text object's parent."); }
    }

    public void SetText(DynValue text) {
        // disable late start if SetText is used on the same frame the text is created
        lateStartWaiting = false;

        if (text == null || text.Type != DataType.Table && text.Type != DataType.String)
            throw new CYFException("Text.SetText: the text argument must be a non-empty array of strings or a simple string.");

        // Converts the text argument into a table if it's a simple string
        text = text.Type == DataType.String ? DynValue.NewTable(null, text) : text;

        TextMessage[] msgs = new TextMessage[text.Table.Length];
        for (int i = 0; i < text.Table.Length; i++)
            msgs[i] = new TextMessage(text.Table.Get(i + 1).String, false, false);
        if (bubble)
            containerBubble.SetActive(true);
        try { SetTextQueue(msgs); }
        catch { /* ignored */ }

        if (text.Table.Length != 0 && bubble)
            ResizeBubble();
    }

    [MoonSharpHidden] public void LateStart() { StartCoroutine(LateStartSetText()); }

    private IEnumerator LateStartSetText() {
        yield return new WaitForEndOfFrame();

        if (!isactive)
            yield break;

        letterSound.clip = default_voice ?? default_charset.Sound;

        // only allow inline text commands and letter sounds on the second frame
        lateStartWaiting = false;

        currentLine = -1;
        Advance();
        if (bubble)
            ResizeBubble();
    }

    public void AddText(DynValue text) {
        CheckExists();

        // Checks if the parameter given is valid
        if (text == null || text.Type != DataType.Table && text.Type != DataType.String)
            throw new CYFException("Text.AddText: the text argument must be a non-empty array of strings or a simple string.");

        // Converts the text argument into a table if it's a simple string
        text = text.Type == DataType.String ? DynValue.NewTable(null, text) : text;

        if (AllLinesComplete()) {
            SetText(text);
            return;
        }
        TextMessage[] msgs = new TextMessage[text.Table.Length];
        for (int i = 0; i < text.Table.Length; i++)
            msgs[i] = new MonsterMessage(text.Table.Get(i + 1).String);
        AddToTextQueue(msgs);
    }

    public void SetVoice(string voiceName) {
        if (voiceName == null)
            throw new CYFException("Text.SetVoice: The first argument (the voice name) is nil.\n\nSee the documentation for proper usage.");
        CheckExists();
        default_voice = voiceName == "none" ? null : AudioClipRegistry.GetVoice(voiceName);
    }

    public void SetFont(string fontName, bool firstTime = false) {
        if (fontName == null)
            throw new CYFException("Text.SetFont: The first argument (the font name) is nil.\n\nSee the documentation for proper usage.");
        CheckExists();
        UnderFont uf = SpriteFontRegistry.Get(fontName);
        if (uf == null)
            throw new CYFException("The font \"" + fontName + "\" doesn't exist.\nYou should check if you made a typo, or if the font really is in your mod.");
        SetFont(uf, firstTime);
        if (!firstTime)
            default_charset = uf;
        UpdateBubble();
    }

    [MoonSharpHidden] public void UpdateBubble() {
        containerBubble.GetComponent<RectTransform>().localPosition = new Vector2(-12, 24);
        // GetComponent<RectTransform>().localPosition = new Vector2(0, 16);
        GetComponent<RectTransform>().localPosition = new Vector2(0, 0);
    }

    public void SetEffect(string effect, float intensity = -1) {
        if (effect == null)
            throw new CYFException("Text.SetEffect: The first argument (the effect name) is nil.\n\nSee the documentation for proper usage.");
        CheckExists();
        switch (effect.ToLower()) {
            case "none":   textEffect = null;                                                                                         break;
            case "twitch": textEffect = intensity != -1 ? new TwitchEffect(this, intensity)   : new TwitchEffect(this);   break;
            case "shake":  textEffect = intensity != -1 ? new ShakeEffect(this, intensity)    : new ShakeEffect(this);    break;
            case "rotate": textEffect = intensity != -1 ? new RotatingEffect(this, intensity) : new RotatingEffect(this); break;

            default:
                throw new CYFException("The effect \"" + effect + "\" doesn't exist.\nYou can only choose between \"none\", \"twitch\", \"shake\" and \"rotate\".");
        }
    }

    public void ShowBubble(string side = null, DynValue position = null) {
        bubble = true;
        containerBubble.SetActive(true);
        SetSpeechThingPositionAndSide(side, position);
    }

    // Shortcut to `SetSpeechThingPositionAndSide`
    public void SetTail(string side, DynValue position) { SetSpeechThingPositionAndSide(side, position); }

    public void SetSpeechThingPositionAndSide(string side, DynValue position) {
        CheckExists();
        bubbleLastVar = position;
        try { bubbleSide = side != null ? (BubbleSide)Enum.Parse(typeof(BubbleSide), side.ToUpper()) : BubbleSide.NONE; }
        catch { throw new CYFException("The speech thing (tail) can only take \"RIGHT\", \"DOWN\" ,\"LEFT\" ,\"UP\" or \"NONE\" as a positional value, but you entered \"" + side.ToUpper() + "\"."); }

        if (bubbleSide != BubbleSide.NONE) {
            speechThing.gameObject.SetActive(true);
            speechThingShadow.gameObject.SetActive(true);
            speechThing.anchorMin = speechThing.anchorMax = speechThingShadow.anchorMin = speechThingShadow.anchorMax =
                new Vector2(bubbleSide == BubbleSide.LEFT ? 0 : bubbleSide == BubbleSide.RIGHT ? 1 : 0.5f,
                            bubbleSide == BubbleSide.DOWN ? 0 : bubbleSide == BubbleSide.UP ? 1 : 0.5f);
            speechThing.rotation = speechThingShadow.rotation = Quaternion.Euler(0, 0, (int)bubbleSide);
            bool isSide = bubbleSide == BubbleSide.LEFT || bubbleSide == BubbleSide.RIGHT;
            int size = isSide ? (int)containerBubble.GetComponent<RectTransform>().sizeDelta.y - 20 : (int)containerBubble.GetComponent<RectTransform>().sizeDelta.x - 20;
            if (position == null)
                speechThing.anchoredPosition = speechThingShadow.anchoredPosition = new Vector3(0, 0);
            else {
                switch (position.Type) {
                    case DataType.Number: {
                        float number = (float)position.Number < 0 ? (float)position.Number : (float)position.Number - size / 2f;
                        speechThing.anchoredPosition = speechThingShadow.anchoredPosition = new Vector3(isSide  ? 0 : Mathf.Clamp(number, -size / 2f, size / 2f),
                                                                                                        !isSide ? 0 : Mathf.Clamp(number, -size / 2f, size / 2f));
                        break;
                    }
                    case DataType.String: {
                        string str = position.String.Replace(" ", "");
                        if (str.Contains("%")) {
                            try {
                                float percentage = Mathf.Clamp01(ParseUtil.GetFloat(str.Replace("%", "")) / 100),
                                      x          = isSide  ? 0 : Mathf.Round(percentage * size) - size / 2f,
                                      y          = !isSide ? 0 : Mathf.Round(percentage * size) - size / 2f;
                                speechThing.anchoredPosition = speechThingShadow.anchoredPosition = new Vector3(x, y);
                            } catch { throw new CYFException("If you use a '%' in your string, you should only have a number with it."); }
                        } else
                            throw new CYFException("You need to use a '%' in order to exploit the string.");

                        break;
                    }
                }
            }
        } else {
            speechThing.gameObject.SetActive(false);
            speechThingShadow.gameObject.SetActive(false);
        }
    }

    public void HideBubble() {
        CheckExists();
        bubble = false;
        containerBubble.SetActive(false);
    }

    public override void SkipLine() {
        if (noSkip1stFrame) return;
        if (GlobalControls.isInFight && EnemyEncounter.script.GetVar("playerskipdocommand").Boolean
         || UnitaleUtil.IsOverworld && (EventManager.instance.script != null && EventManager.instance.script.GetVar("playerskipdocommand").Boolean
         || GlobalControls.isInShop && GameObject.Find("Canvas").GetComponent<ShopScript>().script.GetVar("playerskipdocommand").Boolean))
            DoSkipFromPlayer();
        else
            base.SkipLine();
    }

    private void Advance() {
        NextLine();
        if (caller.script.Globals["OnTextAdvance"] == null || caller.script.Globals.Get("OnTextAdvance") == null) return;
        try {caller.script.Call(caller.script.Globals["OnTextAdvance"], this, autoDestroyed); }
        catch (ScriptRuntimeException ex) { UnitaleUtil.DisplayLuaError(caller.scriptname, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message, ex.DoNotDecorateMessage); }
    }

    public void NextLine() {
        CheckExists();
        if (AllLinesComplete()) {
            if (bubble)
                containerBubble.SetActive(false);
            DestroyText();
            autoDestroyed = true;
        } else {
            ShowLine(++currentLine);
            if (bubble)
                ResizeBubble();
        }
    }

    // Shortcut to `SetAutoWaitTimeBetweenTexts`
    public void SetWaitTime(int time) { SetAutoWaitTimeBetweenTexts(time); }

    public void SetAutoWaitTimeBetweenTexts(int time) {
        CheckExists();
        framesWait = time;
    }

    public void MoveTo(int newX, int newY) {
        CheckExists();
        container.transform.localPosition = new Vector3(newX, newY, container.transform.localPosition.z);
    }

    public void MoveToAbs(int newX, int newY) {
        CheckExists();
        container.transform.position = new Vector3(newX, newY, container.transform.position.z);
    }

    public void SetAnchor(float newX, float newY) {
        CheckExists();
        container.GetComponent<RectTransform>().anchorMin = new Vector2(newX, newY);
        container.GetComponent<RectTransform>().anchorMax = new Vector2(newX, newY);
    }

    public int GetTextWidth() {
        CheckExists();
        return (int)UnitaleUtil.CalcTextWidth(this);
    }

    public int GetTextHeight() {
        CheckExists();
        return (int)UnitaleUtil.CalcTextHeight(this);
    }
}