using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Title : MonoBehaviour {
    public int phase = 0;
    public int indexChoice = 0;
    float diff, actualX, actualY;
    private bool initPhase = false;
    private int choiceLetter = 0;
    private string[] firstPhaseEventNames = new string[] { "Continue", "Reset", "ChangeName" };
    private string[] secondPhaseEventNames = new string[] { "No", "Yes" };

    public TextManager tmName, TextManagerName, TextManagerLevel, TextManagerTime, TextManagerMap;
    public GameObject Logo, LogoCrate;
    public SpriteRenderer PressEnterOrZ;

    // Use this for initialization
    void Start () {
        if (!SaveLoad.started) {
            StaticInits.Start();
            SaveLoad.Start();
            new ControlPanel();
            new PlayerCharacter();
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                if (GlobalControls.crate) Misc.WindowName = ControlPanel.instance.WinodwBsaisNmae;
                else Misc.WindowName = ControlPanel.instance.WindowBasisName;
            #endif
            SaveLoad.LoadAlMighty();
            LuaScriptBinder.Set(null, "ModFolder", MoonSharp.Interpreter.DynValue.NewString("@Title"));
            UnitaleUtil.AddKeysToMapCorrespondanceList();
        }
        GameObject firstCamera = GameObject.Find("Main Camera");
        firstCamera.SetActive(false);
        if (GameObject.Find("Main Camera"))
            GameObject.Destroy(firstCamera);
        else
            firstCamera.SetActive(true);
        tmName.SetHorizontalSpacing(2);
        tmName.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_DEFAULT_NAME));
        diff = calcTotalLength(tmName);
        actualX = tmName.transform.localPosition.x;
        actualY = tmName.transform.localPosition.y;
        if (GlobalControls.crate) {
            Logo.GetComponent<SpriteRenderer>().enabled = false;
            LogoCrate.GetComponent<SpriteRenderer>().enabled = true;
        }
        GameObject.DontDestroyOnLoad(Camera.main.gameObject);
        StartCoroutine(TitlePhase1());
	}

    IEnumerator TitlePhase1() {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetSound("intro_noise"));
        while (Camera.main.GetComponent<AudioSource>().isPlaying)
            yield return 0;
        while (phase == 0) {
            if (PressEnterOrZ.color.a == 1)
                PressEnterOrZ.color = new Color(255, 255, 255, 0);
            else
                PressEnterOrZ.color = new Color(255, 255, 255, 1);
            yield return new WaitForSeconds(1);
        }
    }
	
	// Update is called once per frame
	void Update () {
        GlobalControls.lastTitle = true;
        if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED && phase == 0) {
            phase++;
            Camera.main.GetComponent<AudioSource>().Stop();
            StopCoroutine(TitlePhase1());
        } else if (phase == 1) {
            if (!initPhase) {
                initPhase = true;

                Camera.main.GetComponent<AudioSource>().clip = AudioClipRegistry.GetMusic("mus_menu");
                Camera.main.GetComponent<AudioSource>().Play();
                try {
                    if (!SaveLoad.Load()) {
                        SceneManager.LoadScene("EnterName");
                    } else {
                        PressEnterOrZ.gameObject.SetActive(false);
                        Logo.SetActive(false);
                        LogoCrate.SetActive(false);
                        GameObject.Find("Back1").SetActive(false);
                        TextManagerName.SetHorizontalSpacing(2);
                        TextManagerLevel.SetHorizontalSpacing(2);
                        TextManagerTime.SetHorizontalSpacing(2);
                        TextManagerMap.SetHorizontalSpacing(2);
                        TextManagerName.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall]" + PlayerCharacter.instance.Name, false, true) });
                        TextManagerLevel.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall]" + (GlobalControls.crate ? "VL" : "LV") +
                                                                                                                                         PlayerCharacter.instance.LV, false, true) });
                        TextManagerTime.SetTextQueue(new TextMessage[] {new TextMessage("[noskipatall]" + UnitaleUtil.TimeFormatter(SaveLoad.savedGame.playerTime), false, true) });
                        TextManagerMap.SetTextQueue(new TextMessage[] { new TextMessage("[noskipatall]" + SaveLoad.savedGame.lastScene, false, true) });
                        tmName.SetTextQueue(new TextMessage[] { new TextMessage(PlayerCharacter.instance.Name, false, true) });
                        diff = calcTotalLength(tmName);
                        tmName.SetEffect(new ShakeEffect(tmName));
                    }
                } catch {
                    GlobalControls.allowWipeSave = true;
                    if (GlobalControls.crate)
                        UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "U USED AN ODL VERSOIN OF CFY? IT ISN'T COMAPTIBEL.\n\n"
                                                                         + "DELEET UR SAVE OT NOT HVAE DA ERRRO AGAIN. HREE: <b>\n"
                                                                         + Application.persistentDataPath + "/save.gd</b>\n\n"
                                                                         + "OR <b>PERS R NWO</b> TO DELEET SAV N CLOSE YCF.\n\n\n"
                                                                         + "IF MOAR PORBLMES, TELL EM! :D\n\n");
                    else
                        UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "Have you saved on a previous or newer version of CYF? Your save isn't compatible with this version.\n\n"
                                                                         + "To fix this, you must delete your save file. It can be found here: \n<b>"
                                                                         + Application.persistentDataPath + "/save.gd</b>\n\n"
                                                                         + "Or, you can <b>Press R now</b> to delete your save and close CYF.\n\n\n"
                                                                         + "Tell me if you have any more problems, and thanks for following my fork! ^^\n\n");
                }
            } else {
                if (GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED || GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED)
                    setColor(choiceLetter == 2 ? 2 : (choiceLetter + 1) % 2);
                if (GlobalControls.input.Up == UndertaleInput.ButtonState.PRESSED || GlobalControls.input.Down == UndertaleInput.ButtonState.PRESSED)
                    setColor(choiceLetter == 2 ? 0 : 2);
                else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED)
                    switch (choiceLetter) {
                        case 0:
                            phase = -1;
                            StartCoroutine(LoadGame());
                            break;
                        case 1:
                            phase = 2;
                            GameObject.Find(firstPhaseEventNames[choiceLetter]).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                            GameObject.Find("CanvasReset").transform.position = new Vector3(320, 240, -500);
                            setColor(0, 2);
                            break;
                        case 2:
                            SceneManager.LoadScene("EnterName");
                            break;
                    }
            }
        } else if (phase == 2) {
            if (tmName.transform.localScale.x < 3) {
                tmName.transform.localScale = new Vector3(tmName.transform.localScale.x + 0.01f, tmName.transform.localScale.y + 0.01f, 1);
                tmName.transform.localPosition = new Vector3(actualX - (((tmName.transform.localScale.x - 1) * diff) / 2),
                                                             actualY - (((tmName.transform.localScale.x - 1) * diff) / 6), tmName.transform.localPosition.z);
            }
            if (GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED || GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED)
                setColor((choiceLetter + 1) % 2, 2);
            else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED) {
                if (choiceLetter == 1) {
                    Camera.main.GetComponent<AudioSource>().Stop();
                    Camera.main.GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetSound("intro_holdup"));
                    phase = -1;
                    StartCoroutine(NewGame());
                } else {
                    phase = 1;
                    GameObject.Find(secondPhaseEventNames[choiceLetter]).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                    GameObject.Find("CanvasReset").transform.position = new Vector3(320, 240, 50);
                    tmName.transform.localPosition = new Vector3(actualX, actualY, tmName.transform.localPosition.z);
                    tmName.transform.localScale = new Vector3(1, 1, 1);
                    setColor(0);
                }
            }
        }
	}

    void setColor(int nbr, int mode = 1) {
        string obj;
        if (mode == 1) obj = firstPhaseEventNames[choiceLetter];
        else           obj = secondPhaseEventNames[choiceLetter];
        GameObject.Find(obj).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        choiceLetter = nbr;
        if (mode == 1) obj = firstPhaseEventNames[choiceLetter];
        else           obj = secondPhaseEventNames[choiceLetter];
        GameObject.Find(obj).GetComponent<SpriteRenderer>().color = new Color(1, 1, 0, 1);
    }

    IEnumerator LoadGame() {
        GameObject.DontDestroyOnLoad(gameObject);
        UnitaleUtil.ResetOW();
        SceneManager.LoadScene("TransitionOverworld");
        yield return 0;
        //yield return Application.isLoadingLevel;
        //GameObject.Find("Player").transform.position = new Vector3(;
        StaticInits.MODFOLDER = LuaScriptBinder.Get(null, "ModFolder").String;
        StaticInits.Initialized = false;
        StaticInits.InitAll(true);
        if (GameObject.Find("Main Camera"))
            GameObject.Destroy(GameObject.Find("Main Camera"));
        GameObject.Destroy(gameObject);
    }

    IEnumerator NewGame() {
        SpriteRenderer blank = GameObject.Find("Blank").GetComponent<SpriteRenderer>();
        while (blank.color.a <= 1) {
            if (tmName.transform.localScale.x < 3) {
                tmName.transform.localScale = new Vector3(tmName.transform.localScale.x + 0.01f, tmName.transform.localScale.y + 0.01f, 1);
                tmName.transform.localPosition = new Vector3(actualX - (((tmName.transform.localScale.x - 1) * diff) / 2),
                                                             actualY - (((tmName.transform.localScale.x - 1) * diff) / 6), tmName.transform.localPosition.z);
            }
            blank.color = new Color(blank.color.r, blank.color.g, blank.color.b, blank.color.a + 0.003f);
            yield return 0;
        }
        while (Camera.main.GetComponent<AudioSource>().isPlaying)
            yield return 0;
        PlayerCharacter.instance.Reset(false);
        LuaScriptBinder.ClearVariables();
        GlobalControls.GameMapData.Clear();
        Inventory.inventory.Clear();
        GameObject.DontDestroyOnLoad(gameObject);
        UnitaleUtil.ResetOW();
        SceneManager.LoadScene("TransitionOverworld");
        yield return 0;
        GlobalControls.overworldTimestamp += (SaveLoad.savedGame != null ? SaveLoad.savedGame.playerTime : 0f);
        //yield return Application.isLoadingLevel;
        if (GameObject.Find("Main Camera"))
            GameObject.Destroy(GameObject.Find("Main Camera"));
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