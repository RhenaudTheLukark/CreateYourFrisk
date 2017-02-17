using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Title : MonoBehaviour {
    public int phase = 0;
    private bool initPhase = false;
    public int indexChoice = 0;
    private string choiceLetter = "Continue";
    float diff, actualX, actualY;
    TextManager tmName;

    // Use this for initialization
    void Start () {
        tmName = GameObject.Find("TextManagerResetName").GetComponent<TextManager>();
        tmName.setHorizontalSpacing(2);
        tmName.setEffect(new ShakeEffect(tmName));
        diff = calcTotalLength(tmName);
        actualX = tmName.transform.localPosition.x;
        actualY = tmName.transform.localPosition.y;
        if (GlobalControls.crate) {
            GameObject.Find("Title").GetComponent<SpriteRenderer>().enabled = false;
            GameObject.Find("Title (1)").GetComponent<SpriteRenderer>().enabled = true;
        }
        StartCoroutine(TitlePhase1());
	}

    IEnumerator TitlePhase1() {
        GameObject.Find("Main Camera").GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetSound("intro_noise"));
        Color noColor = new Color(0,0,0,0);
        while (GameObject.Find("Main Camera").GetComponent<AudioSource>().isPlaying)
            yield return 0;
        while (phase == 0) {
            if (GameObject.Find("PressEnterOrZ").GetComponent<SpriteRenderer>().color.a == 1)
                GameObject.Find("PressEnterOrZ").GetComponent<SpriteRenderer>().color = noColor;
            else
                GameObject.Find("PressEnterOrZ").GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 1);
            yield return new WaitForSeconds(1);
        }
    }
	
	// Update is called once per frame
	void Update () {
        GlobalControls.lastTitle = true;
        if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED && phase == 0) {
            phase++;
            GameObject.Find("Main Camera").GetComponent<AudioSource>().Stop();
            StopCoroutine(TitlePhase1());
        } else if (phase == 1) {
            if (!initPhase) {
                initPhase = true;

                GameObject.Find("Main Camera").GetComponent<AudioSource>().clip = AudioClipRegistry.GetMusic("mus_menu");
                GameObject.Find("Main Camera").GetComponent<AudioSource>().Play();
                try {
                    if (!SaveLoad.Load()) {
                        SceneManager.LoadScene("EnterName");
                    } else {
                        GameObject.Find("PressEnterOrZ").SetActive(false);
                        GameObject.Find("Title").SetActive(false);
                        GameObject.Find("Title (1)").SetActive(false);
                        GameObject.Find("Back1").SetActive(false);
                        GameObject.Find("TextManagerName").GetComponent<TextManager>().setHorizontalSpacing(2);
                        GameObject.Find("TextManagerLevel").GetComponent<TextManager>().setHorizontalSpacing(2);
                        GameObject.Find("TextManagerTime").GetComponent<TextManager>().setHorizontalSpacing(2);
                        GameObject.Find("TextManagerMap").GetComponent<TextManager>().setHorizontalSpacing(2);
                        GameObject.Find("TextManagerName").GetComponent<TextManager>().setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]" + PlayerCharacter.instance.Name, false, true) });
                        if (GlobalControls.crate)  GameObject.Find("TextManagerLevel").GetComponent<TextManager>().setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]VL" + PlayerCharacter.instance.LV, false, true) });
                        else                       GameObject.Find("TextManagerLevel").GetComponent<TextManager>().setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]LV" + PlayerCharacter.instance.LV, false, true) });
                        GameObject.Find("TextManagerLevel").GetComponent<TextManager>().setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]LV" + PlayerCharacter.instance.LV, false, true) });
                        GameObject.Find("TextManagerTime").GetComponent<TextManager>().setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]0:00", false, true) });
                        GameObject.Find("TextManagerMap").GetComponent<TextManager>().setTextQueue(new TextMessage[] { new TextMessage("[noskipatall]" + SaveLoad.savedGame.lastScene, false, true) });
                        tmName.setTextQueue(new TextMessage[] { new TextMessage(PlayerCharacter.instance.Name, false, true) });
                    }
                } catch {
                    if (GlobalControls.crate)
                        UnitaleUtil.displayLuaError(StaticInits.ENCOUNTER, "U USED AN ODL VERSOIN OF CFY? IT ISN'T RERTOCOMAPTIBEL.\n\n"
                                                                             + "DELEET UR SAVE OT NOT HVAE DA ERRRO AGAIN. HREE: \n"
                                                                             + Application.persistentDataPath + "/save.gd\n"
                                                                             + "IF MOAR PORBLMES, TELL EM! :D\n\n"
                                                                             + "SP : NO ESPACE HERE!!!!!!");
                    else
                        UnitaleUtil.displayLuaError(StaticInits.ENCOUNTER, "Have you saved on one of a previous CYF version ? The save isn't retrocompatible.\n\n"
                                                                         + "To not have this error anymore, you'll have to delete the save file. Here it is : \n"
                                                                         + Application.persistentDataPath + "/save.gd\n"
                                                                         + "Tell me if you have some more problems, and thanks for following my fork ! ^^\n\n"
                                                                         + "PS : Don't try to press ESCAPE, or bad things can happen ;)");
                }
            } else {
                if (GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED || GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED) {
                    if (choiceLetter == "Continue") setColor("Reset");
                    else setColor("Continue");
                } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED) {
                    if (choiceLetter == "Continue") {
                        phase = -1;
                        StartCoroutine(LoadGame());
                    } else {
                        phase = 2;
                        GameObject.Find("CanvasReset").transform.position = new Vector3(320, 240, -500);
                        setColor("Continue");
                        choiceLetter = "No";
                        setColor("No");
                    }
                }
            }
        } else if (phase == 2) {
            if (tmName.transform.localScale.x < 3) {
                tmName.transform.localScale = new Vector3(tmName.transform.localScale.x + 0.01f, tmName.transform.localScale.y + 0.01f, 1);
                tmName.transform.localPosition = new Vector3(actualX - (((tmName.transform.localScale.x - 1f) * diff) / 2f), actualY - (((tmName.transform.localScale.x - 1f) * diff) / 6), tmName.transform.localPosition.z);
            }
            if (GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED || GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED) {
                if (choiceLetter == "Yes") setColor("No");
                else setColor("Yes");
            } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED) {
                if (choiceLetter == "Yes") {
                    GameObject.Find("Main Camera").GetComponent<AudioSource>().Stop();
                    GameObject.Find("Main Camera").GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetSound("intro_holdup"));
                    phase = -1;
                    StartCoroutine(NewGame());
                } else {
                    phase = 1;
                    GameObject.Find("CanvasReset").transform.position = new Vector3(320, 240, 50);
                    tmName.transform.localPosition = new Vector3(actualX, actualY, tmName.transform.localPosition.z);
                    tmName.transform.localScale = new Vector3(1, 1, 1);
                    setColor("No");
                    choiceLetter = "Continue";
                    setColor("Continue");
                }
            }
        }
	}

    void setColor(string str) {
        GameObject.Find(choiceLetter).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        choiceLetter = str;
        GameObject.Find(choiceLetter).GetComponent<SpriteRenderer>().color = new Color(1, 1, 0, 1);
    }

    IEnumerator LoadGame() {
        GameObject.DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("TransitionOverworld");
        yield return 0;
        yield return Application.isLoadingLevel;
        GameObject.Find("Player").transform.position = new Vector3(SaveLoad.currentGame.playerPosX, SaveLoad.currentGame.playerPosY, SaveLoad.currentGame.playerPosZ);
        StaticInits si = GameObject.Find("Main Camera OW").GetComponent<StaticInits>();
        StaticInits.MODFOLDER = LuaScriptBinder.Get(null, "ModFolder").String;
        StaticInits.Initialized = false;
        si.initAll();
        GameObject.Destroy(gameObject);
    }

    IEnumerator NewGame() {
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
        PlayerCharacter.instance.Reset();
        LuaScriptBinder.ClearVariables();
        GlobalControls.MapEventPages.Clear();
        GameObject.DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("TransitionOverworld");
        yield return 0;
        yield return Application.isLoadingLevel;
        StaticInits si = GameObject.Find("Main Camera OW").GetComponent<StaticInits>();
        StaticInits.Initialized = false;
        si.initAll();
        GameObject.Destroy(gameObject);
    }

    public float calcTotalLength(TextManager txtmgr, float addNextValue = 0, int fromLetter = -1, int toLetter = -1) {
        float totalWidth = 0, totalMaxWidth = 0, lastY = 0;

        RectTransform[] rts = txtmgr.gameObject.GetComponentsInChildren<RectTransform>();
        int count = 0, begin = fromLetter > 1 ? fromLetter : 1, objective = toLetter > 1 && toLetter < rts.Length ? toLetter : rts.Length;
        for (int i = begin; i < objective; i++) {
            if (rts[i].position.y != lastY) {
                totalWidth += txtmgr.hSpacing * (count - 1);
                if (totalWidth > totalMaxWidth)
                    totalMaxWidth = totalWidth;
                totalWidth = 0; count = 0;
                lastY = rts[i].position.y;
            }
            totalWidth += rts[i].sizeDelta.x;
            count++;
        }
        totalWidth += addNextValue;
        if (addNextValue != 0) count++;
        if (totalWidth != 0) totalWidth += txtmgr.hSpacing * (count - 1);
        if (totalWidth > totalMaxWidth) totalMaxWidth = totalWidth;
        return totalWidth;
    }
}