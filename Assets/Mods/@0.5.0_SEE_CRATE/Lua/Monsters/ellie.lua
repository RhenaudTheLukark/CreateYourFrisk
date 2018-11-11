comments = {"Ellie is waiting for your answer."}
commands = {}
randomdialogue = {"[effect:none][voice:ellie]I hope we're not\nimposing on you.","[effect:none][voice:ellie]Do you know the\nway out of here?"}
sprite = "ellie" --Always PNG. Extension is added automatically.
name = "Ellie"
hp = 180
atk = 3
def = 999
check = "You shouldn't see this."
dialogbubble = "rightlargeminus" -- See documentation for what bubbles you have available.
canspare = false
cancheck = false
voice = "ellie"

function HandleAttack(attackstatus)
end

function LaunchFade(begin) Encounter.Call("LaunchFade", begin) end

function HandleCustomCommand(command) end