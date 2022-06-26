staticFalseUpperBody = CreateSprite("empty")
staticTail = CreateSprite("Static/tail")
staticTailMask = CreateSprite("Static/tailmask")
staticTailStatic = CreateSprite("Static/staticeffect")

staticLArm = CreateSprite("Static/larm")
staticLHand = CreateSprite("Static/lhand")

staticLegs = CreateSprite("Static/legs")
staticTorso = CreateSprite("Static/torso")

staticRArm = CreateSprite("Static/rarm")
staticRHand = CreateSprite("Static/rhand")

staticFluff = CreateSprite("Static/fluff")
staticFluffMask = CreateSprite("Static/fluffmask")
staticFluffStatic = CreateSprite("Static/staticeffect")

staticFalseHeadBase = CreateSprite("empty")
staticHeadBase = CreateSprite("Static/headbase")
staticFace = CreateSprite("Static/face")
staticFaceMask = CreateSprite("Static/facemask")
staticFaceStatic = CreateSprite("Static/staticeffect")

staticDeathExplosion = CreateSprite("empty")
staticDeathSmokes = { }

staticLEar = CreateSprite("Static/lear")
staticLEarMask = CreateSprite("Static/learmask")
staticLEarStatic = CreateSprite("Static/staticeffect")

staticREar = CreateSprite("Static/rear")
staticREarMask = CreateSprite("Static/rearmask")
staticREarStatic = CreateSprite("Static/staticeffect")

staticHurt = CreateSprite("Static/Hurt/hurt")

staticHurtFluffMask = CreateSprite("Static/Hurt/fluffmask")
staticHurtFluffStatic = CreateSprite("Static/staticeffect")

staticHurtTailMask = CreateSprite("Static/Hurt/tailmask")
staticHurtTailStatic = CreateSprite("Static/staticeffect")

staticHurtFaceMask = CreateSprite("Static/Hurt/facemask")
staticHurtFaceStatic = CreateSprite("Static/staticeffect")

staticHurtLEarMask = CreateSprite("Static/Hurt/learmask")
staticHurtLEarStatic = CreateSprite("Static/staticeffect")

staticHurtREarMask = CreateSprite("Static/Hurt/rearmask")
staticHurtREarStatic = CreateSprite("Static/staticeffect")

staticSpared = CreateSprite("Static/spared")



staticFalseUpperBody.Scale(staticTorso.width * 2, staticTorso.height * 2)
staticTail.Scale(2, 2)
staticTailMask.Scale(2, 2)
staticLArm.Scale(2, 2)
staticLHand.Scale(2, 2)
staticLegs.Scale(2, 2)
staticTorso.Scale(2, 2)
staticRArm.Scale(2, 2)
staticRHand.Scale(2, 2)
staticFluff.Scale(2, 2)
staticFluffMask.Scale(2, 2)
staticFalseHeadBase.Scale(staticHeadBase.width * 2, staticHeadBase.height * 2)
staticHeadBase.Scale(2, 2)
staticFace.Scale(2, 2)
staticFaceMask.Scale(2, 2)
staticDeathExplosion.Scale(2, 2)
staticLEar.Scale(2, 2)
staticLEarMask.Scale(2, 2)
staticREar.Scale(2, 2)
staticREarMask.Scale(2, 2)
staticHurt.Scale(2, 2)
staticHurtFluffMask.Scale(2, 2)
staticHurtTailMask.Scale(2, 2)
staticHurtFaceMask.Scale(2, 2)
staticHurtLEarMask.Scale(2, 2)
staticHurtREarMask.Scale(2, 2)



--[[
Parenting tree:

deathparent
monstersprite
  staticFalseUpperBody +1
    staticTail
      staticTailMask ^
        staticTailStatic X
    staticLArm
      staticLHand
  staticLegs
    staticTorso +1
      staticRArm
        staticRHand
      staticFluff
        staticFluffMask ^
          staticFluffStatic X
      staticHeadBase
        staticLEar
          staticLEarMask ^
            staticLEarStatic X
        staticREar
          staticREarMask ^
            staticREarStatic X
        staticFace ^
          staticFaceMask ^
            staticFaceStatic X
          staticDeathExplosion
  staticHurt ^
    staticHurtFluffMask
      staticHurtFluffStatic X
    staticHurtTailMask
      staticHurtTailStatic X
    staticHurtFaceMask
      staticHurtFaceStatic X
    staticHurtLEarMask
      staticHurtLEarStatic X
    staticHurtREarMask
      staticHurtREarStatic X
  staticSpared ^

^ = same position as parent
X = static sprite
+1 = same position
]]

staticFalseUpperBody.SetParent(monstersprite)
staticTail.SetParent(staticFalseUpperBody)
staticTailMask.SetParent(staticTail)
staticTailStatic.SetParent(staticTailMask)

staticLArm.SetParent(staticFalseUpperBody)
staticLHand.SetParent(staticLArm)

staticLegs.SetParent(monstersprite)
staticTorso.SetParent(staticLegs)

staticRArm.SetParent(staticTorso)
staticRHand.SetParent(staticRArm)

staticFluff.SetParent(staticTorso)
staticFluffMask.SetParent(staticFluff)
staticFluffStatic.SetParent(staticFluffMask)

staticHeadBase.SetParent(staticTorso)

staticLEar.SetParent(staticHeadBase)
staticLEarMask.SetParent(staticLEar)
staticLEarStatic.SetParent(staticLEarMask)

staticREar.SetParent(staticHeadBase)
staticREarMask.SetParent(staticREar)
staticREarStatic.SetParent(staticREarMask)

staticFace.SetParent(staticHeadBase)
staticFaceMask.SetParent(staticFace)
staticFaceStatic.SetParent(staticFaceMask)
staticDeathExplosion.SetParent(staticFace)

staticHurt.SetParent(monstersprite)

staticHurtFluffMask.SetParent(staticHurt)
staticHurtFluffStatic.SetParent(staticHurtFluffMask)

staticHurtTailMask.SetParent(staticHurt)
staticHurtTailStatic.SetParent(staticHurtTailMask)

staticHurtFaceMask.SetParent(staticHurt)
staticHurtFaceStatic.SetParent(staticHurtFaceMask)

staticHurtLEarMask.SetParent(staticHurt)
staticHurtLEarStatic.SetParent(staticHurtLEarMask)

staticHurtREarMask.SetParent(staticHurt)
staticHurtREarStatic.SetParent(staticHurtREarMask)

staticSpared.SetParent(monstersprite)



staticLegs.SetPivot(0.5, 0)
staticLegs.MoveToAbs(monstersprite.absx - 22, monstersprite.absy)
staticTorso.SetAnchor(0.5, 0.9)
staticTorso.SetPivot(0.5, 0.13)
staticTorso.MoveTo(7, 3)
staticFalseUpperBody.SetPivot(0.5, 0.13)
staticFalseUpperBody.MoveToAbs(staticTorso.absx, staticTorso.absy)

staticTail.SetPivot(0.2, 0.17)
staticTail.MoveTo(28, -45)
staticTailMask.MoveTo(0, 0)
staticTailMask.Mask("stencil")
staticTailStatic.MoveTo(0, 0)

staticLArm.SetPivot(0.86, 0.85)
staticLArm.MoveTo(-16, 26)
staticLHand.SetPivot(0.73, 0.22)
staticLHand.MoveTo(-15, -15)

staticRArm.SetPivot(0.23, 0.8)
--staticRArm.MoveToAbs(monstersprite.absx + 5, monstersprite.absy + 154)
staticRArm.MoveTo(20, 26)
staticRHand.SetPivot(0.81, 0.85)
staticRHand.MoveTo(18, -6)

staticFluff.MoveTo(5, 26)
staticFluffMask.MoveTo(0, 0)
staticFluffMask.Mask("stencil")
staticFluffStatic.MoveTo(0, 0)

staticHeadBase.SetPivot(0.45, 0.13)
staticHeadBase.MoveTo(2, 46)

staticLEar.SetPivot(0.75, 0.42)
staticLEar.MoveTo(-30, 12)
staticLEarMask.MoveTo(0, 0)
staticLEarMask.Mask("stencil")
staticLEarStatic.MoveTo(0, 0)

staticREar.SetPivot(0.29, 0.38)
staticREar.MoveTo(40, 10)
staticREarMask.MoveTo(0, 0)
staticREarMask.Mask("stencil")
staticREarStatic.MoveTo(100, 0)

staticFace.SetPivot(0, 0.5)
staticFace.SetAnchor(0, 0.5)
staticFace.MoveTo(0, 0)
staticFaceMask.MoveTo(0, 0)
staticFaceMask.Mask("stencil")
staticFaceStatic.MoveTo(0, 0)

staticDeathExplosion.SetAnchor(0.84, 0.83)
staticDeathExplosion.MoveTo(0, 0)
staticDeathExplosion.loopmode = "ONESHOTEMPTY"

staticHurt.SetPivot(0.5, 0)
staticHurt.MoveToAbs(monstersprite.absx, monstersprite.absy)
staticHurt.alpha = 0

staticHurtFluffMask.MoveTo(5, 18)
staticHurtFluffMask.Mask("stencil")
staticHurtFluffStatic.MoveTo(0, 0)
staticHurtFluffStatic.alpha = 0

staticHurtTailMask.MoveTo(50, -1)
staticHurtTailMask.Mask("stencil")
staticHurtTailStatic.MoveTo(0, 0)
staticHurtTailStatic.alpha = 0

staticHurtFaceMask.MoveTo(1, 64)
staticHurtFaceMask.Mask("stencil")
staticHurtFaceStatic.MoveTo(0, 0)
staticHurtFaceStatic.alpha = 0

staticHurtLEarMask.MoveTo(-26, 93)
staticHurtLEarMask.Mask("stencil")
staticHurtLEarStatic.MoveTo(0, 0)
staticHurtLEarStatic.alpha = 0

staticHurtREarMask.MoveTo(48, 81)
staticHurtREarMask.Mask("stencil")
staticHurtREarStatic.MoveTo(0, 0)
staticHurtREarStatic.alpha = 0

staticSpared.SetPivot(0.5, 0)
staticSpared.MoveToAbs(monstersprite.absx, monstersprite.absy)
staticSpared.color = { 0.5, 0.5, 0.5, 0 }

-- Animation used for the explosion
explosionAnimation = { }
for i = 1, 5 do
    table.insert(explosionAnimation, "Waves/SSLaserPoint/Boom/" .. i)
end

-- Used to know which sprites to update to create a static effect
local staticSprites = {
    { staticTailStatic, 98, 86 },
    { staticFaceStatic, 72, 60 },
    { staticFluffStatic, 94, 64 },
    { staticLEarStatic, 32, 38 },
    { staticREarStatic, 34, 42 },
    { staticHurtFluffStatic, 108, 54 },
    { staticHurtTailStatic, 70, 80 },
    { staticHurtFaceStatic, 40, 34 },
    { staticHurtLEarStatic, 14, 28 },
    { staticHurtREarStatic, 22, 28 }
}

-- Compute how much leeway pixels we have to move the static image around
for _, staticEffect in pairs(staticSprites) do
    staticEffect["diffX"] = 180 - staticEffect[2]
    staticEffect["halfDiffX"] = staticEffect["diffX"] / 2
    staticEffect["diffY"] = 180 - staticEffect[3]
    staticEffect["halfDiffY"] = staticEffect["diffY"] / 2
end

local time = 0
local timer = 0
animation = "Idle"

function SwitchAnimation(anim)
    -- Reset the anim properly
    ResetStatic()
    time = 0
    timer = 0

    -- Differences between the Idle and Death animation
    if anim == "Death" then
        staticSprites[2]["diffX"] = 256 - staticSprites[2][2]
        staticSprites[2]["halfDiffX"] = staticSprites[2]["diffX"] / 2
        staticSprites[2]["diffY"] = 256 - staticSprites[2][3]
        staticSprites[2]["halfDiffY"] = staticSprites[2]["diffY"] / 2

        staticFace.Set("Static/Death/face")
        staticFaceMask.Set("Static/Death/facemask")

        staticDeathExplosion.SetAnimation(explosionAnimation, 1/15)
        Audio.PlaySound("boom")

        staticFalseUpperBody.SetParent(deathParent)
        staticLegs.SetParent(deathParent)
        staticHurt.SetParent(deathParent)
        staticSpared.SetParent(deathParent)
    end

    animation = anim
end

function AnimateStatic()
    -- Update all static sprites so they look random
    if staticSprites[1][1].isactive and timer % 5 == 0 then
        for _, staticEffect in pairs(staticSprites) do
            staticEffect[1].rotation = staticEffect[1].rotation + math.random(1, 3) * 90
            staticEffect[1].Scale(math.random() < .5 and -1 or 1, math.random() < .5 and -1 or 1)
            staticEffect[1].MoveTo(math.random(0, staticEffect["diffX"]) - staticEffect["halfDiffX"],
                                   math.random(0, staticEffect["diffY"]) - staticEffect["halfDiffY"])
        end
    end

    -- Idle looped animation
    if animation == "Idle" then
        staticLegs.yscale = 1.9 + 0.1 * math.cos(time / 2)

        staticTail.rotation = -15 * math.sin(time * 3)

        staticTorso.MoveTo(7, 3 + 2 * math.cos(time * 1.2))
        staticTorso.rotation = -4 * math.sin(time / 1.2)
        staticFalseUpperBody.MoveToAbs(staticTorso.absx, staticTorso.absy)

        staticLArm.rotation = -6 * math.sin(time / 1.5)
        staticLHand.rotation = -8 * math.sin(time / 1.3)

        staticRArm.rotation = 2 - 2 * math.cos(time / 3)
        staticRHand.rotation = 2 - 2 * math.cos(time / 3)

        staticHeadBase.rotation = -5 * math.sin(time)
        staticHeadBase.MoveTo(2, 46 + -3 * math.sin(time / 2))

        staticLEar.rotation = 10 * math.sin(time * 1.5)
        staticREar.rotation = -10 * math.sin(time * 1.5)

        if staticTorso.alpha > 0 then
            time = time + Time.dt
        end
    else
        -- Death oneshot animation
        if timer <= 115 then
            -- Update the face's static sprite
            if timer % 5 == 0 then
                staticSprites[2][1].Set("Static/Death/staticeffect" .. math.random(1, 3))
                staticSprites[2][1].rotation = math.random(0, 1) * 180
                staticSprites[2][1].MoveTo(math.random(0, staticSprites[2]["diffX"]) - staticSprites[2]["halfDiffX"],
                                           math.random(0, staticSprites[2]["diffY"]) - staticSprites[2]["halfDiffY"])
            end

            -- Spawn smoke
            if timer % 15 == 14 then
                local smoke = CreateSprite("Static/Death/smoke", "BelowArena")
                smoke.MoveToAbs(staticDeathExplosion.absx + math.random(-15, 15), staticDeathExplosion.absy + math.random(-15, 15))
                smoke.alpha = 0.5
                smoke.Scale(0, 0)
                smoke["timer"] = timer
                table.insert(staticDeathSmokes, smoke)
            end

            -- Play static sound
            if timer == 30 then
                NewAudio.CreateChannel("static")
                NewAudio.PlaySound("static", "static", false, 0.2)
            end

            -- Ouch! Explosions hurt!
            if timer < 10 then                      staticHeadBase.rotation = staticHeadBase.rotation + 1.5
            elseif timer >= 30 and timer < 60 then  staticHeadBase.rotation = staticHeadBase.rotation - 0.5
            -- Drooping arms
            elseif timer >= 90 and timer < 100 then staticLHand.rotation = staticLHand.rotation + 5
                                                    staticRHand.rotation = staticRHand.rotation + 2
            -- Replace a few hand sprites
            elseif timer == 100 then
                staticLHand.Set("Static/Death/rhand")
                staticLHand.SetPivot(0.33, 0.8)
                staticLHand.xscale = -2
                staticLHand.rotation = 75
                staticLHand.Move(7, -4)

                staticRArm.Move(0, -6)
                staticRHand.Set("Static/Death/rhand")
                staticRHand.SetPivot(0.33, 0.8)
                staticRHand.rotation = -45
                staticRHand.Move(-4, -6)
            -- Life slowly leaves the body as all body parts start to dangle
            elseif timer > 100 and timer < 115 then staticLHand.rotation = staticLHand.rotation - 5
                                                    staticLArm.rotation = staticLArm.rotation + 2
                                                    staticRHand.rotation = staticRHand.rotation + 2
                                                    staticRArm.rotation = staticRArm.rotation - 2
                                                    staticTail.yscale = staticTail.yscale - 2/7
                                                    staticLEar.rotation = staticLEar.rotation + 1.5
                                                    staticREar.rotation = staticREar.rotation - 1.5
            -- ded
            elseif timer == 115 then
                deathParent.Set("Static/Death/dustingmess")
                deathParent.alpha = 1
                deathParent.Move(13, 0)
                DeleteAnimation()
                deathParent.Dust(true, true)
                NewAudio.DestroyChannel("static")
            end
        end

        -- Update smoke
        for i = #staticDeathSmokes, 1, -1 do
            local smoke = staticDeathSmokes[i]
            local time = timer - smoke["timer"]

            smoke.Move(0, 0.5)
            if time < 30 then smoke.Scale((time + 1) / 30, (time + 1) / 30)
            else              smoke.alpha = smoke.alpha - 0.5 / 30
            end

            if smoke.alpha == 0 then
                table.remove(staticDeathSmokes, i)
                smoke.Remove()
            end
        end
    end

    timer = timer + 1
end

-- Reset the animation to its starting Idle animation positions
function ResetStatic()
    staticLegs.yscale = 2

    staticTail.rotation = 0

    staticTorso.MoveTo(7, 3)
    staticTorso.rotation = 0
    staticFalseUpperBody.MoveToAbs(staticTorso.absx, staticTorso.absy)

    staticLArm.rotation = 0
    staticLHand.rotation = 0

    staticRArm.rotation = 0
    staticRHand.rotation = 0

    staticHeadBase.rotation = 0
    staticHeadBase.MoveTo(2, 46)

    staticLEar.rotation = 0
    staticREar.rotation = 0
end

-- It hurts to be knifed
function AnimateHurt(hurt)
    local animAlpha = hurt and 0 or 1

    staticTail.alpha = animAlpha
    staticTailStatic.alpha = animAlpha

    staticLArm.alpha = animAlpha
    staticLHand.alpha = animAlpha

    staticLegs.alpha = animAlpha
    staticTorso.alpha = animAlpha

    staticRArm.alpha = animAlpha
    staticRHand.alpha = animAlpha

    staticFluff.alpha = animAlpha
    staticFluffStatic.alpha = animAlpha

    staticHeadBase.alpha = animAlpha
    staticFace.alpha = animAlpha
    staticFaceStatic.alpha = animAlpha

    staticLEar.alpha = animAlpha
    staticLEarStatic.alpha = animAlpha

    staticREar.alpha = animAlpha
    staticREarStatic.alpha = animAlpha

    staticHurt.alpha = 1 - animAlpha
    staticHurtFluffStatic.alpha = 1 - animAlpha
    staticHurtTailStatic.alpha = 1 - animAlpha
    staticHurtFaceStatic.alpha = 1 - animAlpha
    staticHurtLEarStatic.alpha = 1 - animAlpha
    staticHurtREarStatic.alpha = 1 - animAlpha

    ResetStatic()
    time = 0
end

-- It's nice to be spared
function AnimateSpare(spare)
    local animAlpha = spare and 0 or 1

    staticTail.alpha = animAlpha
    staticTailStatic.alpha = animAlpha

    staticLArm.alpha = animAlpha
    staticLHand.alpha = animAlpha

    staticLegs.alpha = animAlpha
    staticTorso.alpha = animAlpha

    staticRArm.alpha = animAlpha
    staticRHand.alpha = animAlpha

    staticFluff.alpha = animAlpha
    staticFluffStatic.alpha = animAlpha

    staticHeadBase.alpha = animAlpha
    staticFace.alpha = animAlpha
    staticFaceStatic.alpha = animAlpha

    staticLEar.alpha = animAlpha
    staticLEarStatic.alpha = animAlpha

    staticREar.alpha = animAlpha
    staticREarStatic.alpha = animAlpha

    staticSpared.alpha = 1 - animAlpha

    ResetStatic()
    time = 0
end

-- Change Static's TV screen so it displays a quick Mettaton animation
function ChangeChannel()
    table.remove(staticSprites, 2)
    staticFaceStatic.Scale(2, 2)
    staticFaceStatic.rotation = staticFace.rotation
    staticFaceStatic.MoveTo(0, 0)

    local a = "Static/faceMTT1"
    local b = "Static/faceMTT2"
    staticFaceStatic.SetAnimation({ a, a, a, b, b, b, a, b, a, b }, 1 / 8)
end

-- Destroy it all
function DeleteAnimation()
    staticSpared.Remove()

    staticHurtREarStatic.Remove()
    staticHurtREarMask.Remove()

    staticHurtLEarStatic.Remove()
    staticHurtLEarMask.Remove()

    staticHurtFaceStatic.Remove()
    staticHurtFaceMask.Remove()

    staticHurtTailStatic.Remove()
    staticHurtTailMask.Remove()

    staticHurtFluffStatic.Remove()
    staticHurtFluffMask.Remove()

    staticHurt.Remove()

    staticREarStatic.Remove()
    staticREarMask.Remove()
    staticREar.Remove()

    staticLEarStatic.Remove()
    staticLEarMask.Remove()
    staticLEar.Remove()

    staticDeathExplosion.Remove()

    staticFaceStatic.Remove()
    staticFaceMask.Remove()
    staticFace.Remove()
    staticHeadBase.Remove()
    staticFalseHeadBase.Remove()

    staticFluffStatic.Remove()
    staticFluffMask.Remove()
    staticFluff.Remove()

    staticRHand.Remove()
    staticRArm.Remove()

    staticTorso.Remove()
    staticLegs.Remove()

    staticLHand.Remove()
    staticLArm.Remove()

    staticTailStatic.Remove()
    staticTailMask.Remove()
    staticTail.Remove()
    staticFalseUpperBody.Remove()
end