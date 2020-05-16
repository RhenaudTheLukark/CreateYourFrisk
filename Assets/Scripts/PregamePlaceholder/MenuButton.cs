using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public Color NormalColor;
    public Color HoverColor;

    private float animateTimerMaxValue = 2; // number of frames it takes to animate
    private float animateTimer = 0;
    private int animateDirection = -1; // -1 for Fade Out, 1 for Fade In

    public void Update() {
        if (animateDirection == 1 && animateTimer < animateTimerMaxValue) {
            animateTimer++;

            float mult = animateTimer/animateTimerMaxValue;

            GetComponent<Image>().color = new Color(NormalColor.r + (mult * (HoverColor.r - NormalColor.r)),
                                                    NormalColor.g + (mult * (HoverColor.g - NormalColor.g)),
                                                    NormalColor.b + (mult * (HoverColor.b - NormalColor.b)),
                                                    NormalColor.a + (mult * (HoverColor.a - NormalColor.a)));
        } else if (animateDirection == -1 && animateTimer > 0) {
            animateTimer--;

            float mult = animateTimer/animateTimerMaxValue;

            GetComponent<Image>().color = new Color(NormalColor.r + (mult * (HoverColor.r - NormalColor.r)),
                                                    NormalColor.g + (mult * (HoverColor.g - NormalColor.g)),
                                                    NormalColor.b + (mult * (HoverColor.b - NormalColor.b)),
                                                    NormalColor.a + (mult * (HoverColor.a - NormalColor.a)));
        } else if (animateTimer == 0 && animateDirection != 0) {
            animateDirection = 0;
            GetComponent<Image>().color = NormalColor;
        }
    }

    public void StartAnimation(int dir) {
        animateDirection = dir;
    }

    public void Reset() {
        animateDirection = -1;
        animateTimer = 1;
        Update();
    }

    public void OnPointerEnter(PointerEventData ped) {
        StartAnimation( 1);
    }

    public void OnPointerExit (PointerEventData ped) {
        StartAnimation(-1);
    }
}
