-- A basic monster script skeleton you can copy and modify for your own creations.
comments = {"Smells like the work\rof an enemy stand.", "Poseur is posing like his\rlife depends on it.", "Poseur's limbs shouldn't be\rmoving in this way."}
commands = {"Act 1", "Act 2", "Act 3"}
randomdialogue = {"Check\nit\nout."}

sprite = "poseur" --Always PNG. Extension is added automatically.
name = "Poseur"
hp = 60
atk = 4
def = 1
check = "Do not insult its hair."
dialogbubble = "right" -- See documentation for what bubbles you have available.
canspare = false
cancheck = true
xp = 10
gold = 20

-- Happens after the slash animation but before the animation.
function HandleAttack(attackstatus)
    if attackstatus == -1 then
        -- player pressed fight but didn't press Z afterwards
        currentdialogue = {"Do\nno\nharm."}
    else
        -- player did actually attack
        if hp > 30 then
            currentdialogue = {"You're\nstrong!"}
        else
            currentdialogue = {"Too\nstrong\n..."}
        end
    end
end

-- This handles the commands; all-caps versions of the commands list you have above.
function HandleCustomCommand(command)
    if command == "ACT 1" then
        currentdialogue = {"Selected\nAct 1."}
    elseif command == "ACT 2" then
        currentdialogue = {"Selected\nAct 2."}
    elseif command == "ACT 3" then
        currentdialogue = {"Selected\nAct 3."}
    end
    currentdialogue = {currentdialogue[1]}
    BattleDialog({"You selected " .. command .. "."})
end