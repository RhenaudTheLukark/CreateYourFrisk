-- A basic monster script skeleton you can copy and modify for your own creations.
comments = { "RTL seems nervous." }
commands = { "Check", "Threaten", "Insult" }
randomdialogue = { "H-Hello there..." }

sprite = "RTL/full" --Always PNG. Extension is added automatically.
name = "RTL"
hp = 100
atk = 1
def = -999
check = "Just free EXP.\rYou know the drill by now."
dialogbubble = "rightwideminus" -- See documentation for what bubbles you have available.
cancheck = false
canspare = true
sprite_placing = -1

SetBubbleOffset(10, 24)

function BeforeDamageCalculation()
    Encounter.Call("SetRTLAnimation", "attacked")
end

-- Happens after the slash animation but before
function HandleAttack(attackstatus)
    if attackstatus == -1 then
        -- player pressed fight but didn't press Z afterwards
        Encounter.Call("SetRTLAnimation", "close")
        currentdialogue = { "That was close..." }
    else
        -- player did actually attack
        Encounter.Call("SetRTLAnimation", "hurt")
    end
end

-- This handles the commands; all-caps versions of the commands list you have above.
function HandleCustomCommand(command)
    if command == "CHECK" then
        BattleDialog({ "RTL - 1 ATK 1 DEF\n" .. check })
    elseif command == "THREATEN" then
        BattleDialog({ "You threaten RTL.\nHe seems scared." })
        currentdialogue = {"[func:SetAnimRTL,attacked]Leave me alone! You're weird!"}
    elseif command == "INSULT" then
        BattleDialog({ "You insult RTL." })
        currentdialogue = {"[func:SetAnimRTL,angry]Hey, what was that for? I did nothing wrong to you!"}
    end
end

function OnDeath()
    SetAlMightyGlobal("RTLGenoIntro", false)
    hp = 30
    def = 1
    Audio.Pause()
    if not GetAlMightyGlobal("RTLGenoIntro") then
        SetAlMightyGlobal("RTLGenoIntro", true)
        currentdialogue = { "[noskip][effect:none]Heh... [w:15][func:SetAnimRTL,happy]Hahahaha![w:20][next]",
                            "[noskip][effect:none][func:SetAnimRTL,angry]You're pathetic. [w:15]Did you REALLY think I would just keel over and die like that?[w:20][next]",
                            "[noskip][effect:none][func:SetAnimRTL,angryhurt]Well you're [effect:shake][voice:v_floweymad][waitall:2]wrong,[waitall:1] [voice:monsterfont][effect:none][w:20]buckaroo.[w:20][next]",
                            "[noskip][effect:none]Now, I'll send you right where you belong, [w:10]and I'll do it by force if I have to![w:20][next]",
                            "[noskip][func:PlaySE,Asriel TF][func:StartTransformation][w:80][next]",
                            "[noskip][effect:shake][voice:v_floweymad][func:SetLukarkFace,mad]Your rampage will stop right now![w:20][func:SetLukark][func:State,ACTIONSELECT]" }
                            --"[noskip][effect:none][func:SetAnimRTL,happy][func:PlaySE,mus_dogsong_tf]" .. "What were you expecting, seriously?\n[w:20]Well there's nothing here, I'll just die and that's it.[w:20][func:Unpause][func:Kill][next]"}
    else
        shouts = {
            "Your rampage will stop right now!",
            "This time you'll pay for your sins!",
            "I will only stop when everyone will be avenged!",
            "I won't let you go this time!",
            "Retribution time!",
            "I guess last time wasn't enough for you!"
        }
        sillyShouts = {
            "Oh $#!?, [w:10]here we go again!",
            "Oh lawd he comin!",
            "Hey am I like at least 20% Sans if I can remember stuff? [w:15]Neat...?"
        }
        currentdialogue = { "[effect:none]Bah, what's the point. [w:15]Let's just get it over with already.",
                            "[noskip][func:PlaySE,Asriel TF][func:StartTransformation][w:80][next]",
                            "[noskip][effect:shake][voice:v_floweymad][func:SetLukarkFace,mad]" .. (math.random() < 0.04 and sillyShouts[math.random(#sillyShouts)] or shouts[math.random(#shouts)]) .. "[w:20][func:SetLukark][func:State,ACTIONSELECT]" }

    end
end

function OnSpare()
    Encounter.Call("RTLHideAnimation", true)
    Encounter["RTLAnim"].stopped = true
    Spare()
end

-- hanges the current animation
function SetAnimRTL(anim) Encounter.Call("SetRTLAnimation", anim) end
-- Changes Lukark's animation
function SetLukarkFace(anim) Encounter.Call("SetLukarkFace", anim) end

-- Starts the "transformation" animation
function StartTransformation() Encounter["RTLAnim"].inTransformation = true end

function Unpause() Audio.Unpause() end

-- Plays a given sound
function PlaySE(filename) Audio.PlaySound(filename) end

-- Starts the battle with Lukark
function SetLukark()
    Audio.Pitch(1)
    Audio.LoadFile("charafuntroncated")
    SetLukarkFace("normal")
end

-- Disables this enemy
function Deactivate() SetActive(false) end