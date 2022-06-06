using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Script to attach to gameobjects with UnityEngine.UI.Image components, used to dissolve them into particles.
/// </summary>
public class ParticleDuplicator : MonoBehaviour {
    private struct ParticleData {
        public readonly float x;
        public readonly float y;
        public readonly Color c;

        public ParticleData(float x, float y, Color c) {
            this.x = x;
            this.y = y;
            this.c = c;
        }
    }
    private const int particleLimit = 16383;

    public void Activate(LuaSpriteController sprctrl) {
        bool hasImage = GetComponent<Image>();
        Sprite sprite = hasImage ? GetComponent<Image>().sprite : GetComponent<SpriteRenderer>().sprite;

        int xLength = Mathf.FloorToInt(Mathf.Abs(sprctrl.xscale) * sprite.rect.width),
            yLength = Mathf.FloorToInt(Mathf.Abs(sprctrl.yscale) * sprite.rect.height);

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

        List<ParticleData> particleData = new List<ParticleData>();
        float maxY = -9999, minY = 9999;

        // Optimization: Only count valid pixels (non-zero alpha)
        for (int y = 0; y < yLength; y++) {
            float realY = posScaleY ? y / sprctrl.yscale : (y / sprctrl.yscale + sprite.rect.height - 1);
            int usedY = posScaleY ? Mathf.FloorToInt(realY) : Mathf.CeilToInt(realY);
            for (int x = 0; x < xLength; x++) {
                float realX = posScaleX ? x / sprctrl.xscale : (x / sprctrl.xscale + sprite.rect.width - 1);
                int usedX = posScaleX ? Mathf.FloorToInt(realX) : Mathf.CeilToInt(realX);
                Color c = sprite.texture.GetPixel((int)sprite.rect.x + usedX, (int)sprite.rect.y + usedY) * GetComponent<Image>().color;
                if (c.a == 0.0f) continue;

                float xPos = bottomLeft.x + x * movementPerHorzPix.x + y * movementPerVertPix.x,
                      yPos = bottomLeft.y + x * movementPerHorzPix.y + y * movementPerVertPix.y;

                particleData.Add(new ParticleData(xPos, yPos, c));

                if (yPos < minY) minY = yPos;
                if (yPos > maxY) maxY = yPos;
            }
        }

        if (particleData.Count == 0)
            return;

        // Emit particles from particle system and retrieve into particles array
        // Particle Systems have a limit of 16383 particles because of the new BakeMesh function
        // For that reason, each particle system can have up to 16383 particles
        List<ParticleSystem.Particle[]> particleList = new List<ParticleSystem.Particle[]>();
        List<ParticleSystem> particleSystems = new List<ParticleSystem>();

        int particlesLeft = particleData.Count;
        while (particlesLeft > 0) {
            GameObject psgo = Instantiate(Resources.Load<GameObject>("Prefabs/MonsterDuster"));
            psgo.transform.SetParent(gameObject.transform.parent);
            psgo.transform.SetSiblingIndex(gameObject.transform.GetSiblingIndex() + 1);
            ParticleSystem ps = psgo.GetComponent<ParticleSystem>();

            int currParticles = Mathf.Min(particlesLeft, particleLimit);
            ps.Emit(currParticles);
            particlesLeft -= currParticles;

            particleList.Add(new ParticleSystem.Particle[currParticles]);
            particleSystems.Add(ps);

            ps.GetParticles(particleList[particleList.Count - 1]);
        }

        int currentPS = 0;
        for (int i = 0; i < particleData.Count; i++) {
            ParticleData data = particleData[i];

            // Update particle system index
            int particleID = i % particleLimit;
            if (i % particleLimit == 0 && i > 0) {
                particleSystems[currentPS].SetParticles(particleList[currentPS], particleList[currentPS].Length);
                currentPS++;
            }

            particleList[currentPS][particleID].position = new Vector2(data.x, data.y);
            particleList[currentPS][particleID].startColor = data.c;
            particleList[currentPS][particleID].startSize = 1; // We have to assume a square aspect ratio for pixels here
            particleList[currentPS][particleID].remainingLifetime = particleList[currentPS][particleID].startLifetime = (maxY - data.y) / (maxY - minY) * 1.5f + Random.value * 0.3f;
        }

        particleSystems[currentPS].SetParticles(particleList[currentPS], particleList[currentPS].Length);

        sprctrl.alpha = 0;
    }
}