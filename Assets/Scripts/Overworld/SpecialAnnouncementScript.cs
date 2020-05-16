using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SpecialAnnouncementScript : MonoBehaviour {
    // Globally used variables
    public  Text        mainText,  subText;  // Text objects used for the old CYF v0.6 secret and the new part of the secret
    public  AudioSource mainAudio, subAudio; // Audio objects, first one used for the two vocal messages, the second one used for Goat Sound
    private LuaSpriteController mainSprite, subSprite, fadeSprite, pauseSprite; // Sprites used for both animated characters, the fade effect and the pause button
    private int phase = 0; // Current speech phase
    private float punderTime = 0; // Used to store the current time when switching to the old text to the new one
    private string lastPunderSprite = "", lastLuSprite = ""; // Variables used to know whether we should change the current animated sprite or not
    private bool firstTalk = false; // Used in order to know when to fade in the first animated character
    private Dictionary<string, Sprite> punderSprites = new Dictionary<string, Sprite>(), luSprites = new Dictionary<string, Sprite>(); // Dictionaries storing the two animated characters' sprites
    private Dictionary<string, AudioClip> audioFiles = new Dictionary<string, AudioClip>(); // Dictionary storing all of the project's files

    // MisriHalek reference variables
    public  Image misriHalek;
    private Vector2 MHPos;
    private bool MHStarted = false;
    private bool MHJustStarted = false;

    // Volume check variables
    private float lastTime = 0;
    private int sampleDataLength = 256;
    private float clipVolume;
    private float[] clipSampleData;

    // Dictionary associating two functions: the goal is to associate a current time value and its punder face
    private Dictionary<Func<float, bool>, Func<string>> punderFaceList = new Dictionary<Func<float, bool>, Func<string>> {
        { x => x == 0,     () => { return "veryHappy"; }     },
        { x => x < 7.5,    () => { return "question"; }      },
        { x => x < 8.5,    () => { return "idle"; }          },
        { x => x < 18.5,   () => { return "happy"; }         },
        { x => x < 21,     () => { return "perv"; }          },
        { x => x < 23,     () => { return "misriHalek"; }    },
        { x => x < 28.5,   () => { return "happy"; }         },
        { x => x < 42,     () => { return "serious"; }       },
        { x => x < 55.5,   () => { return "happy"; }         },
        { x => x < 57.5,   () => { return "serious"; }       },
        { x => x < 58,     () => { return "happy"; }         },
        { x => x < 59,     () => { return "sad"; }           },
        { x => x < 60,     () => { return "happy"; }         },
        { x => x < 62,     () => { return "sad"; }           },
        { x => x < 64.5,   () => { return "happy"; }         },
        { x => x < 74,     () => { return "serious"; }       },
        { x => x < 80,     () => { return "idle"; }          },
        { x => x < 94.7,   () => { return "serious"; }       },
        { x => x < 107.5,  () => { return "happy"; }         },
        { x => x < 117,    () => { return "serious"; }       },
        { x => x < 120.25, () => { return "happy"; }         },
        { x => x < 120.4,  () => { return "undyne2"; }       },
        { x => x < 120.55, () => { return "undyne"; }        },
        { x => x < 120.65, () => { return "undyne2"; }       },
        { x => x < 120.75, () => { return "undyne3"; }       },
        { x => x < 121,    () => { return "undyne"; }        },
        { x => x < 129,    () => { return "happy"; }         },
        { x => x < 130,    () => { return "questionHappy"; } },
        { x => x < 133.5,  () => { return "happy"; }         },
        { x => x < 137,    () => { return "determined"; }    },
        { x => x < 138.6,  () => { return "happy"; }         },
        { x => x < 139.25, () => { return "laugh"; }         },
        { x => x < 143.1,  () => { return "happy"; }         },
        { x => x < 150,    () => { return "veryHappy"; }     }
    };

    // Dictionary associating two functions: the goal is to associate a current time value and its punder face
    private Dictionary<Func<float, bool>, Func<string>> luFaceList = new Dictionary<Func<float, bool>, Func<string>> {
        { x => x < 2,    () => { return "NormalNormal"; } },
        { x => x < 3.5,  () => { return "WaveNormal";   } },
        { x => x < 6.3,  () => { return "NormalNormal"; } },
        { x => x < 8,    () => { return "PointNormal";  } },
        { x => x < 12.4, () => { return "NormalSad";    } },
        { x => x < 14.5, () => { return "NormalNormal"; } },
        { x => x < 18.4, () => { return "PointHappy";   } },
        { x => x < 19.5, () => { return "NormalNormal"; } },
        { x => x < 22.4, () => { return "HoldNormal";   } },
        { x => x < 27.2, () => { return "PointNormal";  } },
        { x => x < 31.1, () => { return "NormalHappy";  } },
        { x => x < 32.2, () => { return "NormalNormal"; } },
        { x => x < 34.4, () => { return "NormalHappy";  } },
        { x => x < 38,   () => { return "WaveHappy";    } },
    };

    // Use this for initialization
    void Start () {
        // Load CYF's save file
        StaticInits.Start();
        SaveLoad.Start();

        // Set up CYF's basic objects
        new ControlPanel();
        new PlayerCharacter();
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (GlobalControls.crate) Misc.WindowName = ControlPanel.instance.WinodwBsaisNmae;
            else Misc.WindowName = ControlPanel.instance.WindowBasisName;
        #endif
        // Load CYF's AlMighty save file
        SaveLoad.LoadAlMighty();

        // Load all resources needed for this animation to play
        Sprite[] punderSprs = Resources.LoadAll<Sprite>("Sprites/Punder");
        foreach (Sprite spr in punderSprs)
            punderSprites.Add(spr.name, spr);

        Sprite[] luSprs = Resources.LoadAll<Sprite>("Sprites/Lu");
        foreach (Sprite spr in luSprs)
            luSprites.Add(spr.name, spr);

        AudioClip[] adcs = Resources.LoadAll<AudioClip>("Audios");
        foreach (AudioClip adc in adcs)
            audioFiles.Add(adc.name, adc);

        // Create all sprites needed for the animation
        mainSprite = (LuaSpriteController) SpriteUtil.MakeIngameSprite("empty", "Default", -1).UserData.Object;
        mainSprite.alpha = 0;
        mainSprite.SetPivot(.5f, 0);
        mainSprite.x = 0;
        mainSprite.y = 20;

        fadeSprite = (LuaSpriteController) SpriteUtil.MakeIngameSprite("black", "Default", -1).UserData.Object;
        fadeSprite.alpha = 0;

        pauseSprite = (LuaSpriteController) SpriteUtil.MakeIngameSprite("empty", "Default", -1).UserData.Object;
        SetSprite(Resources.Load<Sprite>("Sprites/pause"), "", pauseSprite);
        pauseSprite.alpha = 0;

        subSprite = (LuaSpriteController) SpriteUtil.MakeIngameSprite("empty", "Lu", -1).UserData.Object;
        subSprite.x = 0;
        subSprite.y = -400;
        SetSprite("HoldNormal0", false);

        // Preload all other audio files to prevent lag spikes
        foreach (AudioClip clip in audioFiles.Values) {
            mainAudio.clip = clip;
            mainAudio.Play();
        }
        mainAudio.clip = audioFiles["MisriHalek"];
        mainAudio.loop = false;
        mainAudio.Play();

        clipSampleData = new float[sampleDataLength];
    }

    // Set the main or sub sprite using a string key
    void SetSprite(string key, bool main) {
        LuaSpriteController spr = main ? mainSprite : subSprite;
        SetSprite(key, spr, main ? 0 : 1);
    }

    // Set a sprite using a string key
    void SetSprite(string key, LuaSpriteController spr, int main = -1) {
        Dictionary<string, Sprite> sprDict = main == 0 ? punderSprites : luSprites;
        SetSprite(sprDict[key], key, spr, main);
    }

    // Set a given sprite to a sprite controller
    void SetSprite(Sprite key, string strKey, LuaSpriteController spr, int main = -1) {
        if (key == null)
            throw new Exception("Tried to set sprite with key \"" + strKey + "\".");
        if (main < 0 || main > 1 || (main == 0 ? lastPunderSprite : lastLuSprite) != strKey) {
            spr.img.GetComponent<Image>().sprite = key;
            spr.img.GetComponent<RectTransform>().sizeDelta = new Vector2(key.texture.width, key.texture.height);
            if (main == 0)
                lastPunderSprite = strKey;
            else if (main == 1) {
                lastLuSprite = strKey;
                spr.Scale(2, 2);
            }
        }
    }

    // Update is called once per frame
    void Update() {
        if (mainAudio.time != lastTime) {
            lastTime = mainAudio.time;

            // Compute which sprite to use
            string currentSprite = (punderTime == 0 ? punderFaceList : luFaceList).First(sw => sw.Key(mainAudio.time)).Value();

            int talkLevel = 0;
            if (mainAudio.isPlaying) {
                // Compute the current and following sound samples to know which sprite to display
                // I read 256 samples, which is about 20ms on a 44khz stereo clip, beginning at the current sample position of the clip
                // I compute the average of the volume of these samples and use it to know which sprite to display
                mainAudio.clip.GetData(clipSampleData, mainAudio.timeSamples);
                clipVolume = 0f;
                foreach (float sample in clipSampleData)
                    clipVolume += Mathf.Abs(sample);
                clipVolume /= sampleDataLength;

                if (punderTime == 0) talkLevel = clipVolume > 0.031 ? 1 : 0; // -30dB
                else                 talkLevel = clipVolume > 0.056 ? 2 : clipVolume > 0.01 ? 1 : 0; // -25db & -40db
                if (!firstTalk && talkLevel > 0)
                    firstTalk = true;
            }

            // Actually set the sprite using the volume computed earlier
            string suffix = punderTime == 0 ? (talkLevel > 0 ? "T" : "") : talkLevel.ToString();
            SetSprite(currentSprite + (currentSprite.Contains("undyne") ? "" : suffix), punderTime == 0);
        }

        // Fades the first animated character in when the volume is high enough
        if (firstTalk && mainSprite.alpha < 1)
            mainSprite.alpha += Time.deltaTime;

        if (punderTime > 0) {
            // Fade in the other elements while playing the second thing
            if (mainAudio.time <= 2 && fadeSprite.alpha < 1)
                fadeSprite.alpha = Mathf.Min(.5f, mainAudio.time / 3);
        }

        CheckPhaseEvent();
    }

    // Execute a range of events
    void CheckPhaseEvent() {
        if (punderTime == 0)
            switch (phase) {
                case 0:
                    if (mainAudio.time >= 15) {
                        phase ++;
                        mainText.text = "(This friend in question is MisriHalek, the guy who made...)";
                    }
                    break;
                case 1:
                    if (mainAudio.time >= 21) {
                        if (!MHStarted) {
                            mainText.text = "(This friend in question is MisriHalek, the guy who made this dude down there!)";
                            mainAudio.time = 21;
                            mainAudio.Pause();
                            MHStarted = true;
                            MHJustStarted = true;
                            misriHalek.transform.localPosition = new Vector2(360, -360);
                            MHPos = misriHalek.transform.localPosition;
                            subAudio.loop = false;
                            subAudio.clip = audioFiles["GoatSound"];
                            subAudio.Play();
                        }

                        if (subAudio.time != 0 && MHPos.x > 280 && subAudio.time < subAudio.clip.length / 2) MHPos = new Vector2(MHPos.x - 5, MHPos.y + 10);
                        else if (subAudio.time == 0 && MHPos.x < 360)                                        MHPos = new Vector2(MHPos.x + 5, MHPos.y - 10);
                        else if (MHJustStarted)                                                              MHJustStarted = false;
                        else if (subAudio.time == 0 && !subAudio.isPlaying && !MHJustStarted) {
                            phase++;
                            mainAudio.UnPause();
                            mainText.text = "";
                        }

                        if (subAudio.time != 0) misriHalek.transform.localPosition = new Vector2(MHPos.x + (float)(UnityEngine.Random.value - .5) * 10, MHPos.y + (float)(UnityEngine.Random.value - .5) * 10);
                        else                    misriHalek.transform.localPosition = MHPos;
                    }
                    break;
                case 2:
                    if (mainAudio.time >= 73) {
                        phase++;
                        mainText.text = "(You really should write this down :P)";
                    }
                    break;
                case 3:
                    if (mainAudio.time >= 80) {
                        phase++;
                        mainText.text = "";
                    }
                    break;
                case 4:
                    if (mainAudio.time >= 116) {
                        phase++;
                        mainText.text = "(Yep, shared. However, let the others discover what you discovered by yourself, it's funnier that way for them!)";
                    }
                    break;
                case 5:
                    if (mainAudio.time >= 125) {
                        phase++;
                        mainText.text = "";
                    }
                    break;
                case 6:
                    if (mainAudio.time >= 133.5) {
                        phase++;
                        mainText.text = "(I ordered a Frisk, but all I got was a crate!)";
                    }
                    break;
                case 7:
                    if (mainAudio.time >= 143) {
                        phase++;

                        subAudio.loop = false;
                        subAudio.clip = audioFiles["ButtonSound"];
                        subAudio.Play();

                        pauseSprite.alpha = 1;
                        mainAudio.time = 143;
                        punderTime = mainAudio.time;
                        mainAudio.time = 0;
                        mainAudio.clip = audioFiles["Rhenny"];
                        mainAudio.loop = false;
                        mainAudio.Play();
                    }
                    break;
                default:
                    phase++;
                    mainText.text = "(I ordered a Frisk, but all I got was a crate!)\n\nThanks for listening to the end guys, you can now close the game!";
                    break;
            }
        else {
            switch (phase) {
                case 8:
                    if (mainAudio.time >= 1.2) {
                        subSprite.y = Mathf.Min(-160f, subSprite.y + 6);
                        if (subSprite.y >= -160f) phase++;
                    }
                    break;
                case 9:
                    if (mainAudio.time >= 18.5) {
                        subSprite.y = Mathf.Max(-400f, subSprite.y - 6);
                        if (subSprite.y <= -400f) phase++;
                    }
                    break;
                case 10:
                    if (mainAudio.time >= 20) {
                        subText.rectTransform.localPosition = new Vector3(subText.rectTransform.localPosition.x, Mathf.Min(-92, subText.rectTransform.localPosition.y + 4), subText.rectTransform.localPosition.z);
                        subSprite.y = Mathf.Min(-138f, subSprite.y + 4);
                        if (subSprite.y >= -138f) phase++;
                    }
                    break;
                case 11:
                    if (mainAudio.time >= 22.4) {
                        subSprite.y = Mathf.Max(-160, subSprite.y - 4);
                        if (subSprite.y <= -160f) phase++;
                    }
                    break;
                case 12:
                    if (mainAudio.time >= mainAudio.clip.length - 2) {
                        subSprite.y = Mathf.Max(-400f, subSprite.y - 10);
                        if (subSprite.y <= -400f) phase++;
                    }
                    break;
            }
            if (!mainAudio.isPlaying) {
                subAudio.loop = false;
                subAudio.clip = audioFiles["ButtonSound"];
                subAudio.Play();

                pauseSprite.alpha = 0;
                fadeSprite.alpha = 0;
                mainAudio.clip = audioFiles["MisriHalek"];
                mainAudio.loop = false;
                mainAudio.time = punderTime;
                punderTime = 0;
                mainAudio.Play();
            }
        }
    }
}
