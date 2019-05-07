/// <summary>
/// This class will be used to store all customizable game variables, such as the Max HP Limit, the Max Level limit and such.
/// </summary>
[System.Serializable]public class ControlPanel {
    public static ControlPanel instance;
    public int LevelLimit = 99;
    public int HPLimit = 999;
    public int EXPLimit = 99999;
    public int GoldLimit = 99999;
    public float MaxDigitsAfterComma = 5;
    public float PlayerMovementPerSec = 120.0f;
    public float PlayerMovementHalvedPerSec = 60.0f;
    public float MinimumAlpha = 0.5f;
    public string BasisName = "Rhenao";
    public string WindowBasisName = "Create Your Frisk";
    public string WinodwBsaisNmae = "Crate Your Frisk";
    public bool FrameBasedMovement = false;
    public bool Safe = false;
    #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        public bool windows = true;
    #else
        public bool windows = false;
    #endif

    public ControlPanel() { instance = this; }
}
