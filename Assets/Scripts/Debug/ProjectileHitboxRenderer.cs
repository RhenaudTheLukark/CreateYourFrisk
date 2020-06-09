using System.Collections;
using UnityEngine;

/// <summary>
/// Attempts to render hitboxes for projectiles. Debug functionality attached to the Battle scene's camera.
/// </summary>
public class ProjectileHitboxRenderer : MonoBehaviour {
    private GameObject[] gos;
    private Projectile[] projectiles;

    private GameObject root;

    private Vector3 topLeft;
    private Vector3 topRight;
    private Vector3 bottomLeft;
    private Vector3 bottomRight;
    private int zIndex = -9;
    private Shader shdr;
    private Material mat;

    public static Rect player = new Rect();
    public static int fsScreenWidth = 0;

    private void Start() {
        root = GameObject.Find("Canvas");
        shdr = Shader.Find("Sprites/Default");
        mat = new Material(shdr);
    }

    private IEnumerator OnPostRender() {
        yield return new WaitForEndOfFrame(); // need to wait for UI to finish drawing first, or it'll appear under the UI
        // note: it kinda still appears under the UI due to its rendering settings
        projectiles = root.GetComponentsInChildren<Projectile>();
        Vector2 cameraOffset = new Vector2(Misc.cameraX, Misc.cameraY);
        float screenWidth  = !Screen.fullScreen ? 640 : fsScreenWidth;
        float borderOffset = !Screen.fullScreen ?   0 : (fsScreenWidth - 640) / 2;
        #if UNITY_EDITOR
            screenWidth = 640;
            borderOffset = 0;
        #endif
        for (int i = 0; i < projectiles.Length; i ++) {
            GameObject go = projectiles[i].gameObject;

            bottomRight = go.GetComponent<Projectile>().selfAbs.center - cameraOffset;
            bottomRight.x += borderOffset;
            topLeft.Set    (bottomRight.x - go.GetComponent<Projectile>().selfAbs.width / 2, bottomRight.y + go.GetComponent<Projectile>().selfAbs.height / 2, zIndex);
            topRight.Set   (bottomRight.x + go.GetComponent<Projectile>().selfAbs.width / 2, bottomRight.y + go.GetComponent<Projectile>().selfAbs.height / 2, zIndex);
            bottomLeft.Set (bottomRight.x - go.GetComponent<Projectile>().selfAbs.width / 2, bottomRight.y - go.GetComponent<Projectile>().selfAbs.height / 2, zIndex);
            bottomRight.Set(bottomRight.x + go.GetComponent<Projectile>().selfAbs.width / 2, bottomRight.y - go.GetComponent<Projectile>().selfAbs.height / 2, zIndex);

            topLeft.Set(topLeft.x / screenWidth, topLeft.y / 480, zIndex);
            topRight.Set(topRight.x / screenWidth, topRight.y / 480, zIndex);
            bottomLeft.Set(bottomLeft.x / screenWidth, bottomLeft.y / 480, zIndex);
            bottomRight.Set(bottomRight.x / screenWidth, bottomRight.y / 480, zIndex);

            // draw boxes
            GL.PushMatrix();
            mat.SetPass(0);
            GL.LoadOrtho();
            //GL.MultMatrix(transform.localToWorldMatrix);
            GL.Begin(GL.LINES);
            GL.Color(Color.magenta);

            GL.Vertex(topLeft); GL.Vertex(topRight);
            GL.Vertex(topRight); GL.Vertex(bottomRight);
            GL.Vertex(bottomRight); GL.Vertex(bottomLeft);
            GL.Vertex(bottomLeft); GL.Vertex(topLeft);

            GL.End();
            GL.PopMatrix();
        }

        player = new Rect((PlayerController.instance.playerAbs.x - cameraOffset.x + borderOffset) / screenWidth,
                          (PlayerController.instance.playerAbs.y - cameraOffset.y) / 480,
                          PlayerController.instance.playerAbs.width / screenWidth, PlayerController.instance.playerAbs.height / 480);

        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadOrtho();
        //GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        GL.Color(Color.black);

        GL.Vertex(new Vector3(player.x, player.y, -9));                                GL.Vertex(new Vector3(player.x + player.width, player.y, -9));
        GL.Vertex(new Vector3(player.x + player.width, player.y, -9));                 GL.Vertex(new Vector3(player.x + player.width, player.y + player.height, -9));
        GL.Vertex(new Vector3(player.x + player.width, player.y + player.height, -9)); GL.Vertex(new Vector3(player.x, player.y + player.height, -9));
        GL.Vertex(new Vector3(player.x, player.y + player.height, -9));                GL.Vertex(new Vector3(player.x, player.y, -9));

        GL.End();
        GL.PopMatrix();
    }
}