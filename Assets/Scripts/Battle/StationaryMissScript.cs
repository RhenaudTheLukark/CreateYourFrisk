using UnityEngine;

/// <summary>
/// Fairly hacky way for the static MISS to appear over enemies if you don't press the attack button.
/// </summary>
public class StationaryMissScript : MonoBehaviour {
    private float secondsToDespawn = 1.5f;
    private float despawnTimer = 0.0f;
    public string text = "MISS";

    public void setXPosition(float xpos) {
        GetComponent<RectTransform>().position = new Vector2(xpos - 55, 430); // 55 is the the fairly static 1/2 width of the miss text
    }

    public void SetText(string _text) {
        text = _text;
    }

	void Start () {
        TextManager mgr = GetComponent<TextManager>();
        mgr.SetFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_DAMAGETEXT_NAME));
        mgr.SetText(new TextMessage("[color:c0c0c0]" + text, false, true));
	}

    void Update(){
        if (UIController.instance.frozenState != UIController.UIState.PAUSE)
            return;

        despawnTimer += Time.deltaTime;
        if (despawnTimer > secondsToDespawn)
            Destroy(this.gameObject);
    }
}
