using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityUiParticles
{
    [CustomEditor(typeof(ParticleSystemMeshGenerator), false)]
    [CanEditMultipleObjects]
    public class ParticleSystemMeshGeneratorEditor : Editor
    {
        SerializedProperty _material;
        SerializedProperty _trailsMaterial;
        ParticleSystemMeshGenerator _particleSystemMeshGenerator;

        void OnEnable()
        {
            _material = serializedObject.FindProperty("_material");
            _trailsMaterial = serializedObject.FindProperty("_trailsMaterial");
            _particleSystemMeshGenerator = (ParticleSystemMeshGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_material);
            EditorGUILayout.PropertyField(_trailsMaterial);
            serializedObject.ApplyModifiedProperties();

            string error = Check(_particleSystemMeshGenerator);
            if (!string.IsNullOrEmpty(error))
                EditorGUILayout.HelpBox(error, MessageType.Error);
        }

        public static string Check(ParticleSystemMeshGenerator psmg)
        {
            var errors = new List<string>();

            var ps = psmg.GetComponent<ParticleSystem>();
            var psRenderer = psmg.GetComponent<ParticleSystemRenderer>();
            ParticleSystem.MainModule mainModule = ps.main;
            ParticleSystem.TextureSheetAnimationModule texSheetAnimationModule = ps.textureSheetAnimation;

            if (psRenderer.enabled)
                errors.Add("ParticleSystemRenderer has to be disabled for UI Particles");

            // Using Trails module leads to using 2 materials with 2 different textures determined inside each material.
            // Sprites mode in Texture sheet animation module requires CanvasRenderer.SetTexture that overrides texture for all materials.
            // Also the requirement of the 'Sprites' mode that all the sprites were inside the same texture, makes it redundant.
            // Just use the 'Grid' mode instead.
            if (texSheetAnimationModule.enabled && texSheetAnimationModule.mode == ParticleSystemAnimationMode.Sprites)
                errors.Add("Texture sheet animation 'Sprites' mode is unsupported for UI Particles");

            switch (mainModule.simulationSpace)
            {
                case ParticleSystemSimulationSpace.World:
                    if (mainModule.scalingMode != ParticleSystemScalingMode.Hierarchy)
                        errors.Add("Scaling mode for 'World' simulation space has to be 'Hierarchy' in UI Particles");
                    break;

                case ParticleSystemSimulationSpace.Local:
                    if (mainModule.scalingMode != ParticleSystemScalingMode.Local)
                        errors.Add("Scaling mode for 'Local' simulation space has to be 'Local' in UI Particles");
                    break;
            }

            return errors.Count > 0 ? string.Join("\n", errors.ToArray()) : null;
        }
    }
}
