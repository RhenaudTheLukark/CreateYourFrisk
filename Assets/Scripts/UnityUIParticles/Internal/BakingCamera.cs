using System;
using UnityEngine;

namespace UnityUiParticles.Internal
{
    internal class BakingCamera : MonoBehaviour
    {
        static BakingCamera _instance;
        Camera _camera;
        int _refCount;

        void Awake()
        {
            // Attach camera to special scene
            DontDestroyOnLoad(gameObject);
        }

        public static void RegisterConsumer()
        {
            if (_instance == null)
            {
                var gameObject = new GameObject(typeof(BakingCamera).Name);

                // This camera object is just for internal use
                gameObject.hideFlags = HideFlags.HideAndDontSave;

                _instance = gameObject.AddComponent<BakingCamera>();
                _instance._camera = gameObject.AddComponent<Camera>();
                _instance._camera.orthographic = true;

                // Turn camera off because particle mesh baker will use only camera matrix
                gameObject.SetActive(false);
            }
            _instance._refCount++;
        }

        public static void UnregisterConsumer()
        {
            if (_instance == null)
                throw new Exception("BakingCamera has no instance");

            _instance._refCount--;
            if (_instance._refCount == 0)
            {
                if (Application.isPlaying)
                    Destroy(_instance.gameObject);
                else
                    DestroyImmediate(_instance.gameObject);
                _instance = null;
            }
        }

        public static Camera GetCamera(Canvas canvas)
        {
            // Adjust camera orthographic size to canvas size
            // for canvas-based coordinates of particles' size and speed.
            Vector2 size = ((RectTransform)canvas.transform).rect.size;
            _instance._camera.orthographicSize = Mathf.Max(size.x, size.y) * canvas.scaleFactor;
            return _instance._camera;
        }
    }
}
