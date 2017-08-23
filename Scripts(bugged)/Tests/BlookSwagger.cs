using UnityEngine;
using UnityEngine.UI;

// spook wiggling class - this wiggles the blook
public class BlookSwagger : MonoBehaviour
{
    private RectTransform self;
    private Image selfImg;
    private Color selfColor;
    private float xOrigin;
    private float yOrigin;

    private void Start()
    {
        self = GetComponent<RectTransform>();
        selfImg = GetComponent<Image>();
        selfColor = selfImg.color;
        xOrigin = self.anchoredPosition.x;
        yOrigin = self.anchoredPosition.y;
    }

    private void Update()
    {
        self.anchoredPosition = new Vector2(xOrigin, yOrigin + 10 * Mathf.Sin(Time.time));
        selfColor.a = 0.7f + 0.2f * Mathf.Sin(1f + Time.time * 0.6f);
        selfImg.color = selfColor;
    }
}