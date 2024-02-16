-- A basic monster script skeleton you can copy and modify for your own creations.
comments = { "Lukark is vengeful.", "Lukark prepares his next move.", "Lukark frowns at you.",
             "Lukark brushes his hair as dust gets into it.", "Lukark gazes at you with intensity.",
             "The dust staining your clothes only fuels Lukark's rage." }
commands = { "Pardon" }
randomdialogue = { "[effect:none][func:SetFace,angry]Come back here!",
                   "[effect:none][func:SetFace,angry]You can't escape me now!",
                   "[effect:none][func:SetFace,angry]I'll get you no matter what!",
                   "[effect:none][func:SetFace,angry]Take this!" }

sprite = "Lukark/full" --Always PNG. Extension is added automatically.
name = "Lukark"
hp = 1000
atk = 5
def = 1
gold = math.random(35, 105)
xp = math.random(125, 375)
check = "The Overworld Creator.[w:15]\rDestroy him if you dare."
dialogbubble = "rightwideminus" -- See documentation for what bubbles you have available.
cancheck = true
canspare = false
pardonCount = 0
attackprogression = 0
anim = "normal"
SetActive(false)
effect = "none"

-- Happens after the slash animation but before
function HandleAttack(attackstatus)
    if attackstatus > 0 then
        SetFace("hurt")
    end
end

-- This handles the commands; all-caps versions of the commands list you have above.
function HandleCustomCommand(command)
    if command == "PARDON" then
        if pardonCount == 0 then
            BattleDialog({ "You tell Lukark you'll stop your killing frenzy.", "It doesn't seem to calm him down one bit." })
            currentdialogue = { "[effect:none][func:SetFace,angry]Hah! As if someone like you could feel remorse!", "[next]" }
        elseif pardonCount == 1 then
            BattleDialog({ "Your voice crackles as you mutter an apology to Lukark." })
            currentdialogue = {"[effect:none]...[w:20][noskip][func:SetFace,smiling][noskip:off]I still don't believe you, you killer.", "[next]" }
        elseif pardonCount == 2 then
            BattleDialog({ "A tear roll down your cheek as you bow in front of Lukark, pleading him to stop this nonsense." })
            currentdialogue = { "[effect:none]......[noskip][w:20][func:SetFace,happy][noskip:off]Please stop. We both know this has gone too far by now.", "[next]" }
        else
            BattleDialog({ "You ask for Lukark's forgiveness once more, but he ignores you." })
        end
        pardonCount = pardonCount + 1
    end
end

function OnDeath()
    hp = 1
    Audio.Pause()
    currentdialogue = { "[noskip][effect:none]Gah, [w:20]I knew it...[w:20][next]",
                        "[noskip][effect:none]I should have been more careful...[w:20][next]",
                        "[noskip][effect:none][func:SetFace,happy]What a joke...[w:20][func:FadeKill][func:Kill][next]" }
end

-- Starts the death fade anim
function FadeKill() Encounter.Call("LukarkHideAnimation", true) end

-- Changes Lukark's animation
function SetFace(anim) Encounter.Call("SetLukarkFace", anim) end

function Pause()   Audio.Pause()   end
function Unpause() Audio.Unpause() end

-- Plays a given sound
function PlaySE(filename) Audio.PlaySound(filename) end

-- Enables this enemy
function Activate() SetActive(true) end