using UnityEngine;

/// <summary>
/// Animator used for sprite animations.
/// </summary>
public class KeyframeCollection : MonoBehaviour {
    /// <summary>
    /// Time that must pass in order for the next frame to appear.
    /// </summary>
    public float timePerFrame = 1 / 30f;
    /// <summary>
    /// Array of Keyframes.
    /// </summary>
    public Keyframe[] keyframes;

    /// <summary>
    /// Timer used to determinewhat frame should be displayed.
    /// </summary>
    internal float currTime = 0;
    /// <summary>
    /// Sprite controller affected by the animation.
    /// </summary>
    internal LuaSpriteController spr;
    /// <summary>
    /// Loop mode of the animation.
    /// See the LoopMode enumeration for more details on the possible values.
    /// </summary>
    internal LoopMode loop = LoopMode.LOOP;
    /// <summary>
    /// Total time of the animation.
    /// </summary>
    private float totalTime;

    /// <summary>
    /// Empty sprite.
    /// </summary>
    public Keyframe EMPTY_KEYFRAME = new Keyframe(SpriteRegistry.EMPTY_SPRITE);

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
    public Keyframe GetCurrent() {
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
