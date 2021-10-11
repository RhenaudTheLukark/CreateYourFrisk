using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script to attach to gameobjects with UnityEngine.UI.Image components, used to dissolve them into particles.
/// </summary>
public class ParticleDuplicator : MonoBehaviour {
    private ParticleSystem.Particle[] particles;
    private Vector2[] particlePos;

    public void Activate(LuaSpriteController sprctrl, ParticleSystem ps) {
        bool hasImage = GetComponent<Image>();
        Sprite sprite = hasImage ? GetComponent<Image>().sprite : GetComponent<SpriteRenderer>().sprite;

        int xLength = Mathf.FloorToInt(Mathf.Abs(sprctrl.xscale) * sprite.rect.width),
            yLength = Mathf.FloorToInt(Mathf.Abs(sprctrl.yscale) * sprite.rect.height);
        ps.transform.SetAsLastSibling();

        //Emit particles from particle system and retrieve into particles array
        particles = new ParticleSystem.Particle[xLength * yLength];
        ps.Emit(particles.Length);
        ps.GetParticles(particles);

        //Get sprite viewport coordinates and pixel width/height on display
        RectTransform rt = GetComponent<RectTransform>();
        bool posScaleX = sprctrl.xscale > 0, posScaleY = sprctrl.yscale > 0;
        //Apply horizontal negative scale
        float radSprRot = Mathf.Deg2Rad * sprctrl.rotation * (posScaleX ? 1 : -1);
        //Apply vertical negative scale
        if (!posScaleY) radSprRot -= (radSprRot - Mathf.PI) * 2;
        // Take in account the pivot and scale
        float distFromPivotX = rt.pivot.x * rt.sizeDelta.x * (posScaleX ? 1 : -1) + (posScaleX ? 0 : rt.sizeDelta.x),
              distFromPivotY = rt.pivot.y * rt.sizeDelta.y * (posScaleY ? 1 : -1) + (posScaleY ? 0 : rt.sizeDelta.y);
        Vector2 bottomLeft = new Vector2(rt.position.x - Mathf.Cos(radSprRot) * distFromPivotX + Mathf.Sin(radSprRot) * distFromPivotY,
                                         rt.position.y - Mathf.Sin(radSprRot) * distFromPivotX - Mathf.Cos(radSprRot) * distFromPivotY);

        //Modify particle placement to reform the original sprite, and put back into particle system
        int particleCount = 0;

        Vector2 movementPerHorzPix = new Vector2(Mathf.Cos(radSprRot),  Mathf.Sin(radSprRot));
        Vector2 movementPerVertPix = new Vector2(-Mathf.Sin(radSprRot), Mathf.Cos(radSprRot));

        float maxY = -9999, minY = 9999;
        particlePos = new Vector2[xLength * yLength];

        for (int y = 0; y < yLength; y++) {
            float realY = posScaleY ? y / sprctrl.yscale : (y / sprctrl.yscale + sprite.rect.height - 1);
            int usedY = posScaleY ? Mathf.FloorToInt(realY) : Mathf.CeilToInt(realY);
            for (int x = 0; x < xLength; x++) {
                float realX = posScaleX ? x / sprctrl.xscale : (x / sprctrl.xscale + sprite.rect.width - 1);
                int usedX = posScaleX ? Mathf.FloorToInt(realX) : Mathf.CeilToInt(realX);
                Color c = sprite.texture.GetPixel((int)sprite.rect.x + usedX, (int)sprite.rect.y + usedY) * GetComponent<Image>().color;
                if (c.a == 0.0f || c.r + c.b + c.g == 0.0f)
                    continue;

                Vector2 pos = new Vector2(bottomLeft.x + x * movementPerHorzPix.x + y * movementPerVertPix.x,
                                          bottomLeft.y + x * movementPerHorzPix.y + y * movementPerVertPix.y);
                particles[particleCount].position = pos;
                particlePos[particleCount] = pos;
                particles[particleCount].startColor = c;
                particles[particleCount].startSize = 1; // we have to assume a square aspect ratio for pixels here
                particleCount++;

                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
            }
        }

        // Make so higher pixels are activated first, no matter the rotation/scaling of the sprite
        for (int y = 0; y < yLength; y++)
            for (int x = 0; x < xLength; x++) {
                if (particlePos[y * xLength + x].y == 0)
                    continue;
                particles[y * xLength + x].remainingLifetime = (maxY - particlePos[y * xLength + x].y) / (maxY - minY) * 1.5f + Random.value * 0.3f;
                particles[y * xLength + x].startLifetime = particles[y * xLength + x].remainingLifetime;
            }

        ps.SetParticles(particles, particleCount);
        sprctrl.alpha = 0;
    }
}