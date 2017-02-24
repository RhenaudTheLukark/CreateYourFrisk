comments = {"Smells like the work\rof an enemy stand.", "Poseur is posing like his\rlife depends on it.", "Poseur's limbs shouldn't be\rmoving in this way."}
commands = {"Regular", "Cyan", "Orange", "Green", "Combined"}
randomdialogue = {"Check\nit\nout."}

sprite = "poseur" --Always PNG. Extension is added automatically.
name = "Poseur"
hp = 60
atk = 4
def = 1
check = "Do not insult its hair."
dialogbubble = "right" -- See documentation for what bubbles you have available.
canspare = false
xp = 10
gold = 20

posecount = 0

function HandleAttack(attackstatus)
    if attackstatus == -1 then
        currentdialogue = {"Do\nno\nharm."}
    else
        if hp > 30 then
            currentdialogue = {"You're\nstrong!"}
        else
            currentdialogue = {"Too\nstrong\n..."}
        end
    end
end

function HandleCustomCommand(command)
    SetGlobal("wavetype", command)
    if command == "REGULAR" then
        BattleDialog({"The default bullettest_bouncy."})
    elseif command == "CYAN" then
        BattleDialog({"Cyan bullets. Stand still\rto avoid getting hit."})
    elseif command == "ORANGE" then 
        BattleDialog({"Orange bullets. Move through\rto avoid getting hit."})
    elseif command == "GREEN" then
        BattleDialog({"Green bullets.\rThey heal you for 1 HP."})
    elseif command == "COMBINED" then
        BattleDialog({"All behaviours combined.\nAn example of how to\rachieve this."})
    end
end