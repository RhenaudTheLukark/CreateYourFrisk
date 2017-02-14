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

    private void Start() {
        root = GameObject.Find("Canvas");
        shdr = Shader.Find("Sprites/Default");
        mat = new Material(shdr);
    }

    private IEnumerator OnPostRender() {
        yield return new WaitForEndOfFrame(); // need to wait for UI to finish drawing first, or it'll appear under the UI
        // note: it kinda still appears under the UI due to its rendering settings
        projectiles = root.GetComponentsInChildren<Projectile>();
        gos = new GameObject[projectiles.Length + 1];
        for (int i = 0; i < projectiles.Length; i ++) 
            gos[i] = projectiles[i].gameObject;
        gos[gos.Length - 1] = GameObject.Find("player");
        foreach (GameObject go in gos) {
            if (go == GameObject.Find("player")) {
                bottomRight = go.GetComponent<RectTransform>().position;
                topLeft.Set    (bottomRight.x - go.GetComponent<RectTransform>().rect.width / 4, bottomRight.y + go.GetComponent<RectTransform>().rect.height / 4, zIndex);
                topRight.Set   (bottomRight.x + go.GetComponent<RectTransform>().rect.width / 4, bottomRight.y + go.GetComponent<RectTransform>().rect.height / 4, zIndex);
                bottomLeft.Set (bottomRight.x - go.GetComponent<RectTransform>().rect.width / 4, bottomRight.y - go.GetComponent<RectTransform>().rect.height / 4, zIndex);
                bottomRight.Set(bottomRight.x + go.GetComponent<RectTransform>().rect.width / 4, bottomRight.y - go.GetComponent<RectTransform>().rect.height / 4, zIndex);
            } else /*if (go.GetComponent<Projectile>().ppcollision)*/ {
                bottomRight = go.GetComponent<Projectile>().selfAbs.center;
                topLeft.Set    (bottomRight.x - go.GetComponent<Projectile>().selfAbs.width / 2, bottomRight.y + go.GetComponent<Projectile>().selfAbs.height / 2, zIndex);
                topRight.Set   (bottomRight.x + go.GetComponent<Projectile>().selfAbs.width / 2, bottomRight.y + go.GetComponent<Projectile>().selfAbs.height / 2, zIndex);
                bottomLeft.Set (bottomRight.x - go.GetComponent<Projectile>().selfAbs.width / 2, bottomRight.y - go.GetComponent<Projectile>().selfAbs.height / 2, zIndex);
                bottomRight.Set(bottomRight.x + go.GetComponent<Projectile>().selfAbs.width / 2, bottomRight.y - go.GetComponent<Projectile>().selfAbs.height / 2, zIndex);
            } /*else {
                topLeft.Set    (bottomRight.x - go.GetComponent<RectTransform>().rect.width / 2, bottomRight.y + go.GetComponent<RectTransform>().rect.height / 2, zIndex);
                topRight.Set   (bottomRight.x + go.GetComponent<RectTransform>().rect.width / 2, bottomRight.y + go.GetComponent<RectTransform>().rect.height / 2, zIndex);
                bottomLeft.Set (bottomRight.x - go.GetComponent<RectTransform>().rect.width / 2, bottomRight.y - go.GetComponent<RectTransform>().rect.height / 2, zIndex);
                bottomRight.Set(bottomRight.x + go.GetComponent<RectTransform>().rect.width / 2, bottomRight.y - go.GetComponent<RectTransform>().rect.height / 2, zIndex);
            }*/

            topLeft.Set(topLeft.x / Screen.width, topLeft.y / Screen.height, zIndex);
            topRight.Set(topRight.x / Screen.width, topRight.y / Screen.height, zIndex);
            bottomLeft.Set(bottomLeft.x / Screen.width, bottomLeft.y / Screen.height, zIndex);
            bottomRight.Set(bottomRight.x / Screen.width, bottomRight.y / Screen.height, zIndex);

            // draw boxes
            GL.PushMatrix();
            mat.SetPass(0);
            GL.LoadOrtho();
            //GL.MultMatrix(transform.localToWorldMatrix);
            GL.Begin(GL.LINES);
            if (go == GameObject.Find("player")) GL.Color(Color.black);
            else GL.Color(Color.magenta);

            GL.Vertex(topLeft); GL.Vertex(topRight);
            GL.Vertex(topRight); GL.Vertex(bottomRight);
            GL.Vertex(bottomRight); GL.Vertex(bottomLeft);
            GL.Vertex(bottomLeft); GL.Vertex(topLeft);

            GL.End();
            GL.PopMatrix();
        }
        
        player = new Rect(player.x / Screen.width, player.y / Screen.height, player.width / Screen.width, player.height / Screen.height);

        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadOrtho();
        //GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        GL.Color(Color.yellow);

        GL.Vertex(new Vector3(player.x, player.y, -9));                                GL.Vertex(new Vector3(player.x + player.width, player.y, -9));
        GL.Vertex(new Vector3(player.x + player.width, player.y, -9));                 GL.Vertex(new Vector3(player.x + player.width, player.y + player.height, -9));
        GL.Vertex(new Vector3(player.x + player.width, player.y + player.height, -9)); GL.Vertex(new Vector3(player.x, player.y + player.height, -9));
        GL.Vertex(new Vector3(player.x, player.y + player.height, -9));                GL.Vertex(new Vector3(player.x, player.y, -9));

        GL.End();
        GL.PopMatrix();
    }
}