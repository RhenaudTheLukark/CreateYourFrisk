comments = {"Claribel peeks out at you from\rbehind Ellie's back."}
commands = {}
randomdialogue = {"[effect:none][voice:claribel](Um...)"}
sprite = "claribel" --Always PNG. Extension is added automatically.
name = "Claribel"
hp = 2
atk = 1
def = 1
check = "You shouldn't see this."
dialogbubble = "leftlargeminus" -- See documentation for what bubbles you have available.
canspare = false
cancheck = false
voice = "claribel"

function HandleAttack(attackstatus)
end

function LaunchFade(begin) Encounter.Call("LaunchFade", begin) end

function HandleCustomCommand(command) end