using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// The fairly hacky and somewhat unmaintainable Game Over behaviour class. Written in a hurry as it probably wasn't going to get replaced anytime soon.
/// This script is attached to the Player object to make it persist on scene switch, and immediately switches to the Game Over scene upon attachment.
/// There, the GameOverInit behaviour takes care of calling StartDeath() on this behaviour.
/// </summary>
public class GameOverBehavior : MonoBehaviour {
    private GameObject brokenHeartPrefab;
    private GameObject heartShardPrefab;
    private GameObject utHeart;
    private Transform playerParent;
    public static GameObject battleCamera;
    public static GameObject battleContainer;
    public static GameObject gameOverContainer;
    public static GameObject gameOverContainerOw;
    private GameObject canvasOW;
    private GameObject canvasTwo;
    private string[] heartShardAnim = new string[] { "UI/Battle/heartshard_0", "UI/Battle/heartshard_1", "UI/Battle/heartshard_2", "UI/Battle/heartshard_3" };
    private TextManager gameOverTxt;
    private TextManager reviveText;
    private Image gameOverImage;
    private Image reviveFade;
    private Image reviveFade2;
    private RectTransform[] heartShardInstances = new RectTransform[0];
    private Vector2[] heartShardRelocs;
    private LuaSpriteController[] heartShardCtrl;

    private AudioSource gameOverMusic;

    private float breakHeartAfter = 1.0f;
    private bool  breakHeartReviveAfter = false;
    private float explodeHeartAfter = 2.5f;
    private float gameOverAfter = 4.5f;
    private float fluffybunsAfter = 6.5f;
    private float internalTimer = 0.0f;
    private float internalTimerRevive = 0.0f;
    private float gameOverFadeTimer = 0.0f;
    private bool started = false;
    private bool done = false;
    private bool exiting = false;
    private bool once = false;

    private Vector3 heartPos;
    private Color heartColor;

    //private bool overworld = false;
    private string deathMusic;
    private string[] deathText;

    public int playerIndex = -1;
    public float playerZ = -1;
    public bool autolinebreakstate = false;
    public bool revived = false;
    public bool hasRevived = false;
    public bool reviveTextSet = false;
    public AudioSource musicBefore = null;
    public AudioClip music = null;

    public void ResetGameOver() {
        // Delete instantiated objects
        Destroy(utHeart);
        utHeart = null;
        Destroy(brokenHeartPrefab);
        brokenHeartPrefab = null;
        for (int i = 0; i < heartShardInstances.Length; i++) {
            Destroy(heartShardInstances[i].gameObject);
            heartShardCtrl[i].Remove();
            heartShardCtrl[i] = null;
        }
        if (reviveFade2 != null)
            Destroy(reviveFade2.gameObject);

        if (!UnitaleUtil.IsOverworld) {
            UIController.instance.encounter.gameOverStance = false;
            LuaEnemyEncounter.script.SetVar("autolinebreak", MoonSharp.Interpreter.DynValue.NewBoolean(autolinebreakstate));
        }
        heartShardInstances = new RectTransform[0];
        breakHeartAfter = 1.0f;
        breakHeartReviveAfter = false;
        explodeHeartAfter = 2.5f;
        gameOverAfter = 4.5f;
        gameOverMusic.volume = 1;
        fluffybunsAfter = 6.5f;
        internalTimer = 0.0f;
        internalTimerRevive = 0.0f;
        gameOverFadeTimer = 0.0f;
        gameOverTxt.textQueue = null;
        started = false;
        done = false;
        exiting = false;
        once = false;
        //overworld = false;
        playerIndex = -1;
        playerZ = -1;
        autolinebreakstate = false;
        revived = false;
        reviveTextSet = false;
    }

    public void Revive() { revived = true; }

    public void StartDeath(string[] deathText = null, string deathMusic = null) {
        PlayerOverworld.audioCurrTime = 0;
        if (!UnitaleUtil.IsOverworld) {
            UIController.instance.encounter.EndWave(true);
            autolinebreakstate = LuaEnemyEncounter.script.GetVar("autolinebreak").Boolean;
            LuaEnemyEncounter.script.SetVar("autolinebreak", MoonSharp.Interpreter.DynValue.NewBoolean(true));
            transform.position = new Vector3(transform.position.x - Misc.cameraX, transform.position.y - Misc.cameraY, transform.position.z);
        } else
            autolinebreakstate = true;

        this.deathText = deathText;
        this.deathMusic = deathMusic;

        //Reset the camera's position
        Misc.MoveCameraTo(0, 0);

        playerZ = 130;
        if (UnitaleUtil.IsOverworld) {
            playerParent = transform.parent.parent;
            playerIndex = transform.parent.GetSiblingIndex();
            // transform.parent.SetParent(null);
        } else {
            playerParent = transform.parent;
            playerIndex = transform.GetSiblingIndex();
            transform.SetParent(null);
        }

        if (UnitaleUtil.IsOverworld) {

            /* transform.parent.position = new Vector3(transform.parent.position.x - GameObject.Find("Main Camera OW").transform.position.x - 320,
                                                    transform.parent.position.y - GameObject.Find("Main Camera OW").transform.position.y - 240, transform.parent.position.z); */
            battleCamera = GameObject.Find("Main Camera OW");
            battleCamera.SetActive(false);
            GetComponent<SpriteRenderer>().enabled = true; // stop showing the player
        } else {
            UIController.instance.encounter.gameOverStance = true;
            GetComponent<PlayerController>().invulTimer = 0;
            GetComponent<Image>().enabled = true; // abort the blink animation if it was playing
            battleCamera = GameObject.Find("Main Camera");
            battleCamera.SetActive(false);

            battleContainer = GameObject.Find("Canvas");
            battleContainer.GetComponent<Canvas>().enabled = false;
        }

        // remove all bullets if in retrocompatibility mode
        if (GlobalControls.retroMode) {
            foreach (LuaProjectile p in FindObjectsOfType<LuaProjectile>())
                BulletPool.instance.Requeue(p);
        }

        /*battleContainer = new GameObject("BattleContainer");
        foreach (Transform go in UnitaleUtil.GetFirstChildren(null, false))
            if (go.name != battleContainer.name &&!go.GetComponent<LuaEnemyEncounter>() && go.name != Player.name &&!go.name.Contains("AudioChannel"))
                go.SetParent(battleContainer.transform);
        battleContainer.SetActive(false);*/

        if (UnitaleUtil.IsOverworld)
            gameOverContainerOw.SetActive(true);
        else
            gameOverContainer.SetActive(true);
        ScreenResolution.BoxCameras(Screen.fullScreen);

        Camera.main.GetComponent<AudioSource>().clip = AudioClipRegistry.GetMusic("mus_gameover");
        GameObject.Find("GameOver").GetComponent<Image>().sprite = SpriteRegistry.Get("UI/spr_gameoverbg_0");

        if (UnitaleUtil.IsOverworld) {
            utHeart = Instantiate(GameObject.Find("utHeart"));
            heartColor = utHeart.GetComponent<Image>().color;
            heartColor.a = 1;
        } else {
            heartColor = gameObject.GetComponent<Image>().color;
            gameObject.transform.SetParent(GameObject.Find("Canvas GameOver").transform);
        }

        //if (overworld)
        //    gameObject.transform.SetParent(GameObject.Find("Canvas OW").transform);
        //else
        PlayerCharacter.instance.HP = PlayerCharacter.instance.MaxHP;
        if (UnitaleUtil.IsOverworld)
            gameObject.transform.GetComponent<SpriteRenderer>().enabled = false;// gameObject.transform.parent.SetParent(GameObject.Find("Canvas GameOver").transform);
        else {
            gameObject.transform.SetParent(GameObject.Find("Canvas GameOver").transform);
            UIStats.instance.setHP(PlayerCharacter.instance.MaxHP);
        }
        brokenHeartPrefab = Resources.Load<GameObject>("Prefabs/heart_broken");
        if (SpriteRegistry.GENERIC_SPRITE_PREFAB == null)
            SpriteRegistry.GENERIC_SPRITE_PREFAB = Resources.Load<Image>("Prefabs/generic_sprite");
        heartShardPrefab = SpriteRegistry.GENERIC_SPRITE_PREFAB.gameObject;
        reviveText = GameObject.Find("ReviveText").GetComponent<TextManager>();
        reviveText.SetCaller(LuaEnemyEncounter.script);
        reviveFade = GameObject.Find("ReviveFade").GetComponent<Image>();
        reviveFade.transform.SetAsLastSibling();
        gameOverTxt = GameObject.Find("TextParent").GetComponent<TextManager>();
        gameOverTxt.SetCaller(LuaEnemyEncounter.script);
        gameOverImage = GameObject.Find("GameOver").GetComponent<Image>();
        if (UnitaleUtil.IsOverworld) {
            /*
            heartPos = new Vector3(GetComponent<RectTransform>().position.x - transform.parent.position.x,
                                   GetComponent<RectTransform>().position.y + (GetComponent<RectTransform>().sizeDelta.y / 2) - transform.parent.position.y,
                                   GetComponent<RectTransform>().position.z + 100010);
            */
            heartPos = new Vector3((transform.parent.position.x - GameObject.Find("Canvas OW").transform.position.x) + 320,
                                  ((transform.parent.position.y + (GetComponent<RectTransform>().sizeDelta.y / 2)) - GameObject.Find("Canvas OW").transform.position.y) + 240,
                                   GetComponent<RectTransform>().position.z + 100010);
        } else
            heartPos = gameObject.GetComponent<RectTransform>().position;
        gameOverMusic = Camera.main.GetComponent<AudioSource>();
        started = true;
    }

    void Awake() {

    }

	// Update is called once per frame
	void Update () {
        if (hasRevived && reviveFade2) {
            if (reviveFade2.transform.localPosition != new Vector3(0, 0, 0))
                reviveFade2.transform.localPosition = new Vector3(0, 0, 0);
            if (reviveFade2.color.a > 0.0f)  reviveFade2.color = new Color(1, 1, 1, reviveFade2.color.a - Time.deltaTime / 2);
            else                             GameObject.Destroy(reviveFade2.gameObject);
        }
        if (!started)
            return;
        if (!revived) {
            if (!once && UnitaleUtil.IsOverworld) {
                once = true;
                utHeart.transform.SetParent(GameObject.Find("Canvas GameOver").transform);
                utHeart.transform.position = heartPos;
                utHeart.GetComponent<Image>().color = heartColor;
                canvasOW = GameObject.Find("Canvas OW");
                canvasOW.SetActive(false);
                canvasTwo = GameObject.Find("Canvas Two");
                canvasTwo.SetActive(false);
            } else if (!once) {
                once = true;
                gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(16, 16);
                gameObject.GetComponent<Image>().enabled = true; // abort the blink animation if it was playing
            }

            if (internalTimer > breakHeartAfter) {
                AudioSource.PlayClipAtPoint(AudioClipRegistry.GetSound("heartbeatbreaker"), Camera.main.transform.position, 0.75f);
                brokenHeartPrefab = Instantiate(brokenHeartPrefab);
                if (UnitaleUtil.IsOverworld)
                    brokenHeartPrefab.transform.SetParent(GameObject.Find("Canvas GameOver").transform);
                else
                    brokenHeartPrefab.transform.SetParent(gameObject.transform);
                brokenHeartPrefab.GetComponent<RectTransform>().position = heartPos;
                brokenHeartPrefab.GetComponent<Image>().color = heartColor;
                brokenHeartPrefab.GetComponent<Image>().enabled = true;
                if (UnitaleUtil.IsOverworld)
                    utHeart.GetComponent<Image>().enabled = false;
                else {
                    Color color = gameObject.GetComponent<Image>().color;
                    gameObject.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0);
                    if (LuaEnemyEncounter.script.GetVar("revive").Boolean)
                        Revive();
                }
                breakHeartAfter = 999.0f;
            }

            if (internalTimer > explodeHeartAfter) {
                AudioSource.PlayClipAtPoint(AudioClipRegistry.GetSound("heartsplosion"), Camera.main.transform.position, 0.75f);
                brokenHeartPrefab.GetComponent<Image>().enabled = false;
                heartShardInstances = new RectTransform[6];
                heartShardRelocs = new Vector2[6];
                heartShardCtrl = new LuaSpriteController[6];
                for (int i = 0; i < heartShardInstances.Length; i++) {
                    heartShardInstances[i] = Instantiate(heartShardPrefab).GetComponent<RectTransform>();
                    heartShardCtrl[i] = new LuaSpriteController(heartShardInstances[i].GetComponent<Image>());
                    if (UnitaleUtil.IsOverworld)
                        heartShardInstances[i].transform.SetParent(GameObject.Find("Canvas GameOver").transform);
                    else
                        heartShardInstances[i].transform.SetParent(this.gameObject.transform);
                    heartShardInstances[i].GetComponent<RectTransform>().position = heartPos;
                    heartShardInstances[i].GetComponent<Image>().color = heartColor;
                    heartShardRelocs[i] = UnityEngine.Random.insideUnitCircle * 100.0f;
                    heartShardCtrl[i].Set(heartShardAnim[0]);
                    heartShardCtrl[i].SetAnimation(heartShardAnim, 1 / 5f);
                }
                explodeHeartAfter = 999.0f;
            }

            if (internalTimer > gameOverAfter) {
                AudioClip originMusic = gameOverMusic.clip;
                if (deathMusic != null) {
                    try { gameOverMusic.clip = AudioClipRegistry.GetMusic(deathMusic); }
                    catch { UnitaleUtil.DisplayLuaError("game over screen", "The specified death music doesn't exist. (\"" + deathMusic + "\")"); }
                    if (gameOverMusic.clip == null)
                        gameOverMusic.clip = originMusic;
                }
                gameOverMusic.Play();
                gameOverAfter = 999.0f;
            }

            if (internalTimer > fluffybunsAfter) {
                if (deathText != null) {
                    List<TextMessage> text = new List<TextMessage>();
                    foreach (string str in deathText)
                        text.Add(new TextMessage(str, false, false));
                    TextMessage[] text2 = new TextMessage[text.Count + 1];
                    for (int i = 0; i < text.Count; i++)
                        text2[i] = text[i];
                    text2[text.Count] = new TextMessage("", false, false);
                    if (Random.Range(0, 400) == 44)
                        gameOverTxt.SetTextQueue(new TextMessage[]{
                            new TextMessage("[color:ffffff][voice:v_fluffybuns][waitall:2]4", false, false),
                            new TextMessage("[color:ffffff][voice:v_fluffybuns][waitall:2]" + PlayerCharacter.instance.Name + "!\n[w:15]Stay determined...", false, false),
                            new TextMessage("", false, false) });
                    else
                        gameOverTxt.SetTextQueue(text2);
                } else {
                    //This "4" made us laugh so hard that I kept it :P
                    int fourChance = Random.Range(0, 80);

                    string[] possibleDeathTexts = new string[] { "You cannot give up\njust yet...", "It cannot end\nnow...", "Our fate rests upon\nyou...",
                                                                 "Don't lose hope...", "You're going to\nbe alright!"};
                    if (fourChance == 44)
                        possibleDeathTexts[4] = "4";

                    gameOverTxt.SetTextQueue(new TextMessage[]{
                        new TextMessage("[color:ffffff][voice:v_fluffybuns][waitall:2]" + possibleDeathTexts[Math.RandomRange(0, possibleDeathTexts.Length)], false, false),
                        new TextMessage("[color:ffffff][voice:v_fluffybuns][waitall:2]" + PlayerCharacter.instance.Name + "!\n[w:15]Stay determined...", false, false),
                        new TextMessage("", false, false) });                        }

                fluffybunsAfter = 999.0f;
            }

            if (!done) {
                gameOverImage.color = new Color(1, 1, 1, gameOverFadeTimer);
                if (gameOverAfter >= 999.0f && gameOverFadeTimer < 1.0f) {
                    gameOverFadeTimer += Time.deltaTime / 2;
                    if (gameOverFadeTimer >= 1.0f) {
                        gameOverFadeTimer = 1.0f;
                        done = true;
                    }
                }
                internalTimer += Time.deltaTime; // this is actually dangerous because done can be true before everything's done if timers are modified
            } else if (!exiting &&!gameOverTxt.AllLinesComplete())
                // Note: [noskip] only affects the UI controller's ability to skip, so we have to redo that here.
                if (InputUtil.Pressed(GlobalControls.input.Confirm) && gameOverTxt.LineComplete())
                    gameOverTxt.NextLineText();
        } else {
            if (reviveTextSet && !reviveText.AllLinesComplete()) {
                // Note: [noskip] only affects the UI controller's ability to skip, so we have to redo that here.
                if (InputUtil.Pressed(GlobalControls.input.Confirm) && reviveText.LineComplete())
                    reviveText.NextLineText();
            } else if (reviveTextSet && !exiting) {
                exiting = true;
            } else if (internalTimerRevive >= 5.0f && !reviveTextSet && breakHeartReviveAfter) {
                if (deathText != null) {
                    List<TextMessage> text = new List<TextMessage>();
                    foreach (string str in deathText)
                        text.Add(new TextMessage(str, false, false));
                    TextMessage[] text2 = new TextMessage[text.Count + 1];
                    for (int i = 0; i < text.Count; i++)
                        text2[i] = text[i];
                    text2[text.Count] = new TextMessage("", false, false);
                    reviveText.SetTextQueue(text2);
                }
                reviveTextSet = true;
            } else if (internalTimerRevive > 2.5f && internalTimerRevive < 4.0f) {
                brokenHeartPrefab.transform.localPosition = new Vector2(UnityEngine.Random.Range(-3, 2), UnityEngine.Random.Range(-3, 2));
            } else if (!breakHeartReviveAfter && internalTimerRevive > 2.5f) {
                breakHeartReviveAfter = true;
                AudioSource.PlayClipAtPoint(AudioClipRegistry.GetSound("heartbeatbreaker"), Camera.main.transform.position, 0.75f);
                if (UnitaleUtil.IsOverworld)
                    utHeart.GetComponent<Image>().enabled = true;
                else {
                    Color color = gameObject.GetComponent<Image>().color;
                    gameObject.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 1);
                }
                GameObject.Destroy(brokenHeartPrefab);
            }

            if (!reviveTextSet) internalTimerRevive += Time.deltaTime;

            if (exiting && reviveFade.color.a < 1.0f && reviveFade.isActiveAndEnabled)
                reviveFade.color = new Color(1, 1, 1, reviveFade.color.a + Time.deltaTime / 2);
            else if (exiting) {
                // repurposing the timer as a reset delay
                gameOverFadeTimer -= Time.deltaTime;
                if (gameOverMusic.volume - Time.deltaTime > 0.0f) gameOverMusic.volume -= Time.deltaTime;
                else gameOverMusic.volume = 0.0f;
                if (gameOverFadeTimer < -1.0f) {
                    reviveFade2 = GameObject.Instantiate(reviveFade.gameObject).GetComponent<Image>();
                    reviveFade2.transform.SetParent(playerParent);
                    reviveFade2.transform.SetAsLastSibling();
                    reviveFade2.transform.localPosition = new Vector3(0, 0, 0);
                    reviveFade.color = new Color(1, 1, 1, 0);
                    EndGameOverRevive();
                    if (musicBefore != null) {
                        musicBefore.clip = music;
                        musicBefore.Play();
                    }
                    hasRevived = true;
                }
            }
        }

        for (int i = 0; i < heartShardInstances.Length; i++) {
            heartShardInstances[i].position += (Vector3)heartShardRelocs[i]*Time.deltaTime;
            heartShardRelocs[i].y -= 100f * Time.deltaTime;
        }

        if (gameOverTxt.textQueue != null)
            if (!exiting && gameOverTxt.AllLinesComplete() && gameOverTxt.LineCount() != 0) {
                exiting = true;
                gameOverFadeTimer = 1.0f;
            } else if (exiting && gameOverFadeTimer > 0.0f) {
                gameOverImage.color = new Color(1, 1, 1, gameOverFadeTimer);
                if (gameOverFadeTimer > 0.0f)  {
                    gameOverFadeTimer -= Time.deltaTime / 2;
                    if (gameOverFadeTimer <= 0.0f)
                        gameOverFadeTimer = 0.0f;
                }
            }
            else if (exiting) {
                // repurposing the timer as a reset delay
                gameOverFadeTimer -= Time.deltaTime;
                if (gameOverMusic.volume - Time.deltaTime > 0.0f)
                    gameOverMusic.volume -= Time.deltaTime;
                else
                    gameOverMusic.volume = 0.0f;
                if (gameOverFadeTimer < -1.0f) {
                    //StaticInits.Reset();
                    EndGameOver();
                }
            }
	}

    public void EndGameOver() {
        if (!GlobalControls.modDev)
            SaveLoad.Load(false);
        if (!UnitaleUtil.IsOverworld) {
            UIController.EndBattle(true);
            Destroy(gameObject);
            if (GlobalControls.modDev)
                SceneManager.LoadScene("ModSelect");
            else {
                foreach (string str in NewMusicManager.audioname.Keys)
                    if (str == "StaticKeptAudio") {
                        NewMusicManager.Stop(str);
                        ((AudioSource)NewMusicManager.audiolist[str]).clip = null;
                        ((AudioSource)NewMusicManager.audiolist[str]).time = 0;
                    }
            }
        } else
            EndGameOverRevive();
        if (!GlobalControls.modDev) {
            TPHandler tp = Instantiate(Resources.Load<TPHandler>("Prefabs/TP On-the-fly"));
            tp.sceneName = LuaScriptBinder.Get(null, "PlayerMap").String;

            if (UnitaleUtil.MapCorrespondanceList.ContainsValue(tp.sceneName)) {
                foreach (KeyValuePair<string, string> entry in UnitaleUtil.MapCorrespondanceList) {
                    if (entry.Value == tp.sceneName) {
                        tp.sceneName = entry.Key;
                        break;
                    }
                }
            }

            tp.position = new Vector3((float)LuaScriptBinder.Get(null, "PlayerPosX").Number, (float)LuaScriptBinder.Get(null, "PlayerPosY").Number, LuaScriptBinder.Get(null, "PlayerPosZ") == null ? 0 : (float)LuaScriptBinder.Get(null, "PlayerPosZ").Number);
            tp.direction = 2;
            tp.noFadeIn = true;
            tp.noFadeOut = false;
            GameObject.DontDestroyOnLoad(tp);
            tp.LaunchTPInternal();
        }
    }

    public void EndGameOverRevive() {
        if (!UnitaleUtil.IsOverworld) {
            transform.SetParent(playerParent);
            transform.SetSiblingIndex(playerIndex);
            transform.position = new Vector3(transform.position.x, transform.position.y, playerZ);
        } else {
            transform.parent.SetParent(playerParent);
            transform.parent.SetSiblingIndex(playerIndex);
        }
        battleCamera.SetActive(true);

        if (!UnitaleUtil.IsOverworld)
            battleContainer.GetComponent<Canvas>().enabled = true;

        if (UnitaleUtil.IsOverworld) {
            canvasOW.SetActive(true);
            canvasTwo.SetActive(true);
            PlayerOverworld.instance.enabled = true;
            PlayerOverworld.instance.RestartMusic();
            GetComponent<SpriteRenderer>().enabled = true;
        }
        ResetGameOver();

        if (!UnitaleUtil.IsOverworld) {
            ArenaManager.instance.ResizeImmediate(ArenaManager.UIWidth, ArenaManager.UIHeight);
            UIController.instance.SwitchState(UIController.UIState.ACTIONSELECT);
            gameOverContainer.SetActive(false);
        } else
            gameOverContainerOw.SetActive(false);
    }
}
