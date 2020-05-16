using System;
using System.Linq;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// TODO less code duplicate-y way of pulling commands out of the text.
public class TextManager : MonoBehaviour {
    internal Image[] letterReferences;
    internal Vector2[] letterPositions;

    protected UnderFont default_charset = null;
    protected AudioClip default_voice = null;
    [MoonSharpHidden] public AudioSource letterSound = null;
    protected TextEffect textEffect = null;
    private string letterEffect = "none";
    public static string[] commandList = new string[] { "color", "alpha", "charspacing", "linespacing", "starcolor", "instant", "font", "effect", "noskip", "w", "waitall", "novoice",
                                                        "next", "finished", "nextthisnow", "noskipatall", "waitfor", "speed", "letters", "voice", "func", "mugshot", "name",
                                                        "music", "sound", "health", "lettereffect"};
    private float letterIntensity = 0.0f;
    public int currentLine = 0;
    [MoonSharpHidden] public int _textMaxWidth = 0;
    private int currentCharacter = 0;
    public int currentReferenceCharacter = 0;
    private bool currentSkippable = true;
    private bool decoratedTextOffset = false;
    [MoonSharpHidden] public bool nextMonsterDialogueOnce = false, nmd2 = false, wasStated = false;
    private RectTransform self;
    [MoonSharpHidden] public Vector2 offset;
    private bool offsetSet = false;
    private float currentX;
    //private float _currentY;
    private float currentY; /* {
        get { return _currentY; }
        set {
            if (GetType() == typeof(LuaTextManager))
                print("Change currentY value: " + _currentY + " => " + value);
            _currentY = value;
        }
    }*/

    // Variables that have to do with "[instant]"
    private bool instantActive  = false; // Will be true if "[instant]" or "[instant:allowcommand]" have been activated
    private bool instantCommand = false; // Will be true only if "[instant:allowcommand]" has been activated

    private bool paused = false;
    private bool muted = false;
    private bool autoSkipThis = false;
    private bool autoSkipAll = false;
    private bool autoSkip = false;
    private bool skipFromPlayer = false;
    private bool firstChar = false;
    internal float hSpacing = 3;
    internal float vSpacing = 0;
    private GameObject textframe;
    private LuaSpriteController mugshot;
    private string[] mugshotList = null;
    private string finalMugshot;
    private float mugshotTimer;
    // private int letterSpeed = 1;
    private int letterOnceValue = 0;
    private KeyCode waitingChar = KeyCode.None;

    protected Color currentColor = Color.white;
    private bool colorSet = false;
    protected Color defaultColor = Color.white;
    //private Color defaultColor = Color.white;

    private float letterTimer = 0.0f;
    private float timePerLetter;
    private float singleFrameTiming = 1.0f / 20;

    [MoonSharpHidden] public ScriptWrapper caller;

    [MoonSharpHidden] public UnderFont Charset { get; protected set; }
    [MoonSharpHidden] public TextMessage[] textQueue = null;
    //public string[] mugshotsPath;
    //public bool overworld;
    [MoonSharpHidden] public bool blockSkip = false;
    [MoonSharpHidden] public bool skipNowIfBlocked = false;
    internal bool noSkip1stFrame = true;

    [MoonSharpHidden] public bool lateStartWaiting = false; // Lua text objects will use a late start

    [MoonSharpHidden] public void SetCaller(ScriptWrapper s) { caller = s; }

    public void SetFont(UnderFont font, bool firstTime = false) {
        Charset = font;
        if (default_charset == null)
            default_charset = font;
        if (firstTime) {
            if (letterSound == null && font.Sound != null)
                letterSound.clip = font.Sound;
        } else if (font.Sound != null)
            letterSound.clip = font.Sound;

        vSpacing = 0;
        hSpacing = font.CharSpacing;
        defaultColor = font.DefaultColor;
        if (GetType() == typeof(LuaTextManager)) {
            if (((LuaTextManager) this).hasColorBeenSet) defaultColor = ((LuaTextManager) this)._color;
            if (((LuaTextManager) this).hasAlphaBeenSet) defaultColor.a = ((LuaTextManager) this).alpha;
        }
        currentColor = defaultColor;
    }

    [MoonSharpHidden] public void SetHorizontalSpacing(float spacing = 3) { hSpacing = spacing; }
    [MoonSharpHidden] public void SetVerticalSpacing(float spacing = 0) { vSpacing = spacing; }

    [MoonSharpHidden] public void ResetFont() {
        if (Charset == null || default_charset == null)
            if (GetType() == typeof(LuaTextManager))
                ((LuaTextManager)this).SetFont(SpriteFontRegistry.UI_MONSTERTEXT_NAME);
            else
                SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_DEFAULT_NAME), true);
        Charset = default_charset;
        letterSound.clip = default_voice ?? default_charset.Sound;
        defaultColor = default_charset.DefaultColor;

        // Default voice in the overworld
        if (gameObject.name == "TextManager OW")
            default_voice = AudioClipRegistry.GetVoice("monsterfont");
    }

    protected virtual void Awake() {
        self = gameObject.GetComponent<RectTransform>();
        letterSound = gameObject.AddComponent<AudioSource>();
        letterSound.playOnAwake = false;
        // SetFont(SpriteFontRegistry.F_UI_DIALOGFONT);
        timePerLetter = singleFrameTiming;

        if (UnitaleUtil.IsOverworld && GameObject.Find("textframe_border_outer")) {
            textframe = GameObject.Find("textframe_border_outer");
            mugshot = new LuaSpriteController(GameObject.Find("Mugshot").GetComponent<Image>());
        }
    }

    private void Start() {
        // ResetFont();
        // SetText("the quick brown fox jumps over\rthe lazy dog.\nTHE QUICK BROWN FOX JUMPS OVER\rTHE LAZY DOG.\nJerry.", true, true);
        // SetText(new TextMessage("Here comes Napstablook.", true, false));
        // SetText(new TextMessage(new string[] { "Check", "Compliment", "Ignore", "Steal", "trow temy", "Jerry" }, false));
    }

    public void SetPause(bool pause) { this.paused = pause; }

    public bool IsPaused() { return this.paused; }

    [MoonSharpHidden] public bool IsFinished() {
        if (letterReferences == null)
            return false;
        return currentCharacter >= letterReferences.Length;
    }

    [MoonSharpHidden] public void SetMute(bool muted) { this.muted = muted; }

    public void SetText(TextMessage text) { SetTextQueue(new TextMessage[] { text }); }

    [MoonSharpHidden] public void SetTextQueue(TextMessage[] textQueue) {
        if (UnitaleUtil.IsOverworld && (gameObject.name == "TextManager OW"))
            PlayerOverworld.AutoSetUIPos();

        ResetFont();
        this.textQueue = textQueue;
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

    [MoonSharpHidden] public void AddToTextQueue(TextMessage text) { AddToTextQueue(new TextMessage[] { text }); }

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
        if (textQueue == null)
            return 0;
        return textQueue.Length;
    }

    [MoonSharpHidden] public void SetOffset(float xOff, float yOff) {
        offset = new Vector2(xOff, yOff);
        offsetSet = true;
    }

    public bool LineComplete() {
        if (letterReferences == null)
            return false;
        return (instantActive || currentCharacter == letterReferences.Length);
    }

    [MoonSharpHidden] public bool AllLinesComplete() {
        if (textQueue == null)  return true;
        else                    return currentLine == textQueue.Length - 1 && LineComplete();
    }

    private void SetMugshot(DynValue text) {
        List<string> mugshots = new List<string>();
        float time = -1;
        finalMugshot = null;
        if (text != null) {
            if (text.String != "null")
                if (text.Type == DataType.Table) {
                    int count = 0;
                    foreach (DynValue dv in text.Table.Values) {
                        count++;
                        if (dv.Type == DataType.Number && count >= text.Table.Length - 1 && time == -1)
                            time = (float)dv.Number;
                        else if (time != -1)
                            finalMugshot = "mugshots/" + dv.String;
                        else
                            mugshots.Add("mugshots/" + dv.String);
                    }
                } else
                    mugshots.Add("mugshots/" + text.String);
            else
                mugshots.Add("mugshots/");
        } else
            mugshots.Add("mugshots/");

        bool mugshotSet = false;
        if (mugshot != null && mugshot._img != null) {
            mugshot.StopAnimation();
            if ((mugshots.Count > 1 || (mugshots[0] != "mugshots/" && mugshots[0] != "mugshots/null")) && text != null) {
                try {
                    if (mugshots.Count > 1) {
                        time = time > 0f ? time : 0.2f;
                        mugshot.SetAnimation((string[])UnitaleUtil.ListToArray(mugshots), time);
                        if (finalMugshot == null)
                            finalMugshot = mugshots[mugshots.Count - 1];
                    } else {
                        mugshot.StopAnimation();
                        mugshot.Set(mugshots[0]);
                    }
                } catch (CYFException e) {
                    UnitaleUtil.DisplayLuaError("mugshot system", e.Message);
                }
                mugshotSet = true;
                mugshotTimer = time;
                mugshotList = (string[])UnitaleUtil.ListToArray(mugshots);
                mugshot.color = new float[] { 1, 1, 1, 1 };
                self.localPosition = new Vector3(-150, self.localPosition.y, self.localPosition.z);
            } else {
                mugshot.Set("empty");
                mugshotList = null;
                mugshot.color = new float[] { 1, 1, 1, 0 };
                if (gameObject.name == "TextManager OW")
                    self.localPosition = new Vector3(-267, self.localPosition.y, self.localPosition.z);
            }
        }
        _textMaxWidth = mugshotSet ? 417 : 534;
    }

    protected void ShowLine(int line, bool forceNoAutoLineBreak = false) {
        /*if (overworld) {
            if (mugshotsPath != null)
                if (mugshotsPath[line] != null || mugshotsPath[line] != "")
                    mugshot.sprite = SpriteRegistry.GetMugshot(mugshotsPath[line]);
                else
                    mugshot.sprite = null;

                if (mugshot.sprite == null) {
                    mugshot.color = new Color(mugshot.color.r, mugshot.color.g, mugshot.color.b, 0);
                    self.localPosition = new Vector3(-267, self.localPosition.y, self.localPosition.z);
                } else {
                    mugshot.color = new Color(mugshot.color.r, mugshot.color.g, mugshot.color.b, 1);
                    self.localPosition = new Vector3(-150, self.localPosition.y, self.localPosition.z);
                }
        }*/
        if (textQueue != null)
            if (line < textQueue.Length)
                if (textQueue[line] != null) {
                    if ((UnitaleUtil.IsOverworld || GlobalControls.isInFight) && ((UIController.instance && this == UIController.instance.textmgr) || gameObject.name == "TextManager OW"))
                        SetMugshot(textQueue[line].Mugshot);

                    if (!offsetSet)
                        SetOffset(0, 0);
                    if (GetType() != typeof(LuaTextManager))
                        ResetFont();
                    currentColor = defaultColor;
                    colorSet = false;
                    currentSkippable = true;
                    autoSkipThis = false;
                    autoSkip = false;
                    autoSkipAll = false;
                    instantCommand = false;
                    skipFromPlayer = false;
                    firstChar = false;

                    timePerLetter = singleFrameTiming;
                    letterTimer = 0.0f;
                    this.DestroyChars();
                    currentLine = line;
                    currentX = self.position.x + offset.x;
                    currentY = self.position.y + offset.y;
                    // allow Game Over fonts to enjoy the fixed text positioning, too!
                    if (GetType() != typeof(LuaTextManager) && this.gameObject.name != "TextParent" && this.gameObject.name != "ReviveText")
                        currentY -= Charset.LineSpacing;
                    /*if (GetType() == typeof(LuaTextManager))
                        print("currentY from ShowLine (" + textQueue[currentLine].Text + ") = " + self.position.y + " + " + offset.y + " - " + Charset.LineSpacing + " = " + currentY);*/
                    currentCharacter = 0;
                    currentReferenceCharacter = 0;
                    letterEffect = "none";
                    /*textEffect = null;
                    letterIntensity = 0;*/
                    // letterSpeed = 1;
                    instantActive = textQueue[line].ShowImmediate;
                    SpawnText(forceNoAutoLineBreak);
                    //if (!overworld)
                    //    UIController.instance.encounter.CallOnSelfOrChildren("AfterText");
                    if (UnitaleUtil.IsOverworld && textframe != null && this == PlayerOverworld.instance.textmgr) {
                        if (textQueue[line].ActualText) {
                            if (textframe.GetComponent<Image>().color.a == 0)
                                SetTextFrameAlpha(1);
                            blockSkip = false;
                        } else {
                            if ((textframe.GetComponent<Image>().color.a == 1))
                                SetTextFrameAlpha(0);
                            blockSkip = true;
                            this.DestroyChars();
                        }
                    }

                    // Move the text up a little if there are more than 3 lines so they can possibly fit in the arena
                    if (!GlobalControls.retroMode && !UnitaleUtil.IsOverworld && UIController.instance && this == UIController.instance.textmgr) {
                        int lines = (textQueue[line].Text.Split('\n').Length > 3 && (UIController.instance.state == UIController.UIState.ACTIONSELECT || UIController.instance.state == UIController.UIState.DIALOGRESULT)) ? 4 : 3;
                        Vector3 pos = self.localPosition;

                        // remove the offset
                        self.localPosition = new Vector3(pos.x, pos.y - (decoratedTextOffset ? 9 : 0), pos.z);
                        pos = self.localPosition;
                        decoratedTextOffset = false;

                        // add the offset if necessary
                        if (lines == 4) {
                            decoratedTextOffset = true;
                            self.localPosition = new Vector3(pos.x, pos.y + (decoratedTextOffset ? 9 : 0), pos.z);
                        }
                    } else if (gameObject.name == "TextManager OW") {
                        int lines = textQueue[line].Text.Split('\n').Length;
                        if (lines >= 4) lines = 4;
                        else            lines = 3;
                        Vector3 pos = gameObject.GetComponent<RectTransform>().localPosition;
                        gameObject.GetComponent<RectTransform>().localPosition = new Vector3(pos.x, 22 + ((lines - 1) * Charset.LineSpacing / 2), pos.z);
                    }
                }
    }

    [MoonSharpHidden] public void SetTextFrameAlpha(float a) {
        Image[] imagesChild = null;
        Image[] images = null;

        if (UnitaleUtil.IsOverworld) {
            imagesChild = textframe.GetComponentsInChildren<Image>();
            images = new Image[imagesChild.Length + 1];
            images[0] = textframe.GetComponent<Image>();
        } else {
            imagesChild = GameObject.Find("arena_border_outer").GetComponentsInChildren<Image>();
            images = new Image[imagesChild.Length + 1];
            images[0] = GameObject.Find("arena_border_outer").GetComponent<Image>();
        }

        imagesChild.CopyTo(images, 1);

        foreach (Image img in images)
            img.color = new Color(img.color.r, img.color.g, img.color.b, a);
    }

    [MoonSharpHidden] public bool HasNext() { return currentLine + 1 < LineCount(); }

    [MoonSharpHidden] public void NextLineText() { ShowLine(++currentLine); }

    [MoonSharpHidden] public void SkipText() {
        if (!noSkip1stFrame) {
            while (currentCharacter < letterReferences.Length) {
                if (letterReferences[currentCharacter] != null && Charset.Letters.ContainsKey(textQueue[currentLine].Text[currentCharacter])) {
                    letterReferences[currentCharacter].enabled = true;
                    currentReferenceCharacter++;
                }

                currentCharacter++;
            }
        }
    }

    [MoonSharpHidden] public void DoSkipFromPlayer() {
        skipFromPlayer = true;

        if ((GlobalControls.isInFight && LuaEnemyEncounter.script.GetVar("playerskipdocommand").Boolean) || !GlobalControls.isInFight)
            instantCommand = true;

        if (!GlobalControls.retroMode)
            InUpdateControlCommand(DynValue.NewString("instant"), currentCharacter);
        else
            SkipText();
    }

    public virtual void SkipLine() {
        if (!noSkip1stFrame)
            while (currentCharacter < letterReferences.Length) {
                if (letterReferences[currentCharacter] != null && Charset.Letters.ContainsKey(textQueue[currentLine].Text[currentCharacter])) {
                    letterReferences[currentCharacter].enabled = true;
                    switch (letterEffect.ToLower()) {
                        case "twitch": letterReferences[currentCharacter].GetComponent<Letter>().effect = new TwitchEffectLetter(letterReferences[currentCharacter].GetComponent<Letter>(), letterIntensity);   break;
                        case "rotate": letterReferences[currentCharacter].GetComponent<Letter>().effect = new RotatingEffectLetter(letterReferences[currentCharacter].GetComponent<Letter>(), letterIntensity); break;
                        case "shake":  letterReferences[currentCharacter].GetComponent<Letter>().effect = new ShakeEffectLetter(letterReferences[currentCharacter].GetComponent<Letter>(), letterIntensity);    break;
                        default:       letterReferences[currentCharacter].GetComponent<Letter>().effect = null;                                                                                                 break;
                    }
                    currentReferenceCharacter++;
                    if (textQueue[currentLine].Text[currentCharacter] == '\t') {
                        float indice = currentX / 320f;
                        if (currentX - (indice * 320f) < 36)
                            indice--;
                        currentX = indice * 320 + 356;
                    } else if (textQueue[currentLine].Text[currentCharacter] == '\n') {
                        currentX = self.position.x + offset.x;
                        /*if (GetType() == typeof(LuaTextManager))
                            print("currentY from \\n (" + textQueue[currentLine].Text + ") = " + currentY + " - " + vSpacing + " - " + Charset.LineSpacing + " = " + (currentY - vSpacing - Charset.LineSpacing));*/
                        currentY = currentY - vSpacing - Charset.LineSpacing;
                        currentCharacter++;
                        return;
                    }
                }
                currentCharacter++;
            }
    }

    public void SetEffect(TextEffect effect) { textEffect = effect; }

    [MoonSharpHidden] public void DestroyChars() {
        foreach (Transform child in gameObject.transform)
            Destroy(child.gameObject);
    }

    private void SpawnTextSpaceTest(int i, string currentText, out string currentText2) {
        currentText2 = currentText;
        bool decorated = textQueue[currentLine].Decorated;
        float decorationLength = decorated ? UnitaleUtil.CalcTextWidth(this, 0, 1, true, true) : 0;

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

        if (UnitaleUtil.CalcTextWidth(this, beginIndex, finalIndex, true) > _textMaxWidth && _textMaxWidth > 0) {
            // If the line's too long, do something!
            int wordBeginIndex = currentText2[i] == ' ' ? i + 1 : i;
            if (UnitaleUtil.CalcTextWidth(this, wordBeginIndex, finalIndex, false) > _textMaxWidth - decorationLength) {
                // Word is taking the entire line
                for (int currentIndex = wordBeginIndex; currentIndex <= finalIndex; currentIndex++) {
                    if (UnitaleUtil.CalcTextWidth(this, beginIndex, currentIndex, false) > _textMaxWidth) {
                        currentText2 = currentText2.Substring(0, currentIndex) + "\n" + (decorated ? "  " : "") + currentText2.Substring(currentIndex, currentText2.Length - currentIndex);
                        textQueue[currentLine].Text = currentText2;
                        finalIndex += decorated ? 3 : 1;
                        beginIndex = currentIndex;
                    }
                }
            } else
                // Line is too long
                currentText2 = currentText2.Substring(0, wordBeginIndex - 1) + "\n" + (decorated ? "  " : "") + currentText2.Substring(wordBeginIndex, currentText.Length - wordBeginIndex);

            Array.Resize(ref letterReferences, currentText2.Length);
            Array.Resize(ref letterPositions, currentText2.Length);
        }
        textQueue[currentLine].Text = currentText2;
    }

    private void CreateLetter(string currentText, int index, bool insert = false) {
        if (insert)
            for (int i = letterReferences.Length - 2; i >= index; i--) {
                letterPositions[i + 1] = letterPositions[i];
                letterReferences[i + 1] = letterReferences[i];
            }

        GameObject singleLtr = Instantiate(SpriteFontRegistry.LETTER_OBJECT);
        RectTransform ltrRect = singleLtr.GetComponent<RectTransform>();

        bool isLua = GetType() == typeof(LuaTextManager);
        LuaTextManager luaThis = isLua ? ((LuaTextManager) this) : null;

        ltrRect.localScale = new Vector3((isLua ? luaThis.xscale : 1f) + 0.001f, (isLua ? luaThis.yscale : 1f) + 0.001f, ltrRect.localScale.z);

        Image ltrImg = singleLtr.GetComponent<Image>();
        ltrRect.SetParent(gameObject.transform);
        ltrImg.sprite = Charset.Letters[currentText[index]];

        letterReferences[index] = ltrImg;

        MoveLetter(currentText, index, ltrRect);

        ltrImg.SetNativeSize();
        if (isLua) {
            Color luaColor = luaThis._color;
            if (!colorSet) {
                if (luaThis.hasColorBeenSet) ltrImg.color = luaColor;
                else                         ltrImg.color = currentColor;
                if (luaThis.hasAlphaBeenSet) ltrImg.color = new Color(ltrImg.color.r, ltrImg.color.g, ltrImg.color.b, luaColor.a);
            } else                           ltrImg.color = currentColor;
        } else                               ltrImg.color = currentColor;
        ltrImg.GetComponent<Letter>().colorFromText = currentColor;
        ltrImg.enabled = textQueue[currentLine].ShowImmediate || (GlobalControls.retroMode && instantActive);
    }

    private void MoveLetter(string currentText, int index, RectTransform ltrRect) {
        if (GetType() == typeof(LuaTextManager) || this.gameObject.name == "TextParent" || this.gameObject.name == "ReviveText") {
            // Allow Game Over fonts to enjoy the fixed text positioning, too!
            float diff = (Charset.Letters[currentText[index]].border.w - Charset.Letters[currentText[index]].border.y);
            ltrRect.localPosition = new Vector3(currentX - self.position.x - .9f, (currentY - self.position.y) + diff + .1f, 0);
        } else
            // Keep what we already have for all text boxes that are not Text Objects in an encounter
            ltrRect.position = new Vector3(currentX + .1f, (currentY + Charset.Letters[currentText[index]].border.w - Charset.Letters[currentText[index]].border.y + 2) + .1f, 0);

        letterPositions[index] = ltrRect.anchoredPosition;
    }

    private void SpawnText(bool forceNoAutoLineBreak = false) {
        noSkip1stFrame = true;
        string currentText = textQueue[currentLine].Text;
        letterReferences = new Image[currentText.Length];
        letterPositions = new Vector2[currentText.Length];
        if (currentText.Length > 1 && !forceNoAutoLineBreak)
            if (!GlobalControls.isInFight || LuaEnemyEncounter.script.GetVar("autolinebreak").Boolean || GetType() == typeof(LuaTextManager))
                SpawnTextSpaceTest(0, currentText, out currentText);

        // Work-around for [instant] and [instant:allowcommand] at the beginning of a line of text
        bool skipImmediate = false;
        string skipCommand  = "";

        for (int i = 0; i < currentText.Length; i++) {
            switch (currentText[i]) {
                case '[':
                    int currentChar = i;
                    string command = UnitaleUtil.ParseCommandInline(currentText, ref i);
                    if (command == null || lateStartWaiting)
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
                                    else {
                                        if (commands != null)
                                            commands.Add(precedingText.Substring(j + 1, (k - j) - 1));
                                        precedingText = precedingText.Replace(precedingText.Substring(j, (k - j) + 1), "");
                                    }
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
                case '\n':
                    currentX = self.position.x + offset.x;
                    currentY = currentY - vSpacing - Charset.LineSpacing;
                    break;
                case '\t':
                    currentX = !GlobalControls.isInFight ? (356 + Camera.main.transform.position.x - 320) : 356; // HACK: bad tab usage
                    break;
                case ' ':
                    if (i + 1 == currentText.Length || currentText[i + 1] == ' ' || forceNoAutoLineBreak)
                        break;
                    if (!GlobalControls.isInFight || LuaEnemyEncounter.script.GetVar("autolinebreak").Boolean || GetType() == typeof(LuaTextManager)) {
                        SpawnTextSpaceTest(i, currentText, out currentText);
                        if (currentText[i] != ' ') {
                            i--;
                            continue;
                        }
                    }
                    break;
            }

            if (!Charset.Letters.ContainsKey(currentText[i]))
                continue;

            CreateLetter(currentText, i);
            currentX += letterReferences[i].gameObject.GetComponent<RectTransform>().rect.width + hSpacing; // TODO remove hardcoded letter offset
        }

        // Work-around for [instant] and [instant:allowcommand] at the beginning of a line of text
        if (skipImmediate)
            InUpdateControlCommand(DynValue.NewString(skipCommand));

        if (UnitaleUtil.IsOverworld && SceneManager.GetActiveScene().name != "TitleScreen" && SceneManager.GetActiveScene().name != "EnterName" && !GlobalControls.isInShop)
            try {
                if (mugshot.alpha == 0)
                    mugshot.color = new float[] { 1, 1, 1 };
            } catch { }
        if (!instantActive)
            Update();
    }

    private bool CheckCommand() {
        if (currentLine >= textQueue.Length)
            return false;
        if (currentCharacter < textQueue[currentLine].Text.Length)
            if (textQueue[currentLine].Text[currentCharacter] == '[') {
                int currentChar = currentCharacter;
                string command = UnitaleUtil.ParseCommandInline(textQueue[currentLine].Text, ref currentCharacter);
                if (command != null) {
                    currentCharacter++; // we're not in a continuable loop so move to the character after the ] manually

                    //float lastLetterTimer = letterTimer; // kind of a dirty hack so we can at least release 0.2.0 sigh
                    //float lastTimePerLetter = timePerLetter; // i am so sorry

                    wasStated = false;

                    DynValue commandDV = DynValue.NewString(command);
                    InUpdateControlCommand(commandDV, currentCharacter);

                    //if (lastLetterTimer != letterTimer || lastTimePerLetter != timePerLetter)
                    //if (currentCharacter >= textQueue[currentLine].Text.Length)
                    //    return true;

                    return true;
                }
                currentCharacter = currentChar;
            }
        return false;
    }

    protected virtual void Update() {
        if (!UnitaleUtil.IsOverworld && nextMonsterDialogueOnce) {
            bool test = true;
            foreach (TextManager mgr in UIController.instance.monDialogues) {
                if (!mgr.IsFinished())
                    test = false;
            }
            if (test) {
                if (!wasStated)
                    UIController.instance.DoNextMonsterDialogue(true);
                wasStated = false;
                nextMonsterDialogueOnce = false;
            }
        } else if (mugshot != null && mugshotList != null)
            if (UnitaleUtil.IsOverworld&& mugshot.alpha != 0 && mugshotList.Length > 1) {
                if (!mugshot.animcomplete && (letterTimer < 0 || LineComplete())) {
                    mugshot.StopAnimation();
                    mugshot.Set(finalMugshot);
                } else if (mugshot.animcomplete && !(letterTimer < 0 || LineComplete()))
                    mugshot.SetAnimation(mugshotList, mugshotTimer);
            }

        if (textQueue == null || textQueue.Length == 0 || paused || lateStartWaiting)
            return;
        /*if (currentLine >= lineCount() && overworld) {
            endTextEvent();
            return;
        }*/

        if (textEffect != null)
            textEffect.UpdateEffects();

        if (GlobalControls.retroMode && instantActive || currentCharacter >= letterReferences.Length)
            return;

        if (waitingChar != KeyCode.None) {
            if (Input.GetKeyDown(waitingChar))
                waitingChar = KeyCode.None;
            else
                return;
        }

        /*
        letterTimer += Time.deltaTime;
        if ((letterTimer > timePerLetter || firstChar) && !LineComplete()) {
            firstChar = false;
            letterTimer = 0.0f;
            bool soundPlayed = false;
            int lastLetter = -1;
            if (HandleShowLettersOnce(ref soundPlayed, ref lastLetter))
                return;
            else
                for (int i = 0; (instantCommand || i < letterSpeed) && currentCharacter < letterReferences.Length; i++)
                    if (!HandleShowLetter(ref soundPlayed, ref lastLetter)) {
                        HandleShowLettersOnce(ref soundPlayed, ref lastLetter);
                        return;
                    }
        }
        */

        letterTimer += Time.deltaTime;
        if ((letterTimer >= timePerLetter || firstChar) && !LineComplete()) {
            int repeats = timePerLetter == 0f ? 1 : (int)Mathf.Floor(letterTimer / timePerLetter);

            bool soundPlayed = false;
            int lastLetter = -1;

            for (int i = 0; i < repeats; i++) {
                if (!HandleShowLetter(ref soundPlayed, ref lastLetter)) {
                    HandleShowLettersOnce(ref soundPlayed, ref lastLetter);
                    return;
                }

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

    private bool HandleShowLettersOnce(ref bool soundPlayed, ref int lastLetter) {
        bool wentIn = false;
        while (letterOnceValue != 0 && !instantCommand) {
            wentIn = true;
            if (!HandleShowLetter(ref soundPlayed, ref lastLetter))
                return false;
            letterOnceValue--;
        }
        return wentIn;
    }

    private bool HandleShowLetter(ref bool soundPlayed, ref int lastLetter) {
        if (lastLetter != currentCharacter && ((!GlobalControls.retroMode && (!instantActive || instantCommand)) || GlobalControls.retroMode)) {
            float oldLetterTimer = letterTimer;
            int oldLetterOnceValue = letterOnceValue;
            lastLetter = currentCharacter;
            while (CheckCommand())
                if ((GlobalControls.retroMode && instantActive) || letterTimer != oldLetterTimer || waitingChar != KeyCode.None || letterOnceValue != oldLetterOnceValue || this.paused)
                    return false;
            if (currentCharacter >= letterReferences.Length)
                return false;
        }
        if (letterReferences[currentCharacter] != null) {
            letterReferences[currentCharacter].enabled = true;
            switch (letterEffect.ToLower()) {
                case "twitch": letterReferences[currentCharacter].GetComponent<Letter>().effect = new TwitchEffectLetter(letterReferences[currentCharacter].GetComponent<Letter>(), letterIntensity);   break;
                case "rotate": letterReferences[currentCharacter].GetComponent<Letter>().effect = new RotatingEffectLetter(letterReferences[currentCharacter].GetComponent<Letter>(), letterIntensity); break;
                case "shake":  letterReferences[currentCharacter].GetComponent<Letter>().effect = new ShakeEffectLetter(letterReferences[currentCharacter].GetComponent<Letter>(), letterIntensity);    break;
                default:       letterReferences[currentCharacter].GetComponent<Letter>().effect = null;                                                                                                 break;
            }
            if (letterSound != null && !muted && !soundPlayed && (GlobalControls.retroMode || textQueue[currentLine].Text[currentCharacter] != ' ')) {
                soundPlayed = true;
                if (letterSound.isPlaying) UnitaleUtil.PlaySound("BubbleSound", letterSound.clip.name);
                else                       letterSound.Play();
            }
        }
        currentReferenceCharacter++;
        currentCharacter++;
        return true;
    }

    private void PreCreateControlCommand(string command) {
        string[] cmds = UnitaleUtil.SpecialSplit(':', command);
        string[] args = new string[0];
        if (cmds.Length == 2) {
            args = UnitaleUtil.SpecialSplit(',', cmds[1], true);
            cmds[1] = args[0];
        }
        switch (cmds[0].ToLower()) {
            case "color":
                float oldAlpha = currentColor.a;
                currentColor = ParseUtil.GetColor(cmds[1]);
                currentColor = new Color(currentColor.r, currentColor.g, currentColor.b, oldAlpha);
                colorSet = true;
                break;
            case "alpha":
                if (cmds[1].Length == 2)
                    currentColor = new Color(currentColor.r, currentColor.g, currentColor.b, ParseUtil.GetByte(cmds[1]) / 255);
                break;
            case "charspacing":
                if (cmds.Length > 1 && cmds[1].ToLower() == "default") SetHorizontalSpacing(Charset.CharSpacing);
                else                                                   SetHorizontalSpacing(ParseUtil.GetFloat(cmds[1]));
                break;
            case "linespacing":
                if (cmds.Length > 1)
                    SetVerticalSpacing(ParseUtil.GetFloat(cmds[1]));
                break;

            case "starcolor":
                Color starColor = ParseUtil.GetColor(cmds[1]);
                int indexOfStar = textQueue[currentLine].Text.IndexOf('*'); // HACK oh my god lol
                if (indexOfStar > -1)
                    letterReferences[indexOfStar].color = starColor;
                break;

            case "instant":
                if (GlobalControls.retroMode)
                    instantActive = true;
                else
                    InUpdateControlCommand(DynValue.NewString(command));
                break;

            case "noskip":
                if (args.Length == 0)      currentSkippable = false;
                break;

            case "font":
                UnderFont uf = SpriteFontRegistry.Get(cmds[1]);
                if (uf == null)
                    UnitaleUtil.DisplayLuaError("[font:x] usage", "The font \"" + cmds[1] + "\" doesn't exist.\nYou should check if you made a typo, or if the font really is in your mod.");
                SetFont(uf);
                if (GetType() == typeof(LuaTextManager))
                    ((LuaTextManager) this).UpdateBubble();
                break;

            case "effect":
                switch (cmds[1].ToUpper()) {
                    case "NONE":
                        textEffect = null;
                        break;

                    case "TWITCH":
                        if (args.Length > 1)  textEffect = new TwitchEffect(this, ParseUtil.GetFloat(args[1]));
                        else                  textEffect = new TwitchEffect(this);
                        break;

                    case "SHAKE":
                        if (args.Length > 1)  textEffect = new ShakeEffect(this, ParseUtil.GetFloat(args[1]));
                        else                  textEffect = new ShakeEffect(this);
                        break;

                    case "ROTATE":
                        if (args.Length > 1)  textEffect = new RotatingEffect(this, ParseUtil.GetFloat(args[1]));
                        else                  textEffect = new RotatingEffect(this);
                        break;
                }
                break;
        }
    }

    private void InUpdateControlCommand(DynValue command, int index = 0) {
        string[] cmds = UnitaleUtil.SpecialSplit(':', command.String);
        string[] args = new string[0];
        if (cmds.Length >= 2) {
            if (cmds.Length == 3) {
                if (cmds[2] == "skipover" && instantCommand) return;
                else if (cmds[2] == "skiponly" && !instantCommand) return;
            } else if (cmds[1] == "skipover" && instantCommand) return;
            else if (cmds[1] == "skiponly" && !instantCommand) return;
            args = UnitaleUtil.SpecialSplit(',', cmds[1], true);
            cmds[1] = args[0];
        }
        //print("Frame " + GlobalControls.frame + ": Command " + cmds[0].ToLower() + " found for " + gameObject.name);
        switch (cmds[0].ToLower()) {
            case "noskip":
                if (args.Length == 0)      currentSkippable = false;
                else if (args[0] == "off") currentSkippable = true;
                break;

            case "waitfor":
                try { waitingChar = (KeyCode)Enum.Parse(typeof(KeyCode), cmds[1]); }
                catch { throw new CYFException("The key \"" + cmds[1] + "\" isn't a valid key."); }
                break;

            case "w":
                letterTimer = timePerLetter - (singleFrameTiming * ParseUtil.GetInt(cmds[1]));
                break;

            case "waitall":     timePerLetter = singleFrameTiming * ParseUtil.GetInt(cmds[1]); break;
            case "novoice":     letterSound.clip = null;                                       break;
            case "next":        autoSkipAll = true;                                            break;
            case "finished":    autoSkipThis = true;                                           break;
            case "nextthisnow": autoSkip = true;                                               break;
            case "noskipatall": blockSkip = true;                                              break;
            //case "speed":       letterSpeed = Int32.Parse(args[0]);                            break;
            case "speed":
                //you can only set text speed to a number >= 0
                float newSpeedValue = float.Parse(args[0]);
                // protect against divide-by-zero errors
                if (newSpeedValue > 0f)
                    timePerLetter = singleFrameTiming / newSpeedValue;
                else if (newSpeedValue == 0f)
                    timePerLetter = 0f;
                break;

            case "letters":
                letterOnceValue = Int32.Parse(args[0]);
                break;

            case "voice":
                if (cmds[1].ToLower() == "default")  letterSound.clip = SpriteFontRegistry.Get(SpriteFontRegistry.UI_DEFAULT_NAME).Sound;
                else                                 letterSound.clip = AudioClipRegistry.GetVoice(cmds[1].ToLower());
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
                        if ((currentText.Substring(i)).Length >= 13 && pos - i >= 13 && currentText.Substring(i, 13) == "[instant:stop") {
                            pos = i - 1;
                            break;
                        }
                    } else {
                        if ((currentText.Substring(i)).Length >= 16 && pos - i >= 16 && currentText.Substring(i, 16) == "[instant:stopall") {
                            pos = i - 1;
                            break;
                        }
                    }
                }

                // Third: Show all letters (and execute all commands, if applicable) between `index` and `pos`
                bool soundPlayed = true;
                int lastLetter = -1;
                int destination = System.Math.Min(pos, letterReferences.Length);
                while (currentCharacter < destination)
                    HandleShowLetter(ref soundPlayed, ref lastLetter);

                // This is a catch-all.
                // If a line of text starts with [instant], the above code will not display the letters it passes over,
                // due to how HandleShowLetter is coded.
                for (int i = index; i < pos; i++) {
                    if (letterReferences[i] != null)
                        letterReferences[i].enabled = true;
                }

                // Fourth:  Update variables
                if (pos < currentText.Length) {
                    instantActive  = false;
                    instantCommand = false;
                    letterTimer = timePerLetter;
                }

                skipFromPlayer = false;

                currentCharacter = pos;
                currentReferenceCharacter = pos;
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
                        caller.Call(args[0], argsbis, true); //ADD TRY
                                                             //caller.Call(args[0], DynValue.NewString(args[1]));
                    } else
                        caller.Call(cmds[1], null, true);
                    if (cmds[1] == "State")
                        wasStated = true;
                } catch (InterpreterException ex) { UnitaleUtil.DisplayLuaError(caller.scriptname, UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message); }
                break;

            case "mugshot":
                DynValue temp;
                if (args[0][0] == '{') temp = UnitaleUtil.RebuildTableFromString(args[0]);
                else                   temp = DynValue.NewString(args[0]);

                SetMugshot(temp);
                break;

            case "name":
                string text = textQueue[currentLine].Text;
                string textEnd = text.Substring(0, currentCharacter - 1) + PlayerCharacter.instance.Name + text.Substring(currentCharacter, text.Length - 1);
                textQueue[currentLine].SetText(textEnd);
                break;

            case "music":
                if (args[0] == "play")                                                                 Camera.main.GetComponent<AudioSource>().Play();
                else if (args[0] == "pause")                                                           Camera.main.GetComponent<AudioSource>().Pause();
                else if (args[0] == "unpause")                                                         Camera.main.GetComponent<AudioSource>().UnPause();
                else if (args[0] == "stop" || args[0] == "null" || args[0] == "" || args[0] == "nil")  Camera.main.GetComponent<AudioSource>().Stop();
                else {                                                                                 Camera.main.GetComponent<AudioSource>().clip = AudioClipRegistry.GetSound(args[0]);
                                                                                                       Camera.main.GetComponent<AudioSource>().Play();
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
                if (ParseUtil.TestInt(args[0]))
                    tryHP = ParseUtil.GetInt(args[0]);

                if ((args[0].Contains("-") && args[0] != "Max-1") || args[0] == "kill") PlayerController.PlaySound(AudioClipRegistry.GetSound("hurtsound"));
                else if (args.Length > 1) {
                    if (args[1] == "set" && tryHP < HP)                                 PlayerController.PlaySound(AudioClipRegistry.GetSound("hurtsound"));
                    else                                                                PlayerController.PlaySound(AudioClipRegistry.GetSound("healsound"));
                } else                                                                  PlayerController.PlaySound(AudioClipRegistry.GetSound("healsound"));

                if (args[0] == "kill")                   SetHP(0);
                else if (args[0] == "Max-1")             SetHP(MaxHP - 1);
                else if (args[0] == "Max")               SetHP(MaxHP);
                else if (args.Length > 1 && !killable) {
                    if (args[1] == "set")
                        if (tryHP < 1 && !killable)      SetHP(1);
                        else                             SetHP(tryHP);
                } else if (HP + tryHP <= 0 && !killable) SetHP(1);
                else                                     SetHP(HP + tryHP);
                break;

            case "lettereffect":
                letterEffect = args[0];
                if (args.Length == 2)
                    letterIntensity = ParseUtil.GetFloat(args[1]);
                break;
        }
    }

    private DynValue ComputeArgument(string arg) {
        arg = arg.Trim();
        Type type = UnitaleUtil.CheckRealType(arg);
        DynValue dyn;
        //Boolean
        if (type == typeof(bool)) {
            if (arg.Replace(" ", "") == "true")
                dyn = DynValue.NewBoolean(true);
            else
                dyn = DynValue.NewBoolean(false);
        //Number
        } else if (type == typeof(float)) {
            arg = arg.Replace(" ", "");
            float number = CreateNumber(arg);
            dyn = DynValue.NewNumber(number);
        //String
        } else
            dyn = DynValue.NewString(arg);
        return dyn;
    }

    private void SetHP(float newhp) {
        float HP = 0;
        newhp = Mathf.Round(newhp * Mathf.Pow(10, ControlPanel.instance.MaxDigitsAfterComma)) / Mathf.Pow(10, ControlPanel.instance.MaxDigitsAfterComma);
        if (newhp <= 0) {
            GameOverBehavior gob = GameObject.FindObjectOfType<GameOverBehavior>();
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
            rt.position = new Vector3(rt.position.x, rt.position.y, -1000);
            gob.StartDeath();
            return;
        }
        //HP greater than Max, heal, already more HP than Max
        else if (newhp > PlayerCharacter.instance.MaxHP && newhp > PlayerCharacter.instance.HP && PlayerCharacter.instance.HP > PlayerCharacter.instance.MaxHP)
                                                                                                HP = PlayerCharacter.instance.HP;
        //HP greater than Max, heal
        else if (newhp > PlayerCharacter.instance.MaxHP && newhp > PlayerCharacter.instance.HP) HP = PlayerCharacter.instance.MaxHP;
        else                                                                                    HP = newhp;
        if (HP > ControlPanel.instance.HPLimit)                                                 HP = ControlPanel.instance.HPLimit;
        PlayerCharacter.instance.HP = HP;
        if (!UnitaleUtil.IsOverworld)
            UIStats.instance.setHP(HP);
    }

    private float CreateNumber(string str) {
        float number = 0, dot = -1;
        int index = 0;
        bool negative = false;
        foreach (char c in str) {
            if (c == '-') {
                negative = true;
                continue;
            } else if (c == ' ') {
                if (dot != -1)
                    dot++;
                continue;
            } else if (c == '.') {
                dot = index;
                index++;
                continue;
            }

            if (dot == -1)
                number = number * 10 + c - 48;
            else
                number += ((float)c - 48) / Mathf.Pow(10, - (dot - index));
            index++;
        }
        if (negative)
            return -number;
        return number;
    }
}