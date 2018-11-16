using UnityEngine;

/// <summary>
/// Animator used for sprite animations.
/// </summary>
public class KeyframeCollection : MonoBehaviour {
    public float timePerFrame = 1 / 30f;     // Time that must pass in order for the next frame to appear.
    public Keyframe[] keyframes;             // Array of Keyframes.
    public float currTime = 0;               // Timer used to determine what sprite should be displayed.
    internal LuaSpriteController spr;        // Sprite controller affected by the animation.
    internal LoopMode loop = LoopMode.LOOP;  // Loop mode of the animation.
                                             // See the LoopMode enumeration for more details on the possible values.
    private float totalTime;                 // Total time of the animation.
    public Keyframe EMPTY_KEYFRAME = new Keyframe(SpriteRegistry.EMPTY_SPRITE); // Empty sprite.
    
    public bool paused = false;              // Controls whether an animation is playing or paused.

    /// <summary>
    /// Determines if the animation repeats or not.
    /// LOOP: The animation repeats.
    /// ONESHOT: The animation doesn't repeat.
    /// ONESHTOEMPTY: The animation doesn't repeat and an empty sprite appears at 
    ///               the end of the animation instead of the animation's last frame.
    /// </summary>
    public enum LoopMode { ONESHOT, ONESHOTEMPTY, LOOP }

    /// <summary>
    /// Sets the animation's loop value.
    /// </summary>
    /// <param name="l">loop mode. See the LoopMode enumeration for more details on the possible values.</param>
    public void SetLoop(LoopMode l) {
        loop = l;
        currTime %= totalTime;
    }

    /// <summary>
    /// Initialize the animation.
    /// </summary>
    /// <param name="keyframes">List of Keyframes that'll constitute the animation.</param>
    /// <param name="timePerFrame">Time between each frame.</param>
    public void Set(Keyframe[] keyframes, float timePerFrame = 1/30f) {
        this.keyframes = keyframes;

        this.timePerFrame = timePerFrame;
        totalTime = timePerFrame * keyframes.Length;
        currTime = -Time.deltaTime;
    }

    /// <summary>
    /// Check the animation's time and returns the current Keyframe in use.
    /// </summary>
    /// <returns>The current Keyframe in use.</returns>
    public Keyframe getCurrent() {
        if (!paused)
            // Increase the timer.
            currTime += Time.deltaTime;
        // The animation repeats.
        if (loop == LoopMode.LOOP) {
            // currTime must stay between 0 and totalTime to have a valid Keyframe index.
            int index = (int)((currTime % totalTime) / timePerFrame);
            return keyframes[index];
        // The animation doesn't repeat.
        } else {
            int index = (int)(currTime / timePerFrame);
            // Animation finished.
            if (index >= keyframes.Length)
                // No frame returned on ONESHOT.
                if (loop == LoopMode.ONESHOT) return null;
                // ONESHOTEMPTY: Returns an empty frame.
                else                          return EMPTY_KEYFRAME;
            return keyframes[index];
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

    /// <summary>
    /// Checks if the animation is finished or not.
    /// </summary>
    /// <returns>True if the animation is complete, false otherwise.</returns>
    public bool AnimationComplete() {
        // The animation never ends if the animation repeats.
        if (loop == LoopMode.LOOP) return false;
        // The animation is finished when currTime is greater than totalTime.
        else                       return currTime >= totalTime;
    }

    /// <summary>
    /// Updates the animation.
    /// </summary>
    void Update() { spr.UpdateAnimation(); }
}
