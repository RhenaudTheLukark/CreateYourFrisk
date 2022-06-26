scruffleFalseUpperBody = CreateSprite("empty")
scruffleLArmTop = CreateSprite("Scruffle/leftarmtop")
scruffleLHand = CreateSprite("Scruffle/lefthand")
scruffleLArmBot = CreateSprite("Scruffle/leftarmbot")

scruffleLLeg = CreateSprite("Scruffle/leftleg")
scruffleRLeg = CreateSprite("Scruffle/rightleg")

scruffleUpperBody = CreateSprite("empty")
scruffleHood = CreateSprite("Scruffle/hoodtail")
scruffleTorso = CreateSprite("Scruffle/torso")
scruffleHead = CreateSprite("Scruffle/head")
scruffleBlush = CreateSprite("Scruffle/blush")

scruffleRArmTop = CreateSprite("Scruffle/rightarmtop")
scruffleRHand = CreateSprite("Scruffle/righthand")
scruffleRArmBot = CreateSprite("Scruffle/rightarmbot")

scruffleHurt = CreateSprite("Scruffle/hurt")
scruffleSpared = CreateSprite("Scruffle/spared")

scruffleFalseUpperBody.Scale(scruffleTorso.width * 2, scruffleTorso.height * 2)
scruffleLArmTop.Scale(2, 2)
scruffleLHand.Scale(2, 2)
scruffleLArmBot.Scale(2, 2)
scruffleLLeg.Scale(2, 2)
scruffleRLeg.Scale(2, 2)
scruffleUpperBody.Scale(scruffleTorso.width * 2, scruffleTorso.height * 2)
scruffleHood.Scale(2, 2)
scruffleTorso.Scale(2, 2)
scruffleHead.Scale(2, 2)
scruffleBlush.Scale(2, 2)
scruffleRArmTop.Scale(2, 2)
scruffleRHand.Scale(2, 2)
scruffleRArmBot.Scale(2, 2)
scruffleHurt.Scale(2, 2)

--[[
Parenting tree:

deathParent
monstersprite
  scruffleFalseUpperBody
    scruffleLArmTop
      scruffleLHand
      scruffleLArmBot
  scruffleLLeg
  scruffleRLeg
  scruffleUpperBody
    scruffleHood
    scruffleTorso
      scruffleHead
        scruffleBlush
      scruffleRArmTop
        scruffleRHand
        scruffleRArmBot
  scruffleHurt
  scruffleSpared
]]

scruffleFalseUpperBody.SetParent(monstersprite)
scruffleLArmTop.SetParent(scruffleFalseUpperBody)
scruffleLHand.SetParent(scruffleLArmTop)
scruffleLArmBot.SetParent(scruffleLArmTop)

scruffleLLeg.SetParent(monstersprite)
scruffleRLeg.SetParent(monstersprite)

scruffleUpperBody.SetParent(monstersprite)
scruffleHood.SetParent(scruffleUpperBody)
scruffleTorso.SetParent(scruffleUpperBody)
scruffleHead.SetParent(scruffleTorso)
scruffleBlush.SetParent(scruffleHead)

scruffleRArmTop.SetParent(scruffleTorso)
scruffleRHand.SetParent(scruffleRArmTop)
scruffleRArmBot.SetParent(scruffleRArmTop)

scruffleHurt.SetParent(monstersprite)
scruffleSpared.SetParent(monstersprite)



scruffleFalseUpperBody.MoveToAbs(monstersprite.absx - 4, monstersprite.absy + 80)
scruffleLArmTop.MoveTo(-13, 64)
scruffleLArmTop.SetPivot(0.7, 0.88)
scruffleLHand.MoveTo(-20, -18)
scruffleLHand.SetPivot(0.67, 0.96)
scruffleLArmBot.MoveTo(7, 18)
scruffleLArmBot.SetPivot(0.7, 0.88)

scruffleLLeg.SetPivot(0.5, 0)
scruffleLLeg.MoveToAbs(monstersprite.absx - 31, monstersprite.absy)
scruffleRLeg.SetPivot(0.5, 0)
scruffleRLeg.MoveToAbs(monstersprite.absx + 27, monstersprite.absy)

scruffleUpperBody.MoveToAbs(monstersprite.absx - 4, monstersprite.absy + 80)
scruffleHood.MoveTo(11, 90)
scruffleHood.SetPivot(0.105, 0.6)
scruffleTorso.MoveTo(0, 0)
scruffleTorso.SetPivot(0.5, 0.25)
scruffleHead.MoveTo(5, 56)
scruffleHead.SetPivot(0.7, 0.22)
scruffleBlush.MoveTo(0, 0)
scruffleBlush.alpha = 0

scruffleRArmTop.MoveTo(22, 26)
scruffleRArmTop.SetPivot(0.22, 0.85)
scruffleRHand.MoveTo(13, -15)
scruffleRHand.SetPivot(0.39, 0.96)
scruffleRArmBot.MoveTo(-12, 19)
scruffleRArmBot.SetPivot(0.22, 0.85)

scruffleHurt.SetPivot(.5, 0)
scruffleHurt.SetAnchor(.5, 0)
scruffleHurt.MoveTo(0, 0)
scruffleHurt.alpha = 0

scruffleSpared.SetPivot(.5, 0)
scruffleSpared.SetAnchor(.5, 0)
scruffleSpared.MoveTo(2, -4)
scruffleSpared.color = { 0.5, 0.5, 0.5, 0 }

local time = 0
local timer = 0

animation = "Idle"

function SwitchAnimation(anim)
    -- Reset the anim properly
    ResetScruffle()
    time = 0

    -- Differences between the Idle and Death animation
    if anim == "Death" then
        scruffleLLeg.Set("Scruffle/Death/leftleg")
        scruffleRLeg.Set("Scruffle/Death/rightleg")

        scruffleRArmTop.Set("Scruffle/leftarm")
        scruffleRArmTop.SetPivot(0.7, 0.88)
        scruffleRArmTop.Move(-17, -34)
        scruffleRArmBot.Set("empty")

        scruffleRHand.Set("Scruffle/Death/righthand")
        scruffleRHand.MoveTo(-4, -18)
        scruffleRHand.SetPivot(0.66, 0.06)

        scruffleFalseUpperBody.SetParent(deathParent)
        scruffleLLeg.SetParent(deathParent)
        scruffleRLeg.SetParent(deathParent)
        scruffleUpperBody.SetParent(deathParent)
        scruffleHurt.SetParent(deathParent)
    end

    animation = anim
end

function AnimateScruffle()
    -- Idle looped animation
    if animation == "Idle" then
        scruffleHood.rotation = 10 * math.sin(time * 2)
        scruffleHead.MoveTo(5, 56 + 1.5 * math.sin(time * 1.5))
        scruffleHead.rotation = 5 * math.sin(time)

        scruffleLArmTop.rotation = 5 * math.sin(time * 1.75)
        scruffleLArmBot.rotation = 5 * math.sin(time * 1.75)
        scruffleLHand.rotation = -10 + 6 * math.sin(time / 1.25)

        scruffleRArmTop.rotation = 4 * math.sin(time)
        scruffleRArmBot.rotation = 4 * math.sin(time)
        scruffleRHand.rotation = -4 + 6 * math.sin(time / 1.4)

        scruffleFalseUpperBody.rotation = 3 * math.sin(time / 1.3)
        scruffleFalseUpperBody.MoveToAbs(monstersprite.absx - 4, monstersprite.absy + 81 + 2 * math.sin(time * 1.1))
        scruffleUpperBody.rotation = 3 * math.sin(time / 1.3)
        scruffleUpperBody.MoveToAbs(monstersprite.absx - 4, monstersprite.absy + 81 + 2 * math.sin(time * 1.1))
        if scruffleHood.alpha > 0 then
            time = time + Time.dt
        end
    -- Death oneshot animation
    elseif animation == "Death" then
        -- Right arm detaches from the body (hahaha)
        if timer == 0 then
            scruffleRArmTop.SetParent(deathParent)
            scruffleRArmTop.SetPivot(0.23, 0.17)
            Audio.PlaySound("spear")
        -- Right arm falls on the ground
        elseif timer < 45 then
            scruffleRArmTop.Move(1.4, -timer * 4.2 / 45)
            scruffleRArmTop.rotation = scruffleRArmTop.rotation + 2.5
            scruffleRHand.rotation = scruffleRHand.rotation + 6
        -- Scruffle squeaks and looks at his missing arm
        elseif timer == 90 then
            scruffleHead.Move(14, 0)
            scruffleHead.Set("Scruffle/Death/head2")
            Audio.PlaySound("squeak")
        -- All the parts of the body are detached from the monster (hahahahaha)
        elseif timer == 135 then
            scruffleHead.Set("Scruffle/Death/head3")
            scruffleLArmTop.SetParent(deathParent)
            scruffleTorso.SetParent(deathParent)
            scruffleHood.SetParent(deathParent)
            scruffleLLeg.SetPivot(0.5, 0.5)
            scruffleLLeg.Move(0, scruffleLLeg.height)
            scruffleLLeg.SendToTop()
            scruffleRLeg.SetPivot(0.5, 0.5)
            scruffleRLeg.Move(0, scruffleRLeg.height)
            scruffleRLeg.SendToTop()
            scruffleRArmTop.SendToTop()
            scruffleHead.SetParent(deathParent)
            Audio.PlaySound("sudden")
        -- All the parts fall on the ground
        elseif timer > 135 and timer < 180 then
            scruffleLArmTop.rotation = scruffleLArmTop.rotation - 1.5
            scruffleLArmTop.Move(0.5, -(timer - 135) * 5.4 / 45)

            scruffleLLeg.rotation = scruffleLLeg.rotation - 1.8
            scruffleLLeg.Move(0, -(timer - 135) * 0.5 / 45)

            scruffleRLeg.rotation = scruffleRLeg.rotation + 1.8
            scruffleRLeg.Move(0.5, -(timer - 135) * 0.5 / 45)

            scruffleHood.rotation = scruffleHood.rotation + 3.9
            scruffleHood.Move(1.5, -(timer - 135) * 7 / 45)

            scruffleTorso.rotation = scruffleTorso.rotation + 1.9
            scruffleTorso.Move(0.75, -(timer - 135) * 2 / 45)

            scruffleHead.Move(0.5, -(timer - 135) * 6 / 45)
        -- Destroy the entire animation, replace deathParent by a unique dusting sprite
        elseif timer == 180 then
            deathParent.Set("Scruffle/Death/dustingmess")
            deathParent.alpha = 1
            deathParent.Move(13, -8)
            DeleteAnimation()
            deathParent.Dust(true, true)
        end

        timer = timer + 1
    end
end

-- Reset the animation to its starting Idle animation positions
function ResetScruffle()
    if animation == "Idle" then
        scruffleHood.rotation = 0
        scruffleHead.MoveTo(5, 56)
        scruffleHead.rotation = 0

        scruffleLArmTop.rotation = 0
        scruffleLArmBot.rotation = 0
        scruffleLHand.rotation = 0

        scruffleRArmTop.rotation = 0
        scruffleRArmBot.rotation = 0
        scruffleRHand.rotation = 0

        scruffleFalseUpperBody.rotation = 0
        scruffleFalseUpperBody.MoveToAbs(monstersprite.absx - 4, monstersprite.absy + 80)
        scruffleUpperBody.rotation = 0
        scruffleUpperBody.MoveToAbs(monstersprite.absx - 4, monstersprite.absy + 80)
    end
end

blushAlpha = 0
-- Makes the blushing sprite visible with a given alpha value
function Blush(alpha)
    scruffleBlush.alpha = alpha
    blushAlpha = alpha
end

-- It hurts to be knifed
function AnimateHurt(hurt)
    local animAlpha = hurt and 0 or 1

    scruffleLArmTop.alpha = animAlpha
    scruffleLHand.alpha = animAlpha
    scruffleLArmBot.alpha = animAlpha

    scruffleLLeg.alpha = animAlpha
    scruffleRLeg.alpha = animAlpha

    scruffleHood.alpha = animAlpha
    scruffleTorso.alpha = animAlpha
    scruffleHead.alpha = animAlpha
    scruffleBlush.alpha = animAlpha == 0 and 0 or blushAlpha

    scruffleRArmTop.alpha = animAlpha
    scruffleRHand.alpha = animAlpha
    scruffleRArmBot.alpha = animAlpha

    scruffleHurt.alpha = 1 - animAlpha

    ResetScruffle()
    time = 0
end

-- It's nice to be spared
function AnimateSpare(spare)
    local animAlpha = spare and 0 or 1

    scruffleLArmTop.alpha = animAlpha
    scruffleLHand.alpha = animAlpha
    scruffleLArmBot.alpha = animAlpha

    scruffleLLeg.alpha = animAlpha
    scruffleRLeg.alpha = animAlpha

    scruffleHood.alpha = animAlpha
    scruffleTorso.alpha = animAlpha
    scruffleHead.alpha = animAlpha
    scruffleBlush.alpha = animAlpha == 0 and 0 or blushAlpha

    scruffleRArmTop.alpha = animAlpha
    scruffleRHand.alpha = animAlpha
    scruffleRArmBot.alpha = animAlpha

    scruffleSpared.alpha = 1 - animAlpha

    ResetScruffle()
    time = 0
end

-- Destroy it all
function DeleteAnimation()
    scruffleHurt.Remove()

    scruffleRArmBot.Remove()
    scruffleRHand.Remove()
    scruffleRArmTop.Remove()

    scruffleBlush.Remove()
    scruffleHead.Remove()
    scruffleTorso.Remove()
    scruffleHood.Remove()
    scruffleUpperBody.Remove()

    scruffleRLeg.Remove()
    scruffleLLeg.Remove()

    scruffleLArmBot.Remove()
    scruffleLHand.Remove()
    scruffleLArmTop.Remove()
    scruffleFalseUpperBody.Remove()
end