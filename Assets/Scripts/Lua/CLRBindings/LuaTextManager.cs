using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

public class LuaTextManager : TextManager {
    private GameObject container;
    private bool removed;
    private bool hidden;
    private GameObject containerBubble;
    private RectTransform speechThing;
    private RectTransform speechThingShadow;
    private DynValue bubbleLastVar = DynValue.NewNil();
    public bool bubble;
    private int framesWait = 60;
    private int countFrames;
    private int _bubbleHeight = -1;
    private BubbleSide bubbleSide = BubbleSide.NONE;
    [SerializeField] private ProgressMode progress = ProgressMode.AUTO;
    private float xScale = 1;
    private float yScale = 1;
    private string _linePrefix = "";
    [MoonSharpHidden] public bool autoSetLayer = true;
    private readonly Dictionary<string, DynValue> vars = new Dictionary<string, DynValue>();
    [MoonSharpHidden] public bool needFontReset = false;
    [MoonSharpHidden] public bool isMainTextObject = false;

    // Variables used to store text display adjustment variables
    public Dictionary<int, float> xScaleSizes = new Dictionary<int, float>();
    public Dictionary<int, float> yScaleSizes = new Dictionary<int, float>();
    private float lastXScale = 1, lastYScale = 1;

    // Whether we correct the text's display (position, scale) to not look jagged
    private static bool globalAdjustTextPos {
        get {
            if (!GlobalControls.isInFight)
                return false;
            if (EnemyEncounter.script.GetVar("noadjusttextdisplay").Type == DataType.Boolean)
                return !EnemyEncounter.script.GetVar("noadjusttextdisplay").Boolean;
            return true;
        }
    }
    private bool adjustTextDisplaySet = false;
    private bool _adjustTextDisplay = true;
    public bool adjustTextDisplay {
        get {
            if (adjustTextDisplaySet)
                return _adjustTextDisplay;
            if (rotation % 90 != 0)
                return false;
            return globalAdjustTextPos;
        }
        set {
            adjustTextDisplaySet = true;
            _adjustTextDisplay = value;
            if (!adjustTextDisplay)
                foreach (LetterData l in letters)
                    l.image.GetComponent<RectTransform>().localScale = Vector2.one;
            Move(0, 0);
            Scale(xscale, yscale);
        }
    }

    public bool isactive {
        get { return !removed && !hidden; }
    }

    // The rotation of the text object
    public new float rotation {
        get { return container.transform.eulerAngles.z; }
        set {
            // We mod the value from 0 to 360 because angles are between 0 and 360 normally
            internalRotation.z = Math.Mod(value, 360);
            container.transform.eulerAngles = internalRotation;
        }
    }

    private enum BubbleSide { LEFT = 0, DOWN = 90, RIGHT = 180, UP = 270, NONE = -1 }
    private enum ProgressMode { AUTO, MANUAL, NONE }

    public bool deleteWhenFinished = true;

    public GameObject GetContainer() {
        return container;
    }

    protected override void Awake() {
        container = transform.parent.gameObject;
        base.Awake();
        if (!UnitaleUtil.IsOverworld && autoSetLayer)
            transform.parent.SetParent(GameObject.Find("TopLayer").transform);

        Transform bubbleTransform = UnitaleUtil.GetChildPerName(container.transform, "BubbleContainer", true);
        if (bubbleTransform != null) {
            containerBubble = bubbleTransform.gameObject;
            speechThing = UnitaleUtil.GetChildPerName(containerBubble.transform, "SpeechThing", false, true).GetComponent<RectTransform>();
            speechThingShadow = UnitaleUtil.GetChildPerName(containerBubble.transform, "SpeechThingShadow", false, true).GetComponent<RectTransform>();
        }
    }

    protected override void Update() {
        if (hidden) return;
        base.Update();

        if (!isactive || textQueue == null || textQueue.Length == 0) return;
        //Next line/EOF check
        switch (progress) {
            case ProgressMode.MANUAL: {
                if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED && LineComplete())
                    NextLine();
                break;
            }
            case ProgressMode.AUTO: {
                if (LineComplete())
                    if (countFrames == framesWait) {
                        NextLine();
                        countFrames = 0;
                    } else
                        countFrames++;
                break;
            }
        }
        if ((CanAutoSkipAll() || CanAutoSkipThis()) && (!UIController.instance || !UIController.instance.monsterDialogues.Contains(this)))
            NextLine();
        if (CanSkip() && !LineComplete() && GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED)
            DoSkipFromPlayer();
    }

    // Used to test if a text object still exists.
    private void CheckExists() {
        if (removed)
            throw new CYFException("Attempt to perform action on removed text object.");
    }

    public void Remove() { DestroyText(); }
    public void DestroyText() {
        if (!removed) Destroy(transform.parent.gameObject);
        removed = true;
    }

    [MoonSharpHidden] public void HideTextObject() {
        DestroyChars();
        hidden = true;
    }

    private void ResizeBubble() {
        float effectiveBubbleHeight = bubbleHeight != -1 ? bubbleHeight < 16 ? 40 : bubbleHeight + 24 : UnitaleUtil.CalcTextHeight(this) < 16 ? 40 : UnitaleUtil.CalcTextHeight(this) + 24;
        containerBubble.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(textMaxWidth + 20, effectiveBubbleHeight);                                                      //To set the borders
        if (UnitaleUtil.GetChildPerName(containerBubble.transform, "BackHorz")) {
            UnitaleUtil.GetChildPerName(containerBubble.transform, "BackHorz").GetComponent<RectTransform>().sizeDelta = new Vector2(textMaxWidth + 20, effectiveBubbleHeight - 20 * 2);    //BackHorz
            UnitaleUtil.GetChildPerName(containerBubble.transform, "BackVert").GetComponent<RectTransform>().sizeDelta = new Vector2(textMaxWidth - 20, effectiveBubbleHeight);             //BackVert
            UnitaleUtil.GetChildPerName(containerBubble.transform, "CenterHorz").GetComponent<RectTransform>().sizeDelta = new Vector2(textMaxWidth + 16, effectiveBubbleHeight - 16 * 2);  //CenterHorz
            UnitaleUtil.GetChildPerName(containerBubble.transform, "CenterVert").GetComponent<RectTransform>().sizeDelta = new Vector2(textMaxWidth - 16, effectiveBubbleHeight - 4);       //CenterVert
        }
        SetTail(bubbleSide.ToString(), bubbleLastVar);
    }

    public Vector2 GetBubbleSize() {
        return containerBubble.transform.GetComponent<RectTransform>().sizeDelta;
    }

    public Vector2 GetBubbleShift() {
        return containerBubble.GetComponent<RectTransform>().localPosition;
    }

    public DynValue text {
        get {
            CheckExists();
            DynValue[] texts = new DynValue[textQueue.Length];
            for (int i = 0; i < textQueue.Length; i++)
                texts[i] = DynValue.NewString(textQueue[i].Text);
            return DynValue.NewTable(caller.script, texts);
        }
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

    public float x {
        get {
            CheckExists();
            return container.transform.localPosition.x;
        }
        set { MoveTo(value, y); }
    }

    public float y {
        get {
            CheckExists();
            return container.transform.localPosition.y;
        }
        set { MoveTo(x, value); }
    }

    public float absx {
        get {
            CheckExists();
            return container.transform.position.x;
        }
        set { MoveToAbs(value, absy); }
    }

    public float absy {
        get {
            CheckExists();
            return container.transform.position.y;
        }
        set { MoveToAbs(absx, value); }
    }

    public int width {
        get {
            CheckExists();
            return _textMaxWidth;
        }
        set {
            CheckExists();
            _textMaxWidth = value < 16 ? 16 : value;
        }
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
        get {
            CheckExists();
            return xScale;
        }
        set {
            CheckExists();
            xScale = value;
            Scale(xScale, yScale);
        }
    }

    public float yscale {
        get {
            CheckExists();
            return yScale;
        }
        set {
            CheckExists();
            yScale = value;
            Scale(xScale, yScale);
        }
    }

    public void Scale(float xs, float ys) {
        CheckExists();
        xScale = xs;
        yScale = ys;

        container.transform.localScale = new Vector2(xs, ys);
        if (adjustTextDisplay) {
            PostScaleHandling();
            MoveLetters();
        }
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
                SetParent(GameObject.Find(value + "Layer").transform);
                foreach (Transform child in container.transform) {
                    MaskImage childmask = child.gameObject.GetComponent<MaskImage>();
                    if (childmask != null)
                        childmask.inverted = false;
                }
            }
            catch { throw new CYFException("The layer \"" + value + "\" doesn't exist."); }
        }
    }

    public void SendToTop() {
        CheckExists();
        container.transform.SetAsLastSibling();
    }

    public void SendToBottom() {
        CheckExists();
        container.transform.SetAsFirstSibling();
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

    public void ResetColor(bool resetAlpha = false) {
        CheckExists();
        if (resetAlpha)
            ResetAlpha();
        color = new[] { fontDefaultColor.r, fontDefaultColor.g, fontDefaultColor.b };
        textColorSet = false;
    }

    public void ResetAlpha() {
        CheckExists();
        alpha = fontDefaultColor.a;
        textAlphaSet = false;
    }

    [MoonSharpHidden] public Color _color = Color.white;
    [MoonSharpHidden] public bool textColorSet, textAlphaSet;
    // The color of the text. It uses an array of three floats between 0 and 1
    public float[] color {
        get {
            CheckExists();
            return new[] { _color.r, _color.g, _color.b };
        }
        set {
            CheckExists();
            if (value == null)
                throw new CYFException("text.color can not be set to a nil value.");
            if (value.Length < 3 || value.Length > 4)
                throw new CYFException("You need 3 or 4 numeric values when setting a text's color.");

            _color.r = value[0];
            _color.g = value[1];
            _color.b = value[2];
            if (value.Length == 4)
                alpha = value[3];

            textColorSet = true;

            foreach (LetterData l in letters.Where(i => !i.commandColorSet))
                l.image.color = new Color(_color.r, _color.g, _color.b, l.image.color.a);

            if (!commandColorSet)
                commandColor = _color;
            defaultColor = _color;
        }
    }

    // The color of the text on a 32 bits format. It uses an array of three or four floats between 0 and 255
    public float[] color32 {
        // We need first to convert the Color into a Color32, and then get the values.
        get {
            CheckExists();
            return new float[] { ((Color32)_color).r, ((Color32)_color).g, ((Color32)_color).b };
        }
        set {
            CheckExists();
            if (value == null)
                throw new CYFException("text.color32 can not be set to a nil value.");
            if (value.Length < 3 || value.Length > 4)
                throw new CYFException("You need 3 or 4 numeric values when setting a text's color.");
            color = value.Select(v => v / 255).ToArray();
        }
    }

    // The alpha of the text. It is clamped between 0 and 1
    public float alpha {
        get {
            CheckExists();
            return _color.a;
        }
        set {
            CheckExists();
            _color.a = Mathf.Clamp01(value);
            textAlphaSet = true;
            textColorSet = true; // TODO: Remove in 0.7

            foreach (LetterData l in letters.Where(i => !i.commandAlphaSet))
                //l.image.color = new Color(l.image.color.r, l.image.color.g, l.image.color.b, _color.a); // TODO: Remove in 0.7
                l.image.color = _color;

            if (!commandAlphaSet)
                commandColor.a = _color.a;
            defaultColor.a = _color.a;
        }
    }

    // The alpha of the text in a 32 bits format. It is clamped between 0 and 255
    public float alpha32 {
        get {
            CheckExists();
            return ((Color32)_color).a;
        }
        set {
            CheckExists();
            alpha = value / 255;
        }
    }


    public string linePrefix {
        get {
            CheckExists();
            return _linePrefix;
        }
        set {
            CheckExists();
            _linePrefix = value;
        }
    }

    private DynValue _OnTextDisplay = DynValue.Nil;
    public DynValue OnTextDisplay {
        get { return _OnTextDisplay; }
        set {
            if ((value.Type & (DataType.Nil | DataType.Function | DataType.ClrFunction)) == 0)
                throw new CYFException("Text.OnTextDisplay: This variable has to be a function!");
            if (value.Type == DataType.Function && value.Function.OwnerScript != caller.script)
                throw new CYFException("Text.OnTextDisplay: You can only use a function created in the same script as the text object!");
            _OnTextDisplay = value;
        }
    }

    public DynValue GetLetters() {
        CheckExists();
        if (lateStartWaiting)
            throw new CYFException("You cannot fetch a text object's letters on the first frame it was created, unless you use the [instant] command at the beginning of its line.");
        Table table = new Table(null);
        int key = 0;
        foreach (LetterData d in letters) {
            key++;
            LuaSpriteController letter = LuaSpriteController.GetOrCreate(d.image.gameObject);
            letter.tag = "letter";
            letter.spritename = textQueue[currentLine].Text[d.index].ToString();
            letter.img.GetComponent<Letter>().characterNumber = d.index;
            table.Set(key, UserData.Create(letter, LuaSpriteController.data));
        }

        return DynValue.NewTable(table);
    }

    public bool lineComplete {
        get {
            return LineComplete();
        }
    }

    public bool allLinesComplete {
        get {
            return AllLinesComplete();
        }
    }

    public void SetText(DynValue text, bool resetLateStart = true) {
        CheckExists();
        hidden = false;

        // Disable late start if SetText is used on the same frame the text is created
        if (resetLateStart)
            lateStartWaiting = false;

        if (text == null || text.Type != DataType.Table && text.Type != DataType.String)
            throw new CYFException("Text.SetText: the text argument must be a non-empty array of strings or a simple string.");

        // Convert the text argument into a table if it's a simple string
        text = text.Type == DataType.String ? DynValue.NewTable(null, text) : text;

        TextMessage[] msgs = new TextMessage[text.Table.Length];
        for (int i = 0; i < text.Table.Length; i++)
            msgs[i] = new TextMessage(linePrefix + text.Table.Get(i + 1).String, false, false);
        if (bubble)
            containerBubble.SetActive(true);
        try { SetTextQueue(msgs); }
        catch { /* ignored */ }

        if (text.Table.Length != 0 && bubble)
            ResizeBubble();
    }

    protected override void SpawnText() {
        base.SpawnText();
        if ((OnTextDisplay.Type & (DataType.Function | DataType.ClrFunction)) != 0)
            caller.Call(OnTextDisplay, "OnTextDisplay", UserData.Create(this));
        else if (GlobalControls.isInFight && (EnemyEncounter.script.script.Globals.Get("OnTextDisplay").Type & (DataType.Function | DataType.ClrFunction)) != 0)
            EnemyEncounter.script.Call("OnTextDisplay", UserData.Create(this));
    }

    [MoonSharpHidden] public void LateStart() { StartCoroutine(LateStartSetText()); }

    private IEnumerator LateStartSetText(bool waitUntilEndOfFrame = true) {
        if (waitUntilEndOfFrame)
            yield return new WaitForEndOfFrame();

        if (!isactive || !lateStartWaiting)
            yield break;

        if (linePrefix != "")
            foreach (TextMessage tm in textQueue)
                tm.Text = linePrefix + tm.Text;

        // Only allow inline text commands and letter sounds on the second frame
        lateStartWaiting = false;

        ShowLine(0);
        if (bubble)
            UpdateBubble();
    }

    public void AddText(DynValue text) {
        CheckExists();

        // Checks if the parameter given is valid
        if (text == null || text.Type != DataType.Table && text.Type != DataType.String)
            throw new CYFException("Text.AddText: the text argument must be a non-empty array of strings or a simple string.");

        // Converts the text argument into a table if it's a simple string
        text = text.Type == DataType.String ? DynValue.NewTable(null, text) : text;

        if (AllLinesComplete() || hidden) {
            SetText(text);
            return;
        }
        TextMessage[] msgs = new TextMessage[text.Table.Length];
        for (int i = 0; i < text.Table.Length; i++)
            msgs[i] = new MonsterMessage(linePrefix + text.Table.Get(i + 1).String);
        AddToTextQueue(msgs);
    }

    public void SetVoice(string voiceName) {
        if (voiceName == null)
            throw new CYFException("Text.SetVoice: The first argument (the voice name) is nil.\n\nSee the documentation for proper usage.");
        CheckExists();
        defaultVoice = voiceName == "none" ? null : voiceName;
    }

    public void SetFont(string fontName) {
        if (fontName == null)
            throw new CYFException("Text.SetFont: The first argument (the font name) is nil.\n\nSee the documentation for proper usage.");
        CheckExists();
        UnderFont uf = SpriteFontRegistry.Get(fontName);
        if (uf == null)
            throw new CYFException("The font \"" + fontName + "\" doesn't exist.\nYou should check if you made a typo, or if the font really is in your mod.");
        SetFont(uf);
        if (bubble)
            UpdateBubble();
    }

    [MoonSharpHidden] public void UpdateBubble() {
        containerBubble.GetComponent<RectTransform>().localPosition = new Vector2(-12, 24);
        ResizeBubble();
    }

    public void SetEffect(string effect, float intensity = -1, float step = 0) {
        if (effect == null)
            throw new CYFException("Text.SetEffect: The first argument (the effect name) is nil.\n\nSee the documentation for proper usage.");
        CheckExists();
        switch (effect.ToLower()) {
            case "none":   textEffect = null;                                                               break;
            case "twitch": textEffect = new TwitchEffect(this, intensity != -1 ? intensity : 2, (int)step); break;
            case "shake":  textEffect = new ShakeEffect(this, intensity != -1 ? intensity : 1);             break;
            case "rotate": textEffect = new RotatingEffect(this, intensity != -1 ? intensity : 1.5f, step); break;

            default:
                throw new CYFException("The effect \"" + effect + "\" doesn't exist.\nYou can only choose between \"none\", \"twitch\", \"shake\" and \"rotate\".");
        }
    }

    public void ShowBubble(string side = null, DynValue position = null) {
        CheckExists();
        bubble = true;
        containerBubble.SetActive(true);
        UpdateBubble();
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
            speechThing.localRotation = speechThingShadow.localRotation = Quaternion.Euler(0, 0, (int)bubbleSide);
            speechThing.localScale = speechThingShadow.localScale = new Vector2(speechThing.lossyScale.x < 0 ? -1 : 1, speechThing.lossyScale.y < 0 ? -1 : 1);
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
        CheckExists();
        if (GlobalControls.isInFight && EnemyEncounter.script.GetVar("playerskipdocommand").Boolean
         || UnitaleUtil.IsOverworld && (EventManager.instance.script != null && EventManager.instance.script.GetVar("playerskipdocommand").Boolean
         || GlobalControls.isInShop && GameObject.Find("Canvas").GetComponent<ShopScript>().script.GetVar("playerskipdocommand").Boolean))
            DoSkipFromPlayer();
        else
            base.SkipLine();
    }

    public void NextLine() {
        CheckExists();
        if (AllLinesComplete() || currentLine + 1 == LineCount()) {
            if (!deleteWhenFinished) {
                HideTextObject();
                if (bubble)
                    containerBubble.SetActive(false);
            } else
                DestroyText();
        } else {
            ShowLine(++currentLine);
            if (bubble)
                ResizeBubble();
        }
    }
    private void PostScaleHandling() {
        if (xscale == 0 || yscale == 0)
            return;

        if (xscale != lastXScale) xScaleSizes.Clear();
        if (yscale != lastYScale) yScaleSizes.Clear();

        foreach (LetterData l in letters) {
            RectTransform r = l.image.GetComponent<RectTransform>();

            float xLocalScale;
            if (xScaleSizes.ContainsKey(Mathf.RoundToInt(r.rect.width)))
                xLocalScale = xScaleSizes[Mathf.RoundToInt(r.rect.width)];
            else {
                float newXSize = r.rect.width * xscale,
                      chosenX = Mathf.Round(newXSize - 0.001f);
                xLocalScale = chosenX / newXSize;
                xScaleSizes[Mathf.RoundToInt(r.rect.width)] = xLocalScale;
            }

            float yLocalScale;
            if (yScaleSizes.ContainsKey(Mathf.RoundToInt(r.rect.height)))
                yLocalScale = yScaleSizes[Mathf.RoundToInt(r.rect.height)];
            else {
                float newYSize = r.rect.height * yscale,
                      chosenY = Mathf.Round(newYSize - 0.001f);
                yLocalScale = chosenY / newYSize;
                yScaleSizes[Mathf.RoundToInt(r.rect.height)] = yLocalScale;
            }
            r.localScale = new Vector2(xLocalScale, yLocalScale);
        }
    }

    // Shortcut to `SetAutoWaitTimeBetweenTexts`
    public void SetWaitTime(int time) { SetAutoWaitTimeBetweenTexts(time); }

    public void SetAutoWaitTimeBetweenTexts(int time) {
        CheckExists();
        framesWait = time;
    }

    public override void Move(float newX, float newY) {
        MoveToAbs(container.transform.position.x + newX, container.transform.position.y + newY);
    }

    public override void MoveTo(float newX, float newY) {
        MoveToAbs(container.transform.parent.position.x + newX, container.transform.parent.position.y + newY);
    }

    public override void MoveToAbs(float newX, float newY) {
        CheckExists();
        container.transform.position = adjustTextDisplay ? new Vector3(Mathf.Round(newX), Mathf.Round(newY), transform.position.z)
                                                         : new Vector3(newX, newY, transform.position.z);
    }

    public void SetAnchor(float newX, float newY) {
        CheckExists();
        container.GetComponent<RectTransform>().anchorMin = new Vector2(newX, newY);
        container.GetComponent<RectTransform>().anchorMax = new Vector2(newX, newY);
    }

    public int GetTextWidth(int firstLetter = 0, int lastLetter = 999999) {
        CheckExists();
        if (textQueue == null || textQueue[currentLine] == null)
            return 0;

        if (firstLetter < 0) firstLetter += textQueue[currentLine].Text.Length - 1;
        if (lastLetter < 0) lastLetter += textQueue[currentLine].Text.Length - 1;

        if (firstLetter >= textQueue[currentLine].Text.Length) firstLetter = textQueue[currentLine].Text.Length - 1;
        if (lastLetter >= textQueue[currentLine].Text.Length) lastLetter = textQueue[currentLine].Text.Length - 1;

        return (int)UnitaleUtil.CalcTextWidth(this, firstLetter, lastLetter);
    }

    public int GetTextHeight(int firstLetter = 0, int lastLetter = 999999) {
        CheckExists();
        if (textQueue == null || textQueue[currentLine] == null)
            return 0;

        if (firstLetter < 0) firstLetter += textQueue[currentLine].Text.Length;
        if (lastLetter < 0) lastLetter += textQueue[currentLine].Text.Length;

        if (firstLetter >= textQueue[currentLine].Text.Length) firstLetter = textQueue[currentLine].Text.Length - 1;
        if (lastLetter >= textQueue[currentLine].Text.Length) lastLetter = textQueue[currentLine].Text.Length - 1;

        return (int)UnitaleUtil.CalcTextHeight(this, firstLetter, lastLetter);
    }

    public void SetVar(string key, DynValue value) {
        if (key == null)
            throw new CYFException("text.SetVar: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        vars[key] = value;
    }

    public DynValue GetVar(string key) {
        if (key == null)
            throw new CYFException("text.GetVar: The first argument (the index) is nil.\n\nSee the documentation for proper usage.");
        DynValue retval;
        return vars.TryGetValue(key, out retval) ? retval : DynValue.NewNil();
    }

    public DynValue this[string key] {
        get { return GetVar(key); }
        set { SetVar(key, value); }
    }

    ////////////////////
    // Children stuff //
    ////////////////////

    #pragma warning disable 108,114
    public string name {
        get { return container.name; }
    }
    #pragma warning restore 108,114

    public int childIndex {
        get { return container.transform.GetSiblingIndex() + 1; }
        set { container.transform.SetSiblingIndex(value - 1); }
    }
    public int childCount {
        get { return container.transform.childCount; }
    }

    public DynValue GetParent() {
        return UnitaleUtil.GetObjectParent(container.transform);
    }

    public void SetParent(object parent) {
        CheckExists();

        Transform t = UnitaleUtil.GetTransform(parent);
        if (t == null)
            return;
        UnitaleUtil.SetObjectParent(this, parent);

        LuaSpriteController sParent = parent as LuaSpriteController;
        ProjectileController pParent = parent as ProjectileController;
        if (pParent != null)
            sParent = pParent.sprite;
        if (sParent == null)
            return;
        foreach (Transform child in container.transform) {
            MaskImage childmask = child.gameObject.GetComponent<MaskImage>();
            if (childmask != null)
                childmask.inverted = sParent._masked == LuaSpriteController.MaskMode.INVERTEDSPRITE || sParent._masked == LuaSpriteController.MaskMode.INVERTEDSTENCIL;
        }
    }
}