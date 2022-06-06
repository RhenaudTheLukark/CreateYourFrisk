music = "Anticipation_Amplified"
encountertext = "It's time to kick his ass."
nextwaves = { }
wavetimer = 0
arenasize = { 130, 130 }

enemies = { "RTL", "Lukark" }
enemypositions = { { 0, 10 }, { 0, 0 } }

autolinebreak = true

-- A custom list with attacks to choose from. Actual selection happens in EnemyDialogueEnding(). Put here in case you want to use it.
possible_attacks = { "BallArms" }

function EncounterStarting()
    RTLAnim = (require "Animations/RTL_anim")()
    LukarkAnim = (require "Animations/Lukark_anim")()
    HPPulse = (require "Libraries/HeartPulse")()

    Player.lv = 12
    Player.hp = 64
    Audio.Pitch(0.2)

    LukarkAnim.HideAnimation()
    LukarkAnim.Animate()
    RTLAnim.SetAnimation("close")
    RTLAnim.Animate()
    enemies[1]["monstersprite"].alpha = 0
    enemies[2]["monstersprite"].alpha = 0
end

function SwitchEnemies()
    enemies[2].Call("Activate")
    enemies[1].Call("Deactivate")
    enemies[1].Call("SetBubbleOffset", { 29, 56.5 })
    encountertext = "The true battle begins now."
    wavetimer = 10
end

function Update()
    RTLAnim.Animate()
    LukarkAnim.Animate()
    if HPPulse then HPPulse.PulseByHP() end
end

function EnemyDialogueEnding()
    if enemies[2]["isactive"] then
        if enemies[2]["hp"] <= 200 then
            encountertext = "You notice Lukark seems quite beat up."
        end
        nextwaves = { possible_attacks[math.random(#possible_attacks)] }
    end
    encountertext = RandomEncounterText()

    RTLAnim.SetAnimation("close")
    LukarkAnim.SetFace("normal")
end

function DefenseEnding() LukarkAnim.SetFace("normal") end

function HandleSpare() State("ENEMYDIALOGUE") end

function SetLukarkArmAnim(anim)        LukarkAnim.SetArmAnim(anim)        end
function SetLukarkFace(anim)           LukarkAnim.SetFace(anim)           end
function LukarkHideAnimation(isKilled) LukarkAnim.HideAnimation(isKilled) end
function SetRTLAnimation(anim)         RTLAnim.SetAnimation(anim)         end
function RTLHideAnimation(isSpared)    RTLAnim.HideAnimation(isSpared)    end