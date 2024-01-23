using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Title : MonoBehaviour {
    public int phase;
    public int indexChoice = 0;
    private float diff, actualX, actualY;
    private bool initPhase;
    private int choiceLetter;
    private readonly string[] firstPhaseEventNames = { "Continue", "Reset", "ChangeName" };
    private readonly string[] secondPhaseEventNames = { "No", "Yes" };

    public TextManager tmName, TextManagerName, TextManagerLevel, TextManagerTime, TextManagerMap;
    public GameObject Logo, LogoCrate, RetromodeCanvas;
    public SpriteRenderer PressEnterOrZ;

    // Use this for initialization
    private void Start() {
        if (!SaveLoad.started) {
            StaticInits.Start();
            SaveLoad.Start();
            new ControlPanel();
            new PlayerCharacter();
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                Misc.WindowName = GlobalControls.crate ? ControlPanel.instance.WinodwBsaisNmae : ControlPanel.instance.WindowBasisName;
            #endif
            SaveLoad.LoadAlMighty();
            LuaScriptBinder.Set(null, "ModFolder", MoonSharp.Interpreter.DynValue.NewString("@Title"));
            UnitaleUtil.AddKeysToMapCorrespondanceList();
        }
        GameObject firstCamera = GameObject.Find("Main Camera");
        firstCamera.SetActive(false);
        if (GameObject.Find("Main Camera")) Destroy(firstCamera);
        else                                firstCamera.SetActive(true);
        tmName.SetHorizontalSpacing(2);
        tmName.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_DEFAULT_NAME));
        diff = calcTotalLength(tmName);
        actualX = tmName.transform.localPosition.x;
        actualY = tmName.transform.localPosition.y;
        if (GlobalControls.crate) {
            Logo.GetComponent<SpriteRenderer>().enabled = false;
            LogoCrate.GetComponent<SpriteRenderer>().enabled = true;
        }
        DontDestroyOnLoad(Camera.main.gameObject);
        StartCoroutine(TitlePhase1());
    }

    private IEnumerator TitlePhase1() {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetSound("intro_noise"));
        while (Camera.main.GetComponent<AudioSource>().isPlaying)
            yield return 0;
        while (phase == 0) {
            PressEnterOrZ.color = new Color(255, 255, 255, PressEnterOrZ.color.a == 1 ? 0 : 1);
            yield return new WaitForSeconds(1);
        }
    }

    // Update is called once per frame
    private void Update() {
        if (GlobalControls.input.Confirm == ButtonState.PRESSED && phase == 0) {
            phase++;
            Camera.main.GetComponent<AudioSource>().Stop();
            Destroy(RetromodeCanvas);
            StopCoroutine(TitlePhase1());
        } else switch (phase) {
            case 1: {
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
                            TextManagerName.SetTextQueue(new[] { new TextMessage(PlayerCharacter.instance.Name, false, true) });
                            TextManagerLevel.SetTextQueue(new[] { new TextMessage((GlobalControls.crate ? "VL" : "LV") + PlayerCharacter.instance.LV, false, true) });
                            TextManagerTime.SetTextQueue(new[] {new TextMessage(UnitaleUtil.TimeFormatter(SaveLoad.savedGame.playerTime), false, true) });
                            TextManagerMap.SetTextQueue(new[] { new TextMessage(SaveLoad.savedGame.lastScene, false, true) });
                            tmName.SetTextQueue(new[] { new TextMessage(PlayerCharacter.instance.Name, false, true) });
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
                    if (GlobalControls.input.Right == ButtonState.PRESSED || GlobalControls.input.Left == ButtonState.PRESSED)
                        setColor(choiceLetter == 2 ? 2 : (choiceLetter + 1) % 2);
                    if (GlobalControls.input.Up == ButtonState.PRESSED || GlobalControls.input.Down == ButtonState.PRESSED)
                        setColor(choiceLetter == 2 ? 0 : 2);
                    else if (GlobalControls.input.Confirm == ButtonState.PRESSED)
                        switch (choiceLetter) {
                            case 0:
                                phase = -1;
                                StartCoroutine(LoadGame());
                                break;
                            case 1:
                                phase                                                                                    = 2;
                                GameObject.Find(firstPhaseEventNames[choiceLetter]).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                                GameObject.Find("CanvasReset").transform.position                                        = new Vector3(320, 240, -500);
                                setColor(0, 2);
                                break;
                            case 2:
                                SceneManager.LoadScene("EnterName");
                                break;
                        }
                }

                break;
            }
            case 2: {
                if (tmName.transform.localScale.x < 3) {
                    float scale = Mathf.Min(3, tmName.transform.localScale.x + 0.01f);
                    tmName.transform.localScale = new Vector3(scale, scale, 1);
                    tmName.MoveTo(actualX - ((tmName.transform.localScale.x - 1) * diff / 2),
                                  actualY - ((tmName.transform.localScale.x - 1) * diff / 6));
                }
                if (GlobalControls.input.Right == ButtonState.PRESSED || GlobalControls.input.Left == ButtonState.PRESSED)
                    setColor((choiceLetter + 1) % 2, 2);
                else if (GlobalControls.input.Confirm == ButtonState.PRESSED) {
                    if (choiceLetter == 1) {
                        Camera.main.GetComponent<AudioSource>().Stop();
                        Camera.main.GetComponent<AudioSource>().PlayOneShot(AudioClipRegistry.GetSound("intro_holdup"));
                        phase = -1;
                        StartCoroutine(NewGame());
                    } else {
                        phase                                                                                     = 1;
                        GameObject.Find(secondPhaseEventNames[choiceLetter]).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                        GameObject.Find("CanvasReset").transform.position                                         = new Vector3(320,     240,     50);
                        tmName.transform.localPosition                                                            = new Vector3(actualX, actualY, tmName.transform.localPosition.z);
                        tmName.transform.localScale                                                               = new Vector3(1,       1,       1);
                        setColor(0);
                    }
                }

                break;
            }
        }
    }

    private void setColor(int nbr, int mode = 1) {
        string obj = mode == 1 ? firstPhaseEventNames[choiceLetter] : secondPhaseEventNames[choiceLetter];
        GameObject.Find(obj).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        choiceLetter = nbr;
        obj = mode == 1 ? firstPhaseEventNames[choiceLetter] : secondPhaseEventNames[choiceLetter];
        GameObject.Find(obj).GetComponent<SpriteRenderer>().color = new Color(1, 1, 0, 1);
    }

    private IEnumerator LoadGame() {
        DontDestroyOnLoad(gameObject);
        UnitaleUtil.ResetOW();
        SceneManager.LoadScene("TransitionOverworld");
        DiscordControls.StartOW();
        yield return 0;
        //yield return Application.isLoadingLevel;
        //GameObject.Find("Player").transform.position = new Vector3(;
        StaticInits.InitAll(LuaScriptBinder.Get(null, "ModFolder").String, true);
        if (GameObject.Find("Main Camera"))
            Destroy(GameObject.Find("Main Camera"));
        Destroy(gameObject);
    }

    private IEnumerator NewGame() {
        SpriteRenderer blank = GameObject.Find("Blank").GetComponent<SpriteRenderer>();
        while (blank.color.a <= 1) {
            if (tmName.transform.localScale.x < 3) {
                float scale = Mathf.Min(3, tmName.transform.localScale.x + 0.01f);
                tmName.transform.localScale = new Vector3(scale, scale, 1);
                tmName.MoveTo(actualX - ((tmName.transform.localScale.x - 1) * diff / 2),
                              actualY - ((tmName.transform.localScale.x - 1) * diff / 6));
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
        DontDestroyOnLoad(gameObject);
        UnitaleUtil.ResetOW();
        SceneManager.LoadScene("TransitionOverworld");
        DiscordControls.StartOW();
        yield return 0;
        GlobalControls.overworldTimestamp += (SaveLoad.savedGame != null ? SaveLoad.savedGame.playerTime : 0f);
        //yield return Application.isLoadingLevel;
        if (GameObject.Find("Main Camera"))
            Destroy(GameObject.Find("Main Camera"));
        Destroy(gameObject);
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
        return totalWidth;
    }
}