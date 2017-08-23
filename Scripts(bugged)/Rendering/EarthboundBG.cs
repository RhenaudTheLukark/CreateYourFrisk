using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A C# version of the Earthbound background which was in the MOTHER 3 / Undertale crossover battle, as seen here:
/// https://www.youtube.com/watch?v=2t8LbQYbFsk
/// Performs terribly but still included for programming reference. The video version was actually a shader.
/// </summary>
public class EarthboundBG : MonoBehaviour
{
    public Sprite layer1;
    public Sprite layer2;
    private Color32[] l1Src;

    private Sprite l1Mod;

    private Dictionary<int, Color32> colorTable = new Dictionary<int, Color32>();

    private BGMode mode = BGMode.SINE;

    private float A = 16.0f; //Amplitude
    private float F = 0.1f; //Frequency
    private float S = 0.1f; //Speed
    private float C = 1.0f; //Offset used for vertical compression mode only

    //private int palIndex = 0;

    public enum BGMode
    {
        SINE,
        INTERLACED_SINE,
        VERTICAL_COMPRESSION
    }

    // Use this for initialization
    private void Start()
    {
        Texture2D l1ModTex = new Texture2D(layer1.texture.width, layer1.texture.height);
        l1Mod = Sprite.Create(l1ModTex, new Rect(0, 0, l1ModTex.width, l1ModTex.height), Vector2.zero);
        if (GetComponent<Image>() != null)
        {
            GetComponent<Image>().sprite = l1Mod;
        }
        l1Mod.name = "test";

        Color32[] imgPalCycle = layer1.texture.GetPixels32();
        foreach (Color32 color in imgPalCycle)
        {
            if (!colorTable.ContainsKey(color.GetHashCode()))
                colorTable.Add(color.GetHashCode(), color);
        }

        l1Src = layer1.texture.GetPixels32();
    }

    public void A_Change(string inField)
    {
        float.TryParse(inField, out A);
    }

    public void F_Change(string inField)
    {
        float.TryParse(inField, out F);
    }

    public void S_Change(string inField)
    {
        float.TryParse(inField, out S);
    }

    public void C_Change(string inField)
    {
        float.TryParse(inField, out C);
    }

    public void setMode(BGMode m)
    {
        this.mode = m;
    }

    private void handleControls()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            setMode(BGMode.SINE);
        else if (Input.GetKeyDown(KeyCode.W))
            setMode(BGMode.INTERLACED_SINE);
        else if (Input.GetKeyDown(KeyCode.E))
            setMode(BGMode.VERTICAL_COMPRESSION);
    }

    // Update is called once per frame
    private void Update()
    {
        handleControls();
        for (int y = 0; y < layer1.texture.height; y++)
        {
            int offset = (int)(A * Mathf.Sin(F * y + S * Time.frameCount));

            int new_x = 0;
            int new_y = y;

            switch (mode)
            {
                case BGMode.SINE:
                    new_x = offset;
                    break;

                case BGMode.INTERLACED_SINE:
                    new_x = (y % 2 == 0) ? offset : -offset;
                    break;

                case BGMode.VERTICAL_COMPRESSION:
                    new_y = Math.Mod((int)(y * C + offset), layer1.texture.height);
                    break;
            }
            for (int x = 0; x < layer1.texture.width; x++)
            {
                new_x = Math.Mod(new_x, layer1.texture.width);
                // Color32 newColor = l1Src[new_y * layer1.texture.width + new_x];
                // int index = Math.mod(colorTable[newColor.GetHashCode()], colorTable.Count);
                // im1Mod[y * layer1.texture.width + x] = colorTable[newColor.GetHashCode()];
                l1Mod.texture.SetPixel(x, y, l1Src[new_y * layer1.texture.width + new_x]);
                new_x++;
            }
        }
        l1Mod.texture.Apply();
    }
}