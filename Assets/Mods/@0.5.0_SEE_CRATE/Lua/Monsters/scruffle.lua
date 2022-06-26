comments = {
    "Smells like stuffing.",
    "Scruffle looks you up and down, watching your movements intensely.",
    "A little bit of cotton falls from an opening in Scruffle's side, which he covers with his big hand."
}

commands = {}

randomdialogue = { "[effect:none]..." }

sprite = "Scruffle/hollow"
name = "Scruffle"
hp = 100
atk = 0
def = 0
fakeatk = 2
fakedef = 5
check = "Tired of being mistaken for pajamas."
dialogbubble = "scruffle"
canspare = false
cancheck = false
effect = "none"

gold = 12
xp = 100

function EncounterStarting()
    SetBubbleOffset(10, 60)

    -- Parent sprite used for the death animation, since the actual monstersprite is destroyed as soon as the enemy is killed
    deathParent = CreateSprite("Scruffle/hollow", "BelowArena")
    deathParent.SetPivot(0.5, 0)
    deathParent.MoveToAbs(monstersprite.absx, monstersprite.absy)

    require "Animations/ScruffleAnim"
end

function Update()
    AnimateScruffle()
end

function HandleAttack(attackstatus)
end

function LaunchFade(begin) Encounter.Call("LaunchFade", begin) end

function HandleCustomCommand(command) end