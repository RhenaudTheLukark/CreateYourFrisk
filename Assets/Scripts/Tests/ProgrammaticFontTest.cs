using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The testing class that preceded TextManager (Which is somehow even worse). Kept for historical reasons.
/// </summary>
public class ProgrammaticFontTest : MonoBehaviour
{
    private string fontName = "unnamed_2012";

    private Dictionary<char, Sprite> letters = new Dictionary<char, Sprite>();
    public GameObject letterObj;
    private float letterTimer = -0.1f;
    private float timePerLetter = 1.0f / 30;
    private int currentLetter;
    private AudioSource letterSound;
    private Image[] letterReferences;
    private Sprite[] letterSprites;
    private GameObject canvas;
    private float currentX = 15;
    private float currentY = 450;
    private string teststr = "* the quick brown fox jumps over\n  the lazy dog.\n* THE QUICK BROWN FOX JUMPS OVER\n  THE LAZY DOG.\n* Jerry.";
    // string teststr = "* ";

    // Use this for initialization
    private void Start() {
        letterSound = GetComponent<AudioSource>();
        canvas = GameObject.Find("Canvas");
        letterSprites = Resources.LoadAll<Sprite>("Fonts/" + fontName);
        foreach (Sprite s in letterSprites) {
            string name = s.name;
            if (name.Length == 1) {
                letters.Add(name[0], s);
                continue;
            } else
                switch (name) {
                    case "slash":        letters.Add('/', s);  break;
                    case "dot":          letters.Add('.', s);  break;
                    case "pipe":         letters.Add('|', s);  break;
                    case "backslash":    letters.Add('\\', s); break;
                    case "colon":        letters.Add(':', s);  break;
                    case "questionmark": letters.Add('?', s);  break;
                    case "doublequote":  letters.Add('"', s);  break;
                    case "asterisk":     letters.Add('*', s);  break;
                    case "space":        letters.Add(' ', s);  break;
                }
        }
        NewCopy();
    }

    private void NewCopy() {
        letterReferences = new Image[teststr.Length];
        for (int i = 0; i < teststr.Length; i++) {
            if (teststr[i] == '\n') {
                currentX = 15;
                currentY -= 28;
                continue;
            }

            GameObject singleLtr = Instantiate(letterObj);
            RectTransform ltrRect = singleLtr.GetComponent<RectTransform>();
            Image ltrImg = singleLtr.GetComponent<Image>();

            ltrRect.SetParent(canvas.transform);

            if (letters.ContainsKey(teststr[i])) ltrImg.sprite = letters[teststr[i]];
            else                                 ltrImg.sprite = letters['?'];

            letterReferences[i] = ltrImg;

            if (letters.ContainsKey(teststr[i])) ltrRect.position = new Vector2(currentX, currentY + letters[teststr[i]].border.w - letters[teststr[i]].border.y);
            else                                 ltrRect.position = new Vector2(currentX, currentY);
            ltrImg.SetNativeSize();
            ltrImg.enabled = false;

            currentX += ltrRect.rect.width + 2;
        }
    }

    // Update is called once per frame
    private void Update() {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            currentX = 15;
            currentY -= 56;
            newCopy();
            currentLetter = 0;
        }*/
        letterTimer += Time.deltaTime;
        if (letterTimer > timePerLetter)
            if (currentLetter < letterReferences.Length) {
                if (teststr[currentLetter] == '\n')
                    letterTimer = -1.0f;
                else {
                    letterTimer = 0.0f;
                    letterReferences[currentLetter].enabled = true;
                    letterSound.Play();
                }
                currentLetter++;
            }
    }

    public void OnGUI() {
        if (Event.current.type == EventType.KeyDown) {
            char c = Event.current.character;
            if (c != '\0') {
                if (c == '\n') {
                    currentX = 15;
                    currentY -= 28;
                    return;
                }

                GameObject singleLtr = Instantiate(letterObj);
                RectTransform ltrRect = singleLtr.GetComponent<RectTransform>();
                Image ltrImg = singleLtr.GetComponent<Image>();

                ltrRect.SetParent(canvas.transform);

                if (letters.ContainsKey(c)) ltrImg.sprite = letters[c];
                else                        ltrImg.sprite = letters['?'];

                if (letters.ContainsKey(c)) ltrRect.position = new Vector2(currentX, currentY + letters[c].border.w - letters[c].border.y);
                else                        ltrRect.position = new Vector2(currentX, currentY);
                ltrImg.SetNativeSize();
                ltrImg.enabled = true;

                currentX += ltrRect.rect.width + 2;
                letterSound.Play();
            }
        }
    }
}