using System.Collections.Generic;
using UnityEngine;

namespace UnityUiParticles.Internal
{
    internal class MeshHelper
    {
        public Mesh mainMesh 
        {
            get 
            {
                return _mainMesh;
            }
        }

        Mesh _mainMesh;
        List<Mesh> _temporaryMeshes;
        int _temporaryMeshIndex;
        CombineInstance[] _meshCombine;

        public static MeshHelper Create()
        {
            return new MeshHelper
            {
                _mainMesh = CreateMesh()
            };
        }

        public void Destroy()
        {
            DestroyMesh(_mainMesh);
            _mainMesh = null;

            if (_temporaryMeshes != null)
            {
                foreach (Mesh temporaryMesh in _temporaryMeshes)
                    DestroyMesh(temporaryMesh);
                _temporaryMeshes = null;
            }

            _meshCombine = null;
        }

        public void Clear()
        {
            _mainMesh.Clear();

            if (_temporaryMeshes != null)
            {
                foreach (Mesh mesh in _temporaryMeshes)
                    mesh.Clear();
            }
        }

        public Mesh GetTemporaryMesh()
        {
            if (_temporaryMeshes == null)
                _temporaryMeshes = new List<Mesh>();

            if (_temporaryMeshIndex >= _temporaryMeshes.Count)
                _temporaryMeshes.Add(CreateMesh());

            return _temporaryMeshes[_temporaryMeshIndex++];
        }

        public void CombineTemporaryMeshes(Matrix4x4 m)
        {
            int temporaryMeshesCount = _temporaryMeshIndex;
            if (_meshCombine == null || _meshCombine.Length != temporaryMeshesCount)
            {
                _meshCombine = new CombineInstance[temporaryMeshesCount];
                for (int i = 0; i < temporaryMeshesCount; i++)
                    _meshCombine[i].mesh = _temporaryMeshes[i];
            }

            for (int i = 0; i < temporaryMeshesCount; i++)
                _meshCombine[i].transform = m;

            _mainMesh.CombineMeshes(_meshCombine, false, true);
            _temporaryMeshIndex = 0;
        }

        static Mesh CreateMesh()
        {
            var mesh = new Mesh();
            mesh.MarkDynamic();
            return mesh;
        }

        static void DestroyMesh(Mesh mesh)
        {
            if (Application.isPlaying)
                Object.Destroy(mesh);
            else
                Object.DestroyImmediate(mesh);
        }
    }
}
