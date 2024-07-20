comments = {
    "Smells like a lack of HDMI connection.",
    "Static stamps her paw in the ground, barely missing you with her claws.",
    "Static looks away. It's not polite to stare, and looking too closely will hurt your eyes, after all."
}

commands = {}

randomdialogue = {
    "[effect:none]Don't get too ruff!",
    "[effect:none]Quit staring!",
    "[effect:shake]*static noises*",
    "[effect:shake]*crackles*",
    "[effect:none]Distant connection..."
}

sprite = "Static/hollow"
name = "Static"
hp = 80
atk = 0
def = 0
fakeatk = 4
fakedef = 2
check = "Both their fur and their face are fuzzy."
dialogbubble = "static"
canspare = false
cancheck = false

gold = 100
xp = 110

function EncounterStarting()
    SetBubbleOffset(-10, 60)

    -- Parent sprite used for the death animation, since the actual monstersprite is destroyed as soon as the enemy is killed
    deathParent = CreateSprite("Static/hollow", "BelowArena")
    deathParent.SetPivot(0.5, 0)
    deathParent.MoveToAbs(monstersprite.absx, monstersprite.absy)

    require "Animations/StaticAnim"
end

function Update()
    AnimateStatic()
end

function HandleAttack(attackstatus)
end

function LaunchFade(begin) Encounter.Call("LaunchFade", begin) end

function HandleCustomCommand(command) end