using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The testing class that preceded TextManager (Which is somehow even worse). Kept for historical reasons.
/// </summary>
public class ProgrammaticFontTest : MonoBehaviour {
    private const string fontName = "unnamed_2012";

    private readonly Dictionary<char, Sprite> letters = new Dictionary<char, Sprite>();
    public GameObject letterObj;
    private float letterTimer = -0.1f;
    private const float timePerLetter = 1.0f / 30;
    private int currentLetter;
    private AudioSource letterSound;
    private List<Image> letterReferences;
    private Sprite[] letterSprites;
    private GameObject canvas;
    private float currentX = 15;
    private float currentY = 450;
    private const string teststr = "* the quick brown fox jumps over\n  the lazy dog.\n* THE QUICK BROWN FOX JUMPS OVER\n  THE LAZY DOG.\n* Jerry.";

    // string teststr = "* ";

    // Use this for initialization
    private void Start() {
        letterSound = GetComponent<AudioSource>();
        canvas = GameObject.Find("Canvas");
        letterSprites = Resources.LoadAll<Sprite>("Fonts/" + fontName);
        foreach (Sprite s in letterSprites) {
            string letterName = s.name;
            if (letterName.Length == 1) {
                letters.Add(letterName[0], s);
            } else
                switch (letterName) {
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
        letterReferences = new List<Image>();
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

            ltrImg.sprite = letters[letters.ContainsKey(teststr[i]) ? teststr[i] : '?'];

            letterReferences.Add(ltrImg);

            ltrRect.position = new Vector2(currentX, currentY + (letters.ContainsKey(teststr[i]) ? letters[teststr[i]].border.w - letters[teststr[i]].border.y : 0));
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
        if (!(letterTimer > timePerLetter)) return;
        if (currentLetter >= letterReferences.Count) return;
        if (teststr[currentLetter] == '\n')
            letterTimer = -1.0f;
        else {
            letterTimer                             = 0.0f;
            letterReferences[currentLetter].enabled = true;
            letterSound.Play();
        }
        currentLetter++;
    }

    public void OnGUI() {
        if (Event.current.type != EventType.KeyDown) return;
        char c = Event.current.character;
        switch (c) {
            case '\0': return;
            case '\n': currentX =  15;
                       currentY -= 28;
                       return;
        }

        GameObject    singleLtr = Instantiate(letterObj);
        RectTransform ltrRect   = singleLtr.GetComponent<RectTransform>();
        Image         ltrImg    = singleLtr.GetComponent<Image>();

        ltrRect.SetParent(canvas.transform);

        ltrImg.sprite    = letters[letters.ContainsKey(c) ? c : '?'];
        ltrRect.position = new Vector2(currentX, currentY + (letters.ContainsKey(c) ? letters[c].border.w - letters[c].border.y : 0));
        ltrImg.SetNativeSize();
        ltrImg.enabled = true;

        currentX += ltrRect.rect.width + 2;
        letterSound.Play();
    }
}