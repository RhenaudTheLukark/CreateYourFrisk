using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Script to attach to gameobjects with UnityEngine.UI.Image components, used to dissolve them into particles.
/// </summary>
public class ParticleDuplicator : MonoBehaviour {
    private const int particleLimit = 16383;

    public void Activate(LuaSpriteController sprctrl) {
        bool hasImage = GetComponent<Image>();
        Sprite sprite = hasImage ? GetComponent<Image>().sprite : GetComponent<SpriteRenderer>().sprite;

        int xLength = Mathf.FloorToInt(Mathf.Abs(sprctrl.xscale) * sprite.rect.width),
            yLength = Mathf.FloorToInt(Mathf.Abs(sprctrl.yscale) * sprite.rect.height);

        // Emit particles from particle system and retrieve into particles array
        // Particle Systems have a limit of 16383 particles because of the new BakeMesh function
        // For that reason, each particle system can have up to 16383 particles
        int totalParticles = xLength * yLength, particleIndex = totalParticles;
        List<ParticleSystem.Particle[]> particleList = new List<ParticleSystem.Particle[]>();
        List<ParticleSystem> particleSystems = new List<ParticleSystem>();

        while (particleIndex > 0) {
            GameObject psgo = Instantiate(Resources.Load<GameObject>("Prefabs/MonsterDuster"));
            psgo.transform.SetParent(gameObject.transform.parent);
            psgo.transform.SetSiblingIndex(gameObject.transform.GetSiblingIndex() + 1);
            ParticleSystem ps = psgo.GetComponent<ParticleSystem>();

            int currParticles = Mathf.Min(particleIndex, particleLimit);
            ps.Emit(currParticles);
            particleIndex -= currParticles;

            particleList.Add(new ParticleSystem.Particle[currParticles]);
            particleSystems.Add(ps);

            ps.GetParticles(particleList[particleList.Count - 1]);
        }

        // Get sprite viewport coordinates and pixel width/height on display
        RectTransform rt = GetComponent<RectTransform>();
        bool posScaleX = sprctrl.xscale > 0, posScaleY = sprctrl.yscale > 0;
        // Apply horizontal negative scale
        float radSprRot = Mathf.Deg2Rad * sprctrl.rotation * (posScaleX ? 1 : -1);
        // Apply vertical negative scale
        if (!posScaleY) radSprRot -= (radSprRot - Mathf.PI) * 2;
        // Take in account the pivot and scale
        float distFromPivotX = rt.pivot.x * rt.sizeDelta.x * (posScaleX ? 1 : -1) + (posScaleX ? 0 : rt.sizeDelta.x),
              distFromPivotY = rt.pivot.y * rt.sizeDelta.y * (posScaleY ? 1 : -1) + (posScaleY ? 0 : rt.sizeDelta.y);
        Vector2 bottomLeft = new Vector2(rt.position.x - Mathf.Cos(radSprRot) * distFromPivotX + Mathf.Sin(radSprRot) * distFromPivotY,
                                         rt.position.y - Mathf.Sin(radSprRot) * distFromPivotX - Mathf.Cos(radSprRot) * distFromPivotY);

        // Modify particle placement to reform the original sprite, and put back into particle system
        Vector2 movementPerHorzPix = new Vector2(Mathf.Cos(radSprRot),  Mathf.Sin(radSprRot));
        Vector2 movementPerVertPix = new Vector2(-Mathf.Sin(radSprRot), Mathf.Cos(radSprRot));

        float maxY = -9999, minY = 9999;
        Vector2[] particlePos = new Vector2[xLength * yLength];

        int currentPS = 0;

        for (int y = 0; y < yLength; y++) {
            float realY = posScaleY ? y / sprctrl.yscale : (y / sprctrl.yscale + sprite.rect.height - 1);
            int usedY = posScaleY ? Mathf.FloorToInt(realY) : Mathf.CeilToInt(realY);
            for (int x = 0; x < xLength; x++) {
                // Update particle system index
                int particleID = particleIndex % particleLimit;
                if (particleIndex % particleLimit == 0 && particleIndex > 0)
                    currentPS++;

                float realX = posScaleX ? x / sprctrl.xscale : (x / sprctrl.xscale + sprite.rect.width - 1);
                int usedX = posScaleX ? Mathf.FloorToInt(realX) : Mathf.CeilToInt(realX);
                Color c = sprite.texture.GetPixel((int)sprite.rect.x + usedX, (int)sprite.rect.y + usedY) * GetComponent<Image>().color;
                if (c.a == 0.0f || c.r + c.b + c.g == 0.0f)
                    continue;

                Vector2 pos = new Vector2(bottomLeft.x + x * movementPerHorzPix.x + y * movementPerVertPix.x,
                                          bottomLeft.y + x * movementPerHorzPix.y + y * movementPerVertPix.y);
                particleList[currentPS][particleID].position = pos;
                particlePos[particleIndex] = pos;
                particleList[currentPS][particleID].startColor = c;
                particleList[currentPS][particleID].startSize = 1; // we have to assume a square aspect ratio for pixels here
                particleIndex++;

                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
            }
        }

        particleIndex = -1;
        currentPS = 0;

        // Make so higher pixels are activated first, no matter the rotation/scaling of the sprite
        for (int y = 0; y < yLength; y++)
            for (int x = 0; x < xLength; x++) {
                particleIndex++;
                // Update particle system index
                int particleID = particleIndex % particleLimit;
                if (particleIndex % particleLimit == 0 && particleIndex > 0) {
                    // Actually set the particles once everything's done
                    particleSystems[currentPS].SetParticles(particleList[currentPS], particleList[currentPS].Length);
                    currentPS++;
                }

                if (particlePos[y * xLength + x].y == 0)
                    continue;
                particleList[currentPS][particleID].remainingLifetime = particleList[currentPS][particleID].startLifetime = (maxY - particlePos[y * xLength + x].y) / (maxY - minY) * 1.5f + Random.value * 0.3f;
            }

        sprctrl.alpha = 0;
    }
}