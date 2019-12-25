using UnityEngine;

public class RedSoul : AbstractSoul {
    public RedSoul(PlayerController player) : base(player) { }

    public override Color color {
        get { return Color.red; }
    }

    public override Vector2 GetMovement(float xDir, float yDir) {
        Vector2 newDir = new Vector2(xDir * speed, yDir * speed);
        return newDir;
    }
}