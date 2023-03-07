using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class EnterNameScript : MonoBehaviour {
    private bool weirdBackspaceShift;
    private bool isNewGame = true;
    private bool confirm;
    private bool hackFirstString;
    private string choiceLetter = "A", playerName = "";
    private readonly Dictionary<string, string> specialNameDict = new Dictionary<string, string>();
    private readonly string[] ForbiddenNames = { "lukark", "rtl", "rhenaud" };
    private string confirmText;

    public GameObject textObjFolder;
    public AudioSource uiAudio;
    public TextManager tmInstr, tmName, tmLettersMaj, tmLettersMin;

    // Use this for initialization
    private void Start() {
        AddToDict();
        isNewGame = SaveLoad.savedGame == null;
        try { GameObject.Find("textframe_border_outer").SetActive(false); }
        catch { /* ignored */ }
        tmInstr.SetTextQueue(new[] { new TextMessage("[noskipatall]" + (GlobalControls.crate ? "GIV HMI A NAME!!!" : "Name the fallen human."), false, true) });
        tmInstr.SetHorizontalSpacing(2);
        tmName.SetHorizontalSpacing(2);
        GameObject firstCamera = GameObject.Find("Main Camera");
        firstCamera.name = "temp";
        if (GameObject.Find("Main Camera"))
            Destroy(GameObject.Find("Main Camera"));
        firstCamera.name = "Main Camera";
        if (!isNewGame) {
            playerName = PlayerCharacter.instance.Name;
        } else {
            Camera.main.GetComponent<AudioSource>().clip = AudioClipRegistry.GetMusic("mus_menu");
            Camera.main.GetComponent<AudioSource>().Play();
        }
        tmName.SetTextQueue(new[] { new TextMessage(playerName, false, true) });
        tmLettersMaj.SetTextQueue(new[] { new TextMessage("[noskipatall][charspacing:52.2][linespacing:-1]ABCDEFG\nHIJKLMN\nOPQRSTU\nVWXYZ", false, true) });
        tmLettersMaj.SetEffect(new ShakeEffect(tmLettersMaj));
        tmLettersMin.SetTextQueue(new[] { new TextMessage("[noskipatall][charspacing:52.2][linespacing:-1]abcdefg\nhijklmn\nopqrstu\nvwxyz", false, true) });
        tmLettersMin.SetEffect(new ShakeEffect(tmLettersMin));
        for (int i = 0; i < tmLettersMaj.GetComponentsInChildren<Image>().Length; i ++)
            tmLettersMaj.GetComponentsInChildren<Image>()[i].name = tmLettersMaj.GetComponentsInChildren<Image>()[i].sprite.name;
        for (int i = 0; i < tmLettersMin.GetComponentsInChildren<Image>().Length; i ++)
            tmLettersMin.GetComponentsInChildren<Image>()[i].name = tmLettersMaj.GetComponentsInChildren<Image>()[i].sprite.name.ToLower();
        GameObject.Find("A").GetComponent<Image>().color = new Color(1, 1, 0, 1);
    }

    // Update is called once per frame
    private void Update() {
        if (confirm) return;
        if (!hackFirstString && tmName.transform.childCount != 0 && !isNewGame) {
            hackFirstString = true;
            tmName.SetTextQueue(new[] { new TextMessage(playerName, false, true) });
            tmName.MoveTo(-calcTotalLength(tmName) / 2, tmName.transform.localPosition.y);
        }
        if (GlobalControls.input.Down == UndertaleInput.ButtonState.PRESSED) {
            switch (choiceLetter) {
                case "Quit":      setColor("A");                  break;
                case "Backspace": setColor("D");                  break;
                case "Done":      setColor("G");                  break;
                case "T":
                case "U":         setColor(choiceLetter[0] + 18); break;
                case "V":
                case "W":
                case "X":
                case "Y":
                case "Z":         setColor(choiceLetter[0] + 11); break;
                case "t":
                case "u":         setColor("Done");               break;
                case "v":
                case "w":         setColor("Quit");               break;
                case "x":
                case "y":
                case "z":         setColor("Backspace");          break;
                default:          setColor(choiceLetter[0] + 7);  break;
            }
        } else if (GlobalControls.input.Up == UndertaleInput.ButtonState.PRESSED) {
            switch (choiceLetter) {
                case "Quit":      setColor("v");                  break;
                case "Backspace": setColor("y");                  break;
                case "Done":      setColor("u");                  break;
                case "a":
                case "b":
                case "c":
                case "d":
                case "e":         setColor(choiceLetter[0] - 11); break;
                case "f":
                case "g":         setColor(choiceLetter[0] - 18); break;
                case "A":
                case "B":         setColor("Quit");               break;
                case "C":
                case "D":
                case "E":         setColor("Backspace");          break;
                case "F":
                case "G":         setColor("Done");               break;
                default:          setColor(choiceLetter[0] - 7);  break;
            }
        } else if (GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED) {
            switch (choiceLetter) {
                case "Quit":      setColor("Backspace");         break;
                case "Backspace": setColor("Done");              break;
                case "Done":      setColor("Quit");              break;
                case "G":
                case "N":
                case "U":
                case "g":
                case "n":
                case "u":         setColor(choiceLetter[0] - 6); break;
                case "Z":
                case "z":         setColor(choiceLetter[0] - 4); break;
                default:          setColor(choiceLetter[0] + 1); break;
            }
        } else if (GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED) {
            switch (choiceLetter) {
                case "Quit":      setColor("Done");              break;
                case "Backspace": setColor("Quit");              break;
                case "Done":      setColor("Backspace");         break;
                case "A":
                case "H":
                case "O":
                case "a":
                case "h":
                case "o":         setColor(choiceLetter[0] + 6); break;
                case "V":
                case "v":         setColor(choiceLetter[0] + 4); break;
                default:          setColor(choiceLetter[0] - 1); break;
            }
        } else if (GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED) {
            weirdBackspaceShift = true;
            if (playerName.Length > 0)
                playerName = playerName.Substring(0, playerName.Length - 1);
            else
                weirdBackspaceShift = false;
            tmName.SetTextQueue(new[] { new TextMessage(playerName, false, true) });
            tmName.MoveTo(-calcTotalLength(tmName) / 2, tmName.transform.localPosition.y);
        } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED) {
            switch (choiceLetter) {
                case "Quit":
                    GameObject.Find("Main Camera").GetComponent<AudioSource>().Stop();
                    SceneManager.LoadScene("TitleScreen");
                    break;
                case "Backspace": {
                    weirdBackspaceShift = true;
                    if (playerName.Length > 0)
                        playerName = playerName.Substring(0, playerName.Length - 1);
                    else
                        weirdBackspaceShift = false;
                    break;
                }
                case "Done": {
                    if (playerName.Length > 0) {
                        weirdBackspaceShift = false;
                        confirm             = true;
                        specialNameDict.TryGetValue(playerName.ToLower(), out confirmText);
                        StartCoroutine(waitConfirm(ForbiddenNames.Contains(playerName.ToLower())));
                        textObjFolder.SetActive(false);
                    }

                    break;
                }
                default: {
                    if (playerName.Length < 9) playerName += choiceLetter;
                    else                       playerName = playerName.Substring(0, 8) + choiceLetter;
                    weirdBackspaceShift = false;
                    break;
                }
            }
            tmName.SetTextQueue(new[] { new TextMessage(playerName, false, true) });
            tmName.MoveTo(-calcTotalLength(tmName) / 2, tmName.transform.localPosition.y);
            uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menuconfirm"));
            return;
        } else
            return;
        uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menumove"));
    }

    private void setColor(int a) { setColor(((char)a).ToString());  }
    private void setColor(string str) {
        if (choiceLetter.Length != 1) GameObject.Find(choiceLetter).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        else                          GameObject.Find(choiceLetter).GetComponent<Image>().color = new Color(1, 1, 1, 1);
        choiceLetter = str;
        if (choiceLetter.Length != 1) GameObject.Find(choiceLetter).GetComponent<SpriteRenderer>().color = new Color(1, 1, 0, 1);
        else                          GameObject.Find(choiceLetter).GetComponent<Image>().color = new Color(1, 1, 0, 1);
    }

    private IEnumerator waitConfirm(bool isForbidden = false) {
        yield return 0;
        tmInstr.SetTextQueue(new[] { new TextMessage("[noskipatall]" + (confirmText ?? (GlobalControls.crate ? "LAL GUD???" : "[noskipatall]Is this name correct?")), false, true) });
        tmName.SetEffect(new ShakeEffect(tmName));
        GameObject.Find("Backspace").GetComponent<SpriteRenderer>().enabled = false;
        tmLettersMaj.gameObject.SetActive(false);
        tmLettersMin.gameObject.SetActive(false);
        setColor("Quit");
        GameObject.Find("Done").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, isForbidden ? 0 : 1);
        float diff = calcTotalLength(tmName)*2;
        float actualX = tmName.transform.localPosition.x, actualY = tmName.transform.localPosition.y;
        while (GlobalControls.input.Confirm != UndertaleInput.ButtonState.PRESSED) {
            if (tmName.transform.localScale.x < 3) {
                float scale = Mathf.Min(3, tmName.transform.localScale.x + 0.01f);
                tmName.transform.localScale = new Vector3(scale, scale, 1);
                tmName.MoveTo(actualX - (tmName.transform.localScale.x - 1) * diff / 2, actualY - (tmName.transform.localScale.x - 1) * diff / 6);
            }
            if ((GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED || GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED)
                    && GameObject.Find("Done").GetComponent<SpriteRenderer>().enabled &&!isForbidden) {
                setColor(choiceLetter == "Quit" ? "Done": "Quit");
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
            tmName.SetTextQueue(new[] { new TextMessage(playerName, false, true) });
            tmName.MoveTo(-calcTotalLength(tmName)/2, 145);
            tmInstr.SetTextQueue(new[] { new TextMessage("[noskipatall]" + (GlobalControls.crate ? "QWIK QWIK QWIK!!!" : "Name the fallen human."), false, true) });
            tmLettersMaj.gameObject.SetActive(true);
            tmLettersMin.gameObject.SetActive(true);
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
                        float scale = Mathf.Min(3, tmName.transform.localScale.x + 0.01f);
                        tmName.transform.localScale = new Vector3(scale, scale, 1);
                        tmName.MoveTo(actualX - (tmName.transform.localScale.x - 1f) * diff / 2f, actualY - (tmName.transform.localScale.x - 1f) * diff / 6);
                    }
                    blank.color = new Color(blank.color.r, blank.color.g, blank.color.b, blank.color.a + 0.003f);
                    yield return 0;
                }
                while (GameObject.Find("Main Camera").GetComponent<AudioSource>().isPlaying)
                    yield return 0;
                UnitaleUtil.ResetOW();
                SceneManager.LoadScene("TransitionOverworld");
                DiscordControls.StartOW();
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
            if (weirdBackspaceShift && i == rts.Length /2 - 1)
                break;
            totalWidth += rts[i*2].sizeDelta.x;
            count++;
        }
        totalWidth += txtmgr.hSpacing * count;
        return totalWidth;
    }

    private void AddToDict() {
        specialNameDict.Add("lukark",    "Hey, that's my name!\nDon't copy me.");
        specialNameDict.Add("rtl",       "Still my name, dude.");
        specialNameDict.Add("rhenao",    "The basis name.");
        specialNameDict.Add("rhenaud",   "My real name.");

        specialNameDict.Add("uduu",      "(Broken) The path to victory. Go to\nthe 2nd map. Real name: UDUUL");
        specialNameDict.Add("thefail",   "(Broken) DO 3 BARREL ROLLS!!!");
        specialNameDict.Add("exception", "(Broken) It's me.");
        specialNameDict.Add("fugitive",  "(Broken) *flees*\n/me flees");
        specialNameDict.Add("four",      "4");

        specialNameDict.Add("outbounds", "Go behind that dog!");
        specialNameDict.Add("soulless",  "They shall fall, one\nafter another.");

        specialNameDict.Add("notfound",  "404");
        specialNameDict.Add("404",       "Name not found.");
        specialNameDict.Add("cyf",       "The true name.\nCreate Your Frisk FTW!");
        specialNameDict.Add("credits",   "RhenaudTheLukark and lvkuln.\nThat's it, I think.");
        specialNameDict.Add("mmmmmmmmm", "You just want to watch\nthe engine burn.");
        specialNameDict.Add("wwwwwwwww", "You just want to watch\nthe engine burn.");
        specialNameDict.Add("undertale", "Without this game,\nthis wouldn't exist.");

        specialNameDict.Add("frisk",     "That'll do nothing here.");
        specialNameDict.Add("chara",     "Classic af. Your \"The true name.\"\nis in another castle.");
        specialNameDict.Add("undyne",    "It's not like you'll\nfind her, anyway.");
        specialNameDict.Add("alphys",    "Chances to see\nher: 0%.");
        specialNameDict.Add("asgore",    "Goatdad can't kill\nyou here.");
        specialNameDict.Add("toriel",    "Goatmom can't help\nyou here.");
        specialNameDict.Add("papyrus",   "HAVING MORE THAN\n6 CHARACTERS HELPS!");
        specialNameDict.Add("sans",      "no bad time for ya.");
        specialNameDict.Add("asriel",    "If that's your\nchoice...");
        specialNameDict.Add("flowey",    "You sadist.");
    }
}
