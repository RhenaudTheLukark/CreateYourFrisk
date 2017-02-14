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

    private UnderFont default_charset = null;
    public AudioSource letterSound = null;
    protected TextEffect textEffect;
    public List<Letter> letters = new List<Letter>();
    private string letterEffect = "none";
    private float letterIntensity = 0.0f;
    public int currentLine = 0;
    private int currentCharacter = 0;
    public int currentReferenceCharacter = 0;
    private bool displayImmediate = false;
    private bool currentSkippable = true;
    private RectTransform self;
    public Vector2 offset;
    private bool offsetSet = false;
    private float currentX;
    private float currentY;
    private bool paused = false;
    private bool muted = false;
    private bool autoSkipThis = false;
    private bool autoSkipAll = false;
    private bool autoSkip = false;
    internal float hSpacing = 3;
    internal float vSpacing = 0;
    private Image mugshot;
    private int letterSpeed = 1;
    private int letterOnceValue = 0;
    private KeyCode waitingChar = KeyCode.None;

    private Color currentColor = Color.white;
    //private Color defaultColor = Color.white;

    private float letterTimer = 0.0f;
    private float timePerLetter;
    private float singleFrameTiming = 1.0f / 30;

    private ScriptWrapper caller;

    public UnderFont Charset { get; private set; }
    public TextMessage[] textQueue = null;
    //public string[] mugshotsPath;
    public bool overworld;
    public bool blockSkip = false;
    public bool hidden = false;
    public bool skipNowIfBlocked = false;
    internal bool noSkip1stFrame = true;

    public void setCaller(ScriptWrapper s) { caller = s; }

    public void setFont(UnderFont font, bool firstTime = false) {
        Charset = font;
        if (default_charset == null)
            default_charset = font;
        if (firstTime) {
            if (letterSound == null)          letterSound.clip = Charset.Sound;
            if (currentColor == Color.white)  currentColor = Charset.DefaultColor;
            if (hSpacing == 3)                hSpacing = Charset.CharSpacing;
        } else {
            letterSound.clip = Charset.Sound;
            currentColor = Charset.DefaultColor;
            hSpacing = Charset.CharSpacing;
        }
    }

    public void setHorizontalSpacing(float spacing = 3) { this.hSpacing = spacing; }

    public void setVerticalSpacing(float spacing = 0) { this.vSpacing = spacing; }

    public void resetFont() {
        if (Charset == null || default_charset == null)
            setFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_DEFAULT_NAME), true);

        Charset = default_charset;
        letterSound.clip = default_charset.Sound;
    }

    protected virtual void Awake() {
        self = gameObject.GetComponent<RectTransform>();
        letterSound = gameObject.AddComponent<AudioSource>();
        letterSound.playOnAwake = false;
        // setFont(SpriteFontRegistry.F_UI_DIALOGFONT);
        timePerLetter = singleFrameTiming;
        if (GlobalControls.nonOWScenes.Contains(SceneManager.GetActiveScene().name) && SceneManager.GetActiveScene().name != "TransitionOverworld")
            overworld = false;
        else
            overworld = true;

        if (overworld && GameObject.Find("textframe_border_outer"))
            mugshot = GameObject.Find("Mugshot").GetComponent<Image>();
    }

    private void Start() {
        // setText("the quick brown fox jumps over\rthe lazy dog.\nTHE QUICK BROWN FOX JUMPS OVER\rTHE LAZY DOG.\nJerry.", true, true);
        // setText(new TextMessage("Here comes Napstablook.", true, false));
        // setText(new TextMessage(new string[] { "Check", "Compliment", "Ignore", "Steal", "trow temy", "Jerry" }, false));
    }

    public void setPause(bool pause) { this.paused = pause; }

    public bool isPaused() { return this.paused; }

    public bool isFinished() { return currentCharacter >= letterReferences.Length; }

    public void setMute(bool muted) { this.muted = muted; }

    public void setText(TextMessage text) { setTextQueue(new TextMessage[] { text }); }

    public void setTextQueue(TextMessage[] textQueue) {
        resetFont();
        this.textQueue = textQueue;
        currentLine = 0;
        showLine(0);
    }

    public void setTextQueueAfterValue(int BeginText) {
        resetFont();
        currentLine = BeginText;
        showLine(BeginText);
    }

    public void ResetCurrentCharacter() {
        currentCharacter = 0;
        currentReferenceCharacter = 0;
    }

    public void addToTextQueue(TextMessage text) { addToTextQueue(new TextMessage[] { text }); }

    public void addToTextQueue(TextMessage[] textQueueToAdd) {
        if (allLinesComplete())
            setTextQueue(textQueueToAdd);
        else {
            int length = textQueue.Length + textQueueToAdd.Length;
            TextMessage[] newTextQueue = new TextMessage[length];
            textQueue.CopyTo(newTextQueue, 0);
            textQueueToAdd.CopyTo(newTextQueue, textQueue.Length);
            textQueue = newTextQueue;
        }
    }

    public bool canSkip() { return currentSkippable; }

    public bool canAutoSkip() { return autoSkip; }
    public bool canAutoSkipThis() { return autoSkipThis; }
    public bool canAutoSkipAll() { return autoSkipAll; }

    public int lineCount() {
        if (textQueue == null)
            return 0;
        return textQueue.Length;
    }

    public void setOffset(float xOff, float yOff) {
        offset = new Vector2(xOff, yOff);
        offsetSet = true;
    }

    public bool lineComplete() {
        if (letterReferences == null)
            return false;
        return displayImmediate || currentCharacter == letterReferences.Length;
    }

    public bool allLinesComplete() {
        if (textQueue == null)  return true;
        else                    return currentLine == textQueue.Length - 1 && lineComplete();
    }

    protected void showLine(int line) {
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
                    if (overworld && GameObject.Find("textframe_border_outer")) {
                        if (textQueue[line].Mugshot != string.Empty && textQueue[line].Mugshot != null) {
                            mugshot.sprite = SpriteRegistry.GetMugshot(textQueue[line].Mugshot);
                            mugshot.color = new Color(mugshot.color.r, mugshot.color.g, mugshot.color.b, 1);
                            self.localPosition = new Vector3(-150, self.localPosition.y, self.localPosition.z);
                        } else {
                            mugshot.sprite = null;
                            mugshot.color = new Color(mugshot.color.r, mugshot.color.g, mugshot.color.b, 0);
                            if (gameObject.name == "TextManager OW")
                                self.localPosition = new Vector3(-267, self.localPosition.y, self.localPosition.z);
                        }
                
                        if (textQueue[line].ActualText) {
                            if (GameObject.Find("textframe_border_outer").GetComponent<Image>().color.a == 0)
                                setTextFrameAlpha(1);
                            blockSkip = false;
                        } else {
                            if ((GameObject.Find("textframe_border_outer").GetComponent<Image>().color.a == 1))
                                setTextFrameAlpha(0);
                            blockSkip = true;
                        }
                    }

                    if (!offsetSet)
                        setOffset(0, 0);
                    currentColor = Charset.DefaultColor;
                    currentSkippable = true;
                    autoSkipThis = false;
                    autoSkip = false;
                    autoSkipAll = false;
                    letterSound.clip = Charset.Sound;
                    timePerLetter = singleFrameTiming;
                    letterTimer = 0.0f;
                    destroyText();
                    currentLine = line;
                    currentX = self.position.x + offset.x;
                    currentY = self.position.y + offset.y - Charset.LineSpacing;
                    currentCharacter = 0;
                    currentReferenceCharacter = 0;
                    letterEffect = "none";
                    letterIntensity = 0;
                    letterSpeed = 1;
                    displayImmediate = textQueue[line].ShowImmediate;
                    spawnText();
                    //if (!overworld)
                    //    UIController.instance.encounter.CallOnSelfOrChildren("AfterText");
                    if (overworld && GameObject.Find("textframe_border_outer")) {
                        if (textQueue[line].ActualText) {
                            if (GameObject.Find("textframe_border_outer").GetComponent<Image>().color.a == 0)
                                setTextFrameAlpha(1);
                        } else {
                            if ((GameObject.Find("textframe_border_outer").GetComponent<Image>().color.a == 1))
                                setTextFrameAlpha(0);
                            destroyText();
                        }
                        int lines = textQueue[line].Text.Split('\n').Length;
                        if (lines > 4)
                            lines = 4;
                        Vector3 pos = GameObject.Find("TextManager OW").GetComponent<RectTransform>().localPosition;
                        GameObject.Find("TextManager OW").GetComponent<RectTransform>().localPosition = new Vector3(pos.x, 22 + ((lines - 1) * Charset.LineSpacing / 2), pos.z);
                    }
                }
    }

    public void setTextFrameAlpha(float a) {
        Image[] imagesChild = null;
        Image[] images = null;

        if (overworld) {
            imagesChild = GameObject.Find("textframe_border_outer").GetComponentsInChildren<Image>();
            images = new Image[imagesChild.Length + 1];
            images[0] = GameObject.Find("textframe_border_outer").GetComponent<Image>();
        } else {
            imagesChild = GameObject.Find("arena_border_outer").GetComponentsInChildren<Image>();
            images = new Image[imagesChild.Length + 1];
            images[0] = GameObject.Find("arena_border_outer").GetComponent<Image>();
        }

        imagesChild.CopyTo(images, 1);

        foreach (Image img in images)
            img.color = new Color(img.color.r, img.color.g, img.color.b, a);
    }

    public bool hasNext() { return currentLine + 1 < lineCount(); }

    public void nextLine() { showLine(++currentLine); }

    public void skipText() {
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

    public void skipLine() {
        if (!noSkip1stFrame)
            while (currentCharacter < letterReferences.Length) {
                if (letterReferences[currentCharacter] != null && Charset.Letters.ContainsKey(textQueue[currentLine].Text[currentCharacter])) {
                    letterReferences[currentCharacter].enabled = true;
                    currentReferenceCharacter++;
                    if (textQueue[currentLine].Text[currentCharacter] == '\t') {
                        float indice = currentX / 320f;
                        if (currentX - (indice * 320f) < 36)
                            indice--;
                        currentX = indice * 320 + 356;
                    } else if (textQueue[currentLine].Text[currentCharacter] == '\n') {
                        currentX = self.position.x + offset.x;
                        currentY = currentY - vSpacing - Charset.LineSpacing;
                        currentCharacter++;
                        return;
                    }
                }
                currentCharacter++;
            }
    }

    public int CharacterCount(string str) {
        int count = 0;
        bool noCountable = false;
        foreach (char ch in str) {
            if (ch == '[') noCountable = true;
            if (!noCountable) count++;
            if (ch == ']') noCountable = false;
        }
        return count;
    }

    public void setEffect(TextEffect effect) { textEffect = effect; }

    public void destroyText() {
        foreach (Transform child in gameObject.transform)
            Destroy(child.gameObject);
    }

    private void spawnTextSpaceTest(int i, string currentText, out string currentText2) {
        int finalIndex = i + 1, beginIndex = i;

        for (; beginIndex > 0; beginIndex--)
            if (currentText[beginIndex] == '\n' || currentText[beginIndex] == '\r')
                break;
        for (; finalIndex < currentText.Length - 1; finalIndex++)
            if (currentText[finalIndex] == ' ' || currentText[finalIndex] == '\n' || currentText[finalIndex] == '\r')
                break;

        if (currentText[beginIndex] == '\n' || currentText[beginIndex] == ' ' || currentText[beginIndex] == '\r' ) beginIndex++;
        if (currentText[finalIndex] == '\n' || currentText[finalIndex] == ' ' || currentText[finalIndex] == '\r')  finalIndex--;
        
        int limit = 0;
        if (SceneManager.GetActiveScene().name == "Intro")          limit = 400;
        else if (overworld && mugshot != null) {
            if (mugshot.sprite != null)                             limit = 417;
            else                                                    limit = 534;
        } else if (SceneManager.GetActiveScene().name == "Battle") {
            if (UIController.instance.inited) {
                if (UIController.instance.encounter.gameOverStance) limit = 320;
                else if (name == "DialogBubble(Clone)")             limit = (int)transform.parent.GetComponent<LuaEnemyController>().bubbleWideness;
                else if (GetType() == typeof(LuaTextManager))       limit = gameObject.GetComponent<LuaTextManager>().textWidth;
                else                                                limit = 534;
            } else                                                  limit = 534;
        } else                                                      limit = 534;
        if (UnitaleUtil.calcTotalLength(this, beginIndex, finalIndex) > limit && limit > 0) {
            int realBeginIndex = beginIndex, realFinalIndex = finalIndex;
            beginIndex = finalIndex - 1;
            while (textQueue[currentLine].Text[beginIndex] != ' ' && textQueue[currentLine].Text[beginIndex] != '\n' && textQueue[currentLine].Text[beginIndex] != '\r' && beginIndex > 0)
                beginIndex--;
            if (textQueue[currentLine].Text[beginIndex] == ' ' || textQueue[currentLine].Text[beginIndex] == '\n' || textQueue[currentLine].Text[beginIndex] == '\r' || beginIndex < 0)
                beginIndex++;
            if (UnitaleUtil.calcTotalLength(this, beginIndex, finalIndex) > limit) {
                finalIndex = beginIndex;
                int testFinal = finalIndex;
                beginIndex = realBeginIndex;
                string currentText3 = currentText;
                for (; finalIndex <= realFinalIndex && finalIndex < currentText3.Length; finalIndex++)
                    if (UnitaleUtil.calcTotalLength(this, beginIndex, finalIndex) > limit) {
                        if (finalIndex == testFinal) {
                            currentX = self.position.x + offset.x;
                            currentY = currentY - vSpacing - Charset.LineSpacing;
                            if (!overworld) {
                                if (SceneManager.GetActiveScene().name == "Intro")
                                    currentText3 = currentText3.Substring(0, finalIndex - 1) + "\n" + currentText3.Substring(finalIndex, currentText.Length - finalIndex);
                                else if (name == "DialogBubble(Clone)" || UIController.instance.encounter.gameOverStance || GetType() == typeof(LuaTextManager))
                                    currentText3 = currentText3.Substring(0, finalIndex - 1) + "\n" + currentText3.Substring(finalIndex, currentText.Length - finalIndex);
                                else
                                    currentText3 = currentText3.Substring(0, finalIndex - 1) + "\n  " + currentText3.Substring(finalIndex, currentText.Length - finalIndex);
                            } else {
                                currentText3 = currentText3.Substring(0, finalIndex - 1) + "\n  " + currentText3.Substring(finalIndex, currentText.Length - finalIndex);
                                realFinalIndex += 2;
                            }
                        } else {
                            if (!overworld) {
                                if (SceneManager.GetActiveScene().name == "Intro") {
                                    currentText3 = currentText3.Substring(0, finalIndex - 1) + "\n" + currentText3.Substring(finalIndex - 1, currentText.Length - finalIndex + 1);
                                    realFinalIndex++;
                                } else if (name == "DialogBubble(Clone)" || UIController.instance.encounter.gameOverStance || GetType() == typeof(LuaTextManager)) {
                                    currentText3 = currentText3.Substring(0, finalIndex - 1) + "\n" + currentText3.Substring(finalIndex - 1, currentText.Length - finalIndex + 1);
                                    realFinalIndex++;
                                } else {
                                    currentText3 = currentText3.Substring(0, finalIndex - 1) + "\n  " + currentText3.Substring(finalIndex - 1, currentText.Length - finalIndex + 1);
                                    realFinalIndex += 3;
                                }
                            } else {
                                currentText3 = currentText3.Substring(0, finalIndex - 1) + "\n  " + currentText3.Substring(finalIndex - 1, currentText.Length - finalIndex + 1);
                                realFinalIndex += 3;
                            }
                        }
                        Array.Resize(ref letterReferences, currentText3.Length);
                        Array.Resize(ref letterPositions, currentText3.Length);
                        beginIndex = finalIndex;
                    }
                currentText2 = currentText3;
                return;
            }
            beginIndex--;
            if (!overworld) {
                if (SceneManager.GetActiveScene().name == "Intro")
                    currentText2 = currentText.Substring(0, beginIndex) + "\n" + currentText.Substring(beginIndex + 1, currentText.Length - beginIndex - 1);
                else if (name == "DialogBubble(Clone)" || UIController.instance.encounter.gameOverStance || GetType() == typeof(LuaTextManager))
                    currentText2 = currentText.Substring(0, beginIndex) + "\n" + currentText.Substring(beginIndex + 1, currentText.Length - beginIndex - 1);
                else {
                    currentText2 = currentText.Substring(0, beginIndex) + "\n  " + currentText.Substring(beginIndex + 1, currentText.Length - beginIndex - 1);
                    Array.Resize(ref letterReferences, currentText2.Length);
                    Array.Resize(ref letterPositions, currentText2.Length);
                }
            } else {
                currentText2 = currentText.Substring(0, beginIndex) + "\n  " + currentText.Substring(beginIndex + 1, currentText.Length - beginIndex - 1);
                Array.Resize(ref letterReferences, currentText2.Length);
                Array.Resize(ref letterPositions, currentText2.Length);
            }
            currentX = self.position.x + offset.x;
            currentY = currentY - vSpacing - Charset.LineSpacing;
        } else
            currentText2 = currentText;
    }

    private void spawnText() {
        letters.Clear();
        noSkip1stFrame = true;
        string currentText = textQueue[currentLine].Text;
        letterReferences = new Image[currentText.Length];
        letterPositions = new Vector2[currentText.Length];
        if (currentText.Length > 1)
            if (currentText[1] != ' ')
                if (SceneManager.GetActiveScene().name != "Battle") {
                    string currentText2;
                    spawnTextSpaceTest(0, currentText, out currentText2);
                    if (currentText != currentText2)
                        textQueue[currentLine].Text = currentText = currentText2;
                } else if (LuaEnemyEncounter.script.GetVar("autolinebreak").Boolean || GetType() == typeof(LuaTextManager)) {
                    string currentText2;
                    spawnTextSpaceTest(0, currentText, out currentText2);
                    if (currentText != currentText2)
                        textQueue[currentLine].Text = currentText = currentText2;
                }
        for (int i = 0; i < currentText.Length; i++) {
            switch (currentText[i]) {
                case '[':
                    int currentChar = i;
                    string command = parseCommandInline(currentText, ref i);
                    if (command != null) {
                        preCreateControlCommand(command);
                        continue;
                    } else
                        i = currentChar;
                    break;
                case '\n':
                    currentX = self.position.x + offset.x;
                    currentY = currentY - vSpacing - Charset.LineSpacing;
                    break;
                case '\t':
                    currentX = 356; // HACK: bad tab usage
                    break;
                case ' ':
                    if (i + 1 == currentText.Length)
                        break;
                    if (currentText[i + 1] == ' ' )
                        break;
                    if (SceneManager.GetActiveScene().name != "Battle") {
                        string currentText2;
                        spawnTextSpaceTest(i, currentText, out currentText2);
                        if (currentText != currentText2)
                            textQueue[currentLine].Text = currentText = currentText2;
                    } else if (LuaEnemyEncounter.script.GetVar("autolinebreak").Boolean || GetType() == typeof(LuaTextManager)) {
                        string currentText2;
                        spawnTextSpaceTest(i, currentText, out currentText2);
                        if (currentText != currentText2)
                            textQueue[currentLine].Text = currentText = currentText2;
                    }
                    break;
            }

            if (!Charset.Letters.ContainsKey(currentText[i]))
                continue;

            GameObject singleLtr = Instantiate(SpriteFontRegistry.LETTER_OBJECT);
            RectTransform ltrRect = singleLtr.GetComponent<RectTransform>();
            ltrRect.localScale = new Vector3(1.001f, 1.001f, ltrRect.localScale.z);

            Image ltrImg = singleLtr.GetComponent<Image>();
            ltrRect.SetParent(gameObject.transform);
            ltrImg.sprite = Charset.Letters[currentText[i]];

            letterReferences[i] = ltrImg;
            
            ltrRect.position = new Vector3(currentX + .1f, (currentY + Charset.Letters[currentText[i]].border.w - Charset.Letters[currentText[i]].border.y + 2) + .1f, 0);

            letterPositions[i] = ltrRect.anchoredPosition;
            ltrImg.SetNativeSize();
            ltrImg.color = currentColor;
            ltrImg.enabled = displayImmediate;
            letters.Add(singleLtr.GetComponent<Letter>());

            currentX += ltrRect.rect.width + hSpacing; // TODO remove hardcoded letter offset
        }
        if (overworld && SceneManager.GetActiveScene().name != "TitleScreen" && SceneManager.GetActiveScene().name != "EnterName")
            if (mugshot.sprite == null)
                mugshot.color = new Color(mugshot.color.r, mugshot.color.g, mugshot.color.b, 0);
    }

    private bool CheckCommand() {
        if (currentLine >= textQueue.Length)
            return false;
        if (textQueue[currentLine].Text[currentCharacter] == '[') {
            int currentChar = currentCharacter;
            string command = parseCommandInline(textQueue[currentLine].Text, ref currentCharacter);
            if (command != null) {
                currentCharacter++; // we're not in a continuable loop so move to the character after the ] manually

                //float lastLetterTimer = letterTimer; // kind of a dirty hack so we can at least release 0.2.0 sigh
                //float lastTimePerLetter = timePerLetter; // i am so sorry
                DynValue commandDV = DynValue.NewString(command);
                inUpdateControlCommand(commandDV);
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
        if (textQueue == null || textQueue.Length == 0)           
            return;
        if (paused)
            return;
        /*if (currentLine >= lineCount() && overworld) {
            endTextEvent();
            return;
        }*/

        if (textEffect != null)
            textEffect.updateEffects();        

        if (displayImmediate)
            return;
        if (currentCharacter >= letterReferences.Length)
            return;

        if (waitingChar != KeyCode.None) {
            if (Input.GetKeyDown(waitingChar)) {
                //Debug.Log("The key " + Enum.GetName(typeof(KeyCode), waitingChar) + " has been pressed correctly, as waited.");
                waitingChar = KeyCode.None;
            } else
                return;
        }

        letterTimer += Time.deltaTime;

        if (letterTimer > timePerLetter) {
            
            if (letterOnceValue != 0) {
                while (letterOnceValue != 0 && currentCharacter < letterReferences.Length) {
                    if (CheckCommand())
                        return;
                    if (letterReferences[currentCharacter] != null) {
                        letterReferences[currentCharacter].enabled = true;
                        switch (letterEffect.ToLower()) {
                            case "twitch": letterReferences[currentCharacter].GetComponent<Letter>().effect = new TwitchEffectLetter(letterReferences[currentCharacter].GetComponent<Letter>(), letterIntensity); break;
                            case "rotate": letterReferences[currentCharacter].GetComponent<Letter>().effect = new RotatingEffectLetter(letterReferences[currentCharacter].GetComponent<Letter>(), letterIntensity); break;
                            case "shake": letterReferences[currentCharacter].GetComponent<Letter>().effect = new ShakeEffectLetter(letterReferences[currentCharacter].GetComponent<Letter>(), letterIntensity); break;
                            default: letterReferences[currentCharacter].GetComponent<Letter>().effect = null; break;
                        }
                        if (letterSound != null && !muted)
                            if (letterSound.isPlaying)  UnitaleUtil.PlaySound("BubbleSound", letterSound.clip.name);
                            else                        letterSound.Play();
                    }
                    currentReferenceCharacter++;
                    currentCharacter++;
                    letterOnceValue --;
                }
            } else {
                for (int i = 0; i < letterSpeed && currentCharacter < letterReferences.Length; i++) {
                    if (CheckCommand())
                        return;
                    if (letterReferences[currentCharacter] != null) {
                        letterReferences[currentCharacter].enabled = true;
                        switch (letterEffect.ToLower()) {
                            case "twitch": letterReferences[currentCharacter].GetComponent<Letter>().effect = new TwitchEffectLetter(letterReferences[currentCharacter].GetComponent<Letter>(), letterIntensity); break;
                            case "rotate": letterReferences[currentCharacter].GetComponent<Letter>().effect = new RotatingEffectLetter(letterReferences[currentCharacter].GetComponent<Letter>(), letterIntensity); break;
                            case "shake": letterReferences[currentCharacter].GetComponent<Letter>().effect = new ShakeEffectLetter(letterReferences[currentCharacter].GetComponent<Letter>(), letterIntensity); break;
                            default: letterReferences[currentCharacter].GetComponent<Letter>().effect = null; break;
                        }
                        if (letterSound != null && !muted)
                            if (letterSound.isPlaying)  UnitaleUtil.PlaySound("BubbleSound", letterSound.clip.name);
                            else                        letterSound.Play();
                    }
                    currentReferenceCharacter++;
                    currentCharacter++;
                }
            }
            letterTimer = 0.0f;
        }
        noSkip1stFrame = false;
    }

    private string oldParseCommandInline(string input, ref int currentChar) {
        currentChar++; // skip past found bracket
        checkCharInBounds(currentChar, input.Length);
        string control = "";
        while (input[currentChar] != ']') {
            control += input[currentChar];
            currentChar++;
            checkCharInBounds(currentChar, input.Length);
        }
        return control;
    }

    private string parseCommandInline(string input, ref int currentChar) {
        currentChar++; // skip past found bracket
        if (checkCharInBounds(currentChar, input.Length))
            return null;
        string control = ""; int count = 1;
        while (true) {
            if (input[currentChar] == '[')
                count++;
            else if (input[currentChar] == ']') {
                count--;
                if (count == 0)
                    break;
            }
            control += input[currentChar];
            currentChar++;
            if (checkCharInBounds(currentChar, input.Length))
                return null;
        }
        return control;
    }

    private bool checkCharInBounds(int i, int length) {
        if (i >= length) {
            Debug.LogWarning("Went out of bounds looking for arguments after control character.");
            return true;
        } else
            return false;
    }

    private void preCreateControlCommand(string command) {
        string[] cmds = UnitaleUtil.specialSplit(':', command);
        string[] args = new string[0];
        if (cmds.Length == 2) {
            args = UnitaleUtil.specialSplit(',', cmds[1], true);
            cmds[1] = args[0];
        }
        switch (cmds[0].ToLower()) {
            case "noskip":      currentSkippable = false;                          break;
            case "instant":     displayImmediate = true;                           break;
            case "color":       currentColor = ParseUtil.getColor(cmds[1]);        break;
            case "charspacing": setHorizontalSpacing(ParseUtil.getFloat(cmds[1])); break;
            case "linespacing": setVerticalSpacing(ParseUtil.getFloat(cmds[1]));   break;

            case "starcolor":
                Color starColor = ParseUtil.getColor(cmds[1]);
                int indexOfStar = textQueue[currentLine].Text.IndexOf('*'); // HACK oh my god lol
                if (indexOfStar > -1)
                    letterReferences[indexOfStar].color = starColor;
                break;

            case "font":
                AudioClip oldClip = letterSound.clip;
                float oldLineThing = Charset.LineSpacing;
                setFont(SpriteFontRegistry.Get(cmds[1]));
                letterSound.clip = oldClip;
                foreach (Letter l in letters)
                    l.transform.position = new Vector2(l.transform.position.x, l.transform.position.y + (oldLineThing - Charset.LineSpacing));
                currentY += (oldLineThing - Charset.LineSpacing);
                break;

            case "effect":
                switch (cmds[1].ToUpper()) {
                    case "NONE":
                        textEffect = null;
                        break;

                    case "TWITCH":
                        if (args.Length > 1)  textEffect = new TwitchEffect(this, ParseUtil.getFloat(args[1]));
                        else                  textEffect = new TwitchEffect(this);
                        break;

                    case "SHAKE":
                        if (args.Length > 1)  textEffect = new ShakeEffect(this, ParseUtil.getFloat(args[1]));
                        else                  textEffect = new ShakeEffect(this);
                        break;

                    case "ROTATE":
                        if (args.Length > 1)  textEffect = new RotatingEffect(this, ParseUtil.getFloat(args[1]));
                        else                  textEffect = new RotatingEffect(this);
                        break;
                }
                break;
        }
    }

    private void inUpdateControlCommand(DynValue command) {
        string[] cmds = UnitaleUtil.specialSplit(':', command.String);
        string[] args = new string[0];
        if (cmds.Length == 2) {
            args = UnitaleUtil.specialSplit(',', cmds[1], true);
            cmds[1] = args[0];
        }
        switch (cmds[0].ToLower()) {
            case "w":            letterTimer = timePerLetter - (singleFrameTiming * ParseUtil.getInt(cmds[1]));  break;
            case "waitall":      timePerLetter = singleFrameTiming * ParseUtil.getInt(cmds[1]);                  break;
            case "novoice":      letterSound.clip = null;                                                        break;
            case "next":         autoSkipAll = true;                                                             break;
            case "finished":     autoSkipThis = true;                                                            break;
            case "nextthisnow":  autoSkip = true;                                                                break;
            case "font":         letterSound.clip = SpriteFontRegistry.Get(cmds[1].ToLower()).Sound;             break;
            case "waitfor":      waitingChar = (KeyCode)Enum.Parse(typeof(KeyCode), cmds[1]);                    break;
            case "speed":        letterSpeed = Int32.Parse(args[0]);                                             break;
            case "letters":      letterOnceValue = Int32.Parse(args[0]);                                         break;
            case "noskipatall":  blockSkip = true;                                                               break;

            case "voice":
                if (cmds[1].ToLower() == "default")  letterSound.clip = SpriteFontRegistry.Get(SpriteFontRegistry.UI_DEFAULT_NAME).Sound;
                else                                 letterSound.clip = AudioClipRegistry.GetVoice(cmds[1].ToLower());
                break;

            case "func":
                try {
                    if (caller == null)
                        UnitaleUtil.displayLuaError("???", "Func called but no script to reference. This is the engine's fault, not yours.");
                    if (args.Length > 1) {
                        DynValue[] argsbis = new DynValue[args.Length - 1];

                        for (int i = 1; i < args.Length; i++) {
                            //The character " is forbidden at the beginning and at the end of the string, as it is the sign of a String.
                            args[i] = args[i].TrimStart('"').TrimEnd('"');
                            Type type = UnitaleUtil.CheckRealType(args[i]);
                            //Boolean
                            if (type == typeof(bool)) {
                                if (args[i].Replace(" ", "") == "true")
                                    argsbis[i - 1] = DynValue.NewBoolean(true);
                                else
                                    argsbis[i - 1] = DynValue.NewBoolean(false);
                                //Number
                            } else if (type == typeof(float)) {
                                args[i] = args[i].Replace(" ", "");
                                float number = CreateNumber(args[i]);
                                argsbis[i - 1] = DynValue.NewNumber(number);
                                //Array
                                /*} else if (type != typeof(string)) {
                                    int rank = 0;
                                    print("type = " + type.ToString());
                                    Type basisType = type;
                                    while (basisType.IsArray) {
                                        rank++;
                                        basisType = basisType.GetElementType();
                                    }
                                    int basisRank = rank;
                                    int[] lengths = new int[0];
                                    object arr = UnitaleUtil.stringToArray(args[i], out lengths);
                                    string[] str = UnitaleUtil.specialSplit(',', args[i].Substring(1, args[i].Length - 2), true);
                                    object dv = new DynValue[lengths[rank]];
                                    object dvtemp = dv;

                                    while (rank > 0) {
                                        if (rank > 1) {
                                            dvtemp = dv;
                                            if (rank == lengths.Length) {
                                                Table temp = new Table(null);
                                                DynValue dynv = (DynValue)dvtemp;
                                                temp.Set(1, dynv);
                                                dvtemp = DynValue.NewTable(temp);
                                            }
                                            dv = DynValue.NewTable(UnitaleUtil.DynValueArrayToTable(new DynValue[lengths[rank - 1]]));
                                        }

                                        for (int j = 0; j < ((DynValue[])dv).Length; j++)
                                            if (rank == 1 && basisType == typeof(float)) {
                                                Table array = ((DynValue)arr).Table;
                                                float f = CreateNumber((string)array[j]);
                                                ((DynValue[])dv)[j] = DynValue.NewNumber(f);
                                            } else if (rank == 1 && basisType == typeof(bool)) {
                                                if (((DynValue[])arr)[j].String.Replace(" ", "") == "true")
                                                    ((DynValue)dv).Table[j] = DynValue.NewBoolean(true);
                                                else
                                                    ((DynValue[])dv)[j] = DynValue.NewBoolean(false);
                                            } else if (rank > 1) {
                                                DynValue[] dvs = ((DynValue[][])dvtemp)[j];
                                                ((DynValue[])dv)[j] = DynValue.NewTable(UnitaleUtil.DynValueArrayToTable(dvs));
                                            } else {
                                                string s = ((DynValue[])arr)[j].String;
                                                ((DynValue[])dv)[j] = DynValue.NewString(s);
                                            }
                                        rank--;
                                    }

                                    dv = DynValue.NewTable(((DynValue)dv).Table);
                                    Table dv2 = ((DynValue)dv).Table;
                                    UnitaleUtil.CompleteTableFromArray((Array)arr, ref dv2, basisRank, basisType);*/
                                //String
                            } else
                                argsbis[i - 1] = DynValue.NewString(args[i]);
                        }
                        caller.Call(args[0], argsbis, true); //ADD TRY
                                                             //caller.Call(args[0], DynValue.NewString(args[1]));
                    } else
                        caller.Call(cmds[1], null, true);
                } catch (InterpreterException ex) { UnitaleUtil.displayLuaError(caller.scriptname, ex.DecoratedMessage); }
                break;

            case "mugshot":
                if (args[0] == "null")
                    mugshot.color = new Color(mugshot.color.r, mugshot.color.g, mugshot.color.b, 0);
                mugshot.sprite = SpriteRegistry.GetMugshot(args[0]);
                break;

            case "name":
                string text = textQueue[currentLine].Text;
                string textEnd = text.Substring(0, currentCharacter - 1) + PlayerCharacter.instance.Name + text.Substring(currentCharacter, text.Length - 1);
                textQueue[currentLine].setText(textEnd);
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
                if (GameObject.Find("player"))  GameObject.Find("player").GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetSound(args[0]));
                else                            GameObject.Find("Player").GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetSound(args[0]));
                break;

            case "health":
                args[0] = args[0].Replace(" ", "");
                bool killable = false;
                if (args.Length > 1) {
                    args[1] = args[1].Replace(" ", "");
                    if (args[1] == "killable")
                        killable = true;
                }
                if ((args[0].Contains("-") && args[0] != "Max-1") || args[0] == "kill")         PlayerController.PlaySound(AudioClipRegistry.GetSound("hurtsound"));
                else if (args.Length > 1) {
                    if (args[1] == "set" && ParseUtil.getInt(args[0]) < PlayerCharacter.instance.HP) PlayerController.PlaySound(AudioClipRegistry.GetSound("hurtsound"));
                    else                                                                        PlayerController.PlaySound(AudioClipRegistry.GetSound("healsound"));
                } else                                                                          PlayerController.PlaySound(AudioClipRegistry.GetSound("healsound"));

                if (args[0] == "kill")                                                          setHP(0);
                else if (args[0] == "Max-1" && PlayerCharacter.instance.HP < PlayerCharacter.instance.MaxHP - 1)  setHP(PlayerCharacter.instance.MaxHP - 1);
                else if (args[0] == "Max-1")                                                    args[0] = "0"; //Does nothing
                else if (args[0] == "Max" && PlayerCharacter.instance.HP < PlayerCharacter.instance.MaxHP)        setHP(PlayerCharacter.instance.MaxHP);
                else if (args[0] == "Max")                                                      args[0] = "0"; //Does nothing
                else if (args.Length > 1 && !killable) {
                    if (args[1] == "set")
                        if (ParseUtil.getInt(args[0]) < 1)                                      setHP(1);
                        else                                                                    setHP(ParseUtil.getInt(args[0]));
                } else if (PlayerCharacter.instance.HP + ParseUtil.getInt(args[0]) <= 0 && !killable)    setHP(1);
                else                                                                            setHP(PlayerCharacter.instance.HP + ParseUtil.getInt(args[0]));
                break;

            case "lettereffect":
                letterEffect = args[0];
                if (args.Length == 2)
                    letterIntensity = ParseUtil.getFloat(args[1]);
                break;
        }
    }

    private void setHP(float HP) {
        if (GameObject.Find("player")) PlayerController.instance.setHP(HP);
        else if (HP <= 0)              PlayerCharacter.instance.HP = 1;
        else                           PlayerCharacter.instance.HP = PlayerCharacter.instance.MaxHP;
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