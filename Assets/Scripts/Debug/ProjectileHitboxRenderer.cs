using System.Collections;
using UnityEngine;

/// <summary>
/// Attempts to render hitboxes for projectiles. Debug functionality attached to the Battle scene's camera.
/// </summary>
public class ProjectileHitboxRenderer : MonoBehaviour {
    private GameObject[] gos;
    private Projectile[] projectiles;

    private GameObject root;

    private Vector3 topLeft, topRight, bottomLeft, bottomRight;
    private const int zIndex = -9;
    private Shader shdr;
    private Material mat;

    public static Rect player;
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
        float screenWidth = !Screen.fullScreen ? ScreenResolution.windowSize.x : fsScreenWidth;
        float screenHeight = !Screen.fullScreen ? ScreenResolution.windowSize.y : 480;
        float xOffset = (screenWidth - 640) / 2;
        float yOffset = (screenHeight - 480) / 2;
        foreach (Projectile p in projectiles) {
            GameObject go = p.gameObject;

            bottomRight   =  go.GetComponent<Projectile>().selfAbs.center - cameraOffset + new Vector2(xOffset, yOffset);
            topLeft.Set    (bottomRight.x - go.GetComponent<Projectile>().selfAbs.width / 2, bottomRight.y + go.GetComponent<Projectile>().selfAbs.height / 2, zIndex);
            topRight.Set   (bottomRight.x + go.GetComponent<Projectile>().selfAbs.width / 2, bottomRight.y + go.GetComponent<Projectile>().selfAbs.height / 2, zIndex);
            bottomLeft.Set (bottomRight.x - go.GetComponent<Projectile>().selfAbs.width / 2, bottomRight.y - go.GetComponent<Projectile>().selfAbs.height / 2, zIndex);
            bottomRight.Set(bottomRight.x + go.GetComponent<Projectile>().selfAbs.width / 2, bottomRight.y - go.GetComponent<Projectile>().selfAbs.height / 2, zIndex);

            topLeft.Set    (topLeft.x     / screenWidth, topLeft.y     / screenHeight, zIndex);
            topRight.Set   (topRight.x    / screenWidth, topRight.y    / screenHeight, zIndex);
            bottomLeft.Set (bottomLeft.x  / screenWidth, bottomLeft.y  / screenHeight, zIndex);
            bottomRight.Set(bottomRight.x / screenWidth, bottomRight.y / screenHeight, zIndex);

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

        player = new Rect((PlayerController.instance.playerAbs.x - cameraOffset.x + xOffset) / screenWidth,
                          (PlayerController.instance.playerAbs.y - cameraOffset.y + yOffset) / screenHeight,
                          PlayerController.instance.playerAbs.width / screenWidth, PlayerController.instance.playerAbs.height / screenHeight);

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