using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Behaviour that retrieves resources from the built-in registry rather than setting them on the components in the Unity Editor.
/// </summary>
class AutoloadResourcesFromRegistry : MonoBehaviour {
    [Header("Image Resource")]
    public string SpritePath;
    [Header("Audio Resource")]
    public bool Loop;

    private bool loadRequested;
    private int tries;

    private void Start() {
        LateStart();
    }

    private void OnEnable() {
        if (UnitaleUtil.IsOverworld) Fading.StartFade += LateStart;
        StaticInits.Loaded += LateStart;
    }

    private void OnDisable() {
        if (UnitaleUtil.IsOverworld) Fading.StartFade -= LateStart;
        StaticInits.Loaded -= LateStart;
    }

    private void LateStart() {
        if (!StaticInits.Initialized) return;
        loadRequested = true;
        if (string.IsNullOrEmpty(SpritePath))
            Destroy(this);
        else if (GlobalControls.isInFight || StaticInits.MODFOLDER == FindObjectOfType<MapInfos>().modToLoad) {
            Sprite spr = null;
            try { spr = SpriteRegistry.Get(SpritePath); }
            catch (MoonSharp.Interpreter.ScriptRuntimeException ex) {
                UnitaleUtil.DisplayLuaError("Autoloading a resource - " + SpritePath, ex.DecoratedMessage != null ? UnitaleUtil.FormatErrorSource(ex.DecoratedMessage, ex.Message) + ex.Message : ex.Message);
            }

            if (spr == null) {
                // Needs to wait for mod loading
                if (tries < 10) tries++;
                else            UnitaleUtil.DisplayLuaError("AutoloadResourcesFromRegistry", "You tried to load the sprite \"" + SpritePath + "\", but it doesn't exist.");
                return;
            }
            Image                  img  = GetComponent<Image>();
            SpriteRenderer         img2 = GetComponent<SpriteRenderer>();
            ParticleSystemRenderer img3 = GetComponent<ParticleSystemRenderer>();
            if (img != null || img2 != null) {
                LuaSpriteController sprCtrl = LuaSpriteController.GetOrCreate(gameObject);
                sprCtrl.Set(SpritePath);
            } else if (img3 != null)
                img3.material.mainTexture = spr.texture;
            else
                throw new CYFException("The GameObject " + name + " doesn't have an Image, Sprite Renderer or Particle System component.");

            Destroy(this);
        }
    }

    public void Update() {
        if (loadRequested)
            LateStart();
    }
}
