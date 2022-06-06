using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class used to draw a warning if retromode is on.
/// </summary>
public class RetromodeWarning : MonoBehaviour {
    private float hoverTimer;
    private bool hovered;

    private void Start () {
        if (!GlobalControls.retroMode)
            Destroy(gameObject);
        transform.position = new Vector3(320, -140, transform.position.z);
    }

    /// <summary>
    /// Will move the warning and increase its alpha if the mouse hovers over it
    /// </summary>
    private void Update () {
        hoverTimer = Mathf.Clamp01(hoverTimer + (hovered ? 1 : -1) * Time.deltaTime * 3);
        transform.position = new Vector3(320, (1 - hoverTimer) * -140, transform.position.z);
        transform.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f + 0.5f * hoverTimer);
        hovered = false;
    }

    private void OnMouseOver() {
        hovered = true;
    }
}
