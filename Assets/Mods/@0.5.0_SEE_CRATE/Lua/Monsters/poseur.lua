comments = {"Smells like the work\rof an enemy stand.", "Poseur is posing like his\rlife depends on it.", "Poseur's limbs shouldn't be\rmoving in this way."}
commands = {}
randomdialogue = {"Random\nDialogue\n1.", "Random\nDialogue\n2.", "Random\nDialogue\n3."}
sprite = "poseur" --Always PNG. Extension is added automatically.
name = "Poseur"
hp = 100
atk = 1
def = 1
check = "Check message goes here."
dialogbubble = "rightlargeminus" -- See documentation for what bubbles you have available.
cancheck = true
canspare = false

function HandleAttack(attackstatus) end

function LaunchFade(begin) Encounter.Call("LaunchFade", begin) end

function HandleCustomCommand(command) end