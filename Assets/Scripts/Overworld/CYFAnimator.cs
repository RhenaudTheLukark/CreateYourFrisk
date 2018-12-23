using UnityEngine;

public class CYFAnimator : MonoBehaviour {
    public int movementDirection = 0;
    public string beginAnim = "StopDown";
    public string specialHeader = "";
    public static string specialPlayerHeader = "";
    private int threeFramePass = 0;
    private bool waitForStart = true;
    private LuaSpriteController sprctrl;
    private Vector2 lastPos;
    private bool firstCall = true;

    void OnEnable()  { StaticInits.Loaded += LateStart; }
    void OnDisable() { StaticInits.Loaded -= LateStart; }

    [System.Serializable] //Permits to be able to change the data of anims via the Editor
    public struct Anim {
        public string name;
        public string anims;
        public float transitionTime;
    }
    public Anim[] anims;
    private Anim GetAnimPerName(string name) {
        foreach (Anim anim in anims)
            if (anim.name == name)
                return anim;
        return new Anim();
    }

    // Use this for initialization
    public void LateStart() {
        if (EventManager.instance.sprCtrls.ContainsKey(gameObject.name)) sprctrl = EventManager.instance.sprCtrls[gameObject.name];
        else if (gameObject.name == "Player")                            sprctrl = PlayerOverworld.instance.sprctrl;
        else {
            EventManager.instance.ResetEvents(false);
            if (!EventManager.instance.sprCtrls.ContainsKey(gameObject.name))
                throw new CYFException("A CYFAnimator component must be tied to an event, however the GameObject " + gameObject.name + " doesn't seem to have one.");
        }
        lastPos = gameObject.transform.position;
        waitForStart = false;
        if (firstCall) {
            Anim anim = GetAnimPerName(beginAnim);
            try { sprctrl.SetAnimation(anim.anims.Replace(" ", "").Replace("{", "").Replace("}", "").Split(','), anim.transitionTime); } catch { }
            firstCall = false;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (waitForStart)
            return;
        string animName = gameObject.name == "Player" ? specialPlayerHeader : specialHeader;

        if (GetAnimPerName(animName).name == null) {
            if ((Vector2)gameObject.transform.position != lastPos) {
                animName += "Moving";
                threeFramePass = 0;
            } else if (threeFramePass < 3) {
                animName += "Moving";
                threeFramePass++;
            } else
                animName += "Stop";
            int currentDirection = movementDirection;

            switch (currentDirection) {
                case 2: animName += "Down"; break;
                case 4: animName += "Left"; break;
                case 6: animName += "Right"; break;
                case 8: animName += "Up"; break;
                case 0:
                    if (beginAnim.Contains("Up")) animName += "Up";
                    if (beginAnim.Contains("Right")) animName += "Right";
                    if (beginAnim.Contains("Left")) animName += "Left";
                    if (beginAnim.Contains("Down")) animName += "Down";
                    break;
            }
        }

        if (animName != beginAnim)
            ReplaceAnim(animName);
        lastPos = gameObject.transform.position;
    }

    void ReplaceAnim(string animName) {
        Anim anim = GetAnimPerName(animName);
        try { sprctrl.SetAnimation(anim.anims.Replace(" ", "").Replace("{", "").Replace("}", "").Split(','), anim.transitionTime); } catch { }
        beginAnim = animName;
    }
}
