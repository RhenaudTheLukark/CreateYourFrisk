comments = {"The world is covered in\ndarkness."}
commands = {}
randomdialogue = {"[func:Hurting]..."}
sprite = "ExRumia/1" --Always PNG. Extension is added automatically.
name = "Rumia EX"
hp = 1150
atk = 20
def = 20
check = "Another Monster to Kill."
dialogbubble = "rightlargeminus" -- See documentation for what bubbles you have available.
cancheck = true
canspare = false
voice = "rum"

function HandleAttack(attackstatus) end

function LaunchFade(begin) Encounter.Call("LaunchFade", begin) end

function HandleCustomCommand(command) end