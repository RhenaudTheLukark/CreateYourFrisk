comments = {"Smells like endorphins."}
commands = {}
randomdialogue = {"[func:think][effect:none][voice:000029fc]Mm-hmm...\nYes...[func:normal]"}
sprite = "emptycereb" --Always PNG. Extension is added automatically.
name = "Cereb"
hp = 320
atk = 20
def = 0
check = "A broad-minded individual."
dialogbubble = "rightlargeminus" -- See documentation for what bubbles you have available.
cancheck = true
canspare = false
voice = "000029fc"

function HandleAttack(attackstatus)
end

function LaunchFade(begin) Encounter.Call("LaunchFade", begin) end

function HandleCustomCommand(command) end