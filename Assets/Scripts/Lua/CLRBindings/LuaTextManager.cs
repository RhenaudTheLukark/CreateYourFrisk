using UnityEngine;
using System;
using MoonSharp.Interpreter;

public class LuaTextManager : TextManager {
    private GameObject container;
    private GameObject containerBubble;
    private RectTransform speechThing;
    private RectTransform speechThingShadow;
    private DynValue bubbleLastVar = DynValue.NewNil();
    private bool bubble = true;
    private bool isActive = false;
    private int framesWait = 60;
    private int countFrames = 0;
    private int _textWidth;
    private int _bubbleHeight = -1;
    private BubbleSide bubbleSide = BubbleSide.NONE;
    private ProgressMode progress = ProgressMode.AUTO;
    private Color textColor;

    enum BubbleSide { LEFT = 0, DOWN = 90, RIGHT = 180, UP = 270, NONE = -1 }
    enum ProgressMode { AUTO, MANUAL, NONE }

    protected override void Awake() {
        base.Awake();
        transform.parent.SetParent(GameObject.Find("TopLayer").transform);
        container = transform.parent.gameObject;
        containerBubble = UnitaleUtil.GetChildPerName(container.transform, "BubbleContainer").gameObject;
        speechThing = UnitaleUtil.GetChildPerName(containerBubble.transform, "SpeechThing", false, true).GetComponent<RectTransform>();
        speechThingShadow = UnitaleUtil.GetChildPerName(containerBubble.transform, "SpeechThingShadow", false, true).GetComponent<RectTransform>();
        isActive = true;
    }

    protected override void Update() {
        base.Update();

        //Next line/EOF check
        if (isActive) {
            if (progress == ProgressMode.MANUAL) {
                if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED && LineComplete())
                    NextLine();
            } else if (progress == ProgressMode.AUTO) {
                if (LineComplete())
                    if (countFrames == framesWait) {
                        NextLine();
                        countFrames = 0;
                    } else
                        countFrames++;
            }
            if (CanSkip() && !LineComplete() && GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED)
                SkipLine();
        }
    }

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
        get { return progress.ToString(); }
        set {
            try { progress = (ProgressMode)Enum.Parse(typeof(ProgressMode), value.ToUpper()); } 
            catch { throw new CYFException("text.progressmode can only have either \"AUTO\", \"MANUAL\" or \"NONE\", but you entered \"" + value.ToUpper() + "\"."); }
        }
    }

    public int x {
        get { return Mathf.RoundToInt(container.transform.localPosition.x); }
        set { MoveTo(value, y); }
    }

    public int y {
        get { return Mathf.RoundToInt(container.transform.localPosition.y); }
        set { MoveTo(x, value); }
    }

    public int absx {
        get { return Mathf.RoundToInt(container.transform.position.x); }
        set { MoveToAbs(value, absy); }
    }

    public int absy {
        get { return Mathf.RoundToInt(container.transform.position.y); }
        set { MoveTo(absx, value); }
    }

    public int textMaxWidth {
        get { return _textWidth; }
        set { _textWidth = value < 16 ? 16 : value; }
    }

    public int bubbleHeight {
        get { return _bubbleHeight; }
        set { _bubbleHeight = value == -1 ? -1 : value < 40 ? 40 : value; }
    }

    public string layer {
        get {
            if (!container.transform.parent.name.Contains("Layer"))
                return "spriteObject";
            return container.transform.parent.name.Substring(0, transform.parent.name.Length - 5);
        }
        set {
            Transform parent = container.transform.parent;
            try { container.transform.SetParent(GameObject.Find(value + "Layer").transform); } 
            catch { throw new CYFException("The layer \"" + value + "\" doesn't exist."); }
        }
    }

    public Color _color = Color.white;
    public bool hasColorBeenSet = false;
    public bool hasAlphaBeenSet = false;
    // The color of the text. It uses an array of three floats between 0 and 1
    public float[] color {
        get { return new float[] { _color.r, _color.g, _color.b }; }
        set {
            // If we don't have three floats, we throw an error
            if (value.Length == 3)      _color = new Color(value[0], value[1], value[2], alpha);
            else if (value.Length == 4) _color = new Color(value[0], value[1], value[2], value[3]);
            else                        throw new CYFException("You need 3 or 4 numeric values when setting a text's color.");

            hasColorBeenSet = true;
            hasAlphaBeenSet = false;

            foreach (Letter l in letters) {
                try {
                    if (l.GetComponent<UnityEngine.UI.Image>().color == Charset.DefaultColor) {
                        l.GetComponent<UnityEngine.UI.Image>().color = _color;
                    }
                } catch { }
            }

            Debug.Log("currentColor = " + currentColor);
            Debug.Log("Charset.DefaultColor = " + Charset.DefaultColor);
            if (currentColor == Charset.DefaultColor) {
                currentColor = _color;
            }
            Charset.DefaultColor = _color;
        }
    }

    // The color of the text on a 32 bits format. It uses an array of three or four floats between 0 and 255
    public float[] color32 {
        // We need first to convert the Color into a Color32, and then get the values.
        get { return new float[] { ((Color32)_color).r, ((Color32)_color).g, ((Color32)_color).b }; }
        set { color = new float[] { value[0] / 255, value[1] / 255, value[2] / 255, value.Length == 3 ? alpha : value[3] / 255 }; }
    }

    // The alpha of the text. It is clamped between 0 and 1
    public float alpha {
        get { return _color.a; }
        set {
            color = new float[] { _color.r, _color.g, _color.b, Mathf.Clamp01(value) };
            hasAlphaBeenSet = true;
            hasColorBeenSet = false;
        }
    }

    // The alpha of the text in a 32 bits format. It is clamped between 0 and 255
    public float alpha32 {
        get { return ((Color32)_color).a; }
        set { alpha = value / 255; }
    }

    public bool lineComplete {
        get { return LineComplete(); }
    }

    public bool allLinesComplete {
        get { return AllLinesComplete(); }
    }

    public void SetParent(LuaSpriteController parent) {
        try { container.transform.SetParent(parent.img.transform); } 
        catch { throw new CYFException("You tried to set a removed sprite/unexisting sprite as this text's parent."); }
    }

    public void SetText(DynValue text) {
        TextMessage[] msgs = null;
        if (text == null)
            throw new CYFException("In Text.SetText: the text argument must be a non-empty array.");
        if (text.Type != DataType.Table)
            throw new CYFException("In Text.SetText: the text argument must be a non-empty array.");

        msgs = new TextMessage[text.Table.Length];
        for (int i = 0; i < text.Table.Length; i++)
            msgs[i] = new TextMessage(text.Table.Get(i + 1).String, false, false);
        isActive = true;
        if (bubble)
            containerBubble.SetActive(true);
        try { SetTextQueue(msgs); } catch { }
        if (text.Table.Length != 0 && bubble)
            ResizeBubble();
    }

    public void AddText(DynValue text) {
        if (AllLinesComplete()) {
            SetText(text);
            return;
        }
        TextMessage[] msgs = new TextMessage[text.Table.Length];
        for (int i = 0; i < text.Table.Length; i++)
            msgs[i] = new MonsterMessage(text.Table.Get(i + 1).String);
        base.AddToTextQueue(msgs);
    }

    public void SetVoice(string voiceName) {
        if (voiceName == "none")
            default_voice = null;
        else
            default_voice = AudioClipRegistry.GetVoice(voiceName);
    }

    public void SetFont(string fontName, bool firstTime = false) {
        UnderFont uf = SpriteFontRegistry.Get(fontName);
        if (uf == null)
            throw new CYFException("The font \"" + fontName + "\" doesn't exist.\nYou should check if you haven't made a typo or if the font really is in your mod.");
        SetFont(uf, firstTime);
        //if (forced)
        //    default_charset = uf;
        containerBubble.GetComponent<RectTransform>().localPosition = new Vector2(-12, 24);
        GetComponent<RectTransform>().localPosition = new Vector2(0, 16);
    }

    public void SetEffect(string effect, float intensity) {
        switch (effect.ToLower()) {
            case "none":
                textEffect = null;
                break;

            case "twitch":
                if (intensity != -1) textEffect = new TwitchEffect(this, intensity);
                else                 textEffect = new TwitchEffect(this);
                break;

            case "shake":
                if (intensity != -1) textEffect = new ShakeEffect(this, intensity);
                else                 textEffect = new ShakeEffect(this);
                break;

            case "rotate":
                if (intensity != -1) textEffect = new RotatingEffect(this, intensity);
                else                 textEffect = new RotatingEffect(this);
                break;

            default:
                throw new CYFException("The effect \"" + effect + "\" doesn't exist.\nYou can only choose between \"none\", \"twitch\", \"shake\" and \"rotate\".");
        }
    }

    public bool IsTheLineFinished() { return lineComplete; }
    public bool IsTheTextFinished() { return allLinesComplete; }

    public void ShowBubble(string side = null, DynValue position = null) {
        bubble = true;
        containerBubble.SetActive(true);
        SetSpeechThingPositionAndSide(side, position);
    }

    public void SetSpeechThingPositionAndSide(string side, DynValue position) {
        bubbleLastVar = position;
        try { bubbleSide = side != null ? (BubbleSide)Enum.Parse(typeof(BubbleSide), side.ToUpper()) : BubbleSide.NONE; } 
        catch { throw new CYFException("The speech thing can only take \"RIGHT\", \"DOWN\" ,\"LEFT\" ,\"UP\" or \"NONE\" as value, but you entered \"" + side.ToUpper() + "\"."); }

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
                if (position.Type == DataType.Number) {
                    float number = (float)position.Number < 0 ? (float)position.Number : (size - (float)position.Number) - size / 2;
                    speechThing.anchoredPosition = speechThingShadow.anchoredPosition = new Vector3(isSide ? 0 : Mathf.Clamp(number, -size / 2, size / 2),
                                                                                                    isSide ? Mathf.Clamp(number, -size / 2, size / 2) : 0);
                } else if (position.Type == DataType.String) {
                    string str = position.String.Replace(" ", "");
                    if (str.Contains("%")) {
                        size -= 20;
                        try {
                            float percentage = Mathf.Clamp01(ParseUtil.GetFloat(str.Replace("%", "")) / 100);
                            float x = isSide ? 0 : 10 + Mathf.Round(percentage * size) - size / 2;
                            float y = isSide ? 10 + Mathf.Round(percentage * size) - size / 2 : 0;
                            speechThing.anchoredPosition = speechThingShadow.anchoredPosition = new Vector3(x, y);
                        } catch { throw new CYFException("If you use a '%' in your string, you should only have a number with it."); }
                    } else
                        throw new CYFException("You need to use a '%' in order to exploit the string.");
                }
            }
        } else {
            speechThing.gameObject.SetActive(false);
            speechThingShadow.gameObject.SetActive(false);
        }
    }

    public void HideBubble() {
        bubble = false;
        containerBubble.SetActive(false);
    }

    public void NextLine() {
        if (AllLinesComplete()) {
            isActive = false;
            if (bubble)
                containerBubble.SetActive(false);
            DestroyText();
        } else {
            ShowLine(++currentLine); 
            if (bubble)
                ResizeBubble();
        }
    }

    public void SetAutoWaitTimeBetweenTexts(int time) { framesWait = time; }

    public void MoveTo(int x, int y) {
        container.transform.localPosition = new Vector3(x, y, container.transform.localPosition.z);
    }

    public void MoveToAbs(int x, int y) {
        container.transform.position = new Vector3(x, y, container.transform.position.z);
    }

    public void SetPivot(float x, float y) {
        container.GetComponent<RectTransform>().pivot = new Vector2(x, y);
    }

    public int GetTextWidth() {
        return (int)UnitaleUtil.CalcTextWidth(this);
    }

    public int GetTextHeight() {
        return (int)UnitaleUtil.CalcTextHeight(this);
    }
}