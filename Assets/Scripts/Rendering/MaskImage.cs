// Base code by nicloay on the Unity forums
// https://answers.unity.com/questions/1243493/invert-ui-mask.html?childToView=1492803#answer-1492803
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MaskImage : Image {
    public bool inverted = false;
    public override Material materialForRendering {
        get {
            Material result = base.materialForRendering;
            if (result.HasProperty("_StencilComp"))
                result.SetInt("_StencilComp", inverted ? 6 : result.GetInt("_StencilComp"));
            if (inverted)
                result.EnableKeyword("CYF_INVERTED_MASK");
            else
                result.DisableKeyword("CYF_INVERTED_MASK");
            return result;
        }
    }
}
