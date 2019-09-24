﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class EnterNameScript : MonoBehaviour {
    private bool weirdBackspaceShift = false;
    private bool isNewGame = true;
    private bool confirm = false;
    private bool hackFirstString = false;
    private AudioSource uiAudio;
    private string choiceLetter = "A", playerName = "";
    private TextManager tmInstr, tmName, tmLettersMaj, tmLettersMin;
    private Dictionary<string, string> specialNameDict = new Dictionary<string, string>();
    private string[] ForbiddenNames = new string[] { "lukark", "rtl", "rhenaud", "rtlgeno", "rtlukark", "hacker" };
    private string confirmText = null;
	GameObject textObjFolder;

	// Use this for initialization
	void Start () {
		textObjFolder = GameObject.Find("NameText");
		AddToDict();
        isNewGame = SaveLoad.savedGame == null;
        uiAudio = GameObject.Find("TextManager Instructions").GetComponent<AudioSource>();
        try { GameObject.Find("textframe_border_outer").SetActive(false); } catch { }
        tmInstr = GameObject.Find("TextManager Instructions").GetComponent<TextManager>();
        tmInstr.SetHorizontalSpacing(2);
        if (GlobalControls.crate)
            tmInstr.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall]GIV HMI A NAME!!!", false, true) });
        else 
            tmInstr.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall]Name the fallen human.", false, true) });
        tmName = GameObject.Find("TextManager Name").GetComponent<TextManager>();
        tmName.SetHorizontalSpacing(2);
        GameObject firstCamera = GameObject.Find("Main Camera");
        firstCamera.name = "temp";
        if (GameObject.Find("Main Camera"))
            GameObject.Destroy(GameObject.Find("Main Camera"));
        firstCamera.name = "Main Camera";
        if (!isNewGame) {
            playerName = PlayerCharacter.instance.Name;
        } else {
            Camera.main.GetComponent<AudioSource>().clip = AudioClipRegistry.GetMusic("mus_menu");
            Camera.main.GetComponent<AudioSource>().Play();
        }
        tmName.SetTextQueue(new TextMessage[] { new TextMessage(playerName, false, true) });
        tmLettersMaj = GameObject.Find("TextManager LettersMaj").GetComponent<TextManager>();
        tmLettersMaj.SetHorizontalSpacing(52.2f);
        tmLettersMaj.SetVerticalSpacing(-1);
        tmLettersMaj.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall]ABCDEFG\nHIJKLMN\nOPQRSTU\nVWXYZ", false, true) });
        tmLettersMaj.SetEffect(new ShakeEffect(tmLettersMaj));
        tmLettersMin = GameObject.Find("TextManager LettersMin").GetComponent<TextManager>();
        tmLettersMin.SetHorizontalSpacing(52.2f);
        tmLettersMin.SetVerticalSpacing(-1);
        tmLettersMin.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall]abcdefg\nhijklmn\nopqrstu\nvwxyz", false, true) });
        tmLettersMin.SetEffect(new ShakeEffect(tmLettersMin));
        for (int i = 0; i < GameObject.Find("TextManager LettersMaj").GetComponentsInChildren<Image>().Length; i ++)
            GameObject.Find("TextManager LettersMaj").GetComponentsInChildren<Image>()[i].name = GameObject.Find("TextManager LettersMaj").GetComponentsInChildren<Image>()[i].sprite.name;
        for (int i = 0; i < GameObject.Find("TextManager LettersMin").GetComponentsInChildren<Image>().Length; i ++)
            GameObject.Find("TextManager LettersMin").GetComponentsInChildren<Image>()[i].name = GameObject.Find("TextManager LettersMaj").GetComponentsInChildren<Image>()[i].sprite.name.ToLower();
        GameObject.Find("A").GetComponent<Image>().color = new Color(1, 1, 0, 1);
    }
	
	// Update is called once per frame
	void Update () {
        if (!confirm) {
            if (!hackFirstString && tmName.transform.childCount != 0 && !isNewGame) {
                hackFirstString = true;
                tmName.SetTextQueue(new TextMessage[] { new TextMessage(playerName, false, true) });
                tmName.transform.localPosition = new Vector3(-calcTotalLength(tmName) / 2, tmName.transform.localPosition.y, tmName.transform.localPosition.z);
            } 
            if (GlobalControls.input.Down == UndertaleInput.ButtonState.PRESSED) {
                if (choiceLetter == "Quit")                                                                                                 setColor("A");
                else if (choiceLetter == "Backspace")                                                                                       setColor("D");
                else if (choiceLetter == "Done")                                                                                            setColor("G");
                else if (choiceLetter == "T" || choiceLetter == "U")                                                                        setColor(choiceLetter[0] + 18);
                else if (choiceLetter == "V" || choiceLetter == "W" || choiceLetter == "X" || choiceLetter == "Y" || choiceLetter == "Z")   setColor(choiceLetter[0] + 11);
                else if (choiceLetter == "t" || choiceLetter == "u")                                                                        setColor("Done");
                else if (choiceLetter == "v" || choiceLetter == "w")                                                                        setColor("Quit");
                else if (choiceLetter == "x" || choiceLetter == "y" || choiceLetter == "z")                                                 setColor("Backspace");
                else                                                                                                                        setColor(choiceLetter[0] + 7);
            } else if (GlobalControls.input.Up == UndertaleInput.ButtonState.PRESSED) {
                if (choiceLetter == "Quit")                                                                                                 setColor("v");
                else if (choiceLetter == "Backspace")                                                                                       setColor("y");
                else if (choiceLetter == "Done")                                                                                            setColor("u");
                else if (choiceLetter == "a" || choiceLetter == "b" || choiceLetter == "c" || choiceLetter == "d" || choiceLetter == "e")   setColor(choiceLetter[0] - 11);
                else if (choiceLetter == "f" || choiceLetter == "g")                                                                        setColor(choiceLetter[0] - 18);
                else if (choiceLetter == "A" || choiceLetter == "B")                                                                        setColor("Quit");
                else if (choiceLetter == "C" || choiceLetter == "D" || choiceLetter == "E")                                                 setColor("Backspace");
                else if (choiceLetter == "F" || choiceLetter == "G")                                                                        setColor("Done");
                else                                                                                                                        setColor(choiceLetter[0] - 7);
            } else if (GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED) {
                if (choiceLetter == "Quit")                                                                                                                         setColor("Backspace");
                else if (choiceLetter == "Backspace")                                                                                                               setColor("Done");
                else if (choiceLetter == "Done")                                                                                                                    setColor("Quit");
                else if (choiceLetter == "G" || choiceLetter == "N" || choiceLetter == "U" || choiceLetter == "g" || choiceLetter == "n" || choiceLetter == "u")    setColor(choiceLetter[0] - 6);
                else if (choiceLetter == "Z" || choiceLetter == "z")                                                                                                setColor(choiceLetter[0] - 4);
                else                                                                                                                                                setColor(choiceLetter[0] + 1);
            } else if (GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED) {
                if (choiceLetter == "Quit")                                                                                                                         setColor("Done");
                else if (choiceLetter == "Backspace")                                                                                                               setColor("Quit");
                else if (choiceLetter == "Done")                                                                                                                    setColor("Backspace");
                else if (choiceLetter == "A" || choiceLetter == "H" || choiceLetter == "O" || choiceLetter == "a" || choiceLetter == "h" || choiceLetter == "o")    setColor(choiceLetter[0] + 6);
                else if (choiceLetter == "V" || choiceLetter == "v")                                                                                                setColor(choiceLetter[0] + 4);
                else                                                                                                                                                setColor(choiceLetter[0] - 1);
            } else if (GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED) {
                weirdBackspaceShift = true;
                if (playerName.Length > 0)
                    playerName = playerName.Substring(0, playerName.Length - 1);
                else
                    weirdBackspaceShift = false;
                tmName.SetTextQueue(new TextMessage[] { new TextMessage(playerName, false, true) });
                tmName.transform.localPosition = new Vector3(-calcTotalLength(tmName) / 2, tmName.transform.localPosition.y, tmName.transform.localPosition.z);
            } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED) {
                if (choiceLetter == "Quit") {
                    GameObject.Find("Main Camera").GetComponent<AudioSource>().Stop();
                    SceneManager.LoadScene("TitleScreen");
                } else if (choiceLetter == "Backspace") {
                    weirdBackspaceShift = true;
                    if (playerName.Length > 0)
                        playerName = playerName.Substring(0, playerName.Length - 1);
                    else
                        weirdBackspaceShift = false;
                } else if (choiceLetter == "Done") {
                    if (playerName.Length > 0) {
                        weirdBackspaceShift = false;
                        confirm = true;
                        specialNameDict.TryGetValue(playerName.ToLower(), out confirmText);
                        StartCoroutine(waitConfirm(ForbiddenNames.Contains(playerName.ToLower())));
						textObjFolder.SetActive(false);
					}
                } else {
                    if (playerName.Length < 9)
                        playerName = playerName + choiceLetter;
                    else
                        playerName = playerName.Substring(0, 8) + choiceLetter;
                    weirdBackspaceShift = false;
                }
                tmName.SetTextQueue(new TextMessage[] { new TextMessage(playerName, false, true) });
                tmName.transform.localPosition = new Vector3(-calcTotalLength(tmName) / 2, tmName.transform.localPosition.y, tmName.transform.localPosition.z);
                uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menuconfirm"));
                return;
            } else
                return;
            uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menumove"));
        }
    }

    void setColor(string str) {
        if (choiceLetter.Length != 1)
            GameObject.Find(choiceLetter).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        else
            GameObject.Find(choiceLetter).GetComponent<Image>().color = new Color(1, 1, 1, 1);
        choiceLetter = str;
        if (choiceLetter.Length != 1)
            GameObject.Find(choiceLetter).GetComponent<SpriteRenderer>().color = new Color(1, 1, 0, 1);
        else
            GameObject.Find(choiceLetter).GetComponent<Image>().color = new Color(1, 1, 0, 1);
    }

    void setColor(int a) { setColor(((char)a).ToString());  }

    IEnumerator waitConfirm(bool isForbidden = false) {
        yield return 0;
        if (confirmText != null)
            tmInstr.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall]" + confirmText, false, true) });
        else if (GlobalControls.crate)
            tmInstr.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall]LAL GUD???", false, true) });
        else
            tmInstr.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall]Is this name correct?", false, true) });
        tmName.SetEffect(new ShakeEffect(tmName));
        GameObject.Find("Backspace").GetComponent<SpriteRenderer>().enabled = false;
        tmLettersMaj.transform.position = new Vector3(tmLettersMaj.transform.position.x, tmLettersMaj.transform.position.y, 10000);
        tmLettersMin.transform.position = new Vector3(tmLettersMin.transform.position.x, tmLettersMin.transform.position.y, 10000);
        setColor("Quit");
        if (isForbidden)
            GameObject.Find("Done").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        else
            GameObject.Find("Done").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        float diff = calcTotalLength(tmName)*2;
        float actualX = tmName.transform.localPosition.x, actualY = tmName.transform.localPosition.y;
        while (GlobalControls.input.Confirm != UndertaleInput.ButtonState.PRESSED) {
            if (tmName.transform.localScale.x < 3) {
                tmName.transform.localScale = new Vector3(tmName.transform.localScale.x + 0.01f, tmName.transform.localScale.y + 0.01f, 1);
                tmName.transform.localPosition = new Vector3(actualX - (((tmName.transform.localScale.x - 1) * diff) / 2), 
                                                             actualY - (((tmName.transform.localScale.x - 1) * diff) / 6), tmName.transform.localPosition.z);
            }
            if ((GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED || GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED)
                    && GameObject.Find("Done").GetComponent<SpriteRenderer>().enabled &&!isForbidden) {
                if (choiceLetter == "Quit")     setColor("Done");
                else                            setColor("Quit");
                uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menumove"));
            }
            yield return 0;
        }
        uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menuconfirm"));
        if (choiceLetter == "Quit") {
			textObjFolder.SetActive(true);
			confirmText = null;
            confirm = false;
            tmName.transform.localScale = new Vector3(1, 1, 1);
            tmName.SetEffect(null);
            tmName.SetTextQueue(new TextMessage[] { new TextMessage(playerName, false, true) });
            tmName.transform.localPosition = new Vector3(-calcTotalLength(tmName)/2, 145, tmName.transform.localPosition.z);
            if (GlobalControls.crate)
                tmInstr.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall]QWIK QWIK QWIK!!!", false, true) });
            else
                tmInstr.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall]Name the fallen human.", false, true) });
            tmLettersMaj.transform.position = new Vector3(tmLettersMaj.transform.position.x, tmLettersMaj.transform.position.y, 0);
            tmLettersMin.transform.position = new Vector3(tmLettersMin.transform.position.x, tmLettersMin.transform.position.y, 0);
            GameObject.Find("Backspace").GetComponent<SpriteRenderer>().enabled = true;
            setColor("Done");
        } else {
            PlayerCharacter.instance.Name = playerName;
            if (isNewGame) {
                GameObject.Find("Main Camera").GetComponent<AudioSource>().Stop();
                GameObject.Find("Main Camera").GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetSound("intro_holdup"));
                SpriteRenderer blank = GameObject.Find("Blank").GetComponent<SpriteRenderer>();
                while (blank.color.a <= 1) {
                    if (tmName.transform.localScale.x < 3) {
                        tmName.transform.localScale = new Vector3(tmName.transform.localScale.x + 0.01f, tmName.transform.localScale.y + 0.01f, 1);
                        tmName.transform.localPosition = new Vector3(actualX - (((tmName.transform.localScale.x - 1f) * diff) / 2f), actualY - (((tmName.transform.localScale.x - 1f) * diff) / 6), tmName.transform.localPosition.z);
                    }
                    blank.color = new Color(blank.color.r, blank.color.g, blank.color.b, blank.color.a + 0.003f);
                    yield return 0;
                }
                while (GameObject.Find("Main Camera").GetComponent<AudioSource>().isPlaying)
                    yield return 0;
                SceneManager.LoadScene("TransitionOverworld");
            } else {
                SaveLoad.Save();
                SceneManager.LoadScene("TitleScreen");
            }
        }
    }

    float calcTotalLength(TextManager txtmgr) {
        int count = 0;
        float totalWidth = 0;
        RectTransform[] rts = txtmgr.gameObject.GetComponentsInChildren<RectTransform>();
        for (int i = 0; i < rts.Length/2; i++) {
            if (weirdBackspaceShift && i == (rts.Length/2) - 1)
                break;
            totalWidth += rts[i*2].sizeDelta.x;
            count++;
        }
        totalWidth += txtmgr.hSpacing * count;
        return totalWidth;
    }

    bool isInDict(string test) {
        test = test.ToLower();
        foreach (string key in specialNameDict.Keys)
            if (key.ToLower() == test)
                return true;
        return false;
    }

    void AddToDict() {
        specialNameDict.Add("lukark",    "Hey, that's my name!\nDon't copy me.");                              
        specialNameDict.Add("rtl",       "Still my name, dude.");
        specialNameDict.Add("rhenao",    "The basis name.");
        specialNameDict.Add("rhenaud",   "My real name.");
        specialNameDict.Add("uduu",      "The path to victory. Go to\nthe 2nd map. Real name: UDUUL");
        specialNameDict.Add("thefail",   "DO 3 BARREL ROLLS!!!");
        specialNameDict.Add("exception", "It's me.");
        specialNameDict.Add("fugitive",  "*flees*\n/me flees");
        specialNameDict.Add("outbounds", "Go behind that dog!");
        specialNameDict.Add("soulless",  "They shall fall, the one\nafter the other.");
        specialNameDict.Add("notfound",  "404");
        specialNameDict.Add("frisk",     "That'll do nothing here.");
        specialNameDict.Add("cyka",      "CYKA BLYAT RUSH B");
        specialNameDict.Add("cyf",       "The true name.\nCreate Your Frisk FTW!");
        specialNameDict.Add("credits",   "RhenaudTheLukark and lvkuln.\nThat's it, I think.");
        specialNameDict.Add("mionn",     "No HANDZ");
        specialNameDict.Add("handz",     "RIP Mionn");
        specialNameDict.Add("campfire",  "These guys helped me too.\nDon't tell anyone! ;)");
        specialNameDict.Add("morsay",    "CLIQUEZ SALOPES! Hi Kaiser :D");
        specialNameDict.Add("rtlgeno",   "You nosy little thing.");
        specialNameDict.Add("rtlukark",  "You can't.");
        specialNameDict.Add("chara",     "Classic af. Your \"The true name.\"\nis in another castle.");
        specialNameDict.Add("ytp",       "Heehee, you know me, don't you ;P");
        specialNameDict.Add("unity",     "Yup, that's the right engine.");
        specialNameDict.Add("unitale",   "Good work! You know the\nname of the project!");
        specialNameDict.Add("csharp",    "That's the language.");
        specialNameDict.Add("js",        "I hate you.");
        specialNameDict.Add("hacker",    "OH NO YOU DON'T.");
        specialNameDict.Add("mmmmmmmmm", "You just want to watch\nthe engine burn.");
        specialNameDict.Add("wwwwwwwww", "You just want to watch\nthe engine burn.");
        specialNameDict.Add("undertale", "Without this game,\nthis wouldn't exist.");
        specialNameDict.Add("gamemaker", "...Bleh.");
        specialNameDict.Add("punder",    "ARR PEE TIME!");
        specialNameDict.Add("rptale",    "Thanks a lot,\nBanzy <3");
        specialNameDict.Add("undyne",    "It's not like you'll\nfind her, anyway.");
        specialNameDict.Add("alphys",    "Chances to see\nher: 0%.");
        specialNameDict.Add("asgore",    "Goatdad can't kill\nyou here.");
        specialNameDict.Add("toriel",    "Goatmom can't help\nyou here.");
        specialNameDict.Add("papyrus",   "[font:papyrus]Having more than\n6 characters helps!");
        specialNameDict.Add("sans",      "[font:sans]no bad time for\nya.");
        specialNameDict.Add("asriel",    "If that's your\nchoice...");
        specialNameDict.Add("flowey",    "You sadist.");
        specialNameDict.Add("four",      "4");
        specialNameDict.Add("sendnudes", "(.)(.)");
    }
}
