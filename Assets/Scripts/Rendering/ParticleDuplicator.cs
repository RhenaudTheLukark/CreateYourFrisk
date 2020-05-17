using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script to attach to gameobjects with UnityEngine.UI.Image components, used to dissolve them into particles.
/// </summary>
public class ParticleDuplicator : MonoBehaviour {
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    public void Activate(LuaSpriteController sprctrl) {
        Sprite sprite;
        if (sprctrl.img.GetComponent<Image>()) sprite = sprctrl.img.GetComponent<Image>().sprite;
        else                                   sprite = sprctrl.img.GetComponent<SpriteRenderer>().sprite;

        int xLength = Mathf.Abs(Mathf.FloorToInt(sprctrl.xscale * sprite.texture.width)),
            yLength = Mathf.Abs(Mathf.FloorToInt(sprctrl.yscale * sprite.texture.height));
        GetComponentsInChildren<ParticleSystem>(true);
        ps = FindObjectOfType<ParticleSystem>();
        ps.transform.SetAsLastSibling();

        //Emit particles from particle system and retrieve into particles array
        particles = new ParticleSystem.Particle[xLength * yLength];
        ps.Emit(particles.Length);
        ps.GetParticles(particles);

        //Get sprite viewport coordinates and pixel width/height on display
        RectTransform rt = GetComponent<RectTransform>();
        //Vector2 bottomLeft = new Vector2((rt.position.x - rt.rect.width / 2) / (float)Screen.width, (rt.position.y) / (float)Screen.height);
        //Vector2 topRight = new Vector2((rt.position.x + rt.rect.width / 2) / (float)Screen.width, (rt.position.y + rt.rect.height) / (float)Screen.height);
        Vector2 bottomLeft = new Vector2(rt.position.x - rt.pivot.x * rt.sizeDelta.x, rt.position.y - rt.pivot.y * rt.sizeDelta.y);
        //Vector2 topRight = new Vector2(rt.position.x + (1 - rt.anchorMin.x) * rt.sizeDelta.x, rt.position.y + (1 - rt.anchorMin.x) * rt.sizeDelta.y);
        //Vector2 vpbl = Camera.main.ViewportToWorldPoint(bottomLeft);
        //Vector2 vptr = Camera.main.ViewportToWorldPoint(topRight);
        //float pxWidth = (topRight.x - vpbl.x) / rt.sizeDelta.x;
        //float pxHeight = (topRight.y - vpbl.y) / rt.sizeDelta.y;
        //Modify particle placement to reform the original sprite, and put back into particle system
        int particleCount = 0;
        bool xIncrement = sprctrl.xscale > 0, yIncrement = sprctrl.yscale > 0;
        for (int y = 0; y < yLength; y++) {
            float REALY = yIncrement ? y / sprctrl.yscale : (y / sprctrl.yscale + sprite.texture.height - 1);
            int realY = yIncrement ? Mathf.FloorToInt(REALY) : Mathf.CeilToInt(REALY);
            float yFrac = (yLength - y) / (float)yLength;
            for (int x = 0; x < xLength; x++) {
                float REALX = xIncrement ? x / sprctrl.xscale : (x / sprctrl.xscale + sprite.texture.width - 1);
                int realX = yIncrement ? Mathf.FloorToInt(REALX) : Mathf.CeilToInt(REALX);
                Color c = sprite.texture.GetPixel(realX, realY);
                if (c.a == 0.0f || (c.r + c.b + c.g) == 0.0f)
                    continue;
                //particles[particleCount].position = new Vector3(vpbl.x + x * pxWidth, vpbl.y + y * pxHeight, -5.0f);
                particles[particleCount].position = new Vector3(bottomLeft.x + Mathf.RoundToInt(REALX * sprctrl.xscale) + (xIncrement ? 0 : xLength),
                                                                bottomLeft.y + Mathf.RoundToInt(REALY * sprctrl.yscale) + (yIncrement ? 0 : yLength), -5.0f);
                particles[particleCount].startColor = c;
                particles[particleCount].startSize = 1; // we have to assume a square aspect ratio for pixels here
                particles[particleCount].remainingLifetime = yFrac * 1.5f + UnityEngine.Random.value * 0.3f;
                particles[particleCount].startLifetime = particles[particleCount].remainingLifetime;
                particleCount++;
            }
        }
        ps.SetParticles(particles, particleCount);
        GetComponent<Image>().enabled = false;
        //GameObject.Destroy(gameObject);
    }
}