using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SpecialAnnouncementScript : MonoBehaviour {
    //Globally used variables
    private Image MisriHalek;
    private Text MainText;
    private AudioSource MainAudio, SubAudio;
    private LuaSpriteController MainSprite;
    private int Phase = 0;
    private string LastPunderSprite = "";
    private string CurrentPunderSprite = "idle";
    private bool isTalking = false;
    private bool muted = false;
    private bool firstTalk = false;
    private Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
    private Dictionary<string, AudioClip> Audios = new Dictionary<string, AudioClip>();

    //MisriHalek reference variables
    private Vector2 MHPos;
    private bool MHStarted = false;
    private bool MHJustStarted = false;

    //Db check variables
    private float lastTime = 0;
    private int sampleDataLength = 256;
    private float clipLoudness;
    private float[] clipSampleData;

    //wtf
    private Dictionary<Func<float, bool>, Func<string>> FaceList = new Dictionary<Func<float, bool>, Func<string>> {
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
        { x => x < 143,    () => { return "happy"; }         },
        { x => x < 150,    () => { return "veryHappy"; }     }
    };

    // Use this for initialization
    void Start () {
        StaticInits.Start();
        SaveLoad.Start();
        new ControlPanel();
        new PlayerCharacter();
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (GlobalControls.crate) Misc.WindowName = ControlPanel.instance.WinodwBsaisNmae;
            else Misc.WindowName = ControlPanel.instance.WindowBasisName;
        #endif
        SaveLoad.LoadAlMighty();
        MisriHalek = GameObject.Find("MisriHalek").GetComponent<Image>();
        MainText = FindObjectOfType<Text>();
        MainAudio = Camera.main.GetComponent<AudioSource>();
        SubAudio = GameObject.Find("SubAudio").GetComponent<AudioSource>();
        MainSprite = ((LuaSpriteController)SpriteUtil.MakeIngameSprite("empty", "none", -1).UserData.Object);
        MainSprite.alpha = 0;
        MainSprite.SetPivot(.5f, 0);
        MainSprite.absy = 260;

        Sprite[] sprs = Resources.LoadAll<Sprite>("Sprites/Punder");
        foreach (Sprite spr in sprs)
            Sprites.Add(spr.name, spr);
        AudioClip[] adcs = Resources.LoadAll<AudioClip>("Audios");
        foreach (AudioClip adc in adcs)
            Audios.Add(adc.name, adc);
        MainAudio.clip = Audios["MisriHalek"];
        MainAudio.loop = false;
        MainAudio.Play();
        //MainAudio.time = 137;
        clipSampleData = new float[sampleDataLength];
    }

    void SetSprite(string key) {
        if (LastPunderSprite != key) {
            MainSprite.img.GetComponent<Image>().sprite = Sprites[key];
            MainSprite.img.GetComponent<RectTransform>().sizeDelta = new Vector2(Sprites[key].texture.width, Sprites[key].texture.height);
            LastPunderSprite = key;
        }
    }

    // Update is called once per frame
    void Update() {
        if (MainAudio.isPlaying && MainAudio.time > MainAudio.clip.length - 0.1) {
            MainAudio.Stop();
            MainAudio.time = 0;
        }
        if (MainAudio.time != lastTime) {
            lastTime = MainAudio.time;
            CurrentPunderSprite = FaceList.First(sw => sw.Key(MainAudio.time)).Value();
            if (MainAudio.isPlaying && !muted) {
                MainAudio.clip.GetData(clipSampleData, MainAudio.timeSamples); //I read 256 samples, which is about 20 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
                clipLoudness = 0f;
                foreach (var sample in clipSampleData)
                    clipLoudness += Mathf.Abs(sample);
                clipLoudness /= sampleDataLength;
                //print(MainAudio.time + ": " + (10 * Mathf.Log10(clipLoudness)) + " dB");
                isTalking = clipLoudness > 0.031; //-30 dB
                if (!firstTalk && isTalking)
                    firstTalk = true;
            } else
                isTalking = false;
            SetSprite(CurrentPunderSprite + (isTalking && !CurrentPunderSprite.Contains("undyne") ? "T" : ""));
        }

        if (firstTalk && MainSprite.alpha < 1)
            MainSprite.alpha += Time.deltaTime;

        CheckPhaseEvent();
        CheckAnimEvent();
    }

    void CheckAnimEvent() {
    }

    void CheckPhaseEvent() { 
        switch (Phase) {
            case 0:
                if (MainAudio.time >= 15 /*&& MainAudio.time < 16*/) {
                    Phase ++;
                    MainText.text = "(This friend in question is MisriHalek, the guy who made...)";
                }
                break;
            case 1:
                if (MainAudio.time >= 21) {
                    if (!MHStarted) {
                        MainText.text = "(This friend in question is MisriHalek, the guy who made this dude down there!)";
                        MainAudio.time = 21;
                        MainAudio.Pause();
                        MHStarted = true;
                        MHJustStarted = true;
                        MisriHalek.transform.localPosition = new Vector2(360, -360);
                        MHPos = MisriHalek.transform.localPosition;
                        SubAudio.Stop();
                        SubAudio.clip = Audios["autistic screeching"];
                        SubAudio.loop = false;
                        SubAudio.Play();
                    }
                    if (MHStarted) {
                        if (SubAudio.time != 0 && MHPos.x > 280 && SubAudio.time < SubAudio.clip.length / 2) MHPos = new Vector2(MHPos.x - 5, MHPos.y + 10);
                        else if (SubAudio.time == 0 && MHPos.x < 360) MHPos = new Vector2(MHPos.x + 5, MHPos.y - 10);
                        else if (SubAudio.time == 0 && !SubAudio.isPlaying && !MHJustStarted) {
                            Phase++;
                            MainAudio.UnPause();
                            MainText.text = "";
                        }

                        if (SubAudio.time != 0) MisriHalek.transform.localPosition = new Vector2(MHPos.x + (float)(UnityEngine.Random.value - .5) * 10, MHPos.y + (float)(UnityEngine.Random.value - .5) * 10);
                        else MisriHalek.transform.localPosition = MHPos;

                        if (MHJustStarted)
                            MHJustStarted = false;
                    }
                }
                break;
            case 2:
                if (MainAudio.time >= 73) {
                    Phase++;
                    MainText.text = "(You really should write this down :P)";
                }
                break;
            case 3:
                if (MainAudio.time >= 80) {
                    Phase++;
                    MainText.text = "";
                }
                break;
            case 4:
                if (MainAudio.time >= 116) {
                    Phase++;
                    MainText.text = "(Yep, shared. However, let the others discover what you discovered by yourself, it's funnier that way for them!)";
                }
                break;
            case 5:
                if (MainAudio.time >= 125) {
                    Phase++;
                    MainText.text = "";
                }
                break;
            case 6:
                if (MainAudio.time >= 133.5) {
                    Phase++;
                    MainText.text = "(I ordered a Frisk, but all I got was a crate!)";
                }
                break;
            case 7:
                if (MainAudio.time >= 142.5) {
                    Phase++;
                    MainText.text = "(I ordered a Frisk, but all I got was a crate!)\nThanks for listening to the end guys, you can now close the game!";
                }
                break;
        }
    }
}
