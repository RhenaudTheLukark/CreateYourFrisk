using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;

public class PlayerOverworld : MonoBehaviour {
    public static PlayerOverworld instance;
    public int BlockingLayer;               //Layer on which collision will be checked
    public int EventLayer;                  //Layer of the events, colliding too
    public int forcedMove = 0;              //Direction of a forced move
    //public int rolled = 0;
    public float speed;
    public static float audioCurrTime = 0;
    public bool firstTime = false;          //Boolean used to not launch another event a the end of the previous event
    public bool inBattleAnim = false;
    public bool PlayerNoMove {              //Is the player not able to move?
        get { return _playerNoMove || forceNoAction || inBattleAnim; }
        set { _playerNoMove = value; isMoving = false; }
    }
    public bool forceNoAction = false;
    public bool[] menuRunning = new bool[] { false, false, false, false, false };
    public Vector2 lastMove;                //The Player's last input
    public Vector2 cameraShift = new Vector2();
    public Vector2 backgroundSize = new Vector3(640, 480);
    public Transform PlayerPos;             //The Transform component attached to this object
    public Image utHeart;
    public static AudioSource audioKept;
    public LuaSpriteController sprctrl;
    public TextManager textmgr;             //The map's text manager
    public List<Transform> parallaxes = new List<Transform>();

    public bool isMoving          = false;
    public bool isMovingWaitEnd   = false;
    public ScriptWrapper isMovingSource;
    public bool isBeingMoved      = false;
    public ScriptWrapper isRotatingSource;
    public bool isRotatingWaitEnd = false;

    private bool _playerNoMove = false;
    private int battleWalkCount;            //Will be used to check the battle appearance
    private float TimeIndicator = 0f;       //A time indicator used for the soul's movement during the pre-Encounter anim
    private Rigidbody2D rb2D;               //The Rigidbody2D component attached to this object
    private AudioSource uiAudio;            //AudioSource used for the pre-Encounter sounds
    private CYFAnimator animator;

    //private bool lockedCamera = false;    //Used to stop the camera's position refresh

    public int UIPos = 0; // 0: Auto-decide UI position; 1: Bottom; 2: Top

    //Start overrides the Start function of MovingObject
    public void Start() {
        instance = this;

        GameObject Player = GameObject.Find("Player");
        uiAudio = Player.GetComponent<AudioSource>();
        utHeart = GameObject.Find("utHeart").GetComponent<Image>();
        utHeart.color = new Color(utHeart.color.r, utHeart.color.g, utHeart.color.b, 0);

        //StartCoroutine(LaunchMusic());

        //Get a component reference to the Player's transform
        PlayerPos = Player.transform.parent;

        //If the player's position already exists, move the player to it
        if (LuaScriptBinder.Get(null, "PlayerPosX") != null && LuaScriptBinder.Get(null, "PlayerPosY") != null && LuaScriptBinder.Get(null, "PlayerPosZ") != null) {
            Vector2 temp = new Vector3((float)LuaScriptBinder.Get(null, "PlayerPosX").Number, (float)LuaScriptBinder.Get(null, "PlayerPosY").Number, (float)LuaScriptBinder.Get(null, "PlayerPosZ").Number);
            PlayerPos.position = temp;
        } else
            PlayerPos.position = Vector3.zero;

        //Get a component reference to the Player's animator component
        //animator = Player.GetComponent<Animator>();
        sprctrl = new LuaSpriteController(Player.GetComponent<SpriteRenderer>()) {
            loopmode = "LOOP"
        };
        animator = Player.GetComponent<CYFAnimator>();

        //Get a component reference to this object's Rigidbody2D
        rb2D = Player.GetComponent<Rigidbody2D>();

        //Get the layer that blocks our object, here BlockingLayer and EventLayer
        BlockingLayer = LayerMask.GetMask("BlockingLayer");
        EventLayer = LayerMask.GetMask("EventLayer");

        textmgr = GameObject.Find("TextManager OW").GetComponent<TextManager>();

        //How many times the player has to move before encounter an enemy
        battleWalkCount = Math.RandomRange(300, 1000);

        //Let's set the last movement to Down.
        lastMove = new Vector2(0, 1);

        EventManager.instance = GameObject.Find("Main Camera OW").GetComponent<EventManager>();

        if (!NewMusicManager.Exists("StaticKeptAudio")) {
            audioKept = NewMusicManager.CreateChannelAndGetAudioSource("StaticKeptAudio");
            audioKept.loop = true;
            GameObject.DontDestroyOnLoad(audioKept);
        }
    }

    IEnumerator OnEnable2() {
        yield return 0;

        if (NewMusicManager.audiolist.ContainsKey("src"))
            NewMusicManager.audiolist.Remove("src");
        if (NewMusicManager.audiolist.ContainsKey("StaticKeptAudio"))
            NewMusicManager.audiolist.Remove("StaticKeptAudio");

        MusicManager.src = Camera.main.GetComponent<AudioSource>();
        NewMusicManager.audiolist.Add("src", MusicManager.src);
        //NewMusicManager.audioname.Add("src", MusicManager.filename);
        if (audioKept)
            NewMusicManager.audiolist.Add("StaticKeptAudio", audioKept);
        //NewMusicManager.audioname.Add("StaticKeptAudio", "Sorry, nyi");

        PlayerPos = GameObject.Find("Player").transform;
        Color color = PlayerPos.GetComponent<SpriteRenderer>().color;
        GameObject.Find("PlayerEncounter").GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0);
        GameObject.Find("EncounterBubble").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        PlayerPos.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 1);

        GameObject.Find("black").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        Image utHeart = GameObject.Find("utHeart").GetComponent<Image>();
        utHeart.color = new Color(utHeart.color.r, utHeart.color.g, utHeart.color.b, 0);

        StartCoroutine(TextCoroutine());
    }

    public void RestartMusic() {
        MapInfos mi = GameObject.Find("Background").GetComponent<MapInfos>();
        AudioSource audio = UnitaleUtil.GetCurrentOverworldAudio();

        if (audio == audioKept) {
            GameObject.Find("Main Camera OW").GetComponent<AudioSource>().Stop();
            GameObject.Find("Main Camera OW").GetComponent<AudioSource>().clip = null;
            GameObject.Find("Main Camera OW").GetComponent<AudioSource>().time = 0;
        } else {
            audioKept.Stop();
            audioKept.clip = null;
            audioKept.time = 0;
        }
        if (audio.name.Contains("Camera"))
            audio = GameObject.Find("Main Camera OW").GetComponent<AudioSource>();

        if (audio.clip == null) {
            if (mi.music != "none") {
                audio.clip = AudioClipRegistry.GetMusic(mi.music);
                audio.Play();
            } else
                audio.Stop();
        } else {
            //Get the file's name with this...thing?
            string test = audio.clip.name.Replace('\\', '/').Split(new string[] { "/Audio/" }, System.StringSplitOptions.RemoveEmptyEntries)[1].Split('.')[0];
            if (test != mi.music) {
                if (mi.music != "none") {
                    audio.Stop();
                    audio.clip = AudioClipRegistry.GetMusic(mi.music);
                    audio.Play();
                } else
                    audio.Stop();
            } else if (!audio.isPlaying && audio != audioKept) {
                audio.time = audioCurrTime;
                audioCurrTime = 0;
                if (audio.time == 0)
                    audio.Play();
                else
                    audio.UnPause();
            }
        }
    }

    void OnEnable() {
        SceneManager.sceneLoaded += LoadScene;
        Fading.FinishFade += FinishFade;

        ControlPanel.instance.FrameBasedMovement = false;
        try {
            EventManager.instance.ScriptLaunched = false;
            EventManager.instance.script = null;
        } catch { }

        if (GlobalControls.realName != null)
            PlayerCharacter.instance.Name = GlobalControls.realName;

        TimeIndicator = 0;
        inBattleAnim = false;

        StartCoroutine(OnEnable2());
    }


    void OnDisable() {
        SceneManager.sceneLoaded -= LoadScene;
        Fading.FinishFade -= FinishFade;
    }

    private void LoadScene(Scene scene, LoadSceneMode mode) { PlayerNoMove = true; } //Scene loading
    private void FinishFade() { PlayerNoMove = false; } // Scene loaded

    private void NextText() {
        if (!textmgr.AllLinesComplete() && (textmgr.CanAutoSkipAll() || textmgr.LineComplete()))
            textmgr.NextLineText();
        else if ((textmgr.AllLinesComplete() || textmgr.CanAutoSkipAll()) && textmgr.LineCount() != 0) {
            EventManager.instance.passPressOnce = true;
            textmgr.transform.parent.parent.SetAsFirstSibling();
            textmgr.SetTextQueue(null);
            textmgr.DestroyChars();
            textmgr.SetHorizontalSpacing(textmgr.Charset.CharSpacing);
            textmgr.SetVerticalSpacing(0);
            textmgr.SetTextFrameAlpha(0);
            if (EventManager.instance.script != null)
                EventManager.instance.script.Call("CYFEventNextCommand");
            else
                PlayerNoMove = false; //End text no event
        }
    }

    IEnumerator TextCoroutine() {
        while (true) {
            yield return 0;
            if (GameObject.Find("textframe_border_outer"))
                if (GameObject.Find("textframe_border_outer").GetComponent<Image>().color.a != 0) {
                    try {
                        if (textmgr.CanAutoSkipAll())
                            NextText();
                        if (GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED && !textmgr.blockSkip && !textmgr.LineComplete() && textmgr.CanSkip()) {
                            if (EventManager.instance.script != null && EventManager.instance.script.GetVar("playerskipdocommand").Boolean)
                                textmgr.DoSkipFromPlayer();
                            else
                                textmgr.SkipLine();
                        } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED && !textmgr.blockSkip && !EventManager.instance.passPressOnce)
                            NextText();
                    } catch { }
                }
        }
    }

    private void Update() {
        if (GameOverBehavior.gameOverContainerOw.activeSelf)
            return;

        //Used to increment TimeIndicator for our pre-Encounter anim
        if (TimeIndicator > 0 && TimeIndicator < 1) {
            TimeIndicator += Time.deltaTime;

            if (TimeIndicator > 1)
                TimeIndicator = 1;

            Image utHeart = GameObject.Find("utHeart").GetComponent<Image>();
            Image playerMask = GameObject.Find("PlayerEncounter").GetComponent<Image>();
            Vector2 positionCamera = Camera.main.transform.position;
            Vector2 end = new Vector2(PlayerPos.position.x - (positionCamera.x - 320 + 48), PlayerPos.position.y + (playerMask.sprite.texture.height / 2.5f) - (positionCamera.y - 240 + 25));

            //Here we move the heart to the place it'll be on the beginning of the battle
            if ((Vector2)utHeart.transform.position != end)
                utHeart.transform.position = new Vector3(PlayerPos.position.x - (end.x * TimeIndicator), (PlayerPos.position.y + (playerMask.sprite.texture.height / 2.5f)) - (end.y * TimeIndicator), 0);
        }

        int horizontal = 0;     //Used to store the horizontal move direction
        int vertical = 0;       //Used to store the vertical move direction
        int currentDirection = 0;
        //If you locked the player, do nothing
        if (!PlayerNoMove) {
            horizontal = (int)(Input.GetAxisRaw("Horizontal"));
            vertical = (int)(Input.GetAxisRaw("Vertical"));
            //Just some animations switches
            if (animator.movementDirection == 0) {
                if (GlobalControls.input.Up > 0)         currentDirection = 8;
                else if (GlobalControls.input.Down > 0)  currentDirection = 2;
                else if (GlobalControls.input.Right > 0) currentDirection = 6;
                else if (GlobalControls.input.Left > 0)  currentDirection = 4;
            }
            if (GlobalControls.input.Up == UndertaleInput.ButtonState.PRESSED)         currentDirection = 8;
            else if (GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED) currentDirection = 6;
            else if (GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED)  currentDirection = 4;
            else if (GlobalControls.input.Down == UndertaleInput.ButtonState.PRESSED)  currentDirection = 2;
            if ((animator.beginAnim.Contains("Up") && GlobalControls.input.Up <= 0) ||  (animator.beginAnim.Contains("Right") && GlobalControls.input.Right <= 0) ||
                (animator.beginAnim.Contains("Left") && GlobalControls.input.Left <= 0) ||  (animator.beginAnim.Contains("Down") && GlobalControls.input.Down <= 0)) {
                if (horizontal < 0)      currentDirection = 4;
                else if (horizontal > 0) currentDirection = 6;
                else if (vertical > 0)   currentDirection = 8;
                else if (vertical < 0)   currentDirection = 2;
            }
        }

        if (currentDirection != 0) animator.movementDirection = currentDirection;

        if (!isBeingMoved)
            isMoving = AttemptMove(horizontal, vertical);

        if (GlobalControls.input.Menu == UndertaleInput.ButtonState.PRESSED)
            if (menuRunning[2] && !menuRunning[3] && !menuRunning[4])
                CloseMenu(true);
        menuRunning[4] = false;
    }

    //Moves the object
    public void Move(float xDir, float yDir, GameObject go) {
        Transform transform = go.transform;
        if (transform.parent != null)
            if (transform.parent.name == "SpritePivot")
                transform = transform.parent;

        //Don't calculate anything if no movement
        if (xDir != 0 || yDir != 0) {
            //Creates the movement of our object
            Vector2 end = new Vector2(xDir, yDir);
            if (go == rb2D.gameObject) end *= speed;
            else                       end *= go.GetComponent<EventOW>().moveSpeed;

            transform.position += (Vector3)end;
        }

        //If the GameObject is the player, check if the camera can follow him or not
        if (go == gameObject && !inBattleAnim && GameObject.Find("Background") != null) {
            RectifyCameraPosition(new Vector2(transform.GetChild(0).position.x, transform.GetChild(0).position.y + Mathf.Round(PlayerPos.GetChild(0).GetComponent<SpriteRenderer>().sprite.texture.height / 2f)));
            GameObject.Find("Canvas OW").transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -10);
        }

        //Decrease battleWalkCount if the player is moving freely
        if ((xDir != 0 || yDir != 0) && !PlayerNoMove && EventManager.instance.script == null && go == gameObject)
            battleWalkCount--;
    }

    //testMove returns true if it is able to move and false if not.
    //testMove takes parameters for x direction, y direction and a RaycastHit2D to check collision
    public bool TestMove(float xDir, float yDir, out RaycastHit2D hit, GameObject go) {
        Transform transform = go.transform;
        BoxCollider2D boxCollider = go.GetComponent<BoxCollider2D>();
        if (!boxCollider) {
            hit = new RaycastHit2D();
            return true;
        }

        //Store start position to move from, based on objects current transform position
        Vector2 start = new Vector2(transform.position.x + transform.localScale.x * boxCollider.offset.x,
                                    transform.position.y + transform.localScale.x * boxCollider.offset.y);

        //Calculate end position based on the direction parameters passed in when calling Move and using our boxCollider
        Vector2 dir = new Vector2(xDir, yDir);

        //Calculate the current size of the object's boxCollider
        Vector2 size = new Vector2(boxCollider.size.x * transform.localScale.x, boxCollider.size.y * transform.localScale.y);

        //Disable the boxCollider so that linecast doesn't hit this object's own collider
        boxCollider.enabled = false;

        float speed = go == rb2D.gameObject ? this.speed : go.GetComponent<EventOW>().moveSpeed;
        //Cast a line from start point to end point checking collision on blockingLayer and then EventLayer
        hit = Physics2D.BoxCast(start, size, 0, dir, Mathf.Sqrt(Mathf.Pow(xDir * speed, 2) + Mathf.Pow(yDir * speed, 2)), BlockingLayer);
        if (hit.transform == null)
            hit = Physics2D.BoxCast(start, size, 0, dir, Mathf.Sqrt(Mathf.Pow(xDir * speed, 2) + Mathf.Pow(yDir * speed, 2)), EventLayer);

        //Re-enable boxCollider after BoxCast
        boxCollider.enabled = true;

        //Check if anything was hit
        if (hit.transform == null)
            return true;
        //If something was hit, return false, Move was unsuccesful
        else
            return false;
    }

    public bool AttemptMove(float xDir, float yDir, GameObject go = null, bool wallPass = false) {
        if (go == null)
            go = gameObject;

        //If there's an input, register the last input
        if (!(xDir == 0 && yDir == 0) && go == gameObject)
            lastMove = new Vector2(xDir, yDir);

        bool canMove2 = false;

        //Check if nothing was hit by BoxCast
        //If nothing was hit, move normally
        if (wallPass || go.layer == 0 || (xDir == 0 && yDir == 0)) {
            canMove2 = !(xDir == 0 && yDir == 0);
            Move(xDir, yDir, go);
        } else {
            //Hit will store whatever our linecast hits when Move is called
            RaycastHit2D hit;

            //Set canMove to true if Move was successful, false if failed
            bool canMove = TestMove(xDir, yDir, out hit, go);
            canMove2 = canMove;

            if (hit.transform == null) Move(xDir, yDir, go);
            //If we can go on sides
            else if (TestMove(xDir, 0, out hit, go)) Move(xDir, 0, go);
            //if we can go up or down
            else if (TestMove(0, yDir, out hit, go)) Move(0, yDir, go);
            else Move(0, 0, go);
        }

        //If we moved enough to set battleWalkCount to 0...
        if (battleWalkCount == 0)
            if (!GameObject.Find("Background").GetComponent<MapInfos>().noRandomEncounter) {
                battleWalkCount = -1;

                //...let's set an encounter!
                SetEncounterAnim();
            } else
                battleWalkCount = Math.RandomRange(300, 1000);

        return canMove2;
    }

    /// <summary>
    /// The encounter anim, that ends to a battle
    /// </summary>
    /// <param name="encounterName">The name of the encounter. If not set, the encounter will be random.</param>
    public void SetEncounterAnim(string encounterName = "", string anim = "normal", bool ForceNoFlee = false) { StartCoroutine(AnimationBeforeBattle(encounterName, anim, ForceNoFlee)); }

    //The function that creates the animation before the encounter
    IEnumerator AnimationBeforeBattle(string encounterName = "", string anim = "normal", bool ForceNoFlee = false) {
        bool quickAnim = (anim ==    "fast");
        bool instant   = (anim == "instant");

        PlayerNoMove = true; //Begin encounter
        inBattleAnim = true;
        //Here are the player's soul and a black sprite, we'll need to make them go up
        Image utHeart = GameObject.Find("utHeart").GetComponent<Image>();
        Image playerMask = GameObject.Find("PlayerEncounter").GetComponent<Image>();
        Image blackFont = GameObject.Find("black").GetComponent<Image>();

        Vector2 positionCamera, end;
        playerMask.GetComponent<Image>().sprite = PlayerPos.GetComponent<SpriteRenderer>().sprite;
        audioCurrTime = MusicManager.src.time;
        Camera.main.GetComponent<AudioSource>().Stop();

        blackFont.transform.SetAsLastSibling();
        playerMask.transform.SetAsLastSibling();
        utHeart.transform.SetAsLastSibling();

        //If you want a quick animation, we just keep the end of the anim
        if (!quickAnim && !instant) {
            uiAudio.PlayOneShot(AudioClipRegistry.GetSound("BeginBattle1"));

            //Shows the encounter bubble, the "!" on the player
            SpriteRenderer EncounterBubble = GameObject.Find("EncounterBubble").GetComponent<SpriteRenderer>();
            EncounterBubble.color = new Color(EncounterBubble.color.r, EncounterBubble.color.g, EncounterBubble.color.b, 1f);
            EncounterBubble.transform.position = new Vector3(EncounterBubble.transform.position.x, PlayerPos.position.y + playerMask.sprite.texture.height + 6);

            yield return new WaitForSeconds(0.5f);
        }
        //Set the heart's position to the player's position
        utHeart.transform.position = new Vector3(PlayerPos.position.x, PlayerPos.position.y + (playerMask.sprite.texture.height / 2.5f), -5100);
        positionCamera = Camera.main.transform.position;
        end = new Vector2(PlayerPos.position.x - (positionCamera.x - 320 + 48), PlayerPos.position.y + PlayerPos.GetComponent<RectTransform>().sizeDelta.y / 2 - (positionCamera.y - 240 + 25));
        blackFont.transform.position = new Vector3(positionCamera.x, positionCamera.y, blackFont.transform.position.z);
        blackFont.color = new Color(blackFont.color.r, blackFont.color.g, blackFont.color.b, 1f);

        Vector2 nativeSizeDelta = new Vector2(PlayerPos.GetComponent<SpriteRenderer>().sprite.texture.width, PlayerPos.GetComponent<SpriteRenderer>().sprite.texture.height);
        playerMask.transform.position = new Vector3(PlayerPos.position.x, PlayerPos.position.y, -5040);
        playerMask.sprite = PlayerPos.GetComponent<SpriteRenderer>().sprite;
        playerMask.rectTransform.sizeDelta = new Vector2(nativeSizeDelta.x * Mathf.Abs(sprctrl.xscale), nativeSizeDelta.y * Mathf.Abs(sprctrl.yscale)) / 100;
        //playerMask.transform.localScale = new Vector3(PlayerPos.lossyScale.x / playerMask.transform.lossyScale.x, PlayerPos.lossyScale.y / playerMask.transform.lossyScale.y, 1);

        Color color = PlayerPos.GetComponent<SpriteRenderer>().color;
        PlayerPos.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 0);
        playerMask.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 1);

        if (!instant){
            for (int i = 0; i < 2; i++) {
                utHeart.color = new Color(utHeart.color.r, utHeart.color.g, utHeart.color.b, 1f);
                uiAudio.PlayOneShot(AudioClipRegistry.GetSound("BeginBattle2"));
                yield return new WaitForSeconds(0.075f);

                utHeart.color = new Color(utHeart.color.r, utHeart.color.g, utHeart.color.b, 0f);
                yield return new WaitForSeconds(0.075f);
            }
        }

        playerMask.color = new Color(color.r, color.g, color.b, 0);
        utHeart.color = new Color(utHeart.color.r, utHeart.color.g, utHeart.color.b, 1f);
        blackFont.color = new Color(blackFont.color.r, blackFont.color.g, blackFont.color.b, 1f);

        //-----------------------------------------------

        uiAudio.PlayOneShot(AudioClipRegistry.GetSound("BeginBattle3"));

        TimeIndicator += Time.deltaTime;

        if (TimeIndicator > 1)
            TimeIndicator = 1;

        Vector3 finalPosition = new Vector3(positionCamera.x - 320 + 48, positionCamera.y - 240 + 25, -5100f);
        //Here we move the heart to the place it'll be on the beginning of the battle
        if (utHeart.transform.position != finalPosition)
            utHeart.transform.position = new Vector3(PlayerPos.position.x - (end.x * TimeIndicator), (PlayerPos.position.y + (playerMask.sprite.texture.height / 2.5f)) - (end.y * TimeIndicator), 0);

        if (!instant)
            yield return new WaitForSeconds(1f);

        //Set the heart's position
        utHeart.transform.position = finalPosition;

        if (instant) {
            color = PlayerPos.GetComponent<SpriteRenderer>().color;
            GameObject.Find("PlayerEncounter").GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0);
            GameObject.Find("EncounterBubble").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
            PlayerPos.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 1);

            GameObject.Find("black").GetComponent<Image>().color = new Color(0, 0, 0, 0);
            utHeart.color = new Color(utHeart.color.r, utHeart.color.g, utHeart.color.b, 0);
        }

        //Launch the battle
        StartCoroutine(SetEncounter(encounterName, ForceNoFlee));
        yield return 0;
    }

    //The function that is used to launch a battle
    private IEnumerator SetEncounter(string encounterName = "", bool ForceNoFlee = false, bool instant = false) {
        //Saves our last map and the position of our player, before the battle
        string mapName;
        if (UnitaleUtil.MapCorrespondanceList.ContainsKey(SceneManager.GetActiveScene().name)) mapName = UnitaleUtil.MapCorrespondanceList[SceneManager.GetActiveScene().name];
        else mapName = SceneManager.GetActiveScene().name;
        LuaScriptBinder.Set(null, "PlayerMap", DynValue.NewString(mapName));
        Transform tf = rb2D.transform;
        LuaScriptBinder.Set(null, "PlayerPosX", DynValue.NewNumber(tf.position.x));
        LuaScriptBinder.Set(null, "PlayerPosY", DynValue.NewNumber(tf.position.y));
        LuaScriptBinder.Set(null, "PlayerPosZ", DynValue.NewNumber(tf.position.z));

        //Sets the mod's folder and the encounter file's name to know what file we have to load
        string ModFolder = StaticInits.MODFOLDER, Encounter;
        LuaScriptBinder.Set(null, "ModFolder", DynValue.NewString(ModFolder));
        if (ForceNoFlee)
            LuaScriptBinder.Set(null, "ForceNoFlee", DynValue.NewBoolean(true));

        DirectoryInfo di = new DirectoryInfo(Path.Combine(FileLoader.DataRoot, "Mods/" + StaticInits.MODFOLDER + "/Lua/Encounters"));
        FileInfo[] encounterFiles = di.GetFiles();

        if (encounterName == null || encounterName == "") {
            ArrayList encounterNames = new ArrayList();
            foreach (FileInfo encounterFile in encounterFiles) {
                if (!encounterFile.Name.EndsWith(".lua") || encounterFile.Name[0] == '#')
                    continue;
                encounterNames.Add(Path.GetFileNameWithoutExtension(encounterFile.Name));
            }
            if (encounterNames.Count == 0) {
                UnitaleUtil.DisplayLuaError("Overworld System", "There's no valid encounter to launch.\nYou need to have at least 1 encounter in your mod that doesn't have a '#' as its first character!");
                yield break;
            } else {
                if (encounterNames.Count == 1)
                    Encounter = Path.GetFileNameWithoutExtension(encounterNames[0].ToString());
                else
                    Encounter = Path.GetFileNameWithoutExtension(encounterNames[Math.RandomRange(0, encounterNames.Count)].ToString());
            }
        } else
            Encounter = Path.GetFileNameWithoutExtension(encounterName);

        //Let's set the folder and file we want to load.
        StaticInits.MODFOLDER = ModFolder;
        StaticInits.ENCOUNTER = Encounter;

        //We save the state of the events.
        EventManager.instance.SetEventStates(true);

        LuaScriptBinder.ClearBattleVar();

        if (!instant)
            yield return new WaitForEndOfFrame();
        GameObject.FindObjectOfType<Fading>().FadeInstant(1);

        //Reset how many times the player has to move before encounter an enemy
        battleWalkCount = Math.RandomRange(300, 1000);

        //GameObject.Find("Main Camera OW").tag = "Untagged";
        HideOverworld("Battle");

        //Now, we load our battle.
        GlobalControls.isInFight = true;
        SceneManager.LoadScene("Battle", LoadSceneMode.Additive);
        yield return 0;
    }

    /// <summary>
    /// Permits to set a dialogue (old method).
    /// </summary>
    /// <param name="textTable">The text that'll be printed</param>
    /// <param name="rearranged">Will the text be rearranged ? (\r replacements etc)</param>
    /// <param name="mugshots">The mugshots' name that'll be used in the dialogue</param>
    public void SetDialog(string[] textTable, bool rearranged, DynValue mugshots = null) {
        if (textTable[0] == string.Empty) {
            UnitaleUtil.WriteInLogAndDebugger("Old SetDialog: There is no text to print!");
            return;
        }

        TextMessage[] textmsg = new TextMessage[textTable.Length];

        if (mugshots != null)
            for (int i = 0; i < textTable.Length; i++)
                textmsg[i] = new TextMessage(textTable[i], rearranged, false, mugshots);
        else
            for (int i = 0; i < textTable.Length; i++)
                textmsg[i] = new TextMessage(textTable[i], rearranged, false);
        PlayerNoMove = true; //Old SetDialog
        EventManager.instance.passPressOnce = true;

        textmgr.SetTextFrameAlpha(1);
        textmgr.blockSkip = false;

        //textmgr.setTextQueue(textmsg, mugshots);
        textmgr.SetTextQueue(textmsg);
        textmgr.transform.parent.parent.SetAsLastSibling();
    }

    /// <summary>
    /// Rectifies the position of the camera, to be sure that the background will always be on tha camera.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public void RectifyCameraPosition(Vector3 pos) {
        if (pos.x < 320) pos.x = 320;
        else if (pos.x > backgroundSize.x - 320) pos.x = Mathf.RoundToInt(backgroundSize.x - 320);
        if (pos.y < 240) pos.y = 240;
        else if (pos.y > backgroundSize.y - 240) pos.y = Mathf.RoundToInt(backgroundSize.y - 240);

        pos += (Vector3)cameraShift;
        pos.z = -10000;
        Camera.main.transform.position = pos;

        foreach (Transform t in parallaxes) {
            if (!t)
                continue;
            Vector3 dimPlx = t.GetComponent<RectTransform>().sizeDelta * t.localScale.x;
            t.position = new Vector3(Mathf.Round(dimPlx.x) > 640 ? (dimPlx.x / 2 + (backgroundSize.x - dimPlx.x) * ((pos.x - 320) / (backgroundSize.x - 640))) : t.position.x,
                                     Mathf.Round(dimPlx.y) > 480 ? (dimPlx.y / 2 + (backgroundSize.y - dimPlx.y) * ((pos.y - 240) / (backgroundSize.y - 480))) : t.position.y, t.position.z);
        }
    }

    public static void AutoSetUIPos() {
        if (instance.UIPos == 0) {
            float cameraOffset = (GameObject.Find("Main Camera OW").GetComponent<RectTransform>().position.y - 240);
            float playerPos = Mathf.Ceil(instance.gameObject.GetComponent<RectTransform>().position.y - cameraOffset);

            SetUIPos(playerPos < 230);
        } else
            SetUIPos(instance.UIPos == 2 ? true : false);
    }

    public static void SetUIPos(bool top = false) {
        instance.UIPos = top ? 2 : 1;
        float cameraOffset = (GameObject.Find("Main Camera OW").GetComponent<RectTransform>().position.y - 240);

        RectTransform textframe = GameObject.Find("textframe_border_outer").GetComponent<RectTransform>();
        RectTransform menustat = GameObject.Find("menustat_border_outer").GetComponent<RectTransform>();

        // Inverted position
        if (top) {
            // Text box
            textframe.position = new Vector3(textframe.position.x, 318 + cameraOffset, textframe.position.z);
            // Stat box
            menustat.localPosition = new Vector3(menustat.localPosition.x, -192, menustat.localPosition.z);
        // Normal position
        } else {
            // Text box
            textframe.position = new Vector3(textframe.position.x, 8 + cameraOffset, textframe.position.z);
            // Stat box
            menustat.localPosition = new Vector3(menustat.localPosition.x, 78, menustat.localPosition.z);
        }
    }

    public static IEnumerator LaunchMenu() {
        instance.PlayerNoMove = true; instance.menuRunning[2] = true; instance.menuRunning[3] = false; instance.menuRunning[4] = true; //Start menu
        GameObject.Find("MenuContainer").transform.SetAsLastSibling();
        TextManager[] txtmgrs = GameObject.Find("MenuContainer").GetComponentsInChildren<TextManager>();
        instance.uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menumove"));
        /* 0-6   : Menu
           7-17  : Item
           18-27 : Stat */
        foreach (TextManager txt in txtmgrs)
            txt.SetHorizontalSpacing(2);

        instance.UIPos = 0;
        AutoSetUIPos();

        GameObject.Find("TextManager OW").GetComponent<TextManager>().SetText(new TextMessage("[noskipatall]", false, false));
        GameObject.Find("menustat_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 1);
        GameObject.Find("menuchoice_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 1);
        GameObject.Find("menustat_interior").GetComponent<Image>().color = new Color(0, 0, 0, 1);
        GameObject.Find("menuchoice_interior").GetComponent<Image>().color = new Color(0, 0, 0, 1);

        txtmgrs[0].SetText(new TextMessage("[noskipatall]" + PlayerCharacter.instance.Name, false, true));
        if (GlobalControls.crate) {
            txtmgrs[1].SetText(new TextMessage("[noskipatall][font:menu]LV " + PlayerCharacter.instance.LV, false, true));
            txtmgrs[2].SetText(new TextMessage("[noskipatall][font:menu]PH " + (int)PlayerCharacter.instance.HP + "/" + PlayerCharacter.instance.MaxHP, false, true));
            txtmgrs[3].SetText(new TextMessage("[noskipatall][font:menu]G  " + PlayerCharacter.instance.Gold, false, true));
            txtmgrs[4].SetText(new TextMessage("[noskipatall]" + (Inventory.inventory.Count > 0 ? "" : "[color:808080]") + "TEM", false, true));
            txtmgrs[5].SetText(new TextMessage("[noskipatall]TAST", false, true));
            txtmgrs[6].SetText(new TextMessage("[noskipatall]LECL", false, true));
        } else {
            txtmgrs[1].SetText(new TextMessage("[noskipatall][font:menu]LV " + PlayerCharacter.instance.LV, false, true));
            txtmgrs[2].SetText(new TextMessage("[noskipatall][font:menu]HP " + (int)PlayerCharacter.instance.HP + "/" + PlayerCharacter.instance.MaxHP, false, true));
            txtmgrs[3].SetText(new TextMessage("[noskipatall][font:menu]G  " + PlayerCharacter.instance.Gold, false, true));
            txtmgrs[4].SetText(new TextMessage("[noskipatall]" + (Inventory.inventory.Count > 0 ? "" : "[color:808080]") + "ITEM", false, true));
            txtmgrs[5].SetText(new TextMessage("[noskipatall]STAT", false, true));
            txtmgrs[6].SetText(new TextMessage("[noskipatall]CELL", false, true));
        }
        GameObject.Find("Mugshot").GetComponent<Image>().color = new Color(1, 1, 1, 0);
        GameObject.Find("textframe_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
        GameObject.Find("textframe_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        Color c = GameObject.Find("utHeartMenu").GetComponent<Image>().color;
        GameObject.Find("utHeartMenu").GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1);
        GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(-255, 35, GameObject.Find("utHeartMenu").transform.position.z);
        int choice = 2;
        yield return 0;
        while (!instance.menuRunning[3]) {
            if (!instance.menuRunning[0]) {
                if (GlobalControls.input.Up == UndertaleInput.ButtonState.PRESSED) {
                    choice = (choice + 1) % 3;
                    GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(-255, 35 - ((2 - choice % 3) * 36), GameObject.Find("utHeartMenu").transform.position.z);
                } else if (GlobalControls.input.Down == UndertaleInput.ButtonState.PRESSED) {
                    choice = (choice + 2) % 3;
                    GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(-255, 35 - ((2 - choice % 3) * 36), GameObject.Find("utHeartMenu").transform.position.z);
                } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED) {
                    instance.uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menuconfirm"));
                    instance.menuRunning[0] = true;
                    if (choice == 2) { //ITEM
                        int invCount = Inventory.inventory.Count;
                        if (invCount == 0) {
                            instance.menuRunning[0] = false;
                            //yield break;
                        } else {
                            for (int i = 0; i != invCount; i++)
                                txtmgrs[i + 7].SetText(new TextMessage("[noskipatall]" + Inventory.inventory[i].Name, false, true));
                            if (GlobalControls.crate) {
                                txtmgrs[15].SetText(new TextMessage("[noskipatall]SUE", false, true));
                                txtmgrs[16].SetText(new TextMessage("[noskipatall]FINO", false, true));
                                txtmgrs[17].SetText(new TextMessage("[noskipatall]DORP", false, true));
                            } else {
                                txtmgrs[15].SetText(new TextMessage("[noskipatall]USE", false, true));
                                txtmgrs[16].SetText(new TextMessage("[noskipatall]INFO", false, true));
                                txtmgrs[17].SetText(new TextMessage("[noskipatall]DROP", false, true));
                            }
                            GameObject.Find("Mugshot").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                            GameObject.Find("textframe_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                            GameObject.Find("textframe_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                            GameObject.Find("item_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 1);
                            GameObject.Find("item_interior").GetComponent<Image>().color = new Color(0, 0, 0, 1);
                            GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(-48, 143, GameObject.Find("utHeartMenu").transform.position.z);
                            int index = 0;
                            yield return 0;
                            while (instance.menuRunning[0] && !instance.menuRunning[1] && !instance.menuRunning[3]) {
                                if (GlobalControls.input.Down == UndertaleInput.ButtonState.PRESSED) {
                                    index = (index + 1) % invCount;
                                    instance.uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menumove"));
                                    GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(-48, 143 - 32 * index, GameObject.Find("utHeartMenu").transform.position.z);
                                } else if (GlobalControls.input.Up == UndertaleInput.ButtonState.PRESSED) {
                                    index = (index + invCount - 1) % invCount;
                                    instance.uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menumove"));
                                    GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(-48, 143 - 32 * index, GameObject.Find("utHeartMenu").transform.position.z);
                                } else if (GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED) {
                                    instance.menuRunning[0] = false;
                                    for (int i = 7; i <= 17; i++) txtmgrs[i].DestroyChars();
                                    GameObject.Find("Mugshot").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                                    GameObject.Find("textframe_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                                    GameObject.Find("textframe_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                                    GameObject.Find("item_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                                    GameObject.Find("item_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                                    GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(-255, 35 - ((2 - choice % 3) * 36), GameObject.Find("utHeartMenu").transform.position.z);
                                } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED) {
                                    instance.menuRunning[1] = true;
                                    instance.uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menuconfirm"));
                                    int index2 = 0;
                                    GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(-48, -137, GameObject.Find("utHeartMenu").transform.position.z); // -53,42,156
                                    yield return 0;
                                    while (instance.menuRunning[1] && !instance.menuRunning[3]) {
                                        if (GlobalControls.input.Left == UndertaleInput.ButtonState.PRESSED) {
                                            instance.uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menumove"));
                                            index2 = (index2 + 2) % 3;
                                            switch (index2) {
                                                case 0: GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(-48, -137, GameObject.Find("utHeartMenu").transform.position.z); break;
                                                case 1: GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(47, -137, GameObject.Find("utHeartMenu").transform.position.z); break;
                                                case 2: GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(161, -137, GameObject.Find("utHeartMenu").transform.position.z); break;
                                            }
                                        } else if (GlobalControls.input.Right == UndertaleInput.ButtonState.PRESSED) {
                                            instance.uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menumove"));
                                            index2 = (index2 + 1) % 3;
                                            switch (index2) {
                                                case 0: GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(-48, -137, GameObject.Find("utHeartMenu").transform.position.z); break;
                                                case 1: GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(47, -137, GameObject.Find("utHeartMenu").transform.position.z); break;
                                                case 2: GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(161, -137, GameObject.Find("utHeartMenu").transform.position.z); break;
                                            }
                                        } else if (GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED) {
                                            GameObject.Find("utHeartMenu").transform.localPosition = new Vector3(-48, 143 - 32 * index, GameObject.Find("utHeartMenu").transform.position.z);
                                            instance.menuRunning[1] = false;
                                        } else if (GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED) {
                                            instance.uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menuconfirm"));
                                            for (int i = 7; i <= 17; i++) txtmgrs[i].DestroyChars();
                                            GameObject.Find("item_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                                            GameObject.Find("item_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                                            GameObject.Find("utHeartMenu").GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                                            instance.menuRunning[3] = true;
                                            switch (index2) {
                                                case 0:
                                                    instance.textmgr.SetEffect(null);
                                                    Inventory.UseItem(index);
                                                    //Update the stat text managers again, which means you can see the item's effects immediately
                                                    txtmgrs[0].SetText(new TextMessage("[noskipatall]" + PlayerCharacter.instance.Name, false, true));
                                                    if (GlobalControls.crate) {
                                                        txtmgrs[1].SetText(new TextMessage("[noskipatall][font:menu]LV " + PlayerCharacter.instance.LV, false, true));
                                                        txtmgrs[2].SetText(new TextMessage("[noskipatall][font:menu]PH " + (int)PlayerCharacter.instance.HP + "/" + PlayerCharacter.instance.MaxHP, false, true));
                                                        txtmgrs[3].SetText(new TextMessage("[noskipatall][font:menu]G  " + PlayerCharacter.instance.Gold, false, true));
                                                        txtmgrs[4].SetText(new TextMessage("[noskipatall]" + (Inventory.inventory.Count > 0 ? "" : "[color:808080]") + "TEM", false, true));
                                                        txtmgrs[5].SetText(new TextMessage("[noskipatall]TAST", false, true));
                                                        txtmgrs[6].SetText(new TextMessage("[noskipatall]LECL", false, true));
                                                    } else {
                                                        txtmgrs[1].SetText(new TextMessage("[noskipatall][font:menu]LV " + PlayerCharacter.instance.LV, false, true));
                                                        txtmgrs[2].SetText(new TextMessage("[noskipatall][font:menu]HP " + (int)PlayerCharacter.instance.HP + "/" + PlayerCharacter.instance.MaxHP, false, true));
                                                        txtmgrs[3].SetText(new TextMessage("[noskipatall][font:menu]G  " + PlayerCharacter.instance.Gold, false, true));
                                                        txtmgrs[4].SetText(new TextMessage("[noskipatall]" + (Inventory.inventory.Count > 0 ? "" : "[color:808080]") + "ITEM", false, true));
                                                        txtmgrs[5].SetText(new TextMessage("[noskipatall]STAT", false, true));
                                                        txtmgrs[6].SetText(new TextMessage("[noskipatall]CELL", false, true));
                                                    }
                                                    break;
                                                case 1:
                                                    string str;
                                                    Inventory.NametoDesc.TryGetValue(Inventory.inventory[index].Name, out str);
                                                    instance.textmgr.SetEffect(null);
                                                    instance.textmgr.SetText(new TextMessage("\"" + Inventory.inventory[index].Name + "\"\n" + str, true, false));
                                                    instance.textmgr.transform.parent.parent.SetAsLastSibling();
                                                    break;
                                                case 2:
                                                    instance.textmgr.SetEffect(null);
                                                    if (GlobalControls.crate)
                                                        instance.textmgr.SetText(new TextMessage("U DORPED TEH " + Inventory.inventory[index].Name + "!!!!!", true, false));
                                                    else
                                                        instance.textmgr.SetText(new TextMessage("You dropped the " + Inventory.inventory[index].Name + ".", true, false));
                                                    instance.textmgr.transform.parent.parent.SetAsLastSibling();
                                                    Inventory.RemoveItem(index);
                                                    break;
                                            }
                                            while (instance.PlayerNoMove)
                                                yield return 0;
                                            yield return CloseMenu(true);
                                        }
                                        yield return 0;
                                    }
                                }
                                yield return 0;
                            }
                        }
                    } else if (choice == 1) { // STAT
                        GameObject.Find("utHeartMenu").GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
                        txtmgrs[18].SetText(new TextMessage("[noskipatall]\"" + PlayerCharacter.instance.Name + "\"", false, true));
                        txtmgrs[19].SetText(new TextMessage("[noskipatall]LV " + PlayerCharacter.instance.LV, false, true));
                        txtmgrs[20].SetText(new TextMessage("[noskipatall]HP " + PlayerCharacter.instance.HP + "/" + PlayerCharacter.instance.MaxHP, false, true));
                        if (GlobalControls.crate) {
                            txtmgrs[21].SetText(new TextMessage("[noskipatall]TA " + (PlayerCharacter.instance.ATK + PlayerCharacter.instance.WeaponATK) + " (" + PlayerCharacter.instance.WeaponATK + ")", false, true));
                            txtmgrs[22].SetText(new TextMessage("[noskipatall]DF " + (PlayerCharacter.instance.DEF + PlayerCharacter.instance.ArmorDEF) + " (" + PlayerCharacter.instance.ArmorDEF + ")", false, true));
                            txtmgrs[23].SetText(new TextMessage("[noskipatall]EPX: " + PlayerCharacter.instance.EXP, false, true));
                            txtmgrs[24].SetText(new TextMessage("[noskipatall]NETX: " + PlayerCharacter.instance.GetNext(), false, true));
                            txtmgrs[25].SetText(new TextMessage("[noskipatall]WAEPON: " + PlayerCharacter.instance.Weapon, false, true));
                            txtmgrs[26].SetText(new TextMessage("[noskipatall]AROMR: " + PlayerCharacter.instance.Armor, false, true));
                            txtmgrs[27].SetText(new TextMessage("[noskipatall]GLOD: " + PlayerCharacter.instance.Gold, false, true));
                        } else {
                            txtmgrs[21].SetText(new TextMessage("[noskipatall]AT " + (PlayerCharacter.instance.ATK + PlayerCharacter.instance.WeaponATK) + " (" + PlayerCharacter.instance.WeaponATK + ")", false, true));
                            txtmgrs[22].SetText(new TextMessage("[noskipatall]DF " + (PlayerCharacter.instance.DEF + PlayerCharacter.instance.ArmorDEF) + " (" + PlayerCharacter.instance.ArmorDEF + ")", false, true));
                            txtmgrs[23].SetText(new TextMessage("[noskipatall]EXP: " + PlayerCharacter.instance.EXP, false, true));
                            txtmgrs[24].SetText(new TextMessage("[noskipatall]NEXT: " + PlayerCharacter.instance.GetNext(), false, true));
                            txtmgrs[25].SetText(new TextMessage("[noskipatall]WEAPON: " + PlayerCharacter.instance.Weapon, false, true));
                            txtmgrs[26].SetText(new TextMessage("[noskipatall]ARMOR: " + PlayerCharacter.instance.Armor, false, true));
                            txtmgrs[27].SetText(new TextMessage("[noskipatall]GOLD: " + PlayerCharacter.instance.Gold, false, true));
                        }
                        GameObject.Find("Mugshot").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                        GameObject.Find("textframe_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                        GameObject.Find("textframe_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                        GameObject.Find("stat_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 1);
                        GameObject.Find("stat_interior").GetComponent<Image>().color = new Color(0, 0, 0, 1);
                        yield return 0;
                        while (instance.menuRunning[0] && !instance.menuRunning[3]) {
                            if (GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED || GlobalControls.input.Confirm == UndertaleInput.ButtonState.PRESSED) {
                                GameObject.Find("utHeartMenu").GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1);
                                instance.uiAudio.PlayOneShot(AudioClipRegistry.GetSound("menuconfirm"));
                                instance.menuRunning[0] = false;
                                for (int i = 18; i <= 27; i++) txtmgrs[i].DestroyChars();
                                GameObject.Find("Mugshot").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                                GameObject.Find("textframe_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                                GameObject.Find("textframe_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                                GameObject.Find("stat_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
                                GameObject.Find("stat_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                            }
                            yield return 0;
                        }
                    } else { //CELL
                        yield return CloseMenu();
                        instance.textmgr.SetEffect(null);
                        if (GlobalControls.crate)
                            instance.textmgr.SetText(new TextMessage("NO CELPLHONE ALOLWDE!!!", true, false));
                        else
                            instance.textmgr.SetText(new TextMessage("But you don't have a cellphone... [w:10]yet.", true, false));
                        instance.textmgr.transform.parent.parent.SetAsLastSibling();
                    }
                } else if (GlobalControls.input.Cancel == UndertaleInput.ButtonState.PRESSED)
                    yield return CloseMenu(true);
            }
            yield return 0;
        }
        while (instance.PlayerNoMove)
            yield return 0;
        instance.menuRunning[2] = false;
        instance.menuRunning[3] = false;
    }

    private static bool CloseMenu(bool endOfInText = false) {
        foreach (Transform tf in GameObject.Find("MenuContainer").GetComponentsInChildren<Transform>()) {
            if (tf.GetComponent<Image>()) tf.gameObject.GetComponent<Image>().color = new Color(tf.gameObject.GetComponent<Image>().color.r,
                                                                                                tf.gameObject.GetComponent<Image>().color.b,
                                                                                                tf.gameObject.GetComponent<Image>().color.g, 0);
            if (tf.GetComponent<TextManager>()) tf.gameObject.GetComponent<TextManager>().DestroyChars();
        }
        instance.menuRunning = new bool[] { false, false, !endOfInText, true, true };
        GameObject.Find("Mugshot").GetComponent<Image>().color = new Color(1, 1, 1, 0);
        GameObject.Find("textframe_border_outer").GetComponent<Image>().color = new Color(1, 1, 1, 0);
        GameObject.Find("textframe_interior").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        GameObject.Find("TextManager OW").GetComponent<TextManager>().skipNowIfBlocked = true;
        if (endOfInText)
            instance.PlayerNoMove = false; //Close menu
        SetUIPos(false);
        instance.UIPos = 0;
        return true;
    }

    private static List<string> overworldMusics = new List<string>();

    public static void HideOverworld(string callFrom = "Unknown") {
        Camera.main.GetComponent<FPSDisplay>().enabled = false;
        overworldMusics.Clear();
        List<string> toDelete = new List<string>();
        foreach (string str in NewMusicManager.audiolist.Keys) {
            AudioSource audio = (AudioSource)NewMusicManager.audiolist[str];
            if (!audio) {
                toDelete.Add(str);
                continue;
            }
            if (!audio.name.Contains("StaticKeptAudio"))
                if (audio.isPlaying && str != "src") {
                    overworldMusics.Add(str);
                    NewMusicManager.Stop(str);
                }
        }
        foreach (string str in toDelete)
            if (str != "src")
                NewMusicManager.DestroyChannel(str);
        MusicManager.src.Stop();
        GameObject go2 = new GameObject();
        GameObject go = Instantiate(go2);
        Destroy(go2);
        go.name = "GameObject";
        Transform[] root = UnitaleUtil.GetFirstChildren(null, true);
        for (int i = 0; i < root.Length; i++)
            if (root[i] != go && !root[i].name.Contains("AudioChannel") && root[i].name != "BGCamera")
                if (callFrom == "Shop" && root[i].name == "Main Camera OW") {
                    root[i].GetComponent<EventManager>().enabled = false;
                    root[i].GetComponent<TransitionOverworld>().enabled = false;
                    root[i].transform.position = new Vector3(320, 240, root[i].transform.position.z);
                } else
                    root[i].SetParent(go.transform);
        go.SetActive(false);
    }

    public static void ShowOverworld(string callFrom = "Unknown") {
        Transform[] root = UnitaleUtil.GetFirstChildren(null, true);
        GameObject go = null;
        foreach (Transform tf in root)
            if (tf.gameObject.name == "GameObject") {
                go = tf.gameObject;
                break;
            } else if (!tf.gameObject.name.Contains("AudioChannel"))
                Destroy(go);
        go.SetActive(true);
        if (GameObject.Find("Main Camera"))
            GameObject.Destroy(GameObject.Find("Main Camera"));
        Transform[] children = UnitaleUtil.GetFirstChildren(go.transform, true);
        foreach (Transform tf in children) {
            try {
                tf.SetParent(null);
                if (tf.name == "Canvas OW" || tf.name == "Canvas Two" || tf.name == "Main Camera OW" || tf.name == "GameOverContainer" || tf.name == "BGCamera")
                    GameObject.DontDestroyOnLoad(tf.gameObject);
                else if (tf.childCount > 0)
                    if (tf.GetChild(0).name == "Player")
                        GameObject.DontDestroyOnLoad(tf.gameObject);
            } catch { }
        }
        instance.StartCoroutine(instance.ShowOverworld2(callFrom));
    }

    IEnumerator ShowOverworld2(string callFrom) {
        yield return 0;
        if (callFrom == "Battle") {
            GameObject.Destroy(GameObject.Find("psContainer"));
            GameObject.Destroy(GameObject.Find("GameObject"));
        }

        GameObject.FindObjectOfType<Fading>().fade.color = new Color(0, 0, 0, 1);
        foreach (string str in overworldMusics)
            try {
                if (!NewMusicManager.Exists(str))
                    NewMusicManager.CreateChannel(str);
                AudioSource channel = ((AudioSource)NewMusicManager.audiolist[str]);
                string clipNameWithPrefix = NewMusicManager.GetAudioName(str);
                if      (clipNameWithPrefix.StartsWith("music:"))
                    NewMusicManager.PlayMusic(str, NewMusicManager.GetAudioName(str, false), channel.loop, channel.volume);
                else if (clipNameWithPrefix.StartsWith("sound:"))
                    NewMusicManager.PlaySound(str, NewMusicManager.GetAudioName(str, false), channel.loop, channel.volume);
                else if (clipNameWithPrefix.StartsWith("voice:"))
                    NewMusicManager.PlayVoice(str, NewMusicManager.GetAudioName(str, false), channel.loop, channel.volume);
            } catch { }
        overworldMusics.Clear();
        instance.OnDisable();
        instance.OnEnable();
        if (callFrom == "Shop") {
            GameObject.Find("Main Camera OW").GetComponent<EventManager>().enabled = true;
            GameObject.Find("Main Camera OW").GetComponent<TransitionOverworld>().enabled = true;
        } else {
            bool restart = true;
            foreach (TPHandler tp in FindObjectsOfType<TPHandler>())
                if (tp.gameObject.name.Contains("TP On-the-fly"))
                    restart = StaticInits.MODFOLDER == FindObjectOfType<MapInfos>().modToLoad;
            if (restart)
                instance.RestartMusic();
        }
        yield return 0;
        foreach (Transform tf in UnitaleUtil.GetFirstChildren(GameObject.Find("Canvas OW").transform, true))
            if (tf.gameObject.GetComponent<UserDebugger>()) {
                tf.gameObject.GetComponent<UserDebugger>().Start();
                break;
            }
        StaticInits.SendLoaded();
    }
}