using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace UnityUiParticles
{
    using Internal;

    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemMeshGenerator : MaskableGraphic
    {
        static readonly Matrix4x4 ScaleZ = Matrix4x4.Scale(new Vector3(1f, 1f, 0.00001f));

        [SerializeField]
        Material _material;
        [SerializeField]
        Material _trailsMaterial;

        ParticleSystem _particleSystem;
        ParticleSystemRenderer _particleSystemRenderer;
        ParticleSystem.MainModule _mainModule;
        ParticleSystem.TrailModule _trailsModule;
        MeshHelper _meshHelper;
        Material[] _maskMaterials;
        Canvas _parentCanvas;

        protected override void Awake()
        {
            base.Awake();

            _particleSystem = GetComponent<ParticleSystem>();
            _particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
            _mainModule = _particleSystem.main;
            _trailsModule = _particleSystem.trails;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _meshHelper = MeshHelper.Create();
            _maskMaterials = new Material[2];

            BakingCamera.RegisterConsumer();

            _parentCanvas = null;
            Canvas.willRenderCanvases += Refresh;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Canvas.willRenderCanvases -= Refresh;

            _meshHelper.Destroy();
            _meshHelper = null;

            foreach (Material maskMaterial in _maskMaterials)
                StencilMaterial.Remove(maskMaterial);
            _maskMaterials = null;

            BakingCamera.UnregisterConsumer();
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            raycastTarget = false;
            GetComponent<ParticleSystemRenderer>().enabled = false;
        }
#endif

        void Refresh()
        {
            Canvas parentCanvas = GetParentCanvas();
            if (parentCanvas == null)
                return;

            _meshHelper.Clear();

            if (_particleSystem.particleCount > 0)
            {
                Camera meshBakingCamera = BakingCamera.GetCamera(parentCanvas);
                _particleSystemRenderer.BakeMesh(_meshHelper.GetTemporaryMesh(), meshBakingCamera);
                bool isTrailsEnabled = _trailsModule.enabled;

                if (isTrailsEnabled)
                    _particleSystemRenderer.BakeTrailsMesh(_meshHelper.GetTemporaryMesh(), meshBakingCamera);

                if (canvasRenderer.materialCount != (isTrailsEnabled ? 2 : 1))
                    SetMaterialDirty();

                var matrix = _mainModule.simulationSpace == ParticleSystemSimulationSpace.World ?
                    transform.worldToLocalMatrix : Matrix4x4.identity;
                _meshHelper.CombineTemporaryMeshes(ScaleZ * matrix);
            }

            canvasRenderer.SetMesh(_meshHelper.mainMesh);
        }

        Canvas GetParentCanvas()
        {
            if (_parentCanvas == null)
                _parentCanvas = GetComponentInParent<Canvas>();
            return _parentCanvas;
        }

        //
        // Essential overrides
        //

        protected override void UpdateGeometry()
        {
        }

        protected override void OnDidApplyAnimationProperties()
        {
        }

        protected override void UpdateMaterial()
        {
            canvasRenderer.materialCount = _trailsModule.enabled ? 2 : 1;
            canvasRenderer.SetMaterial(GetModifiedMaterial(_material, 0), 0);
            if (_trailsModule.enabled)
                canvasRenderer.SetMaterial(GetModifiedMaterial(_trailsMaterial, 1), 1);
        }

        // Overloaded version for multiple materials
        Material GetModifiedMaterial(Material baseMaterial, int index)
        {
            Material baseMat = baseMaterial;
            if (m_ShouldRecalculateStencil)
            {
                m_ShouldRecalculateStencil = false;

                if (maskable)
                {
                    Transform sortOverrideCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
                    m_StencilValue = MaskUtilities.GetStencilDepth(transform, sortOverrideCanvas) + index;
                }
                else
                {
                    m_StencilValue = 0;
                }
            }

            Mask component = GetComponent<Mask>();
            if (m_StencilValue > 0 && (component == null || !component.IsActive()))
            {
                int stencilId = (1 << m_StencilValue) - 1;
                Material maskMaterial = StencilMaterial.Add(baseMat, stencilId, StencilOp.Keep, CompareFunction.Equal,
                    ColorWriteMask.All, stencilId, 0);
                StencilMaterial.Remove(_maskMaterials[index]);
                _maskMaterials[index] = maskMaterial;
                baseMat = _maskMaterials[index];
            }

            return baseMat;
        }
    }
}
