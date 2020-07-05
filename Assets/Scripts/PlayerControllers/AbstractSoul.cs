using UnityEngine;

public abstract class AbstractSoul {
    protected float speed; // actual player speed used in game update cycles

    //private PlayerController player;

    protected AbstractSoul() { speed = ControlPanel.instance.PlayerMovementPerSec; }

    public abstract Color color { get; }

    public void setHalfSpeed(bool isHalfSpeed) {
        speed = isHalfSpeed ? ControlPanel.instance.PlayerMovementHalvedPerSec : ControlPanel.instance.PlayerMovementPerSec;
    }

    public abstract Vector2 GetMovement(float xDir, float yDir);

    public virtual void PostMovement(float xDelta, float yDelta) { }
}