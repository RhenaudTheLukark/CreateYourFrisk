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
                if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED && lineComplete())
                    NextLine();
            } else if (progress == ProgressMode.AUTO) {
                if (lineComplete())
                    if (countFrames == framesWait) {
                        NextLine();
                        countFrames = 0;
                    } else
                        countFrames++;
            }
            if (canSkip() &&!lineComplete() && GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED)
                skipLine();
        }
    }

    private void ResizeBubble() {
        float effectiveBubbleHeight = bubbleHeight != -1 ? bubbleHeight < 16 ? 40 : bubbleHeight + 24 : UnitaleUtil.calcTotalHeight(this) < 16 ? 40 : UnitaleUtil.calcTotalHeight(this) + 24;
        containerBubble.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth + 20, effectiveBubbleHeight);                                                      //To set the borders
        UnitaleUtil.GetChildPerName(containerBubble.transform, "BackHorz").GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth + 20, effectiveBubbleHeight - 20 * 2);    //BackHorz
        UnitaleUtil.GetChildPerName(containerBubble.transform, "BackVert").GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth - 20, effectiveBubbleHeight);             //BackVert
        UnitaleUtil.GetChildPerName(containerBubble.transform, "CenterHorz").GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth + 16, effectiveBubbleHeight - 16 * 2);  //CenterHorz
        UnitaleUtil.GetChildPerName(containerBubble.transform, "CenterVert").GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth - 16, effectiveBubbleHeight - 4);       //CenterVert
        container.GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth + 24, effectiveBubbleHeight);
        SetSpeechThingPositionAndSide(bubbleSide.ToString(), bubbleLastVar);
    }
    
    public string progressmode {
        get { return progress.ToString(); }
        set {
            try { progress = (ProgressMode)Enum.Parse(typeof(ProgressMode), value.ToUpper()); } 
            catch { throw new CYFException("text.progressmode can only have either \"AUTO\", \"MANUAL\" or \"NONE\", but you entered \"" + value.ToUpper() + "\"."); }
        }
    }

    public bool persistent = false;

    public int x {
        get { return Mathf.RoundToInt(container.transform.position.x); }
        set { MoveTo(value, y); }
    }

    public int y {
        get { return Mathf.RoundToInt(container.transform.position.y); }
        set { MoveTo(x, value); }
    }

    public int textWidth {
        get { return _textWidth; }
        set { _textWidth = value < 16 ? 16 : value; }
    }

    public int bubbleHeight {
        get { return _bubbleHeight; }
        set { _bubbleHeight = value == -1 ? -1 : value < 40 ? 40 : value; }
    }

    public string layer {
        get { return container.transform.parent.name.Substring(0, transform.parent.name.Length - 5); }
        set {
            Transform parent = container.transform.parent;
            try { container.transform.SetParent(GameObject.Find(value + "Layer").transform); } 
            catch { throw new CYFException("The layer \"" + value + "\" doesn't exist."); }
        }
    }

    public void SetText(DynValue text) {
        TextMessage[] msgs = null;
        if (text == null)
            throw new CYFException("In Text.SetText: the text argument must be a non-empty array.");
        if (text.Type != DataType.Table)
            throw new CYFException("In Text.SetText: the text argument must be a non-empty array.");

        msgs = new TextMessage[text.Table.Length];
        for (int i = 0; i < text.Table.Length; i++)
            msgs[i] = new MonsterMessage(text.Table.Get(i + 1).String);
        isActive = true;
        if (bubble)
            containerBubble.SetActive(true);
        base.setTextQueue(msgs);
        if (text.Table.Length != 0 && bubble)
            ResizeBubble();
    }

    public void AddText(DynValue text) {
        if (allLinesComplete()) {
            SetText(text);
            return;
        }
        TextMessage[] msgs = new TextMessage[text.Table.Length];
        for (int i = 0; i < text.Table.Length; i++)
            msgs[i] = new MonsterMessage(text.Table.Get(i + 1).String);
        base.addToTextQueue(msgs);
    }

    public void SetFont(string fontName) {
        UnderFont uf = SpriteFontRegistry.Get(fontName);
        if (uf == null)
            throw new CYFException("The font \"" + fontName + "\" doesn't exist.\nYou should check if you haven't made a typo or if the font really is in your mod.");
        setFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_DAMAGETEXT_NAME));
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

    public bool IsTheLineFinished() { return lineComplete(); }
    public bool IsTheTextFinished() { return allLinesComplete(); }

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
            speechThing.anchorMin = speechThing.anchorMax = new Vector2(bubbleSide == BubbleSide.LEFT ? 0 : bubbleSide == BubbleSide.RIGHT ? 1 : 0.5f,
                                                                        bubbleSide == BubbleSide.DOWN ? 0 : bubbleSide == BubbleSide.UP ? 1 : 0.5f);
            speechThingShadow.anchorMin = speechThingShadow.anchorMax = new Vector2(bubbleSide == BubbleSide.LEFT ? 0 : bubbleSide == BubbleSide.RIGHT ? 1 : 0.5f,
                                                                                    bubbleSide == BubbleSide.DOWN ? 0 : bubbleSide == BubbleSide.UP ? 1 : 0.5f);
            speechThing.rotation = speechThingShadow.rotation = Quaternion.Euler(0, 0, (int)bubbleSide);
            speechThing.localPosition = new Vector2(0, 0);
            bool isSide = bubbleSide == BubbleSide.LEFT || bubbleSide == BubbleSide.RIGHT;
            int size = isSide ? (int)containerBubble.GetComponent<RectTransform>().sizeDelta.y - 20 : (int)containerBubble.GetComponent<RectTransform>().sizeDelta.x - 20;
            int otherSize = isSide ? (int)containerBubble.GetComponent<RectTransform>().sizeDelta.x : (int)containerBubble.GetComponent<RectTransform>().sizeDelta.y;
            float shift = bubbleSide == BubbleSide.LEFT || bubbleSide == BubbleSide.DOWN ? -size / 2 : size / 2;
            float otherShift = bubbleSide == BubbleSide.LEFT || bubbleSide == BubbleSide.DOWN ? -otherSize / 2 : otherSize / 2;
            if (position == null)
                speechThing.localPosition = speechThingShadow.localPosition = new Vector2(0, 0);
            else {
                if (position.Type == DataType.Number) {
                    float number = (float)position.Number + shift;
                    speechThing.localPosition = speechThingShadow.localPosition = new Vector2(isSide ? otherShift : Mathf.Clamp(number, -size / 2 + 10, size / 2 - 10),
                                                                                              isSide ? Mathf.Clamp(number, -size / 2 + 10, size / 2 - 10) : otherShift);
                } else if (position.Type == DataType.String) {
                    string str = position.String.Replace(" ", "");
                    if (str.Contains("%")) {
                        size -= 20;
                        try {
                            float percentage = Mathf.Clamp01(ParseUtil.getFloat(str.Replace("%", "")) / 100);
                            speechThing.localPosition = speechThingShadow.localPosition = new Vector2(isSide ? otherShift : 10 + Mathf.Round(percentage * size) - shift,
                                                                                                      isSide ? 10 + Mathf.Round(percentage * size) - shift : otherShift);
                        } catch { throw new CYFException("If you use a '%' in your string, you should only have a number with it."); }
                    } else
                        throw new CYFException("You need to use a '%' in order to exploit the string.");
                    /*else {
                        str = str.Replace("middle", "0").Replace("top", (size / 2).ToString());
                        try {
                            float f = ParseUtil.getFloat(str);
                            speechThing.localPosition = speechThingShadow.localPosition = new Vector2(isSide ? otherShift : Mathf.Clamp(f + shift, -size / 2 + 10, size / 2 - 10),
                                                                                                      isSide ? Mathf.Clamp(f + shift, -size / 2 + 10, size / 2 - 10) : otherShift);
                        } catch { throw new CYFException("The only keywords are \"middle\" and \"top\", but you entered \"" + position.String + "\"."); }
                    }*/
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
        if (allLinesComplete()) {
            isActive = false;
            if (bubble)
                containerBubble.SetActive(false);
            destroyText();
        } else {
            showLine(++currentLine); 
            if (bubble)
                ResizeBubble();
        }
    }

    public void SetAutoWaitTimeBetweenTexts(int time) { framesWait = time; }

    public void MoveTo(int x, int y) {
        container.transform.position = new Vector3(x, y, container.transform.position.z);
    }

    public void SetPivot(float x, float y) {
        container.GetComponent<RectTransform>().pivot = new Vector2(x, y);
    }
}