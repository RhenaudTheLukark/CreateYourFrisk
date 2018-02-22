using UnityEngine;

public class KeyframeCollection : MonoBehaviour {
    public float timePerFrame = 1 / 30f;
    public Keyframe[] keyframes;
    internal float currTime = 0;
    internal LuaSpriteController spr;
    internal LoopMode loop = LoopMode.LOOP;
    private float totalTime;
    public Keyframe EMPTY_KEYFRAME = new Keyframe(SpriteRegistry.EMPTY_SPRITE);

    public enum LoopMode { ONESHOT, ONESHOTEMPTY, LOOP }

    public void SetLoop(LoopMode l) {
        loop = l;
        currTime %= totalTime;
    }

    public void Set(Keyframe[] keyframes, float timePerFrame = 1/30f) {
        this.keyframes = keyframes;

        this.timePerFrame = timePerFrame;
        totalTime = timePerFrame * keyframes.Length;
        currTime = -Time.deltaTime;
    }

    public Keyframe getCurrent() {
        currTime += Time.deltaTime;
        if (loop == LoopMode.LOOP) {
            int index = (int)((currTime % totalTime) / timePerFrame);
            return keyframes[index];
        } else {
            int index = (int)(currTime / timePerFrame);
            if (index >= keyframes.Length)
                if (loop == LoopMode.ONESHOT) return null;
                else                          return EMPTY_KEYFRAME;
            return keyframes[index];
        }
    }

    public bool animationComplete() {
        if (loop == LoopMode.ONESHOT)           return (currTime / timePerFrame) >= keyframes.Length;
        else if (loop == LoopMode.ONESHOTEMPTY) return getCurrent() == EMPTY_KEYFRAME;
        return false;
    }

    void Update() { spr.UpdateAnimation(); }
}
