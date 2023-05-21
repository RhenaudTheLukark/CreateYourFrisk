using UnityEngine;

/// <summary>
/// Fairly hacky way for the static MISS to appear over enemies if you don't press the attack button.
/// </summary>
public class StationaryMissScript : MonoBehaviour {
    private TextManager mgr;
    private const float secondsToDespawn = 1.5f;
    private float despawnTimer;
    public string text = "MISS";

    public void setPosition(float xPos, float yPos) {
        mgr.transform.position = new Vector2(xPos - UnitaleUtil.PredictTextWidth(mgr) / 2, Mathf.Min(yPos, 430));
    }

    public void SetText(string _text) { text = _text; }

    private void Awake() {
        mgr = GetComponent<TextManager>();
        mgr.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_DAMAGETEXT_NAME));
        mgr.SetText(new TextMessage("[color:c0c0c0]" + text, false, true));
    }

    private void Update(){
        if (UIController.instance.frozenState != "PAUSE")
            return;

        despawnTimer += Time.deltaTime;
        if (despawnTimer > secondsToDespawn)
            Destroy(gameObject);
    }
}
