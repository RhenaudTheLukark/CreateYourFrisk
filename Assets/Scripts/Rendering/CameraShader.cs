using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CameraShader : MonoBehaviour {
    public static Material material;
    public static LuaSpriteShader luashader;
    public static TextureWrapMode H = TextureWrapMode.Clamp;
    public static TextureWrapMode V = TextureWrapMode.Clamp;

    void Awake() {
        material = ShaderRegistry.UI_DEFAULT_MATERIAL;
        luashader = new LuaSpriteShader("camera", gameObject);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        source.wrapModeU = H;
        source.wrapModeV = V;
        Graphics.Blit(source, destination, material);
    }
}
