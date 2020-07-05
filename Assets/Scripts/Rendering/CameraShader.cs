using UnityEngine;
using UnityEditor;

public class CameraShader : MonoBehaviour {
    public Material material;
    public static LuaSpriteShader luashader;
    public static TextureWrapMode H = TextureWrapMode.Clamp;
    public static TextureWrapMode V = TextureWrapMode.Clamp;

    public void Awake() {
        material = ShaderRegistry.UI_DEFAULT_MATERIAL;
        luashader = new LuaSpriteShader("camera", gameObject);
    }

    private void OnRenderImage(Texture source, RenderTexture destination) {
        source.wrapModeU = H;
        source.wrapModeV = V;
        Graphics.Blit(source, destination, material);
    }
}

#if UNITY_EDITOR
    // Base code by Xaurrien on the Unity forums
    // http://answers.unity.com/answers/975894/view.html
    [CustomEditor(typeof(CameraShader))]
    public class CameraShaderEditor : Editor {
        private CameraShader cs;
        private MaterialEditor editor;

        private void OnEnable() {
            cs = (CameraShader)target;
            if (cs != null && cs.material != null)
                editor = (MaterialEditor)CreateEditor(cs.material);
        }

        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("material"));

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();

                if (editor != null)
                    DestroyImmediate(editor);

                if (cs != null && cs.material != null)
                    editor = (MaterialEditor)CreateEditor(cs.material);
            }

            if (editor == null) return;
            editor.DrawHeader();

            bool isDefault = cs != null && !AssetDatabase.GetAssetPath(cs.material).StartsWith("Assets");
            using (new EditorGUI.DisabledGroupScope(isDefault))
                editor.OnInspectorGUI();
        }

        private void OnDisable() {
            if (editor != null)
                DestroyImmediate(editor);
        }
    }
#endif
