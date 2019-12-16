comments = {"Lukark is revengeful."}
commands = {}
randomdialogue = {"[noskip]Come back\nhere!"}
sprite = "emptylukark" --Always PNG. Extension is added automatically.
name = "Lukark"
hp = 1000
atk = 5
def = 1
check = "The Overworld Creator.[w:10]\nJust destroy him."
dialogbubble = "rightlargeminus" -- See documentation for what bubbles you have available.
cancheck = true
canspare = false
voice = "lukark"

function HandleAttack(attackstatus) end

function LaunchFade(begin) Encounter.Call("LaunchFade", begin) end

function Animate(animation) Encounter.Call("Animate", animation) end

function HandleCustomCommand(command) end