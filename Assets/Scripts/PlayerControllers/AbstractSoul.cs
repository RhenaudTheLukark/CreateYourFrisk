using UnityEngine;

public abstract class AbstractSoul {
    public float realSpeed;
    public float speed; // actual player speed used in game update cycles
    private bool isHalfSpeed;

    //private PlayerController player;

    protected AbstractSoul() { realSpeed = speed = ControlPanel.instance.PlayerMovementPerSec; }

    public abstract Color color { get; }

    public void SetSpeed(float s) {
        realSpeed = s;
        speed = realSpeed / (isHalfSpeed ? 2 : 1);
    }

    public void setHalfSpeed(bool _isHalfSpeed) {
        speed = realSpeed / (_isHalfSpeed ? 2 : 1);
        isHalfSpeed = _isHalfSpeed;
    }

    public abstract Vector2 GetMovement(float xDir, float yDir);

    public virtual void PostMovement(float xDelta, float yDelta) { }
}