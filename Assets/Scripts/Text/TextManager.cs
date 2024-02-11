using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

// TODO less code duplicate-y way of pulling commands out of the text.
public class TextManager : MonoBehaviour {

    public struct LetterData {
        public int index;
        public Image image;
        public Sprite sprite;
        public Vector2 position;
        public bool commandColorSet, commandAlphaSet;
        public LetterData(int index, Image image, Sprite sprite, Vector2 position, bool commandColorSet, bool commandAlphaSet) {
            this.index = index;
            this.image = image;
            this.sprite = sprite;
            this.position = position;
            this.commandColorSet = commandColorSet;
            this.commandAlphaSet = commandAlphaSet;
        }
    }
    internal List<LetterData> letters = new List<LetterData>();

    protected UnderFont defaultFont;
    protected string defaultVoice;
    [MoonSharpHidden] public UnderFont font { get; protected set; }
    [MoonSharpHidden] public string fontVoice;
    private string commandVoice;

    protected TextEffect textEffect;
    private string letterEffect = "none";
    private float letterEffectStep;
    private float letterEffectStepCount;
    private float letterIntensity;

    public static string[] commandList = { "color", "alpha", "charspacing", "linespacing", "starcolor", "instant", "font", "effect", "noskip", "w", "waitall", "novoice",
                                           "next", "finished", "nextthisnow", "noskipatall", "waitfor", "speed", "letters", "lettersperframe", "voice", "func", "mugshot",
                                           "music", "sound", "health", "lettereffect"};
    public static string[] movementCommands = { "charspacing", "linespacing", "font" };
    public int currentLine;
    [MoonSharpHidden] public int _textMaxWidth;
    public int currentCharacter;
    public int currentReferenceCharacter;
    private bool currentSkippable = true;
    private bool decoratedTextOffset;
    private RectTransform self;

    private float currentX;
    private float currentY;
    private float startingLineX;
    private float startingLineY;

    // Variables that have to do with "[instant]"
    private bool instantActive; // Will be true if "[instant]" or "[instant:allowcommand]" have been activated
    private bool instantCommand; // Will be true only if "[instant:allowcommand]" has been activated

    private bool paused;
    private bool muted;
    private bool autoSkipThis;
    private bool autoSkipAll;
    private bool autoSkip;
    private bool skipFromPlayer;
    private bool firstChar;

    internal float hSpacing = 3;
    internal float vSpacing;

    public LuaSpriteController mugshotMask;
    public LuaSpriteController mugshot;
    private string[] mugshotList;
    private bool lineHasMugshot;
    private float mugshotTimer;

    // private int letterSpeed = 1;
    private int lettersToDisplay;
    private int lettersToDisplayOnce;
    private KeyCode waitingChar = KeyCode.None;
    private string waitingKeybind = null;

    protected Color commandColor = Color.white;
    protected Color defaultColor = Color.white;
    protected Color fontDefaultColor = Color.white;
    protected bool commandColorSet, commandAlphaSet;

    private float letterTimer;
    private float timePerLetter;
    private const float singleFrameTiming = 1.0f / 20;
    protected Vector3 internalRotation = Vector3.zero;

    public int columnShift = 265;
    public int columnNumber = 2;

    protected bool hidden = true;

    // The rotation of the text
    public float rotation {
        get { return transform.eulerAngles.z; }
        set {
            // We mod the value from 0 to 360 because angles are between 0 and 360 normally
            internalRotation.z = Math.Mod(value, 360);
            transform.eulerAngles = internalRotation;
        }
    }

    [MoonSharpHidden] public ScriptWrapper caller;

    [MoonSharpHidden] public TextMessage[] textQueue { get; protected set; }
    //public string[] mugshotsPath;
    //public bool overworld;
    [MoonSharpHidden] public bool skipNowIfBlocked = false;
    internal bool noSkip1stFrame = true;

    [MoonSharpHidden] public bool lateStartWaiting = false; // Lua text objects will use a late start
    public TextManager() {
        defaultVoice = null;
        textEffect = null;
        letterIntensity = 0.0f;
        currentLine = 0;
        _textMaxWidth = 0;
        currentCharacter = 0;
        currentReferenceCharacter = 0;
        decoratedTextOffset = false;
        instantActive = false;
        instantCommand = false;
        autoSkipAll = false;
        autoSkip = false;
        skipFromPlayer = false;
        firstChar = false;
        vSpacing = 0;
        mugshotList = null;
        lettersToDisplay = 1;
        lettersToDisplayOnce = 0;
        commandColorSet = false;
        commandAlphaSet = false;
        letterTimer = 0.0f;
        textQueue = null;
    }

    [MoonSharpHidden] public void SetCaller(ScriptWrapper s) { caller = s; }

    public void SetFont(UnderFont font) {
        this.font = font;
        defaultFont = font;
        defaultVoice = font.SoundName;

        if (defaultFont == null)
            defaultFont = font;

        vSpacing = 0;
        hSpacing = font.CharSpacing;
        fontDefaultColor = defaultColor = font.DefaultColor;
        if (this as LuaTextManager) {
            if ((this as LuaTextManager).textColorSet) defaultColor =   ((LuaTextManager) this)._color;
            if ((this as LuaTextManager).textAlphaSet) defaultColor.a = ((LuaTextManager) this).alpha;
        }
        commandColor = defaultColor;
    }

    public string GetVoice() {
        string voice = commandVoice ?? fontVoice;
        return voice != "none" ? voice : null;
    }

    [MoonSharpHidden] public void SetHorizontalSpacing(float spacing = 3) { hSpacing = spacing; }
    [MoonSharpHidden] public void SetVerticalSpacing(float spacing = 0) { vSpacing = spacing; }

    [MoonSharpHidden] public void ResetFont() {
        if (font == null || defaultFont == null)
            if (GetType() == typeof(LuaTextManager) && !((LuaTextManager)this).isMainTextObject)
                ((LuaTextManager) this).SetFont(SpriteFontRegistry.UI_MONSTERTEXT_NAME);
            else
                SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_DEFAULT_NAME));
        font = defaultFont;
        System.Diagnostics.Debug.Assert(defaultFont != null, "defaultFont != null");
        fontVoice = defaultVoice ?? font.SoundName;
        fontDefaultColor = font.DefaultColor;
        hSpacing = font.CharSpacing;

        if (!(this as LuaTextManager))
            defaultColor = fontDefaultColor;
        else {
            if (!(this as LuaTextManager).textColorSet) {
                defaultColor.r = fontDefaultColor.r;
                defaultColor.g = fontDefaultColor.g;
                defaultColor.b = fontDefaultColor.b;
            }
            if (!(this as LuaTextManager).textAlphaSet)
                defaultColor.a = fontDefaultColor.a;
        }

        // Default voice in the overworld
        if (gameObject.name == "TextManager OW")
            defaultVoice = "monsterfont";
    }

    protected virtual void Awake() {
        self = gameObject.GetComponent<RectTransform>();
        timePerLetter = singleFrameTiming;

        Transform parent = this as LuaTextManager ? (this as LuaTextManager).GetContainer().transform.parent : transform.parent;
        if (parent == null || (!parent.Find("Mugshot") && !parent.Find("MugshotMask")))
            return;
        mugshot = LuaSpriteController.GetOrCreate((parent.Find("Mugshot") ?? parent.Find("MugshotMask").GetChild(0)).gameObject);
        if (parent.Find("MugshotMask"))
            mugshotMask = LuaSpriteController.GetOrCreate(parent.Find("MugshotMask").gameObject);
    }

    public void SetPause(bool pause) { paused = pause; }

    public bool IsPaused() { return paused; }

    [MoonSharpHidden] public bool IsFinished() {
        return currentCharacter >= textQueue[currentLine].Text.Length;
    }

    [MoonSharpHidden] public void SetMute(bool newMuted) { muted = newMuted; }

    public void SetText(TextMessage text) { SetTextQueue(new[] { text }); }

    public bool GetAutoLineBreak() {
        if (textQueue[currentLine].ForceNoAutoLineBreak) return false;
        if (!GlobalControls.isInFight || EnemyEncounter.script.GetVar("autolinebreak").Boolean) return true;
        return (this as LuaTextManager) != null && this != UIController.instance.mainTextManager;
    }

    [MoonSharpHidden] public void SetTextQueue(TextMessage[] newTextQueue) {
        if (UnitaleUtil.IsOverworld && (gameObject.name == "TextManager OW"))
            PlayerOverworld.AutoSetUIPos();

        ResetFont();
        if (mugshot != null) {
            bool oldLineHasMugshot = lineHasMugshot;
            SetMugshot(DynValue.NewNil());
            SetMugshotShift(oldLineHasMugshot);
        }
        hidden = newTextQueue.Length == 0;
        textQueue = newTextQueue;
        currentLine = 0;
        ShowLine(0);
    }

    [MoonSharpHidden] public void SetTextQueueAfterValue(int BeginText) {
        ResetFont();
        currentLine = BeginText;
        ShowLine(BeginText);
    }

    [MoonSharpHidden] public void ResetCurrentCharacter() {
        currentCharacter = 0;
        currentReferenceCharacter = 0;
    }

    [MoonSharpHidden] public void AddToTextQueue(TextMessage text) { AddToTextQueue(new[] { text }); }

    [MoonSharpHidden] public void AddToTextQueue(TextMessage[] textQueueToAdd) {
        if (AllLinesComplete())
            SetTextQueue(textQueueToAdd);
        else {
            int length = textQueue.Length + textQueueToAdd.Length;
            TextMessage[] newTextQueue = new TextMessage[length];
            textQueue.CopyTo(newTextQueue, 0);
            textQueueToAdd.CopyTo(newTextQueue, textQueue.Length);
            textQueue = newTextQueue;
        }
    }

    [MoonSharpHidden] public bool CanSkip() { return currentSkippable; }
    [MoonSharpHidden] public bool CanAutoSkip() { return autoSkip; }
    [MoonSharpHidden] public bool CanAutoSkipThis() { return autoSkipThis; }
    [MoonSharpHidden] public bool CanAutoSkipAll() { return autoSkipAll; }

    public int LineCount() {
        return textQueue == null ? 0 : textQueue.Length;
    }

    public bool LineComplete() {
        return instantActive || currentCharacter == textQueue[currentLine].Text.Length;
    }

    [MoonSharpHidden] public bool AllLinesComplete() {
        return textQueue == null || currentLine == textQueue.Length - 1 && LineComplete();
    }

    public void SetMugshotShift(bool oldLineHasMugshot) {
        if (lineHasMugshot && !oldLineHasMugshot) {
            Move(117, 0);
            _textMaxWidth -= 117;
        } else if (!lineHasMugshot && oldLineHasMugshot) {
            Move(-117, 0);
            _textMaxWidth += 117;
        }
    }

    public void SetMugshot(DynValue text) {
        if (mugshot == null || text == null)
            return;
        if (text.Type != DataType.String && text.Type != DataType.Table && text.Type != DataType.Nil && text.Type != DataType.Void)
            throw new CYFException("Mugshots can either be nil, strings or tables of strings ending with a number, yet it's currently a " + text.Type + ".");
        if (text.Type == DataType.Table) {
            Table t = text.Table;
            for (int i = 1; i <= t.Length; i++) {
                DynValue d = t.Get(i);
                if (d.Type != DataType.String && !(d.Type == DataType.Number && i == t.Length))
                    throw new CYFException("Mugshots can either be nil, strings or tables of strings ending with a number, yet the current table has a " + d.Type + ".");
            }
        }

        mugshotTimer = 0.2f;
        List<string> mugshots = new List<string>();
        if (text != null && text.String != "null") {
            if (text.Type == DataType.Table) {
                foreach (DynValue dv in text.Table.Values) {
                    if (dv.Type == DataType.String)
                        mugshots.Add("Mugshots/" + dv.String);
                    else if (dv.Type == DataType.Number)
                        mugshotTimer = (float)(dv.Number > 0 ? dv.Number : 0.2f);
                }
            } else
                mugshots.Add("Mugshots/" + text.String);
        } else
            mugshots.Add("Mugshots/");

        mugshot.StopAnimation();
        bool mugshotSet = mugshots.Count > 0 && mugshots[0] != "Mugshots/" && mugshots[0] != "Mugshots/null";
        if (mugshotSet) {
            mugshotList = mugshots.ToArray();
            mugshot.alpha = 1;
            try {
                if (mugshotList.Length > 1)
                    mugshot.SetAnimation(mugshotList, mugshotTimer);
                else
                    mugshot.Set(mugshotList[0]);
            } catch (CYFException e) {
                UnitaleUtil.DisplayLuaError("Setting a mugshot", e.Message);
            }
        } else {
            mugshotList = null;
            mugshot.alpha = 0;
            mugshot.Set("empty");
        }
    }

    protected void ShowLine(int line) {
        if (textQueue == null) return;
        if (line >= textQueue.Length) return;
        if (textQueue[line] == null) return;
        if (lateStartWaiting) return;
        bool oldLineHasMugshot = lineHasMugshot;
        SetMugshot(textQueue[line].Mugshot);

        if (!(this as LuaTextManager) || (this as LuaTextManager).needFontReset)
            ResetFont();
        commandColor     = defaultColor;
        commandColorSet  = false;
        commandAlphaSet  = false;
        currentSkippable = true;
        autoSkipThis     = false;
        autoSkip         = false;
        autoSkipAll      = false;
        instantCommand   = false;
        skipFromPlayer   = false;
        firstChar        = false;
        lineHasMugshot   = mugshotList != null;
        commandVoice     = null;

        SetMugshotShift(oldLineHasMugshot);

        timePerLetter = singleFrameTiming;
        letterTimer   = 0.0f;
        DestroyChars();
        currentLine = line;
        currentCharacter          = 0;
        currentReferenceCharacter = 0;
        letterEffect              = "none";
        instantActive = textQueue[line].ShowImmediate;

        float rot = rotation;
        rotation = 0;
        float xScale = 1, yScale = 1;
        LuaTextManager ltm = this as LuaTextManager;
        if (ltm) {
            xScale = ltm.xscale;
            yScale = ltm.yscale;
        }
        if (ltm) ltm.SpawnText();
        else     SpawnText();
        rotation = rot;

        if (UnitaleUtil.IsOverworld && this == PlayerOverworld.instance.textmgr) {
            if (textQueue[line].ActualText) {
                if (transform.parent.GetComponent<Image>().color.a == 0)
                    SetTextFrameAlpha(1);
            } else {
                if (transform.parent.GetComponent<Image>().color.a == 1)
                    SetTextFrameAlpha(0);
                HideTextObject();
            }
        }

        // Move the text up a little if there are more than 3 lines so they can possibly fit in the arena
        if (!GlobalControls.retroMode && !UnitaleUtil.IsOverworld && UIController.instance && this == UIController.instance.mainTextManager) {
            int     lines = (textQueue[line].Text.Split('\n').Length > 3 && (UIController.instance.state == "ACTIONSELECT" || UIController.instance.state == "DIALOGRESULT")) ? 4 : 3;
            Vector3 pos   = self.localPosition;

            // remove the offset
            self.localPosition  = new Vector3(pos.x, pos.y - (decoratedTextOffset ? 9 : 0), pos.z);
            pos                 = self.localPosition;
            decoratedTextOffset = false;

            // add the offset if necessary
            if (lines != 4) return;
            decoratedTextOffset = true;
            self.localPosition  = new Vector3(pos.x, pos.y + (decoratedTextOffset ? 9 : 0), pos.z);
        } else if (gameObject.name == "TextManager OW") {
            int lines = textQueue[line].Text.Split('\n').Length;
            lines = lines >= 4 ? 4 : 3;
            Vector3 pos = gameObject.GetComponent<RectTransform>().localPosition;
            MoveTo(pos.x, 22 + ((lines - 1) * font.LineSpacing / 2));
        }
    }

    [MoonSharpHidden] public void SetTextFrameAlpha(float a) {
        string objectName = UnitaleUtil.IsOverworld ? "textframe_border_outer" : "arena_border_outer";

        GameObject target = GameObject.Find(objectName);
        List<Image> imagesChild = target.GetComponentsInChildren<Image>().ToList();
        imagesChild.Add(target.GetComponent<Image>());

        foreach (Image img in imagesChild)
            img.color = new Color(img.color.r, img.color.g, img.color.b, a);
    }

    [MoonSharpHidden] public bool HasNext() { return currentLine + 1 < LineCount(); }

    [MoonSharpHidden] public void NextLineText() { ShowLine(++currentLine); }

    [MoonSharpHidden] public void DoSkipFromPlayer() {
        skipFromPlayer = true;

        if ((GlobalControls.isInFight && EnemyEncounter.script.GetVar("playerskipdocommand").Boolean) || !GlobalControls.isInFight)
            instantCommand = true;

        if (!GlobalControls.retroMode)
            InUpdateControlCommand(DynValue.NewString("instant"), currentCharacter);
        else
            SkipLine();
    }

    public virtual void SkipLine() {
        if (noSkip1stFrame) return;
        foreach (LetterData d in letters)
            d.image.enabled = true;
        currentCharacter = textQueue[currentLine].Text.Length;
        currentReferenceCharacter = letters.Count;
    }

    public void SetEffect(TextEffect effect) {
        if (textEffect != null)
            textEffect.ResetPositions();
        textEffect = effect;
    }
    public void SetEffect(string effect, float intensity = -1, float step = 0) {
        if (effect == null)
            throw new CYFException("Text.SetEffect: The first argument (the effect name) is nil.\n\nSee the documentation for proper usage.");
        if (textEffect != null)
            textEffect.ResetPositions();
        switch (effect.ToLower()) {
            case "none":
                textEffect = null;
                break;
            case "twitch":
                textEffect = new TwitchEffect(this, intensity != -1 ? intensity : 2, (int)step);
                break;
            case "shake":
                textEffect = new ShakeEffect(this, intensity != -1 ? intensity : 1);
                break;
            case "rotate":
                textEffect = new RotatingEffect(this, intensity != -1 ? intensity : 1.5f, step);
                break;

            default:
                throw new CYFException("The effect \"" + effect + "\" doesn't exist.\nYou can only choose between \"none\", \"twitch\", \"shake\" and \"rotate\".");
        }
    }

    [MoonSharpHidden] protected void DestroyChars() {
        foreach (Transform child in gameObject.transform) {
            if (child.GetComponent<SpriteRenderer>() == null && child.GetComponent<Image>() == null) continue;
            LuaSpriteController.GetOrCreate(child.gameObject).Remove();
        }
        textEffect = null;
        letters.Clear();
    }

    [MoonSharpHidden] public void HideTextObject() {
        DestroyChars();
        hidden = true;
    }

    private void SpawnTextSpaceTest(int i, string currentText, out string currentText2) {
        currentText2 = currentText;
        bool decorated = textQueue[currentLine].Decorated;
        float decorationLength = decorated ? UnitaleUtil.PredictTextWidth(this, 0, 1, true) : 0;

        // Gets the first character of the line and the last character after the current space
        int finalIndex = i + 1, beginIndex = i;

        for (; beginIndex > 0; beginIndex--)
            if (currentText[beginIndex] == '\n' || currentText[beginIndex] == '\r')
                break;
        for (; finalIndex < currentText.Length - 1; finalIndex++)
            if (currentText[finalIndex] == ' ' || currentText[finalIndex] == '\n' || currentText[finalIndex] == '\r')
                break;

        if (currentText[beginIndex] == '\n' || currentText[beginIndex] == '\r')                                   beginIndex++;
        if (currentText[finalIndex] == '\n' || currentText[finalIndex] == ' ' || currentText[finalIndex] == '\r') finalIndex--;

        if (_textMaxWidth > 0 && UnitaleUtil.PredictTextWidth(this, beginIndex, finalIndex, true) > _textMaxWidth) {
            // If the line's too long, do something!
            int wordBeginIndex = currentText2[i] == ' ' ? i + 1 : i;
            if (UnitaleUtil.PredictTextWidth(this, wordBeginIndex, finalIndex) > _textMaxWidth - decorationLength) {
                // Word is taking the entire line
                for (int currentIndex = wordBeginIndex; currentIndex <= finalIndex; currentIndex++) {
                    if (!(UnitaleUtil.PredictTextWidth(this, beginIndex, currentIndex) > _textMaxWidth)) continue;
                    currentText2                =  currentText2.Substring(0, currentIndex) + "\n" + (decorated ? "  " : "") + currentText2.Substring(currentIndex, currentText2.Length - currentIndex);
                    textQueue[currentLine].Text =  currentText2;
                    finalIndex                  += decorated ? 3 : 1;
                    beginIndex                  =  currentIndex;
                }
            } else
                // Line is too long
                currentText2 = currentText2.Substring(0, wordBeginIndex - 1) + "\n" + (decorated ? "  " : "") + currentText2.Substring(wordBeginIndex, currentText.Length - wordBeginIndex);
        }
        textQueue[currentLine].Text = currentText2;
    }

    private int CreateLetter(string currentText, int index) {
        GameObject singleLtr = Instantiate(SpriteFontRegistry.LETTER_OBJECT);
        RectTransform ltrRect = singleLtr.GetComponent<RectTransform>();

        LuaTextManager luaThis = this as LuaTextManager;
        bool isLua = luaThis != null;

        ltrRect.localScale = new Vector3(isLua ? luaThis.xscale : 1f, isLua ? luaThis.yscale : 1f, ltrRect.localScale.z);

        Image ltrImg = singleLtr.GetComponent<Image>();
        ltrRect.SetParent(gameObject.transform);
        ltrImg.sprite = font.Letters[currentText[index]];

        letters.Add(new LetterData(index, ltrImg, font.Letters[currentText[index]], Vector2.zero, commandColorSet, commandAlphaSet));

        ltrImg.SetNativeSize();

        Color resultColor = commandColor;
        if (isLua) {
            if (!commandColorSet && luaThis.textColorSet) {
                resultColor.r = luaThis._color.r;
                resultColor.g = luaThis._color.g;
                resultColor.b = luaThis._color.b;
            }
            if (!commandAlphaSet && luaThis.textAlphaSet)
                resultColor.a = commandColor.a;
        }
        ltrImg.color = resultColor;
        ltrImg.enabled = textQueue[currentLine].ShowImmediate || (GlobalControls.retroMode && instantActive);

        return letters.Count - 1;
    }

    private void MoveLetter(string currentText, int letterIndex) {
        LetterData letter = letters[letterIndex];
        RectTransform rt = letter.image.GetComponent<RectTransform>();

        float letterShift = letter.sprite.border.w - letter.sprite.border.y;

        LuaTextManager ltm = this as LuaTextManager;
        if (ltm && ltm.adjustTextDisplay)
            letterShift = Mathf.Round(letterShift * ltm.yscale) / ltm.yscale;

        if (GetType() == typeof(LuaTextManager) || gameObject.name == "TextParent" || gameObject.name == "ReviveText")
            // Allow Game Over fonts to enjoy the fixed text positioning, too!
            rt.localPosition = new Vector3(currentX, currentY + letterShift, 0);
        else
            // Keep what we already have for all text boxes that are not Text Objects in an encounter
            rt.localPosition = new Vector3(currentX, currentY + (letterShift + 2), 0);

        rt.eulerAngles = new Vector3(0, 0, rotation);
        letters[letterIndex] = new LetterData(letter.index, letter.image, letter.sprite, rt.anchoredPosition, letters[letterIndex].commandColorSet, letters[letterIndex].commandAlphaSet);
    }

    protected virtual void SpawnText() {
        noSkip1stFrame = true;
        string currentText = textQueue[currentLine].Text;
        letters.Clear();
        if (currentText.Length > 1 && GetAutoLineBreak())
            SpawnTextSpaceTest(0, currentText, out currentText);

        // Work-around for [instant] and [instant:allowcommand] at the beginning of a line of text
        bool skipImmediate = false;
        string skipCommand = "";

        for (int i = 0; i < currentText.Length; i++) {
            switch (currentText[i]) {
                case '[':
                    int currentChar = i;
                    string command = UnitaleUtil.ParseCommandInline(currentText, ref i);
                    if (command == null)
                        i = currentChar;
                    else {
                        // Work-around for [noskip], [instant] and [instant:allowcommand]
                        if (!GlobalControls.retroMode) {
                            // The goal of this is to allow for commands executed "just before" [instant] on the first frame
                            // Example: "[func:test][instant]..."

                            // Special case for "[noskip]", "[instant]" and "[instant:allowcommand]"
                            if (command == "noskip" || command == "instant" || command == "instant:allowcommand") {
                                // Copy all text before the command
                                string precedingText = currentText.Substring(0, i - (command.Length + 1));

                                // Remove all commands, store them for later if using instant
                                List<string> commands = command == "noskip" ? null : new List<string>();

                                while (precedingText.IndexOf('[') > -1) {
                                    int j = precedingText.IndexOf('['), k = j;
                                    if (UnitaleUtil.ParseCommandInline(precedingText, ref k) == null) break;
                                    if (commands != null)
                                        commands.Add(precedingText.Substring(j + 1, (k - j) - 1));
                                    precedingText = precedingText.Replace(precedingText.Substring(j, (k - j) + 1), "");
                                }

                                // Confirm that our command is at the beginning!
                                if (precedingText.Length == 0)
                                    if (command == "noskip")
                                        PreCreateControlCommand(command);
                                    else {
                                        // Execute all commands that came before [instant] through InUpdateControlCommand
                                        foreach (string cmd in commands)
                                            InUpdateControlCommand(DynValue.NewString(cmd));

                                        skipImmediate = true;
                                        skipCommand = command;
                                        // InUpdateControlCommand(DynValue.NewString(command), i);
                                    }
                            } else if (command.Length < 7 || command.Substring(0, 7) != "instant")
                                PreCreateControlCommand(command);
                        } else
                            PreCreateControlCommand(command);
                        continue;
                    }
                    break;
                case ' ':
                    if (i + 1 == currentText.Length || currentText[i + 1] == ' ')
                        break;
                    if (GetAutoLineBreak()) {
                        SpawnTextSpaceTest(i, currentText, out currentText);
                        if (currentText[i] != ' ') {
                            i--;
                            continue;
                        }
                    }
                    break;
            }

            if (!font.Letters.ContainsKey(currentText[i]))
                continue;

            CreateLetter(currentText, i);
        }
        LuaTextManager ltm = this as LuaTextManager;
        if (ltm && ltm.adjustTextDisplay)
            ltm.Scale(ltm.xscale, ltm.yscale);
        MoveLetters();

        // Work-around for [instant] and [instant:allowcommand] at the beginning of a line of text
        if (skipImmediate)
            InUpdateControlCommand(DynValue.NewString(skipCommand));

        if (!instantActive)
            Update();
    }

    private float[] ComputeTextSpacings() {
        LuaTextManager ltm = this as LuaTextManager;
        float normalizedHSpacing = hSpacing;
        float normalizedVSpacing = vSpacing + font.LineSpacing;
        if (ltm && ltm.adjustTextDisplay) {
            float spaceHeight = font.Letters[' '].rect.height;

            // Normalize shifts so they're integers
            bool isNormalizedHSpacingPositive = normalizedHSpacing >= 0.001f;
            normalizedHSpacing = Mathf.Round((normalizedHSpacing - 0.001f) * ltm.xscale);
            if (isNormalizedHSpacingPositive)
                normalizedHSpacing = Mathf.Max(1, normalizedHSpacing);
            normalizedHSpacing /= ltm.xscale;

            float relativeVSpacing = normalizedVSpacing - spaceHeight;
            bool isRelativeVSpacingPositive = relativeVSpacing >= 0.001f;
            relativeVSpacing = Mathf.Round((relativeVSpacing - 0.001f) * ltm.yscale);
            if (isRelativeVSpacingPositive)
                relativeVSpacing = Mathf.Max(1, relativeVSpacing);
            relativeVSpacing /= ltm.yscale;
            normalizedVSpacing = relativeVSpacing + spaceHeight;
        }

        return new float[] { normalizedHSpacing, normalizedVSpacing };
    }

    public void MoveLetters() {
        ResetFont();

        float baseHSpacing = hSpacing;
        float baseVSpacing = vSpacing;

        LuaTextManager ltm = this as LuaTextManager;
        if (ltm && ltm.adjustTextDisplay) {
            // Compute letter shift to align it to the integer position grid
            float xPos = ltm.GetContainer().transform.position.x,
                  yPos = ltm.GetContainer().transform.position.y;
            currentX = (Mathf.Round(xPos) - xPos + 0.01f) / ltm.xscale;
            currentY = (Mathf.Round(yPos) - yPos + 0.01f) / ltm.yscale;
        } else {
            currentX = 0.01f;
            currentY = 0.01f;
            // allow Game Over fonts to enjoy the fixed text positioning, too!
            if (!ltm && gameObject.name != "TextParent" && gameObject.name != "ReviveText")
                currentY -= font.LineSpacing;
        }
        startingLineX = currentX;
        startingLineY = currentY;

        float[] spacings = ComputeTextSpacings();
        float normalizedHSpacing = spacings[0];
        float normalizedVSpacing = spacings[1];

        string currentText = textQueue[currentLine].Text;
        int tabCount = 0;
        for (int i = 0; i < currentText.Length; i++) {
            int currentChar = i;
            switch (currentText[i]) {
                case '[':
                    string command = UnitaleUtil.ParseCommandInline(currentText, ref i);
                    if (command == null || !movementCommands.Contains(command.Split(':')[0]))
                        i = currentChar;
                    else {
                        PreCreateControlCommand(command, true);
                        spacings = ComputeTextSpacings();
                        normalizedHSpacing = spacings[0];
                        normalizedVSpacing = spacings[1];
                    }
                    break;
                case '\n':
                    currentX = startingLineX + normalizedVSpacing * Mathf.Sin(rotation * Mathf.Deg2Rad);
                    currentY = startingLineY - normalizedVSpacing * Mathf.Cos(rotation * Mathf.Deg2Rad);
                    startingLineX = currentX;
                    startingLineY = currentY;
                    tabCount = 0;
                    break;
                case '\t':
                    currentX = ++tabCount * columnShift;
                    break;
            }
            if (currentChar == i && letters.Exists(l => l.index == i)) {
                LetterData letter = letters.Find(l => l.index == i);
                MoveLetter(currentText, letters.IndexOf(letter));
                RectTransform rt = letter.image.GetComponent<RectTransform>();
                currentX += (rt.rect.width * rt.localScale.x + normalizedHSpacing) * Mathf.Cos(rotation * Mathf.Deg2Rad);
                currentY += (rt.rect.width * rt.localScale.x + normalizedHSpacing) * Mathf.Sin(rotation * Mathf.Deg2Rad);
            }
        }

        hSpacing = baseHSpacing;
        vSpacing = baseVSpacing;
    }

    public void AlignLetters() {
        LuaTextManager ltm = this as LuaTextManager;
        if (!ltm || !ltm.adjustTextDisplay || ltm.xscale == 0 || ltm.yscale == 0 || letters.Count == 0 || !letters.Any(l => Mathf.Abs(l.image.GetComponent<RectTransform>().localPosition.x) < 1))
            return;

        LetterData letter = letters.Find(l => Mathf.Abs(l.image.GetComponent<RectTransform>().localPosition.x) < 1);
        Vector2 positionBase = letter.image.GetComponent<RectTransform>().position;
        Vector2 localPositionBase = letter.image.GetComponent<RectTransform>().localPosition;

        float xShift = 0, yShift = 0;

        float xDiff = Mathf.Abs(positionBase.x % 1 - 0.01f);
        if (xDiff >= 0.001f)
            xShift = -positionBase.x % 1 + 0.01f - Mathf.Round(localPositionBase.x - xDiff);

        float yDiff = Mathf.Abs(positionBase.y % 1 - 0.01f);
        if (yDiff >= 0.001f)
            yShift = -positionBase.y % 1 + 0.01f - Mathf.Round(localPositionBase.y - yDiff);

        if (xShift == 0 && yShift == 0)
            return;

        foreach (LetterData l in letters)
            l.image.GetComponent<RectTransform>().localPosition += new Vector3(xShift / ltm.xscale, yShift / ltm.yscale);
    }

    private bool CheckCommand() {
        if (currentLine >= textQueue.Length)
            return false;
        if (currentCharacter >= textQueue[currentLine].Text.Length) return false;
        if (textQueue[currentLine].Text[currentCharacter] != '[') return false;
        int    currentChar = currentCharacter;
        string command     = UnitaleUtil.ParseCommandInline(textQueue[currentLine].Text, ref currentCharacter);
        if (command != null) {
            currentCharacter++; // we're not in a continuable loop so move to the character after the ] manually

            DynValue commandDV = DynValue.NewString(command);
            InUpdateControlCommand(commandDV, currentCharacter);

            return true;
        }
        currentCharacter = currentChar;
        return false;
    }

    protected virtual void Update() {
        if (mugshot != null && mugshotList != null)
            if (UnitaleUtil.IsOverworld && mugshot.alpha != 0 && mugshotList.Length > 1) {
                if (!mugshot.animcomplete && (letterTimer < 0 || LineComplete())) {
                    mugshot.StopAnimation();
                    mugshot.Set(mugshotList.Last());
                } else if (mugshot.animcomplete && !(letterTimer < 0 || LineComplete()))
                    mugshot.SetAnimation(mugshotList, mugshotTimer);
            }

        if (!isactive || textQueue[currentLine] == null || paused || lateStartWaiting)
            return;

        if (textEffect != null)
            textEffect.UpdateEffects();

        if (GlobalControls.retroMode && instantActive || currentCharacter >= textQueue[currentLine].Text.Length)
            return;

        if (waitingChar != KeyCode.None) {
            if (Input.GetKeyDown(waitingChar)) waitingChar = KeyCode.None;
            else                               return;
        }
        if (waitingKeybind != null) {
            if (KeyboardInput.StateFor(waitingKeybind) == ButtonState.PRESSED) waitingKeybind = null;
            else                                                               return;
        }

        letterTimer += Time.deltaTime;
        if ((letterTimer >= timePerLetter || firstChar) && !LineComplete()) {
            int repeats = timePerLetter == 0f ? 1 : (int)Mathf.Floor(letterTimer / timePerLetter);

            bool soundPlayed = firstChar && lettersToDisplay > 1;
            int lastLetter = -1;

            for (int i = 0; i < repeats; i++) {
                if (lettersToDisplayOnce > 0)
                    HandleShowLettersOnce(ref soundPlayed, ref lastLetter);
                else
                    for (int j = 0; j < lettersToDisplay; j++)
                        if (!HandleShowLetter(ref soundPlayed, ref lastLetter))
                            break;

                if (!firstChar)
                    letterTimer -= timePerLetter;
                else {
                    firstChar = false;
                    return;
                }
            }
        }

        noSkip1stFrame = false;
    }

    private void HandleShowLettersOnce(ref bool soundPlayed, ref int lastLetter) {
        while (lettersToDisplayOnce != 0 && !instantCommand) {
            if (!HandleShowLetter(ref soundPlayed, ref lastLetter, true)) return;
            lettersToDisplayOnce--;
        }
    }

    private bool HandleShowLetter(ref bool soundPlayed, ref int lastLetter, bool fromOnce = false) {
        if (lastLetter != currentCharacter) {
            float oldLetterTimer = letterTimer;
            int oldLettersToDisplay = lettersToDisplay;
            int oldLettersToDisplayOnce = lettersToDisplayOnce;
            lastLetter = currentCharacter;
            while (CheckCommand()) {
                if ((fromOnce && lettersToDisplayOnce != oldLettersToDisplayOnce) || (!fromOnce && lettersToDisplay != oldLettersToDisplay))
                    return false;
                if ((GlobalControls.retroMode && instantActive) || letterTimer != oldLetterTimer || waitingChar != KeyCode.None || paused)
                    return false;
            }
            if (currentCharacter >= textQueue[currentLine].Text.Length)
                return false;
        }

        if (letters.Exists(l => l.index == currentCharacter)) {
            Image im = letters.Find(l => l.index == currentCharacter).image;
            if (im == null) return false;
            im.enabled = true;
            letterEffectStepCount += letterEffectStep;
            if (im.GetComponent<Letter>().effect != null)
                im.GetComponent<Letter>().effect.ResetPositions();
            switch (letterEffect.ToLower()) {
                case "twitch": im.GetComponent<Letter>().effect = new TwitchEffectLetter(im.GetComponent<Letter>(), letterIntensity, (int)letterEffectStep);   break;
                case "rotate": im.GetComponent<Letter>().effect = new RotatingEffectLetter(im.GetComponent<Letter>(), letterIntensity, letterEffectStepCount); break;
                case "shake":  im.GetComponent<Letter>().effect = new ShakeEffectLetter(im.GetComponent<Letter>(), letterIntensity);                           break;
                default:       im.GetComponent<Letter>().effect = null;                                                                                        break;
            }

            currentReferenceCharacter++;
        }

        if (!string.IsNullOrEmpty(GetVoice()) && !muted && !soundPlayed && (GlobalControls.retroMode || textQueue[currentLine].Text[currentCharacter] != ' ')) {
            soundPlayed = true;
            try { UnitaleUtil.PlayVoice("BubbleSound", GetVoice()); }
            catch (CYFException e) { UnitaleUtil.DisplayLuaError("Playing a voice", e.Message); }
        }

        currentCharacter++;
        return true;
    }

    private void PreCreateControlCommand(string command, bool movementCommand = false) {
        string[] cmds = UnitaleUtil.SpecialSplit(':', command);
        // Only allow letter movement commands on the letter movement pass
        if (movementCommand && !movementCommands.Contains(cmds[0]))
            return;
        string[] args = new string[0];
        if (cmds.Length == 2) {
            args = UnitaleUtil.SpecialSplit(',', cmds[1], true);
            cmds[1] = args[0];
        }
        // TODO: Restore errors for 0.7
        switch (cmds[0].ToLower()) {
            case "color":
                float oldAlpha = commandColor.a;
                commandColorSet = args.Length >= 1;
                try { commandColor = commandColorSet ? ParseUtil.GetColor(cmds[1]) : defaultColor; }
                catch { Debug.LogError("[color:x] usage - You used the value \"" + cmds[1] + "\" to set the text's color but it's not a valid hexadecimal color value."); }
                commandColor.a = oldAlpha;
                break;
            case "alpha":
                commandAlphaSet = args.Length >= 1;
                try { commandColor.a = commandAlphaSet ? ParseUtil.GetByte(cmds[1]) / 255 : defaultColor.a; }
                catch { Debug.LogError("[alpha:x] usage - You used the value \"" + cmds[1] + "\" to set the text's alpha but it's not a valid hexadecimal value."); }

                break;
            case "charspacing":
                try {
                    if (cmds.Length > 1 && cmds[1].ToLower() == "default") SetHorizontalSpacing(font.CharSpacing);
                    else                                                   SetHorizontalSpacing(ParseUtil.GetFloat(cmds[1]));
                } catch (CYFException) {
                    Debug.LogError("[charspacing:x] usage - You used the value \"" + cmds[1] + "\" to set the text's horizontal spacing but it's not a valid number value.");
                }
                break;
            case "linespacing":
                try {
                    if (cmds.Length > 1)
                        SetVerticalSpacing(ParseUtil.GetFloat(cmds[1]));
                } catch (CYFException) {
                    Debug.LogError("[linespacing:x] usage - You used the value \"" + cmds[1] + "\" to set the text's vertical spacing but it's not a valid number value.");
                }
                break;

            case "starcolor":
                try {
                    Color starColor = ParseUtil.GetColor(cmds[1]);
                    int indexOfStar = textQueue[currentLine].Text.IndexOf('*'); // HACK oh my god lol
                    if (indexOfStar > -1)
                        if (letters.Exists(l => l.index == indexOfStar))
                            letters.Find(l => l.index == indexOfStar).image.color = starColor;
                } catch (CYFException) {
                    Debug.LogError("[starcolor:x] usage - You used the value \"" + cmds[1] + "\" to set the color of the text's star, but it's not a valid hexadecimal color value.");
                }
                break;

            case "instant":
                if (GlobalControls.retroMode)
                    instantActive = true;
                else
                    InUpdateControlCommand(DynValue.NewString(command));
                break;

            case "noskip":
                if (args.Length == 0) currentSkippable = false;
                break;

            case "font":
                UnderFont uf = SpriteFontRegistry.Get(cmds[1]);
                if (uf == null) {
                    UnitaleUtil.DisplayLuaError("", "[font:x] usage - The font \"" + cmds[1] + "\" doesn't exist.\nYou should check if you made a typo, or if the font really is in your mod.", true);
                    break;
                }
                SetFont(uf);
                if (GetType() == typeof(LuaTextManager) && ((LuaTextManager)this).bubble)
                    ((LuaTextManager) this).UpdateBubble();
                break;

            case "effect":
                float step = args.Length > 2 ? ParseUtil.GetFloat(args[2]) : 0;
                SetEffect(cmds[1].ToLower(), args.Length > 1 ? ParseUtil.GetFloat(args[1]) : -1, step);
                break;

            case "mugshot":
                DynValue temp = DynValue.NewNil();
                if (args.Length > 0)
                    temp = args[0][0] == '{' ? UnitaleUtil.RebuildTableFromString(args[0]) : DynValue.NewString(args[0]);

                bool oldLineHasMugshot = lineHasMugshot;
                if (temp.Type != DataType.Nil && temp.Type != DataType.Void && !(temp.Type == DataType.String && (temp.String == "null" || temp.String == "")))
                    lineHasMugshot = true;
                SetMugshotShift(oldLineHasMugshot);
                break;
        }
    }

    private void InUpdateControlCommand(DynValue command, int index = 0) {
        string[] cmds = UnitaleUtil.SpecialSplit(':', command.String);
        string[] args = new string[0];
        if (cmds.Length > 1) {
            args = UnitaleUtil.SpecialSplit(',', cmds[1], true);
            cmds[1] = args[0];
        }

        string tag = cmds[cmds.Length - 1];
        if (tag == "skipover" && instantActive) return;
        if (tag == "skiponly" && !instantActive) return;

        // TODO: Restore errors for 0.7
        switch (cmds[0].ToLower()) {
            case "noskip":
                if (args.Length == 0)      currentSkippable = false;
                else if (args[0] == "off") currentSkippable = true;
                break;

            case "waitfor":
                try {
                    if (KeyboardInput.KeybindExists(cmds[1])) {
                        waitingKeybind = cmds[1];
                        return;
                    }
                    waitingChar = (KeyCode)Enum.Parse(typeof(KeyCode), cmds[1]);
                }
                catch { Debug.LogError("[waitfor:x] usage - The key \"" + cmds[1] + "\" is neither a valid key or a known keybind."); }
                break;

            case "w":
                try { letterTimer = timePerLetter - singleFrameTiming * ParseUtil.GetInt(cmds[1]); }
                catch { Debug.LogError("[w:x] usage - You used the value \"" + cmds[1] + "\" to wait for a certain amount of frames, but it's not a valid integer value."); }
                break;

            case "waitall":
                try { timePerLetter = singleFrameTiming * ParseUtil.GetInt(cmds[1]); }
                catch { Debug.LogError("[waitall:x] usage - You used the value \"" + cmds[1] + "\" to set the text's waiting time between letters, but it's not a valid integer value."); }
                break;

            case "novoice":     commandVoice = "none"; break;
            case "next":        autoSkip = true;       break;
            case "finished":    autoSkipThis = true;   break;
            case "nextthisnow": autoSkipAll = true;    break;
            case "speed":
                try {
                    //you can only set text speed to a number >= 0
                    float newSpeedValue = ParseUtil.GetFloat(args[0]);
                    // protect against divide-by-zero errors
                    if (newSpeedValue > 0f)
                        timePerLetter = singleFrameTiming / newSpeedValue;
                    else if (newSpeedValue == 0f)
                        timePerLetter = 0f;
                } catch {
                    Debug.LogError("[speed:x] usage - You used the value \"" + args[0] + "\" to set the text's typing speed, but it's not a valid number value.");
                }
                break;

            case "letters":
                try {
                    lettersToDisplayOnce = ParseUtil.GetInt(args[0]);
                    firstChar = true;
                    Update();
                } catch { Debug.LogError("[letters:x] usage - You used the value \"" + args[0] + "\" to display a given amount of letters instantly, but it's not a valid integer value."); }
                break;

            case "lettersperframe":
                try { lettersToDisplay = ParseUtil.GetInt(args[0]); }
                catch { Debug.LogError("[lettersperframe:x] usage - You used the value \"" + args[0] + "\" to display a given amount of letters every frame, but it's not a valid integer value."); }
                break;

            case "voice":
                if (cmds[1].ToLower() != "default") {
                    try {
                        AudioClipRegistry.GetVoice(cmds[1].ToLower());
                        commandVoice = cmds[1].ToLower();
                    } catch (InterpreterException) { UnitaleUtil.Warn("The voice file " + cmds[1].ToLower() + " doesn't exist. Note that all sound files use lowercase letters only.", false); }
                } else
                    commandVoice = null;
                break;

            case "instant":
                if (args.Length != 0 && (args.Length > 1 || args[0] != "allowcommand"))
                    break;

                instantActive = true;

                if (command.String == "instant:allowcommand")
                    instantCommand = true;

                // First:  Find the active line of text
                string currentText = textQueue[currentLine].Text;

                // Second: Find the position to "end" at
                // This will either be: [instant:stop], [instant:stopall] or the end of the string
                int pos = currentText.Length;

                for (int i = index; i < pos; i++) {
                    if (!skipFromPlayer) {
                        if ((currentText.Substring(i)).Length < 13 || pos - i < 13 || currentText.Substring(i, 13) != "[instant:stop") continue;
                        pos = i - 1;
                        break;
                    }

                    if ((currentText.Substring(i)).Length < 16 || pos - i < 16 || currentText.Substring(i, 16) != "[instant:stopall") continue;
                    pos = i - 1;
                    break;
                }

                // Third: Show all letters (and execute all commands, if applicable) between `index` and `pos`
                bool soundPlayed = true;
                int lastLetter = -1;
                int destination = System.Math.Min(pos, textQueue[currentLine].Text.Length);
                while (currentCharacter < destination)
                    HandleShowLetter(ref soundPlayed, ref lastLetter);

                // This is a catch-all.
                // If a line of text starts with [instant], the above code will not display the letters it passes over,
                // due to how HandleShowLetter is coded.
                foreach (LetterData letter in letters.Where(l => l.index >= index && l.index < pos))
                    letter.image.enabled = true;

                // Fourth:  Update variables
                if (pos < currentText.Length) {
                    instantActive  = false;
                    instantCommand = false;
                    letterTimer = timePerLetter;
                }

                skipFromPlayer = false;
                break;

            case "func":
                try {
                    if (caller == null)
                        UnitaleUtil.DisplayLuaError("???", "Func called but no script to reference. This is the engine's fault, not yours.");
                    if (args.Length > 1) {
                        //Check array as argument
                        if (args.Length == 2) {
                            args[1] = args[1].Trim();
                            if (args[1][0] == '{' && args[1][args[1].Length - 1] == '}') {
                                args[1] = args[1].Substring(1, args[1].Length - 2);
                                string[] newArgs = UnitaleUtil.SpecialSplit(',', args[1], true);
                                Array.Resize(ref args, 1 + newArgs.Length);
                                Array.Copy(newArgs, 0, args, 1, newArgs.Length);
                            }
                        }

                        DynValue[] argsbis = new DynValue[args.Length - 1];
                        for (int i = 1; i < args.Length; i++)
                            argsbis[i - 1] = ComputeArgument(args[i]);
                        if (caller != null)
                            caller.Call(args[0], argsbis, true);
                    } else if (caller != null)
                        caller.Call(cmds[1], null, true);
                } catch (InterpreterException ex) { UnitaleUtil.DisplayLuaError(caller.scriptname, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message); }
                break;

            case "mugshot":
                DynValue temp = DynValue.NewNil();
                if (args.Length > 0)
                    temp = args[0][0] == '{' ? UnitaleUtil.RebuildTableFromString(args[0]) : DynValue.NewString(args[0]);

                SetMugshot(temp);
                break;

            case "music":
                switch (args[0]) {
                    case "play":    Camera.main.GetComponent<AudioSource>().Play();    break;
                    case "pause":   Camera.main.GetComponent<AudioSource>().Pause();   break;
                    case "unpause": Camera.main.GetComponent<AudioSource>().UnPause(); break;
                    case "stop":
                    case "null":
                    case "":
                    case "nil":     Camera.main.GetComponent<AudioSource>().Stop();    break;
                    default:
                        Camera.main.GetComponent<AudioSource>().clip = AudioClipRegistry.GetSound(args[0]);
                        Camera.main.GetComponent<AudioSource>().Play();
                        break;
                }
                break;

            case "sound":
                //In a battle
                GameObject.Find(GameObject.Find("player") != null ? "player" : "Player").GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetSound(args[0]));
                break;

            case "health":
                args[0] = args[0].Replace(" ", "");
                bool killable = false;
                if (args.Length > 1) {
                    args[1] = args[1].Replace(" ", "");
                    if (args[1] == "killable")
                        killable = true;
                }
                float HP = PlayerCharacter.instance.HP, MaxHP = PlayerCharacter.instance.MaxHP, tryHP = 0;
                try { tryHP = ParseUtil.GetInt(args[0]); }
                catch {
                    if (args[0] != "Max" && args[0] != "Max-1" && args[0] != "kill") {
                        Debug.LogError("[health:x] usage - You used the value \"" + args[0] + "\" to set the player's HP, but it's not a valid integer value.");
                        return;
                    }
                }

                if ((args[0].Contains("-") && args[0] != "Max-1") || args[0] == "kill") PlayerController.PlaySound("hurtsound");
                else if (args.Length > 1) {
                    if (args[1] == "set" && tryHP < HP)                                 PlayerController.PlaySound("hurtsound");
                    else                                                                PlayerController.PlaySound("healsound");
                } else                                                                  PlayerController.PlaySound("healsound");

                switch (args[0]) {
                    case "kill":  SetHP(0);         break;
                    case "Max-1": SetHP(MaxHP - 1); break;
                    case "Max":   SetHP(MaxHP);     break;
                    default: {
                        if (!killable) {
                            if (args.Length > 1) {
                                if (args[1] == "set")
                                    SetHP(Mathf.Max(tryHP, 1));
                            } else  SetHP(Mathf.Max(HP + tryHP, 1));
                        } else      SetHP(HP + tryHP);

                        break;
                    }
                }
                break;

            case "lettereffect":
                letterEffect = args[0];

                if (args.Length > 1) {
                    try { letterIntensity = ParseUtil.GetFloat(args[1]); }
                    catch { Debug.LogError("[lettereffect:x] usage - You used the value \"" + args[1] + "\" to set the letter effect's intensity, but it's not a valid number value."); }
                } else
                    letterIntensity = 0;

                if (args.Length > 2) {
                    try {
                        letterEffectStep = ParseUtil.GetFloat(args[2]);
                        letterEffectStepCount = 0;
                    } catch { Debug.LogError("[lettereffect:x] usage - You used the value \"" + args[2] + "\" to set the letter effect's step, but it's not a valid number value."); }
                } else {
                    letterEffectStep = 0;
                    letterEffectStepCount = 0;
                }
                break;
        }
    }

    private DynValue ComputeArgument(string arg) {
        arg = arg.Trim();
        Type type = UnitaleUtil.CheckRealType(arg);
        DynValue dyn;
        //Boolean
        if (type == typeof(bool))
            dyn = DynValue.NewBoolean(arg.Replace(" ", "") == "true");
        //Number
        else if (type == typeof(float)) {
            arg = arg.Replace(" ", "");
            float number = CreateNumber(arg);
            dyn = DynValue.NewNumber(number);
        }
        //String
        else
            dyn = DynValue.NewString(arg);
        return dyn;
    }

    private void SetHP(float newhp) {
        float HP;
        newhp = Mathf.Round(newhp * Mathf.Pow(10, ControlPanel.instance.MaxDigitsAfterComma)) / Mathf.Pow(10, ControlPanel.instance.MaxDigitsAfterComma);
        if (newhp <= 0) {
            GameOverBehavior gob = FindObjectOfType<GameOverBehavior>();
            if (!MusicManager.IsStoppedOrNull(PlayerOverworld.audioKept)) {
                gob.musicBefore = PlayerOverworld.audioKept;
                gob.music = gob.musicBefore.clip;
                gob.musicBefore.Stop();
            } else if (!MusicManager.IsStoppedOrNull(Camera.main.GetComponent<AudioSource>())) {
                gob.musicBefore = Camera.main.GetComponent<AudioSource>();
                gob.music = gob.musicBefore.clip;
                gob.musicBefore.Stop();
            } else {
                gob.musicBefore = null;
                gob.music = null;
            }
            PlayerCharacter.instance.HP = 0;
            // gameObject.transform.SetParent(null);
            // GameObject.DontDestroyOnLoad(this.gameObject);
            RectTransform rt = gameObject.GetComponent<RectTransform>();
            MoveToAbs(rt.position.x, rt.position.y);
            gob.StartDeath();
            return;
        }
        //HP greater than Max, heal, already more HP than Max

        if (newhp > PlayerCharacter.instance.MaxHP && newhp > PlayerCharacter.instance.HP && PlayerCharacter.instance.HP > PlayerCharacter.instance.MaxHP)
            HP = PlayerCharacter.instance.HP;
        //HP greater than Max, heal
        else if (newhp > PlayerCharacter.instance.MaxHP && newhp > PlayerCharacter.instance.HP) HP = PlayerCharacter.instance.MaxHP;
        else                                                                                    HP = newhp;
        if (HP > ControlPanel.instance.HPLimit)                                                 HP = ControlPanel.instance.HPLimit;
        PlayerCharacter.instance.HP = HP;
        if (!UnitaleUtil.IsOverworld)
            UIStats.instance.setHP(HP);
    }

    public virtual void Move(float newX, float newY) {
        MoveToAbs(transform.position.x + newX, transform.position.y + newY);
    }

    public virtual void MoveTo(float newX, float newY) {
        MoveToAbs(transform.parent.position.x + newX, transform.parent.position.y + newY);
    }

    public virtual void MoveToAbs(float newX, float newY) {
        transform.position = new Vector3(Mathf.Round(newX), Mathf.Round(newY), transform.position.z);
        LuaTextManager ltm = this as LuaTextManager;
        if (ltm && ltm.adjustTextDisplay)
            MoveLetters();
    }

    private float CreateNumber(string str) {
        float number = 0, dot = -1;
        int index = 0;
        bool negative = false;
        foreach (char c in str) {
            switch (c) {
                case '-': negative = true; continue;
                case ' ': {
                    if (dot != -1)
                        dot++;
                    continue;
                }
                case '.':
                    dot = index;
                    index++;
                    continue;
            }

            if (dot == -1) number = number * 10 + c - 48;
            else           number += ((float)c - 48) / Mathf.Pow(10, - (dot - index));
            index++;
        }
        if (negative)
            return -number;
        return number;
    }

    public virtual bool isactive {
        get { return !hidden; }
    }
}