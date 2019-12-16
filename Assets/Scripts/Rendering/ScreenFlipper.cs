using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]

public class ScreenFlipper : MonoBehaviour {

    new Camera camera;
    public static float xscale = 1;
    public static float yscale = 1;

    void OnPreCull()
    {
        camera.ResetWorldToCameraMatrix();
        camera.ResetProjectionMatrix();
        Vector3 scale = new Vector3(xscale, yscale, 1);
        camera.projectionMatrix = camera.projectionMatrix * Matrix4x4.Scale(scale);
    }
    void OnPreRender()
    {
        GL.invertCulling = (xscale != 1) || (yscale != 1);
    }

    void OnPostRender()
    {
        GL.invertCulling = false;
    }

    // Use this for initialization
    void Start () {
        camera = GetComponent<Camera>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
