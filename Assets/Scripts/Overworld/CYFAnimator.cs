using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CYFAnimator : MonoBehaviour {
    public int movementDirection = 0;
    public string beginAnim = "StopDown";
    public string specialHeader = "";
    private LuaSpriteController sprctrl;

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
    void Start() {
        if (EventManager.instance.sprCtrls.ContainsKey(gameObject.name))  sprctrl = EventManager.instance.sprCtrls[gameObject.name];
        else if (gameObject.name == "Player")                             sprctrl = PlayerOverworld.instance.sprctrl;
        else                                                              throw new CYFException("A CYFAnimator component must be tied to an event, however the GameObject " + gameObject.name + " doesn't seem to be one.");
        Anim anim = GetAnimPerName(beginAnim);
        sprctrl.SetAnimation(anim.anims.Replace(" ", "").Replace("{", "").Replace("}", "").Split(','), anim.transitionTime);
    }
	
	// Update is called once per frame
	void Update () {
        string animName = specialHeader;
        
        if (movementDirection != 0) animName += "Moving";
        else                        animName += "Stop";
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
        if (animName != beginAnim) {
            Anim anim = GetAnimPerName(animName);
            sprctrl.SetAnimation(anim.anims.Replace(" ", "").Replace("{", "").Replace("}", "").Split(','), anim.transitionTime);
            beginAnim = animName;
        }
    }
}
