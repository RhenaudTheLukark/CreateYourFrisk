using UnityEngine;
using UnityEngine.SceneManagement;

public class Fading : MonoBehaviour {

    public Texture2D fadeOutTexture;    // The texture that will overlay the screen. This can be a black image or a loading graphic
    public float fadeSpeed = 0.5f;      // The fading speed
    public float alpha = 1.0f;         // The texture's alpha between 0 and 1

    private int drawDepth = -1000;      // The texture's order in the draw hierarchy: a low number means it renders on top
    private int fadeDir = -1;           // The direction to fade : in = -1 or out = 1
    private bool needFalse;
    
    void Awake() { SceneManager.sceneLoaded += LoadScene; }

    void OnGUI () {
        // Fade in/out the alpha value using a direction, a speed and Time.deltatime to convert the operations to seconds
        alpha += fadeDir * fadeSpeed * Time.deltaTime;

        // Force (clamp) the number between 0 and 1 because GUI.color uses alpha values between 0 and 1
        alpha = Mathf.Clamp01(alpha);

        if (needFalse)
            if (alpha <= 0) {
                needFalse = false;
                alpha = 0;
                PlayerOverworld.inText = false;  //UnitaleUtil.writeInLogAndDebugger("EndFade false");
            } else {
                PlayerOverworld.inText = true;  //UnitaleUtil.writeInLogAndDebugger("WhileFade true");
            }

        // Set color of our GUI (in this case our texture). All color values remain the same and the Alpha is set to the alpha variable
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);                // Set the alpha value
        GUI.depth = drawDepth;                                                              // Make the black texture render on top (drawn last)
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeOutTexture);       // Draw the texture to fit the entire screen area
    }

    // Sets fadeDir to the direction parameter making the scene fade in if -1 and out if 1
    public float BeginFade (int direction) {
        fadeDir = direction;
        needFalse = true;
        return (fadeSpeed);     // Return the fadeSpeed variable so it's easy to time the Application.LoadLevel();
    }

    // LoadScene is called when a level is loaded. It takes loaded level index (int) as a parameter so you can limit the fade in to certain scenes
    public void LoadScene(Scene scene, LoadSceneMode mode) {    
        int index = SceneManager.GetActiveScene().buildIndex;
        if (index != 1)
            BeginFade(-1);          // Call the fade in function
    }
}