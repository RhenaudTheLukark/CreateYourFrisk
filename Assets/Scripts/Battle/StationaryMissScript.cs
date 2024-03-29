﻿using UnityEngine;

/// <summary>
/// Fairly hacky way for the static MISS to appear over enemies if you don't press the attack button.
/// </summary>
public class StationaryMissScript : MonoBehaviour {
    private const float secondsToDespawn = 1.5f;
    private float despawnTimer;
    public string text = "MISS";

    public void setXPosition(float xpos) {
        GetComponent<RectTransform>().position = new Vector2(xpos - 55, 430); // 55 is the the fairly static 1/2 width of the miss text
    }

    public void SetText(string _text) { text = _text; }

    private void Start () {
        TextManager mgr = GetComponent<TextManager>();
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
