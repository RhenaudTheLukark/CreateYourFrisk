using UnityEngine;

public class KeyframeCollection : MonoBehaviour {
    public float timePerFrame = 1 / 30f;
    public Keyframe[] keyframes;
    public float currTime;
    internal LuaSpriteController spr;
    internal LoopMode loop = LoopMode.LOOP;
    public float totalTime;
    public Keyframe EMPTY_KEYFRAME = new Keyframe(SpriteRegistry.EMPTY_SPRITE);

    public bool paused = false;

    public enum LoopMode { ONESHOT, ONESHOTEMPTY, LOOP }

    public void SetLoop(LoopMode l) {
        loop = l;
        currTime %= totalTime;
    }

    public void Set(Keyframe[] newKeyframes, float newTimePerFrame = 1/30f) {
        keyframes = newKeyframes;
        timePerFrame = newTimePerFrame;
        totalTime = newTimePerFrame * newKeyframes.Length;
        currTime = -Time.deltaTime;
    }

    public Keyframe getCurrent() {
        if (!paused)
            currTime += Time.deltaTime;
        if (loop == LoopMode.LOOP) {
            int index = (int)((currTime % totalTime) / timePerFrame);
            return keyframes[index];
        } else {
            int index = (int)(currTime / timePerFrame);
            if (index < keyframes.Length) return keyframes[index];
            return loop == LoopMode.ONESHOT ? null : EMPTY_KEYFRAME;
        }
    }

    // Gets the index of the current sprite.
    public int getIndex() {
        int index;
        if (loop == LoopMode.LOOP)
            index = (int)((currTime % totalTime) / timePerFrame);
        else
            index = (int)(currTime / timePerFrame);

        return index + 1;
    }

    public bool animationComplete() {
        if (loop == LoopMode.ONESHOT || loop == LoopMode.ONESHOTEMPTY)
            return (currTime / timePerFrame) >= keyframes.Length;
        return false;
    }

    private void Update() { spr.UpdateAnimation(); }
}
