using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for all the lifebars in the game. To be used with the HPBar prefab.
/// </summary>
public class LifeBarController : MonoBehaviour {
    public Color fillColor;
    public Color backgroundColor;
    public Image fill;
    public Image background;

    private float currentFill = 1.0f;
    private float oldFill = 1.0f;
    private float desiredFill = 1.0f;
    private float fillLinearTime = 1.0f; // how many seconds does it take to go from current healthbar position to new healthbar position
    private float fillTimer = 0.0f;
    private float totalwidth = 0;
    public bool player = false;
    public bool whenDamage = false;
    public float whenDamageValue = 0.0f;

    /// <summary>
    /// Change the colours of the healthbar's images accordingly.
    /// </summary>
    private void Start() {
        totalwidth = fill.rectTransform.rect.width;
        background.color = backgroundColor;
        fill.color = fillColor;
        // ensure proper layering because tinkering with the prefab screws it up
        background.transform.SetAsLastSibling();
        fill.transform.SetAsLastSibling();
    }

    /// <summary>
    /// Immediately set the healthbar's fill to this value.
    /// </summary>
    /// <param name="fillvalue">Healthbar fill in range of [0.0, 1.0].</param>
    public void setInstant(float fillvalue) {
        currentFill = fillvalue;
        desiredFill = fillvalue;
        //fill.fillAmount = fillvalue;
        //fill.rectTransform.sizeDelta = new Vector2(1, fillvalue);
        if (player)           fill.rectTransform.offsetMax = new Vector2(-(1 - currentFill) * 90, fill.rectTransform.offsetMin.y);
        else if (whenDamage)  fill.rectTransform.offsetMax = new Vector2(-(1 - currentFill) * whenDamageValue, fill.rectTransform.offsetMin.y);
        else {
            if (fillvalue > 1)
                fillvalue = 1;
            if (PlayerCharacter.instance.MaxHP > 100)  fill.rectTransform.offsetMax = new Vector2(-(100 * (1-fillvalue)) * 1.2f, fill.rectTransform.offsetMin.y);
            else                              fill.rectTransform.offsetMax = new Vector2(-(PlayerCharacter.instance.MaxHP * (1 - fillvalue)) * 1.2f, fill.rectTransform.offsetMin.y);
        }
        //fill.rectTransform.offsetMax = new Vector2(-(1 - fillvalue) * PlayerCharacter.instance.MaxHP * 1.2f, fill.rectTransform.offsetMin.y);
    }

    /// <summary>
    /// Start a linear-time transition from current fill to this value.
    /// </summary>
    /// <param name="fillvalue">Healthbar fill in range of [0.0, 1.0].</param>
    public void setLerp(float fillvalue) {
        if (fillvalue > 1)
            fillvalue = 1;
        oldFill = currentFill;
        desiredFill = fillvalue;
        fillTimer = 0.0f;
    }

    /// <summary>
    /// Start a linear-time transition from first value to second value.
    /// </summary>
    /// <param name="originalValue">Value to start the healthbar at, in range of [0.0, 1.0].</param>
    /// <param name="fillValue">Value the healthbar should be at when finished, in range of [0.0, 1.0].</param>
    public void setLerp(float originalValue, float fillValue) {
        setInstant(originalValue);
        setLerp(fillValue);
    }

    /// <summary>
    /// Set the fill color of this healthbar.
    /// </summary>
    /// <param name="c">Color for present health.</param>
    public void setFillColor(Color c) {
        fillColor = c;
        fill.color = c;
    }

    /// <summary>
    /// Set the background color of this healthbar.
    /// </summary>
    /// <param name="c">Color for missing health.</param>
    public void setBackgroundColor(Color c) {
        backgroundColor = c;
        background.color = c;
    }

    /// <summary>
    /// Sets visibility for the image components of the healthbar.
    /// </summary>
    /// <param name="visible">True for visible, false for hidden.</param>
    public void setVisible(bool visible) {
        foreach (Image img in GetComponentsInChildren<Image>())
            img.enabled = visible;
    }

    /// <summary>
    /// Takes care of moving the healthbar to its intended position.
    /// </summary>
    private void Update() {
        if (currentFill == desiredFill || UIController.instance.frozenState != UIController.UIState.PAUSE) return;

        currentFill = Mathf.Lerp(oldFill, desiredFill, fillTimer / fillLinearTime);
        //fill.fillAmount = currentFill;
        //fill.rectTransform.sizeDelta = new Vector2(0, -(1-currentFill) * totalwidth);
        if (player)           fill.rectTransform.offsetMax = new Vector2(-(1 - currentFill) * PlayerCharacter.instance.HP * 1.2f, fill.rectTransform.offsetMin.y);
        else if (whenDamage)  fill.rectTransform.offsetMax = new Vector2(-(1 - currentFill) * whenDamageValue, fill.rectTransform.offsetMin.y);
        else                  fill.rectTransform.offsetMax = new Vector2(-(1 - currentFill) * totalwidth, fill.rectTransform.offsetMin.y);
        fillTimer += Time.deltaTime;
    }
}