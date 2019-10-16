using UnityEngine;

public class CYFAnimator : MonoBehaviour {
    public int movementDirection = 0;
    public string beginAnim = "StopDown";
    public string specialHeader = "";
    public static string specialPlayerHeader = "";
    private int threeFramePass = 3;
    private LuaSpriteController sprctrl;
    private Vector3 lastPos;
    private bool waitingForLateStart = true;
    private string lastHeader = "";

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
        if (waitingForLateStart) {
            ReplaceAnim(beginAnim, true);
            waitingForLateStart = false;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (waitingForLateStart)
            return;
        string animName = gameObject.name == "Player" ? specialPlayerHeader : specialHeader;

        if (GetAnimPerName(animName).name == null) {
            if (gameObject.transform.position != lastPos) {
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
            ReplaceAnim(animName, (gameObject.name == "Player" ? specialPlayerHeader : specialHeader) != lastHeader);
        lastHeader = gameObject.name == "Player" ? specialPlayerHeader : specialHeader;
        lastPos = gameObject.transform.position;
    }

    void ReplaceAnim(string animName, bool headerChanged) {
        Anim anim = GetAnimPerName(animName);
        try { sprctrl.SetAnimation(anim.anims.Replace(" ", "").Replace("{", "").Replace("}", "").Split(','), anim.transitionTime); } 
        catch {
            //if (headerChanged)
                throw new CYFException("Bad animation for event \"" + gameObject.name + "\", animation \"" + animName + "\"");
        }
        beginAnim = animName;
    }
}
