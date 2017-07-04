using UnityEngine;

public class KeyframeCollection : MonoBehaviour {
    public float timePerFrame = 1 / 30f;
    public Keyframe[] keyframes;
    internal float registrationTime;
    internal LuaSpriteController spr;
    internal LoopMode loop = LoopMode.LOOP;
    private float totalTime;
    public Keyframe EMPTY_KEYFRAME = new Keyframe(SpriteRegistry.EMPTY_SPRITE);

    public enum LoopMode { ONESHOT, ONESHOTEMPTY, LOOP }

    public void Set(Keyframe[] keyframes, float timePerFrame = 1/30f) {
        this.keyframes = keyframes;

        this.timePerFrame = timePerFrame;
        totalTime = timePerFrame * keyframes.Length;
        registrationTime = Time.time;
    }

    public Keyframe getCurrent() {
        if (loop == LoopMode.LOOP) {
            int index = (int)(((Time.time - registrationTime) % totalTime) / timePerFrame);
            return keyframes[index];
        } else {
            int index = (int)((Time.time - registrationTime) / timePerFrame);
            if (index >= keyframes.Length)
                if (loop == LoopMode.ONESHOT) return null;
                else                          return EMPTY_KEYFRAME;
            return keyframes[index];
        }
    }

    public bool animationComplete() {
        if (loop == LoopMode.ONESHOT)           return ((Time.time - registrationTime) / timePerFrame) >= keyframes.Length;
        else if (loop == LoopMode.ONESHOTEMPTY) return getCurrent() == EMPTY_KEYFRAME;
        return false;
    }

    void Update() { spr.UpdateAnimation(); }
}
